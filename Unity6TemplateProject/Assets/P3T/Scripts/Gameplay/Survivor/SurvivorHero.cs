﻿using System;
using System.Collections.Generic;
using System.Linq;
using P3T.Scripts.Gameplay.Survivor.Drivers;
using P3T.Scripts.Gameplay.Survivor.ScriptableObjects;
using P3T.Scripts.Managers;
using UnityEngine;
using Random = UnityEngine.Random;

namespace P3T.Scripts.Gameplay.Survivor
{
    public class SurvivorHero : MonoBehaviour
    {
        [SerializeField] private SurvivorHeroConfig Config;
        
        [Header("Components")] 
        [SerializeField] private Transform HeroConfigAssetParent;

        [SerializeField] private Rigidbody Rigidbody;

        [Header("Bullets")] 
        [SerializeField] private SurvivorBulletManager BulletPool;

        [SerializeField] private float SpreadShotDuration = 5f;
        [SerializeField] private float SpreadShotAngle = 15f;
        [SerializeField] private float RapidShotDuration = 5f;
        [SerializeField] private float RapidShotMultiplier = 3f;
        [SerializeField] private float PierceShotDuration = 5f;

        [Header("Movement Variables")] 
        [SerializeField] private float ThrustSpeed = 1f;

        [SerializeField] private float RotationSpeed = 1f;

        [SerializeField] private float BulletsPerSecond = 3f;

        /* No longer increasing firing speed over time
        [SerializeField] private float _bpsIncreasePerPowerUp = 1f;

        [SerializeField] private float _bpsDiminishingReturnMultiplier = 0.9f;
        */
        //[SerializeField] private float _thrustDeadZoneSize = 3f; // Radius around the hero that only rotates and doesn't thrust. A multiplier of the hero size

        [Header("Controller")] 
        [SerializeField] private SurvivorController Controller;

        [SerializeField] private Renderer ShieldRenderer;

        [SerializeField] private float InvulnerabilityTime = 3f;
        [SerializeField] private float FlickerRate = 0.1f;

        [SerializeField] private SurvivorLifeCounter LifeCounter;
        [SerializeField] private SurvivorBombPower BombPower;
        [SerializeField] private Collider PrimaryTargetingTrigger;
        [SerializeField] private Transform ProjectileLocator;

        private float _bulletTimer;
        private Collider[] _facingContacts;
        private float _flickerTimer;
        private Collider _heroCollider;
        private TrailRenderer _heroTrail;
        private float _invulnerableTimer;
        private float _pierceShotTimer;
        private float _rapidShotTimer;
        private bool _setupDone;
        private bool _shooting; // start shooting after first input
        private float _spreadShotTimer;
        private Rigidbody _target;
        private TrailLocator _trailParent;
        private SurvivorAnimatedAsset _spawnedAsset;
        private SurvivorAdvancedTrailFx _spawnedTrailFx;
        private List<Renderer> _flickerRenderers = new();

        private int HitPoints => LifeCounter.LivesRemaining;

        private void FixedUpdate()
        {
            Rigidbody.angularVelocity = Vector3.zero;

            // Invulnerability handler
            CheckInvulnerabilityTimer();

            if (_shooting == false) return;
            CheckBulletTimer();
            CheckSpreadShotTimer();
            CheckPierceShotTimer();
        }

        private void OnCollisionEnter(Collision collision)
        {
            // Don't lose hp during invulnerable time.
            // Only die to obstacles.
            if (collision.rigidbody == null
                || Controller.DoesRigidbodyBelongToHazard(collision.rigidbody) == false)
                return;

            Controller.DamageHazard(collision.rigidbody);

            if (_invulnerableTimer > 0f) return;

            if (ShieldRenderer.enabled)
            {
                ShieldRenderer.enabled = false;
                if (Config != null && Config.HeroShieldHitSound != null && Config.HeroShieldHitSound.Length > 0)
                {
                    AudioClip sfx = Config.HeroShieldHitSound[Random.Range(0, Config.HeroShieldHitSound.Length)];
                    AudioMgr.Instance.PlaySound(sfx);
                }
            }
            else
            {
                LifeCounter.LivesRemaining--;
                if (Config != null && Config.HeroDamagedSound != null && Config.HeroDamagedSound.Length > 0)
                {
                    AudioClip sfx = Config.HeroDamagedSound[Random.Range(0, Config.HeroDamagedSound.Length)];
                    AudioMgr.Instance.PlaySound(sfx);
                }

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
        ///     todo external setup
        /// </summary>
        /// <param name="gameConfigHeroConfig"></param>
        public void Setup(SurvivorHeroConfig gameConfigHeroConfig)
        {
            _spawnedAsset = Instantiate(Config.SurvivorPrefab, HeroConfigAssetParent);
            _setupDone = false;
            // Results array should be fixed size
            _facingContacts = new Collider[50];

            ShieldRenderer.enabled = false;
            //SpawnedArtAsset = Instantiate(Config.art, HeroConfigAssetParent);

            if (_spawnedAsset)
            {
                _heroCollider = _spawnedAsset.GetComponentInChildren<Collider>(); // should only have one
                _flickerRenderers.AddRange(_spawnedAsset.GetComponentsInChildren<Renderer>().Where(r => r.enabled));
            }
            else
            {
                _heroCollider = HeroConfigAssetParent.GetComponentInChildren<Collider>();
            }

            _setupDone = true;
        }

        /// <summary>
        ///     Sets the particle/line renderer trail effect for the character
        ///     todo external setup
        /// </summary>
        /// <param name="trailFxInfo"></param>
        public void SetupTrail(SurvivorAdvancedTrailFx trailFxInfo)
        {

            if (_spawnedAsset == null)
                return;

            var trailParent = _spawnedAsset.TrailParent;
            if (trailParent == null)
            {
                UnityEngine.Debug.LogError("Hero.SetupTrail failed to find TrailLocator in spawnedArtAsset");
                return;
            }
            
            _spawnedTrailFx = Instantiate(Config.TrailFx, trailParent);
            
            if (_spawnedTrailFx == null) UnityEngine.Debug.LogError($"Hero.AttachTrail failed to load");
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
            //var contacts = Physics.OverlapSphere(PrimaryTargetingTrigger, filter, _facingContacts);
            //if (contacts > 0) _target = FindClosestHazardCollider(_facingContacts)?.attachedRigidbody;
            if (_target == null)
                // Find the closest enemy
                _target = FindClosestRigidbody(Controller.GetActiveHazardRigidbodies());
            Array.Clear(_facingContacts, 0, _facingContacts.Length);
        }

        private Collider FindClosestHazardCollider(Collider[] colliders)
        {
            var smallestDistance = float.MaxValue;
            Collider result = null;
            foreach (var col in colliders)
            {
                if (col == null ||
                    col.attachedRigidbody == null ||
                    Controller.DoesRigidbodyBelongToHazard(col.attachedRigidbody) == false) continue;

                var distance = Vector3.Distance(transform.position, col.transform.position);
                if (distance < smallestDistance)
                {
                    smallestDistance = distance;
                    result = col;
                }
            }

            return result;
        }

        private Rigidbody FindClosestRigidbody(Rigidbody[] rigidbodies)
        {
            var smallestDistance = float.MaxValue;
            Rigidbody result = null;
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

                Vector3 xyPlane = new Vector3(inputVector.x, 0, inputVector.y);
                transform.forward =
                    Vector3.Slerp(heroTransform.forward, xyPlane, Time.fixedDeltaTime * RotationSpeed);

                // Move to position
                Rigidbody.linearVelocity = xyPlane * (ThrustSpeed * Time.fixedDeltaTime);
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
            var bulletOrigin = ProjectileLocator;
            var position = bulletOrigin.position;
            var aimingDirection = bulletOrigin.forward;
            
            if (_target != null)
            {
                var dir = _target.position - position;
                aimingDirection = dir;  //new Vector3(dir.x, dir.y, bulletOrigin.forward.z);
            }

            BulletPool.FireBullet(position, aimingDirection, modifiers, _target);
            _target = null;

            if (Config.HeroProjectileFiredSound is { Length: > 0 })
            {
                AudioClip sfx = Config.HeroProjectileFiredSound[Random.Range(0, Config.HeroProjectileFiredSound.Length)];
                AudioMgr.Instance.PlaySound(sfx);
            }

            if (_spreadShotTimer > 0f)
            {
                
                var leftRadians = Mathf.Deg2Rad * -SpreadShotAngle;
                var leftSinTheta = Mathf.Sin(leftRadians);
                var leftCosTheta = Mathf.Cos(leftRadians);
                var leftSpread = new Vector3(
                    leftCosTheta * aimingDirection.x - leftSinTheta * aimingDirection.z,
                    aimingDirection.y,
                    leftSinTheta * aimingDirection.x + leftCosTheta * aimingDirection.z
                );
                
                var rightRadians = Mathf.Deg2Rad * SpreadShotAngle;
                var rightSinTheta = Mathf.Sin(rightRadians);
                var rightCosTheta = Mathf.Cos(rightRadians);
                var rightSpread = new Vector3(
                    rightCosTheta * aimingDirection.x - rightSinTheta * aimingDirection.z,
                    aimingDirection.y,
                    rightSinTheta * aimingDirection.x + rightCosTheta * aimingDirection.z
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
            if (Config != null && Config.HeroShieldActivateSound != null && Config.HeroShieldActivateSound.Length > 0)
            {
                AudioClip sfx = Config.HeroShieldActivateSound[Random.Range(0, Config.HeroShieldActivateSound.Length)];
                AudioMgr.Instance.PlaySound(sfx);
            }
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