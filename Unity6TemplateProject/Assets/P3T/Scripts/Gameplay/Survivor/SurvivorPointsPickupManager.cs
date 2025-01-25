using P3T.Scripts.Managers;
using UnityEngine;
using UnityEngine.Pool;

namespace P3T.Scripts.Gameplay.Survivor
{
    public class SurvivorPointsPickupManager : MonoBehaviour
    {
        [SerializeField] private Transform PointsParent;
        [SerializeField] private SurvivorPointsPickup PointsPrefab;
        [SerializeField] private SurvivorPickupCollectedParticle DefaultCollectParticle;
        private SurvivorPickupCollectedParticle _collectParticlePrefab;

        private SurvivorController _controller;
        private ObjectPool<SurvivorPickupCollectedParticle> _fxPool;
        private ObjectPool<SurvivorPointsPickup> _pool;

        // Build the object pools for points and particles
        public void Setup(SurvivorController controller)
        {
            _controller = controller;
            // See if this asset has a unique explosions particle attached
            _collectParticlePrefab =
                PointsPrefab.GetComponentInChildren<SurvivorPickupCollectedParticle>(true)
                ?? DefaultCollectParticle;

            _pool = new ObjectPool<SurvivorPointsPickup>
            (
                () =>
                {
                    var pooledObject = Instantiate(PointsPrefab)
                        .SetParent(PointsParent)
                        .SetManager(this);

                    var count = _pool.CountAll;
                    pooledObject.name = PointsPrefab.name + count;
                    return pooledObject;
                },
                pooledObject => { pooledObject.gameObject.SetActive(true); },
                pooledObject => { pooledObject.gameObject.SetActive(false); },
                pooledObject => Destroy(pooledObject.gameObject)
            );

            _fxPool = new ObjectPool<SurvivorPickupCollectedParticle>(
                () =>
                {
                    var pooledObject = Instantiate(_collectParticlePrefab);
                    pooledObject.SetManager(this);
                    pooledObject.SetParent(PointsParent);

                    pooledObject.name = _collectParticlePrefab.name + _fxPool.CountAll;
                    return pooledObject;
                },
                pooledObject => { pooledObject.gameObject.SetActive(true); },
                pooledObject => { pooledObject.gameObject.SetActive(false); },
                pooledObject => Destroy(pooledObject.gameObject)
            );
        }

        public void SetConfigurableAsset(SurvivorController controller,
            SurvivorPointsPickupConfig configurableAsset)
        {
            PointsPrefab.SetManager(this);
            Setup(controller);
        }

        public void SpawnPointsPickup(Vector3 spawnPoint)
        {
            var points = _pool.Get();
            // ReSharper disable once Unity.InefficientPropertyAccess
            points.transform.position = new Vector3(spawnPoint.x, spawnPoint.y, points.transform.position.z);
        }

        public void PointsCollected(SurvivorPointsPickup pickup)
        {
            var position = pickup.transform.position;

            var particle = _fxPool.Get();
            particle.SetPosition(position);
            particle.Play();

            _controller.AddPointsPickupPoints(position);
            // Sound loaded from prefab config

            var sound = Random.Range(0, PointsPrefab.Config.CollectedSounds.Length);
            AudioMgr.Instance.PlaySound(PointsPrefab.Config.CollectedSounds[sound]);
        }

        public void Release(SurvivorPointsPickup pickup)
        {
            _pool.Release(pickup);
        }

        public void Release(SurvivorPickupCollectedParticle particle)
        {
            _fxPool.Release(particle);
        }
    }
}