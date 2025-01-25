using UnityEngine;
using UnityEngine.Serialization;

namespace P3T.Scripts.Gameplay.Survivor
{
    public class SurvivorBullet : MonoBehaviour
    {
        [FormerlySerializedAs("Rigidbody")] [SerializeField]
        private Rigidbody2D _rigidbody;

        [FormerlySerializedAs("_collider")] 
        [FormerlySerializedAs("Collider")] [SerializeField]
        private Collider2D _damagingTrigger;

        [FormerlySerializedAs("Renderer")] [SerializeField]
        private SpriteRenderer _renderer;

        [SerializeField] private Collider2D _targetingTrigger;

        [SerializeField] private ParticleSystem _pierceFX;
        
        private float _currentImpulse;
        private SurvivorBulletManager _manager;
        private SurvivorBulletManager.BulletModifiers _modifiers =
            SurvivorBulletManager.BulletModifiers.None;
        private Rigidbody2D _target;
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
                _targetingTrigger.enabled = false;
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
                _targetingTrigger.enabled = true;
                // FindTargetWithRaycast();
            }
        }
        
        

        /// <summary>
        ///     Bullet hit something, check if we can damage it
        /// </summary>
        /// <param name="col"></param>
        private void OnDamageTriggerEnter(Collider2D col)
        {
            bool removeBullet = false;
            if (col == _gameBounds) removeBullet = true;
            else if (col.attachedRigidbody != null && _manager.LookupShootable(col.attachedRigidbody))
            {
                _target = null;
                _manager.DamageHazard(col.attachedRigidbody, _damagingTrigger);
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
                _damagingTrigger.enabled = false;
                _targetingTrigger.enabled = false;
                _manager.Release(this);
            }
        }

        /// <summary>
        ///     Find and sets target with a trigger collider
        /// </summary>
        /// <param name="col"> </param>
        private void OnTargetTriggerEnter(Collider2D col)
        {
            if (col == null) return;
            var rb = col.attachedRigidbody; 
            if (rb != null && _manager.LookupShootable(rb))
                SetTarget(rb);
        }
        
        /// <summary>
        ///     Set the target this bullet will seek
        /// </summary>
        /// <param name="col"></param>
        public SurvivorBullet SetTarget(Rigidbody2D col)
        {
            _target = col;
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
        public SurvivorBullet IgnoreCollider(Collider2D colliderToIgnore)
        {
            Physics2D.IgnoreCollision(colliderToIgnore, _damagingTrigger);
            return this;
        }

        /// <summary>
        ///     Set any flags on the bullet for special effects
        /// </summary>
        /// <param name="bulletModifiers"> </param>
        /// <returns> </returns>
        public SurvivorBullet SetModifiers(SurvivorBulletManager.BulletModifiers bulletModifiers)
        {
            _damagingTrigger.enabled = true;
            _modifiers = bulletModifiers;
            if (_modifiers.HasFlag(SurvivorBulletManager.BulletModifiers.Pierce))
                _pierceFX.Play();
            else
                _pierceFX.Stop();
            return this;
        }

        public void AimAndFireAtTarget()
        {
            var dir = _target.transform.position - transform.position;
            transform.right = new Vector3(dir.x, dir.y, transform.right.z);
            Fire();
        }

        /// <summary>
        ///     Fire off the bullet in it's facing direction
        /// </summary>
        /// <returns> </returns>
        public SurvivorBullet Fire()
        {
            //_rigidbody.AddForce(transform.right * _currentImpulse, ForceMode2D.Impulse);
            _rigidbody.linearVelocity = transform.right * (_currentImpulse * _rigidbody.mass);
            return this;
        }
    }
}