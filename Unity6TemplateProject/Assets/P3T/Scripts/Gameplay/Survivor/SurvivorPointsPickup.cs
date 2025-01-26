using UnityEngine;

namespace P3T.Scripts.Gameplay.Survivor
{
    /// <summary>
    ///     A pooled object with a trigger collider that awards points when the player collides with it
    /// </summary>
    public class SurvivorPointsPickup : MonoBehaviour
    {
        public SurvivorPointsPickupConfig Config;
        
        private static readonly int Collected = Animator.StringToHash("Collected");

        [SerializeField] private Transform VisualsConfigAssetParent;

        [SerializeField] private Renderer SpawnedAssetRenderer;
        [SerializeField] private Animator SpawnedAssetAnimator;
        [SerializeField] private Collider SpawnedAssetCollider;
        [SerializeField] private SurvivorPickupAnimationEventListener AnimationEventListener;

        private SurvivorPointsPickupManager _manager;

        /// <summary>
        ///     Check that collision is only with the player hero
        /// </summary>
        /// <param name="collision"> </param>
        protected void OnTriggerEnter(Collider collision)
        {
            if (collision.isTrigger) return;
            // ReSharper disable once UnusedVariable
            if (!collision.attachedRigidbody.TryGetComponent(out SurvivorHero hero)) return;

            SpawnedAssetCollider.enabled = false;
            _manager.PointsCollected(this);

            if (SpawnedAssetAnimator != null)
                SpawnedAssetAnimator.SetBool(Collected, true);
            else
                CollectionDone();
        }

        public SurvivorPointsPickup SetParent(Transform parent)
        {
            transform.SetParent(parent);
            return this;
        }

        public SurvivorPointsPickup SetManager(SurvivorPointsPickupManager manager)
        {
            _manager = manager;
            SpawnedAssetCollider.isTrigger = true;
            if (AnimationEventListener != null) AnimationEventListener.OnAnimationComplete += CollectionDone;
            return this;
        }

        private void CollectionDone()
        {
            _manager.Release(this);
            SpawnedAssetCollider.enabled = true;
            if (SpawnedAssetAnimator != null) SpawnedAssetAnimator.SetBool(Collected, false);
        }
    }
}