using UnityEngine;
using UnityEngine.Pool;

namespace P3T.Scripts.Gameplay.Survivor
{
    /// <summary>
    ///     Controls the offscreen indicators
    /// </summary>
    public class OffScreenIndicatorManager : MonoBehaviour
    {
        [SerializeField] private OffScreenIndicator IndicatorPrefab;

        [Header("Colors")] 
        [SerializeField] private Color HazardColor;
        [SerializeField] private Color PowerColor;
        
        private Bounds _cameraBounds;
        private Camera _gameCamera;
        private ObjectPool<OffScreenIndicator> _pool;

        /// <summary>
        ///     Setup the offscreen indicator bounds to match the camera
        ///     Create the Object Pool
        /// </summary>
        /// <param name="gameCamera"> </param>
        public void SetBoundsToCamera(Camera gameCamera)
        {
            _gameCamera = gameCamera;

            // create pool
            _pool = new ObjectPool<OffScreenIndicator>
            (
                () =>
                {
                    var pooledObject = Instantiate(IndicatorPrefab, transform);

                    pooledObject.transform.localScale = Vector2.one;
                    pooledObject.name = IndicatorPrefab.name + _pool.CountAll;
                    pooledObject.SetCamera(_gameCamera);
                    return pooledObject;
                },
                pooledObject => { pooledObject.gameObject.SetActive(true); },
                pooledObject =>
                {
                    pooledObject.Reset();
                    pooledObject.gameObject.SetActive(false);
                },
                pooledObject => Destroy(pooledObject.gameObject)
            );
        }

        /// <summary>
        ///     Gets the indicator out of the pool and set it up with the transform to track
        /// </summary>
        /// <param name="offScreenTransform"> </param>
        /// <returns> </returns>
        public OffScreenIndicator ShowHazardIndicator(Transform offScreenTransform)
        {
            var indicator = _pool.Get();
            indicator.SetTransform(offScreenTransform);
            indicator.SetColor(HazardColor);
            return indicator;
        }
        
        /// <summary>
        ///     Gets the indicator out of the pool and set it up with the transform to track
        /// </summary>
        /// <param name="position"> </param>
        /// <returns> </returns>
        public OffScreenIndicator ShowIncomingHazardIndicator(Vector3 position)
        {
            var indicator = _pool.Get();
            indicator.SetPosition(position);
            indicator.SetColor(HazardColor);
            return indicator;
        }

        /// <summary>
        ///     Gets the indicator out of the pool and set it up with the transform to track
        /// </summary>
        /// <param name="offScreenTransform"> </param>
        /// <returns> </returns>
        public OffScreenIndicator ShowPowerIndicator(Transform offScreenTransform)
        {
            var indicator = _pool.Get();
            indicator.SetTransform(offScreenTransform);
            indicator.SetColor(PowerColor);
            return indicator;
        }

        /// <summary>
        ///     Release the indicator back into the pool
        /// </summary>
        /// <param name="indicator"> </param>
        public void HideIndicator(OffScreenIndicator indicator)
        {
            _pool.Release(indicator);
        }
    }
}