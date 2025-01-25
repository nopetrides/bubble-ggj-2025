using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace P3T.Scripts.Gameplay.Survivor
{
    /// <summary>
    ///     A unique config for a specific hero
    /// </summary>
    [Serializable]
    public class SurvivorHazardConfig : MonoBehaviour
    {
        #region Audio

        public AudioClip[] HazardDamagedSounds;

        #endregion
    }

    /// <summary>
    ///     The class representing a shootable hazard in the ArcadeSurvivor minigame
    ///     These hazards Can split into smaller versions of themselves
    ///     Pooling and spawning logic is handled in the <see cref="SurvivorHazardManager" />
    /// </summary>
    public class SurvivorHazard : MonoBehaviour
    {
        public SurvivorHazardConfig Config;
        
        public Rigidbody2D Rigidbody;
        
        [SerializeField] private Transform HazardConfigAssetParent;

        [HideInInspector] [SerializeField]
        private SurvivorHazardManager Manager;

        [HideInInspector] [SerializeField]
        private TrailRenderer Trail;

        private float _currentSpeed;
        private int _hitPoints;
        private OffScreenIndicator _indicator;

        private float _speedOverLifetimeIncrease;
        private float _startingSpeed;
        public StreakAssist.Item PowerToSpawnOnDestroy { get; private set; }

        public bool DoesSpawnPickupOnDestroy { get; private set; }

        private void FixedUpdate()
        {
            MoveToPlayer();
            //CheckIndicator();
        }

        /// <summary>
        ///     Sets <inheritdoc cref="Manager" />
        /// </summary>
        /// <param name="manager"> </param>
        /// <returns> </returns>
        public SurvivorHazard SetManager(SurvivorHazardManager manager)
        {
            Manager = manager;
            return this;
        }

        /// <summary>
        ///     Sets the parent of the game object so it will be on the right canvas
        /// </summary>
        /// <param name="parentTransform"> </param>
        /// <returns> </returns>
        /// <remarks> Most implementations will not need this set </remarks>
        public SurvivorHazard SetParent(Transform parentTransform)
        {
            transform.SetParent(parentTransform);
            return this;
        }

        /// <summary>
        ///     Sets the current speed to the starting speed times the speed modifier
        /// </summary>
        /// <param name="speed"> </param>
        /// <returns> </returns>
        public SurvivorHazard SetSpeed(float speed)
        {
            _startingSpeed = speed;
            _currentSpeed = _startingSpeed;
            return this;
        }

        /// <summary>
        ///     How much the speed increases over time. Matches the speed of newly spawned hazards.
        /// </summary>
        /// <param name="speedIncrease"> </param>
        /// <returns> </returns>
        public SurvivorHazard SetSpeedIncrease(float speedIncrease)
        {
            _speedOverLifetimeIncrease = speedIncrease;
            return this;
        }

        public SurvivorHazard SetPowerToSpawnOnDestroy(bool spawnsPickup, StreakAssist.Item power)
        {
            // if true, and power is null, then spawn points
            DoesSpawnPickupOnDestroy = spawnsPickup;
            PowerToSpawnOnDestroy = power;
            return this;
        }

        /// <summary>
        ///     How many times the hazard can be hit before it is destroyed
        /// </summary>
        /// <param name="hp"> </param>
        /// <returns> </returns>
        public SurvivorHazard SetHitPoints(int hp)
        {
            _hitPoints = hp;
            return this;
        }

        /// <summary>
        ///     Fire off the bullet in it's facing direction
        /// </summary>
        /// <returns> </returns>
        public SurvivorHazard Spawn()
        {
            MoveToPlayer();
            //_rigidbody.AddForce(transform.right * _currentImpulse, ForceMode2D.Impulse);
            return this;
        }

        /// <summary>
        ///     Ignore a specific collider
        /// </summary>
        /// <param name="colliderToIgnore"> </param>
        /// <returns> </returns>
        public SurvivorHazard IgnoreCollider(Collider2D colliderToIgnore)
        {
            var colliders = new List<Collider2D>();
            Rigidbody.GetAttachedColliders(colliders);
            foreach (var col in colliders) Physics2D.IgnoreCollision(colliderToIgnore, col);
            return this;
        }

        /// <summary>
        ///     Instead of a separate OnCollision event for this class, let the manager handle it
        ///     We need to be able handle it in a specific order
        /// </summary>
        /// <param name="colliderToIgnore"> </param>
        /// <returns> If the object is destroyed </returns>
        public bool DealtDamage(Collider2D colliderToIgnore = null)
        {
            // Ignore bullet that just damaged us
            if (colliderToIgnore != null) IgnoreCollider(colliderToIgnore);
            _hitPoints--;
            if (_hitPoints > 0) return false;

            if (Trail != null) Trail.Clear();
            
            //ClearIndicator();

            return true;
        }

        public void Eliminate()
        {
            _hitPoints = 0;
            if (Trail != null) Trail.Clear();
            //ClearIndicator();
        }

        [Obsolete]
        private void ClearIndicator()
        {
            if (_indicator != null)
            {
                Manager.NoLongerOffscreen(_indicator);
                _indicator = null;
            }
        }

        /// <summary>
        ///     Move towards the player's position
        /// </summary>
        private void MoveToPlayer()
        {
            if (Manager.IsCoreGameLoopRunning())
            {
                _currentSpeed += _speedOverLifetimeIncrease * Time.fixedDeltaTime;
                Vector3 targetPosition = Manager.GetPlayerPosition();
                targetPosition.z = transform.position.z;
                // Rotate towards the player's position
                var moveVector = targetPosition - transform.position;
                transform.right = moveVector;
                // Move towards the player's position
                moveVector.Normalize();
                Rigidbody.linearVelocity = moveVector * (_currentSpeed / Time.fixedDeltaTime);
            }
            else
            {
                Rigidbody.linearVelocity = Vector2.zero;
            }
        }

        public void Setup(SurvivorHazardConfig configurableAsset)
        {
            // ProcessConfig
        }
    }
}