using P3T.Scripts.Gameplay.Survivor.Pooled;
using P3T.Scripts.Gameplay.Survivor.ScriptableObjects;
using UnityEngine;

namespace P3T.Scripts.Gameplay.Survivor
{
    /// <summary>
    ///     The base class for pickup types.
    ///     Derived from <see cref="MonoTriggeredGamePower" />
    /// </summary>
    public class SurvivorPowerUp : MonoTriggeredGamePower
    {
        private static readonly int Collected = Animator.StringToHash("Collected");
        
        [SerializeField] private SurvivorPowerUpConfig Config;
        [SerializeField] private Renderer PrimaryRenderer;
        [SerializeField] private Animator Animator;
        [SerializeField] private Collider Collider;
        [SerializeField] private SurvivorPickupAnimationEventListener AnimationEventListener;

        private OffScreenIndicator _indicator;

        private SurvivorPowerUpManager _manager;

        public SurvivorPowerUpType SurvivorPower => Config.SurvivorPowerUp;

        public AudioClip AppearSound => Config.PickupAppearSound;

        public AudioClip CollectedSound => Config.PickupCollectedSound;

        private void FixedUpdate()
        {
            if (PrimaryRenderer.isVisible && _indicator != null)
            {
                _manager.NoLongerOffscreen(_indicator);
                _indicator = null;
            }
            else if (PrimaryRenderer.isVisible == false && _indicator == null)
            {
                _indicator = _manager.IndicateOffscreen(transform);
            }
        }

        /// <summary>
        ///     Check that collision is only with the player hero
        /// </summary>
        /// <param name="collision"> </param>
        protected override void OnTriggerEnter(Collider collision)
        {
            if (collision.isTrigger) return;
            // ReSharper disable once UnusedVariable
            if (!collision.attachedRigidbody.TryGetComponent(out SurvivorHero hero)) return;

            base.OnTriggerEnter(collision);
            Collider.enabled = false;

            if (Animator != null)
                Animator.SetBool(Collected, true);
            else
                CollectionDone();
        }

        private void CollectionDone()
        {
            base.OnCollectionDone();

            if (_indicator != null)
            {
                _manager.NoLongerOffscreen(_indicator);
                _indicator = null;
            }
            Collider.enabled = true;
            _manager.Release(this);
        }

        public SurvivorPowerUp SetParent(Transform parent)
        {
            transform.SetParent(parent);
            return this;
        }

        public SurvivorPowerUp SetManager(SurvivorPowerUpManager parent)
        {
            _manager = parent;
            if (AnimationEventListener != null) AnimationEventListener.OnAnimationComplete += CollectionDone;
            return this;
        }
    }
}