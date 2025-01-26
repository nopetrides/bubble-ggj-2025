using UnityEngine;

namespace P3T.Scripts.Gameplay.Survivor
{
    /// <summary>
    ///     A sprite used to show that an object is off screen
    /// </summary>
    public class OffScreenIndicator : MonoBehaviour
    {
        [SerializeField] private Renderer Renderer;
        private Camera _gameCamera;

        private Bounds _indicatorBounds;
        private Transform _targetTransform;
        private Vector3 _targetPosition;

        public void FixedUpdate()
        {
            UpdatePosition();
        }

        /// <summary>
        ///     Set the camera the game is using
        ///     todo make more efficient so each indicator doesn't call this
        /// </summary>
        private void UpdateBounds()
        {
            var camHeight = _gameCamera.transform.position.y;
            _indicatorBounds.center = _gameCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, camHeight));
            _indicatorBounds.min = _gameCamera.ViewportToWorldPoint(new Vector3(0.04f, 0.04f, camHeight));
            _indicatorBounds.max = _gameCamera.ViewportToWorldPoint(new Vector3(0.96f, 0.96f, camHeight));
        }

        public void SetCamera(Camera gameCamera)
        {
            _gameCamera = gameCamera;
        }

        /// <summary>
        ///     Set the transform of the target to follow
        /// </summary>
        /// <param name="targetTransform"> </param>
        /// <returns> </returns>
        public void SetTransform(Transform targetTransform)
        {
            _targetTransform = targetTransform;
            UpdatePosition();
        }

        public void SetPosition(Vector3 position)
        {
            _targetPosition = position;
            UpdatePosition();
        }

        /// <summary>
        ///     Set the color of the indicator
        ///     Different colors for different types of objects
        /// </summary>
        /// <param name="color"> </param>
        public void SetColor(Color color)
        {
            Renderer.material.color = color;
        }

        public void Reset()
        {
            _targetTransform = null;
            _targetPosition = Vector3.zero;
        }

        /// <summary>
        ///     Update the position of the indicator
        /// </summary>
        /// <returns> </returns>
        private void UpdatePosition()
        {
            UpdateBounds();
            var objectPosition = _targetTransform != null ? _targetTransform.position : _targetPosition;
            var boundsPoint = _indicatorBounds.ClosestPoint(objectPosition);
            transform.position = boundsPoint;
            transform.LookAt(objectPosition);
        }
    }
}