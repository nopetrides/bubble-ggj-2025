using UnityEngine;

namespace P3T.Scripts.Gameplay.Survivor
{
    public class SurvivorBullet : MonoBehaviour
    {
        [SerializeField] private Rigidbody Rigidbody;

        [SerializeField] private Collider DamagingTrigger;

        [SerializeField] private Renderer PrimaryRenderer;

        [SerializeField] private Collider TargetingTrigger;

        [SerializeField] private ParticleSystem PierceFX;
        
        private float _currentImpulse;
        private SurvivorBulletManager _manager;
        private SurvivorBulletManager.BulletModifiers _modifiers =
            SurvivorBulletManager.BulletModifiers.None;
        private Rigidbody _target;
        private Collider2D _gameBounds;

        private void Start()
        {
            // TODO split OnDamageTriggerEnter and OnTargetTriggerEnter callbacks
            /*
            _damagingTrigger.OnTriggerEnter2DAsObservable()
                .TakeUntilDestroy(this)
                .Subscribe(OnDamageTriggerEnter);
            
            _targetingTrigger.OnTriggerEnter2DAsObservable()
                .TakeUntilDestroy(this)
                .SkipWhile(_ => _target != null)
                .Subscribe(OnTargetTriggerEnter);
            */
        }
        
        /// <summary>
        ///     May need to find the most efficient way to do this
        /// </summary>
        public void FixedUpdate()
        {
            if (_target != null)
            {
                TargetingTrigger.enabled = false;
                if (_target.gameObject.activeInHierarchy)
                {
                    AimAndFireAtTarget();
                }
                else
                {
                    _target = null;
                }
            }
            else
            {
                TargetingTrigger.enabled = true;
                // FindTargetWithRaycast();
            }

            ClampPositionWithinLevelBounds();
        }

        private void ClampPositionWithinLevelBounds()
        {
            var bounds = _gameBounds.bounds;
            var minX = -1 * bounds.extents.x; // + size
            var maxX = bounds.extents.x; // - size
            var minY = -1 * bounds.extents.y; // + size
            var maxY = bounds.extents.y; // - size
            if (Rigidbody.position.x < minX ||
                Rigidbody.position.x > maxX || 
                Rigidbody.position.z < minY || 
                Rigidbody.position.z > maxY)
            {
                if (gameObject.activeSelf && _manager != null)
                {
                    _target = null;
                    DamagingTrigger.enabled = false;
                    TargetingTrigger.enabled = false;
                    _manager.Release(this);
                }
            }
        }

        /// <summary>
        ///     Bullet hit something, check if we can damage it
        /// </summary>
        /// <param name="col"></param>
        private void OnDamageTriggerEnter(Collider col)
        {
            bool removeBullet = false;
            //if (col == _gameBounds) removeBullet = true;
            if (col.attachedRigidbody != null && _manager.LookupHazard(col.attachedRigidbody))
            {
                _target = null;
                _manager.DamageHazard(col.attachedRigidbody, DamagingTrigger);
                IgnoreCollider(col); // could cause problems if collider gets reused but not cleared from the ignore list, but that shouldn't be the case
                // If the bullet has the pierce modifier, don't destroy it on collision with a hazard
                if (_modifiers.HasFlag(SurvivorBulletManager.BulletModifiers.Pierce) == false)
                {
                    removeBullet = true;
                }
            }

            if (removeBullet && gameObject.activeSelf && _manager != null)
            {
                _target = null;
                DamagingTrigger.enabled = false;
                TargetingTrigger.enabled = false;
                _manager.Release(this);
            }
        }

        /// <summary>
        ///     Find and sets target with a trigger collider
        /// </summary>
        /// <param name="col"> </param>
        private void OnTargetTriggerEnter(Collider col)
        {
            if (col == null) return;
            var rb = col.attachedRigidbody; 
            if (rb != null && _manager.LookupHazard(rb))
                SetTarget(rb);
        }
        
        /// <summary>
        ///     Set the target this bullet will seek
        /// </summary>
        /// <param name="rb"></param>
        public SurvivorBullet SetTarget(Rigidbody rb)
        {
            _target = rb;
            return this;
        }

        /// <summary>
        ///     Sets <inheritdoc cref="_manager" />
        /// </summary>
        /// <param name="manager"> </param>
        /// <returns> </returns>
        public SurvivorBullet SetManager(SurvivorBulletManager manager)
        {
            _manager = manager;
            return this;
        }

        /// <summary>
        ///     Sets the parent of the game object so it will be on the right canvas
        /// </summary>
        /// <param name="parentTransform"> </param>
        /// <returns> </returns>
        /// <remarks> Most implementations will not need this set </remarks>
        public SurvivorBullet SetParent(Transform parentTransform)
        {
            transform.SetParent(parentTransform);
            return this;
        }

        /// <summary>
        ///     Sets the <see cref="_currentImpulse" />
        /// </summary>
        /// <param name="speed"> </param>
        /// <returns> </returns>
        public SurvivorBullet SetImpulse(float speed)
        {
            _currentImpulse = speed;
            return this;
        }

        /// <summary>
        ///     Collider that will destroy the bullet when it leaves the game bounds
        /// </summary>
        /// <param name="boundsCollider"> </param>
        /// <returns> </returns>
        public SurvivorBullet SetBoundsCollider(Collider2D boundsCollider)
        {
            _gameBounds = boundsCollider;
            return this;
        }

        /// <summary>
        ///     Ignore a specific collider
        /// </summary>
        /// <param name="colliderToIgnore"> </param>
        /// <returns> </returns>
        public SurvivorBullet IgnoreCollider(Collider colliderToIgnore)
        {
            Physics.IgnoreCollision(colliderToIgnore, DamagingTrigger);
            return this;
        }

        /// <summary>
        ///     Set any flags on the bullet for special effects
        /// </summary>
        /// <param name="bulletModifiers"> </param>
        /// <returns> </returns>
        public SurvivorBullet SetModifiers(SurvivorBulletManager.BulletModifiers bulletModifiers)
        {
            DamagingTrigger.enabled = true;
            _modifiers = bulletModifiers;
            if (_modifiers.HasFlag(SurvivorBulletManager.BulletModifiers.Pierce))
                PierceFX.Play();
            else
                PierceFX.Stop();
            return this;
        }

        public void AimAndFireAtTarget()
        {
            var dir = _target.transform.position - transform.position;
            transform.forward = dir;
            Fire();
        }

        /// <summary>
        ///     Fire off the bullet in it's facing direction
        /// </summary>
        /// <returns> </returns>
        public SurvivorBullet Fire()
        {
            //_rigidbody.AddForce(transform.right * _currentImpulse, ForceMode2D.Impulse);
            Rigidbody.linearVelocity = transform.forward * (_currentImpulse * Rigidbody.mass);
            return this;
        }
    }
}