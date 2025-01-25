using UnityEngine;

namespace P3T.Scripts.Gameplay.Survivor
{
    /// <summary>
    ///     The base class for pickup types.
    ///     Derived from <see cref="MonoTriggeredGamePower" />
    /// </summary>
    public class ShooterPickup : MonoTriggeredGamePower
    {
        /// <summary>
        ///     Types of power ups for the ArcadeSurvivor minigame
        /// </summary>
        public enum PowerUpType
        {
            FireRateUp = 1,
            Shield = 2,
            SpreadShot = 3,
            PierceShot = 4,
            RegainHealth = 5,
            Bomb = 6
        }
        
        private static readonly int Collected = Animator.StringToHash("Collected");

        [SerializeField] private PowerUpType PowerUp;

        [SerializeField] private Renderer PrimaryRenderer;

        [SerializeField] private AudioClip PickupAppearSound;

        [SerializeField] private AudioClip PickupCollectedSound;
        
        [SerializeField] private Animator Animator;
        [SerializeField] private Collider Collider;
        [SerializeField] private SurvivorPickupAnimationEventListener AnimationEventListener;

        private OffScreenIndicator _indicator;

        private SurvivorPowerUpManager _manager;

        public PowerUpType Power => PowerUp;

        public AudioClip AppearSound => PickupAppearSound;

        public AudioClip CollectedSound => PickupCollectedSound;

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
        protected override void OnTriggerEnter2D(Collider2D collision)
        {
            // ReSharper disable once UnusedVariable
            if (collision.attachedRigidbody.TryGetComponent(out SurvivorHero hero))
            {
                base.OnTriggerEnter2D(collision);
                Collider.enabled = false;

                if (Animator != null)
                    Animator.SetBool(Collected, true);
                else
                    CollectionDone();
            }
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

        public ShooterPickup SetParent(Transform parent)
        {
            transform.SetParent(parent);
            return this;
        }

        public ShooterPickup SetManager(SurvivorPowerUpManager parent)
        {
            _manager = parent;
            if (AnimationEventListener != null) AnimationEventListener.OnAnimationComplete += CollectionDone;
            return this;
        }
    }
}