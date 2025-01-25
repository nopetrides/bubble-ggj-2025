using UnityEngine;
using UnityEngine.Serialization;

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

        [FormerlySerializedAs("PowerUp")] [SerializeField]
        private PowerUpType _powerUp;

        [SerializeField] private SpriteRenderer _primaryRenderer;

        [FormerlySerializedAs("OnPickupAppearSound")] [SerializeField]
        private AudioClip _pickupAppearSound;

        [FormerlySerializedAs("OnPickupCollectedSound")] [SerializeField]
        private AudioClip _pickupCollectedSound;
        
        [SerializeField] private Animator _animator;
        [SerializeField] private Collider2D _collider;
        [SerializeField] private SurvivorPickupAnimationEventListener _animationEventListener;

        private OffScreenIndicator _indicator;

        private SurvivorPowerUpManager _manager;

        public PowerUpType Power => _powerUp;

        public AudioClip AppearSound => _pickupAppearSound;

        public AudioClip CollectedSound => _pickupCollectedSound;

        private void FixedUpdate()
        {
            if (_primaryRenderer.isVisible && _indicator != null)
            {
                _manager.NoLongerOffscreen(_indicator);
                _indicator = null;
            }
            else if (_primaryRenderer.isVisible == false && _indicator == null)
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
                _collider.enabled = false;

                if (_animator != null)
                    _animator.SetBool(Collected, true);
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
            _collider.enabled = true;
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
            if (_animationEventListener != null) _animationEventListener.OnAnimationComplete += CollectionDone;
            return this;
        }
    }
}