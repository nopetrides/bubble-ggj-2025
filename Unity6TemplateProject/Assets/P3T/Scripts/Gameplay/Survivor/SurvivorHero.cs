using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace P3T.Scripts.Gameplay.Survivor
{
    public class SurvivorHero : MonoBehaviour
    {
        [SerializeField] private SurvivorHeroConfig Config;
        
        [Header("Components")] 
        [SerializeField] private Transform HeroConfigAssetParent;

        [SerializeField] private Rigidbody2D Rigidbody;

        [Header("Bullets")] 
        [SerializeField] private SurvivorBulletManager BulletPool;

        [SerializeField] private float SpreadShotDuration = 5f;
        [SerializeField] private float SpreadShotAngle = 15f;
        [SerializeField] private float RapidShotDuration = 5f;
        [SerializeField] private float RapidShotMultiplier = 3f;
        [SerializeField] private float PierceShotDuration = 5f;

        [Header("Movement Variables")] [FormerlySerializedAs("ThrustSpeed")] [SerializeField]
        private float ThrustSpeed = 1f;

        //[FormerlySerializedAs("BrakingSpeed")] [SerializeField] private float _brakingSpeed = 1f;
        [FormerlySerializedAs("RotationSpeed")] [SerializeField]
        private float RotationSpeed = 1f;

        //[FormerlySerializedAs("ThrustWhenFacingAngleMin")] [SerializeField] private float _thrustWhenFacingAngleMin = 10f;
        [FormerlySerializedAs("BulletsPerSecond")] [SerializeField]
        private float BulletsPerSecond = 3f;

        /* No longer increasing firing speed over time
        [FormerlySerializedAs("BpsIncreasePerPowerUp")] [SerializeField]
        private float _bpsIncreasePerPowerUp = 1f;

        [FormerlySerializedAs("BpsDiminishingReturnMultiplier")] [SerializeField]
        private float _bpsDiminishingReturnMultiplier = 0.9f;
        */
        //[FormerlySerializedAs("ThrustDeadZoneSize")] [SerializeField] private float _thrustDeadZoneSize = 3f; // Radius around the hero that only rotates and doesn't thrust. A multiplier of the hero size

        [FormerlySerializedAs("Controller")] [Header("Controller")] [SerializeField]
        private SurvivorController Controller;

        [FormerlySerializedAs("ShieldRenderer")] [SerializeField]
        private SpriteRenderer ShieldRenderer;

        [SerializeField] private float InvulnerabilityTime = 3f;
        [SerializeField] private float FlickerRate = 0.1f;

        [SerializeField] private SurvivorLifeCounter LifeCounter;
        [SerializeField] private SurvivorBombPower BombPower;
        [SerializeField] private Collider2D PrimaryTargetingTrigger;
        [SerializeField] private Collider2D SecondaryTargetingTrigger;
        [SerializeField] private GameObject ConfigAsset;

        private float _bulletTimer;
        private Collider2D[] _facingContacts;
        private float _flickerTimer;
        private Collider2D _heroCollider;
        private TrailRenderer _heroTrail;
        private float _invulnerableTimer;
        private float _pierceShotTimer;
        private float _rapidShotTimer;
        private bool _setupDone;
        private bool _shooting; // start shooting after first input
        private SurvivorAdvancedTrailFx _spawnedTrailFx;
        private float _spreadShotTimer;
        private Rigidbody2D _target;
        private TrailLocator _trailParent;
        private List<SpriteRenderer> _flickerRenderers = new();

        public GameObject SpawnedArtAsset { get; private set; }

        private int HitPoints => LifeCounter.LivesRemaining;

        private void FixedUpdate()
        {
            Rigidbody.angularVelocity = 0f;

            // Invulnerability handler
            CheckInvulnerabilityTimer();

            if (_shooting == false) return;
            CheckBulletTimer();
            CheckSpreadShotTimer();
            CheckPierceShotTimer();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            // Don't lose hp during invulnerable time.
            // Only die to obstacles.
            if (collision.rigidbody == null
                || Controller.DoesRigidbodyBelongToShootable(collision.rigidbody) == false)
                return;

            Controller.DamageHazard(collision.rigidbody);

            if (_invulnerableTimer > 0f) return;

            if (ShieldRenderer.enabled)
            {
                ShieldRenderer.enabled = false;
                AudioClip sfx = Config.HeroShieldHitSound[Random.Range(0, Config.HeroShieldHitSound.Length)];
                AudioMgr.Instance.PlaySound(sfx);
            }
            else
            {
                LifeCounter.LivesRemaining--;
                
                AudioClip sfx = Config.HeroDamagedSound[Random.Range(0, Config.HeroDamagedSound.Length)];
                AudioMgr.Instance.PlaySound(sfx);
                if (HitPoints <= 0) Controller.OnPlayerLose();
            }

            // give invulnerability time
            _invulnerableTimer = InvulnerabilityTime;
        }

        public void SetLives(int maxLives)
        {
            LifeCounter.Initialize(maxLives);
        }

        /// <summary>
        ///     Create the configurable asset from the config data and cache component references
        /// </summary>
        /// <param name="gameConfigHeroConfig"></param>
        public void Setup(SurvivorHeroConfig gameConfigHeroConfig)
        {
            _setupDone = false;
            // Results array should be fixed size
            _facingContacts = new Collider2D[50];

            ShieldRenderer.enabled = false;
            SpawnedArtAsset = Instantiate(ConfigAsset, HeroConfigAssetParent);

            if (SpawnedArtAsset)
                _heroCollider = SpawnedArtAsset.GetComponentInChildren<Collider2D>(); // should only have one
            
            _flickerRenderers.AddRange(SpawnedArtAsset.GetComponentsInChildren<SpriteRenderer>().Where(r => r.enabled));

            _setupDone = true;
        }

        public async Task SetupTrail(SurvivorAdvancedTrailFx trailFxInfo)
        {
            var trailParent = SpawnedArtAsset.GetComponentInChildren<TrailLocator>();
            if (trailParent == null)
            {
                UnityEngine.Debug.LogError("Hero.SetupTrail failed to find TrailLocator in spawnedArtAsset");
                return;
            }

            _spawnedTrailFx = Instantiate(trailFxInfo, trailParent.transform);

            if (_spawnedTrailFx == false) UnityEngine.Debug.LogError($"Hero.AttachTrail failed to load");
        }

        /// <summary>
        ///     Validate the target state
        /// </summary>
        /// <returns> </returns>
        private void ValidateTarget()
        {
            var filter = new ContactFilter2D
            {
                useTriggers = true,
                useDepth = true,
                minDepth = 3f, // the z depth of the hazard layer
                maxDepth = 3f
            };

            // Check for hazards to target
            // Right now, this is a either, or scenario where it tries to find targets in the facing cone
            // if not it searches all valid hazards
            // Check for hazards in the cone where the player is facing
            var contacts = Physics2D.OverlapCollider(PrimaryTargetingTrigger, filter, _facingContacts);
            if (contacts > 0) _target = FindClosestHazardCollider(_facingContacts)?.attachedRigidbody;
            if (_target == null)
                // Find the closest enemy
                _target = FindClosestRigidbody(Controller.GetActiveHazardRigidbodies());
            Array.Clear(_facingContacts, 0, _facingContacts.Length);
        }

        private Collider2D FindClosestHazardCollider(Collider2D[] colliders)
        {
            var smallestDistance = float.MaxValue;
            Collider2D result = null;
            foreach (var col in colliders)
            {
                if (col == null ||
                    col.attachedRigidbody == null ||
                    Controller.DoesRigidbodyBelongToShootable(col.attachedRigidbody) == false) continue;

                var distance = Vector3.Distance(transform.position, col.transform.position);
                if (distance < smallestDistance)
                {
                    smallestDistance = distance;
                    result = col;
                }
            }

            return result;
        }

        private Rigidbody2D FindClosestRigidbody(Rigidbody2D[] rigidbodies)
        {
            var smallestDistance = float.MaxValue;
            Rigidbody2D result = null;
            foreach (var rb in rigidbodies)
            {
                if (rb == null) continue;
                var distance = Vector3.Distance(transform.position, rb.transform.position);
                if (distance < smallestDistance)
                {
                    smallestDistance = distance;
                    result = rb;
                }
            }

            return result;
        }

        /// <summary>
        ///     Process a directional input vector, usually from a joystick
        /// </summary>
        /// <param name="inputVector"> </param>
        public void ProcessInput(Vector3 inputVector)
        {
            if (_setupDone == false) return;

            if (inputVector.magnitude > 0f)
            {
                _shooting = true;
                var heroTransform = transform;
                var heroRightAxis = heroTransform.right;

                transform.right =
                    Vector3.Slerp(heroRightAxis, inputVector, Time.fixedDeltaTime * RotationSpeed);

                // Move to position
                Rigidbody.linearVelocity = inputVector * (ThrustSpeed / Time.fixedDeltaTime);
            }
            else
            {
                Rigidbody.linearVelocity = Vector2.zero;
            }
            // Dirty clamp. Causes jitter near the edges
            // TODO check the input vector, if it's near the edge, zero it in that dir. Clamp position as last resort
            Rigidbody.position = Controller.ClampPositionWithinLevelBounds(Rigidbody.position);
        }

        /// <summary>
        ///     Game is over
        /// </summary>
        public void PlayerLose()
        {
            gameObject.SetActive(false); // also stops the bullets firing more
        }

        /// <summary>
        ///     Shoot bullets from the hero. Apply any modifiers like spread shot
        /// </summary>
        private void ShootBullet()
        {
            var modifiers = GetBulletModifiers();
            var bulletOrigin = transform;
            var position = bulletOrigin.position;
            var aimingDirection = bulletOrigin.right;
            
            if (_target != null)
            {
                var v2Pos = new Vector2(position.x, position.y);
                var dir = _target.position - v2Pos;
                aimingDirection = new Vector3(dir.x, dir.y, bulletOrigin.right.z);
            }

            BulletPool.FireBullet(position, aimingDirection, modifiers, _target);
            _target = null;

            AudioClip sfx = Config.HeroProjectileFiredSound[Random.Range(0, Config.HeroProjectileFiredSound.Length)];
            AudioMgr.Instance.PlaySound(sfx);

            if (_spreadShotTimer > 0f)
            {
                
                var leftRadians = Mathf.Deg2Rad * -SpreadShotAngle;
                var leftSinTheta = Mathf.Sin(leftRadians);
                var leftCosTheta = Mathf.Cos(leftRadians);
                var leftSpread = new Vector3(
                    leftCosTheta * aimingDirection.x - leftSinTheta * aimingDirection.y,
                    leftSinTheta * aimingDirection.x + leftCosTheta * aimingDirection.y,
                    aimingDirection.z
                );
                
                var rightRadians = Mathf.Deg2Rad * SpreadShotAngle;
                var rightSinTheta = Mathf.Sin(rightRadians);
                var rightCosTheta = Mathf.Cos(rightRadians);
                var rightSpread = new Vector3(
                    rightCosTheta * aimingDirection.x - rightSinTheta * aimingDirection.y,
                    rightSinTheta * aimingDirection.x + rightCosTheta * aimingDirection.y,
                    aimingDirection.z
                );

                BulletPool.FireBullet(position, leftSpread, modifiers);
                
                BulletPool.FireBullet(position, rightSpread, modifiers);
            }
        }

        private SurvivorBulletManager.BulletModifiers GetBulletModifiers()
        {
            var modifiers = SurvivorBulletManager.BulletModifiers.None;
            if (_pierceShotTimer > 0f) modifiers = SurvivorBulletManager.BulletModifiers.Pierce;
            return modifiers;
        }


        #region timers

        private void CheckSpreadShotTimer()
        {
            // timer based. Could be changed to be shot count based
            if (_spreadShotTimer > 0f) _spreadShotTimer -= Time.fixedDeltaTime;
        }

        private void CheckPierceShotTimer()
        {
            // timer based. Could be changed to be shot count based
            if (_pierceShotTimer > 0f) _pierceShotTimer -= Time.fixedDeltaTime;
        }

        private void CheckBulletTimer()
        {
            // spawn bullet based on simple timer
            _bulletTimer += Time.fixedDeltaTime;
            var bps = BulletsPerSecond;
            if (_rapidShotTimer > 0f)
            {
                bps *= RapidShotMultiplier;
                _rapidShotTimer -= Time.fixedDeltaTime;
            }

            if (_bulletTimer > 1 / bps)
            {
                _bulletTimer = 0;
                ValidateTarget();
                ShootBullet();
            }
        }

        private void CheckInvulnerabilityTimer()
        {
            if (_invulnerableTimer > 0f)
            {
                _invulnerableTimer -= Time.fixedDeltaTime;
                if (_invulnerableTimer > 0f)
                {
                    _heroCollider.isTrigger = true;
                    _flickerTimer -= Time.fixedDeltaTime;
                    if (_flickerTimer <= 0)
                    {
                        _flickerRenderers.ForEach(sr => sr.enabled = !sr.enabled);
                        _flickerTimer = FlickerRate;
                    }
                }
                else
                {
                    _heroCollider.isTrigger = false;
                    _flickerRenderers.ForEach(sr => sr.enabled = true);
                }
            }
        }

        #endregion

        #region Power Ups

        /// <summary>
        ///     Try to activate the shield if not already
        /// </summary>
        public void ActivateShield()
        {
            if (ShieldRenderer.enabled) return;

            ShieldRenderer.enabled = true;
            AudioClip sfx = Config.HeroShieldActivateSound[Random.Range(0,Config.HeroShieldActivateSound.Length)];
            AudioMgr.Instance.PlaySound(sfx);
        }

        /// <summary>
        ///     Activate the rapid shot rate for a short time
        /// </summary>
        public void ActivateRapidShot()
        {
            _rapidShotTimer = RapidShotDuration;
        }

        /// <summary>
        ///     Activate the spread shot for a short time
        /// </summary>
        public void ActivateSpreadShot()
        {
            _spreadShotTimer = SpreadShotDuration;
        }

        public void ActivatePierceShot()
        {
            _pierceShotTimer = PierceShotDuration;
        }

        /// <summary>
        ///     Tell the counter to add a life.
        ///     It handles the animation and adding the life to the UI
        ///     Contains internal safeguard to not allow more life than max
        /// </summary>
        public void RegainHealth()
        {
            LifeCounter.LivesRemaining++;
        }

        public void ActivateBomb()
        {
            BombPower.ActivateBomb(_heroCollider);
        }

        #endregion
    }
}