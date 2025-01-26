using System.Collections;
using System.Collections.Generic;
using System.Linq;
using P3T.Scripts.Managers;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;

namespace P3T.Scripts.Gameplay.Survivor
{
    public class SurvivorHazardManager : MonoBehaviour
    {
        [SerializeField] private Transform HazardParent;

        [SerializeField] private SurvivorHazard SurvivorHazardPrefab;

        [FormerlySerializedAs("ObstacleSpawnInterval")] [SerializeField]
        private float ObstacleSpawnInterval = 1.5f;

        /// <summary>
        ///     The exponential rate at which hazards increase in spawn speed.
        ///     0.01 = 1%
        ///     Starting at 1 hazard every 1.5s with 1% multiplier:
        ///     after  40 hazards, spawn rate is 1 hazard every 1s
        ///     after  68 hazards, spawn rate is 1 hazard every 0.75s
        ///     after 109 hazards, spawn rate is 1 hazard every 0.5s
        ///     after 144 hazards, spawn rate is 1 hazard every 0.3s
        /// </summary>
        [FormerlySerializedAs("SpeedIncreaseMultiplier")] [SerializeField]
        private float SpawnIncreaseMultiplier = 0.01f;

        /// <summary>
        ///     The Hit Points of a freshly spawned hazard
        /// </summary>
        [SerializeField] private int HitsToDestroy = 2;

        [SerializeField] private float SpawnRateLimit = 0.02f; // no more than 1 per frame

        [SerializeField] private float ShootableStartSpeed = 2f;

        [SerializeField] private float SpeedIncreasePerFixedUpdate = 0.03f;

        [SerializeField] private int ShootableSpawnLimit = 50;

        [SerializeField] private SurvivorHazardDestroyParticle _defaultExplosionParticlePrefab;

        // A dictionary of all the active shootables and their colliders for quick lookup
        private readonly Dictionary<Rigidbody, SurvivorHazard> _activeShootablesLookup = new();
        private SurvivorController _controller;
        private float _currentSpeed;
        private SurvivorHazardDestroyParticle _explosionParticlePrefab;
        private ObjectPool<SurvivorHazardDestroyParticle> _fxPool;
        private float _obstacleSpawnTimer;
        private OffScreenIndicatorManager _offScreenIndicatorManager;
        private PlayerBounds _playerBounds;
        private ObjectPool<SurvivorHazard> _pool;

        private bool _setupDone;
        private int _spawnCount;
        private bool _useWaveSpawnSystem;

        private void ReadyPools()
        {
            _currentSpeed = ShootableStartSpeed;
            // See if this asset has a unique explosions particle attached
            _explosionParticlePrefab =
                SurvivorHazardPrefab.GetComponentInChildren<SurvivorHazardDestroyParticle>() ??
                _defaultExplosionParticlePrefab;

            _pool = new ObjectPool<SurvivorHazard>
            (
                () =>
                {
                    var pooledObject = Instantiate(SurvivorHazardPrefab)
                        .SetParent(HazardParent)
                        .SetManager(this)
                        .SetSpeedIncrease(SpeedIncreasePerFixedUpdate);
                    var count = _pool.CountAll;
                    pooledObject.name = SurvivorHazardPrefab.name + count;
                    return pooledObject;
                },
                pooledObject =>
                {
                    pooledObject.gameObject.SetActive(true);
                    //pooledObject.IgnoreCollider(_playerBounds.PlayerMovementBounds);

                    _activeShootablesLookup.Add(pooledObject.Rigidbody, pooledObject);
                },
                pooledObject =>
                {
                    pooledObject.gameObject.SetActive(false);

                    _activeShootablesLookup.Remove(pooledObject.Rigidbody);
                },
                pooledObject => Destroy(pooledObject.gameObject)
            );

            _fxPool = new ObjectPool<SurvivorHazardDestroyParticle>(
                () =>
                {
                    var pooledObject = Instantiate(_explosionParticlePrefab);
                    pooledObject.SetParent(HazardParent);
                    pooledObject.SetManager(this);

                    pooledObject.name = _explosionParticlePrefab.name + _fxPool.CountAll;
                    return pooledObject;
                },
                pooledObject => { pooledObject.gameObject.SetActive(true); },
                pooledObject => { pooledObject.gameObject.SetActive(false); },
                pooledObject => Destroy(pooledObject.gameObject)
            );
        }

        public bool IsRigidbodyActiveHazard(Rigidbody rb)
        {
            if (rb == null) return false;
            return _activeShootablesLookup.TryGetValue(rb, out var _);
        }

        public Rigidbody[] GetActiveHazardRigidbodies()
        {
            return _activeShootablesLookup.Keys.ToArray();
        }

        public void Setup(SurvivorController gameController, PlayerBounds playerBounds,
            OffScreenIndicatorManager offScreenIndicatorManager)
        {
            _controller = gameController;
            _playerBounds = playerBounds;
            _offScreenIndicatorManager = offScreenIndicatorManager;
        }

        /// <summary>
        ///     Update on a fixed time step, called by the minigame controller
        /// </summary>
        public void FixedUpdateHazardManager()
        {
            if (_useWaveSpawnSystem)
            {
                SpawnAsWavesFixedUpdate();
            }
            else
            {
                SpawnOverTimeFixedUpdate();
            }
        }

        // TODO TEMP location
        private readonly int _startingWaveSize = 4;
        private readonly int _maxWaveSize = 9;
        private int _perWave = 4;
        private int _waveAngle = 0; // TODO make start angle random
        private readonly int _angleIncrement = 45;
        private float _waveSpawnTimer;
        private void SpawnAsWavesFixedUpdate()
        {
            _waveSpawnTimer += Time.fixedDeltaTime;
            //var multiplier = 1f - _spawnIncreaseMultiplier * Time.fixedDeltaTime;
            //_obstacleSpawnInterval = Mathf.Max(_obstacleSpawnInterval * multiplier, _spawnRateLimit);
            
            if (_perWave != _startingWaveSize && !(_waveSpawnTimer > ObstacleSpawnInterval * _perWave)) return;
            
            _waveSpawnTimer = 0f;
            _controller.SpawnHazardWave(_perWave,_waveAngle);
            if (_perWave < _maxWaveSize) _perWave++;
            _waveAngle += _angleIncrement;
            if (_waveAngle >= 360) _waveAngle -= 360;
        }

        private void SpawnOverTimeFixedUpdate()
        {
            // // Increase the speed of the enemies over the game's lifetime
            _currentSpeed += SpeedIncreasePerFixedUpdate * Time.fixedDeltaTime;
            // Spawn shootable obstacles
            _obstacleSpawnTimer += Time.fixedDeltaTime;

            //var multiplier = 1f - _spawnIncreaseMultiplier * Time.fixedDeltaTime;
            //_obstacleSpawnInterval = Mathf.Max(_obstacleSpawnInterval * multiplier, _spawnRateLimit);

            if (!(_obstacleSpawnTimer > ObstacleSpawnInterval) ||
                _activeShootablesLookup.Count >= ShootableSpawnLimit) return;
            
            _obstacleSpawnTimer = 0f;
            _controller.SpawnHazard(); // ask the controller to give us the necessary data
            _spawnCount++;
            UnityEngine.Debug.Log($"Spawned # {_spawnCount}, interval {ObstacleSpawnInterval}");
        }

        /// <summary>
        ///     Create a shootable object and rotate it to face the moving direction
        /// </summary>
        /// <param name="gameCamera"> Camera for calculating bounds </param>
        /// <param name="spawnsPickup"> Will the enemy drop anything? </param>
        /// <param name="powerToSpawnOnDestroy"> Will the thing they drop be a power, else its points </param>
        public void SpawnHazard(Camera gameCamera, bool spawnsPickup,
            StreakAssist.Item powerToSpawnOnDestroy = null)
        {
            if (_setupDone == false) return;

            var nextHazard = _pool.Get();

            var hazardTransform = nextHazard.transform;

            // Spawn them off the screen edges and launch them towards the player
            var edge = Random.Range(0, 4); // non-inclusive 4
            Vector3 offscreenSpawn = new Vector3(Random.value, Random.value, gameCamera.transform.position.y);
            var offset = Vector3.zero;
            switch (edge)
            {
                case (int)WrapDirection.Left:
                    offscreenSpawn.x = 0;
                    offset.x = -_playerBounds.SpawnOffset;
                    break;
                case (int)WrapDirection.Right:
                    offscreenSpawn.x = 1;
                    offset.x = _playerBounds.SpawnOffset;
                    break;
                case (int)WrapDirection.Top:
                    offscreenSpawn.y = 1;
                    offset.z = _playerBounds.SpawnOffset;
                    break;
                case (int)WrapDirection.Bottom:
                    offscreenSpawn.y = 0;
                    offset.z = -_playerBounds.SpawnOffset;
                    break;
            }

            // viewport always takes normalize 0-1 for x and y. the z coord is distance from camera.
            var spawnPoint = gameCamera.ViewportToWorldPoint(offscreenSpawn);
            spawnPoint = new Vector3(spawnPoint.x, 0, spawnPoint.z);
            spawnPoint += offset;

            hazardTransform.position = spawnPoint;
            nextHazard.SetSpeed(_currentSpeed)
                .SetPowerToSpawnOnDestroy(spawnsPickup, powerToSpawnOnDestroy)
                .SetHitPoints(HitsToDestroy)
                .Spawn();
        }

        public void SpawnWaveBasedHazard(int waveAngle, int hazardInWaveIndex, Camera gameCamera, bool spawnsPickup,
            StreakAssist.Item powerToSpawnOnDestroy = null)
        {
            if (_setupDone == false) return;
            var bottomLeft = gameCamera.ViewportToWorldPoint(new Vector3(0,0,gameCamera.transform.position.y));
            var topLeft = gameCamera.ViewportToWorldPoint(new Vector3(1,1,gameCamera.transform.position.y));
            var diameter = Vector3.Distance(topLeft, bottomLeft);
            
            var flip = (hazardInWaveIndex % 2) == 0;
            var separationAngle = 10f;
            var angle = waveAngle + separationAngle * (flip ? 1f : -1f) * hazardInWaveIndex;
            
            Vector3 offset = Quaternion.Euler(0, angle, 0) * Vector3.forward * (diameter / 2f);
            offset += _controller.HeroPosition;

            Vector3 spawnPoint = offset;// new Vector3(offset.x, offset.y, HazardParent.position.z);
            StartCoroutine(WarnIncomingSpawn(spawnPoint, spawnsPickup, powerToSpawnOnDestroy));
        }

        private IEnumerator WarnIncomingSpawn(Vector3 spawnPoint, bool spawnsPickup, StreakAssist.Item powerToSpawnOnDestroy)
        {
            // Show incoming hazard spawn warning
            var indicator = _offScreenIndicatorManager.ShowIncomingHazardIndicator(spawnPoint);
            
            indicator.gameObject.SetActive(true);
            yield return new WaitForSeconds(0.2f);
            indicator.gameObject.SetActive(false);
            yield return new WaitForSeconds(0.2f);
            indicator.gameObject.SetActive(true);
            yield return new WaitForSeconds(0.2f);
            _offScreenIndicatorManager.HideIndicator(indicator);
            yield return new WaitForSeconds(0.2f);
            
            var nextHazard = _pool.Get();
            var hazardTransform = nextHazard.transform;
            hazardTransform.position = spawnPoint;
            
            // Spawn hazard
            nextHazard.SetSpeed(_currentSpeed)
                .SetPowerToSpawnOnDestroy(spawnsPickup, powerToSpawnOnDestroy)
                .SetHitPoints(HitsToDestroy)
                .Spawn();
        }

        /// <summary>
        ///     Damage hazard and release them back to the pool
        ///     Do this after the collision event so we can handle it in a specific order
        /// </summary>
        /// <param name="hazardRigidbody"> </param>
        /// <param name="damagingCollider"> </param>
        public void DamageHazard(Rigidbody hazardRigidbody,
            Collider damagingCollider = null)
        {
            var arcadeSurvivorHazard = _activeShootablesLookup[hazardRigidbody];
            var destroyed = arcadeSurvivorHazard.DealtDamage(damagingCollider);
            if (destroyed)
            {
                var spawnsPickup = arcadeSurvivorHazard.DoesSpawnPickupOnDestroy;
                var powerToSpawnOnDestroy = arcadeSurvivorHazard.PowerToSpawnOnDestroy;
                _controller.OnShootableDestroyed(arcadeSurvivorHazard.transform.position, spawnsPickup,
                    powerToSpawnOnDestroy);
                PlayLargeDestroySound();
                Release(arcadeSurvivorHazard);
            }
        }

        /// <summary>
        ///     Destroy hazard and don't let it drop anything
        /// </summary>
        /// <param name="hazardRigidbody"></param>
        public void EliminateHazard(Rigidbody hazardRigidbody)
        {
            var arcadeSurvivorHazard = _activeShootablesLookup[hazardRigidbody];
            arcadeSurvivorHazard.Eliminate();
            
            _controller.OnShootableDestroyed(arcadeSurvivorHazard.transform.position, false);
            PlayLargeDestroySound();
            Release(arcadeSurvivorHazard);
        }

        /// <summary>
        ///     Send this hazard back to the pool
        /// </summary>
        /// <param name="arcadeSurvivorHazard"> </param>
        private void Release(SurvivorHazard arcadeSurvivorHazard)
        {
            var particle = _fxPool.Get();
            particle.SetPosition(arcadeSurvivorHazard.transform.position);
            particle.Play();
            if (arcadeSurvivorHazard.isActiveAndEnabled)
            {
                arcadeSurvivorHazard.gameObject.SetActive(false);
                // Don't return the object to the pool immediately
                StartCoroutine(DelayedRelease(arcadeSurvivorHazard));
            }
        }
        
        private IEnumerator DelayedRelease(SurvivorHazard arcadeSurvivorHazard)
        {
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            _pool.Release(arcadeSurvivorHazard);
        }

        /// <summary>
        ///     Send this particle back to the pool
        /// </summary>
        /// <param name="particle"> </param>
        public void Release(SurvivorHazardDestroyParticle particle)
        {
            _fxPool.Release(particle);
        }

        public void SetUseWaveSpawnSystem(bool useWaveSpawnSystem)
        {
            _useWaveSpawnSystem = useWaveSpawnSystem;
        }

        /// <summary>
        ///     Spawn the configurable asset
        /// </summary>
        /// <param name="configurableAsset"> </param>
        public void SetConfigurableAsset(SurvivorHazardConfig configurableAsset)
        {
            SurvivorHazardPrefab.SetManager(this);
            SurvivorHazardPrefab.Setup(configurableAsset);
            ReadyPools();
            _setupDone = true;
        }

        /// <summary>
        ///     Play a sound clip on behalf of a shootable object.
        ///     As of right now we only have one pool based off a prefab, so we only need to worry about the sound on the prefab
        /// </summary>
        private void PlayLargeDestroySound()
        {
            var sound = SurvivorHazardPrefab.Config.HazardDamagedSounds[Random.Range(0, SurvivorHazardPrefab.Config.HazardDamagedSounds.Length)];
            AudioMgr.Instance.PlaySound(sound);
        }

        /// <summary>
        ///     Get an indicator for an offscreen hazard
        /// </summary>
        /// <param name="shootableTransform"> </param>
        /// <returns> </returns>
        public OffScreenIndicator IndicateOffscreen(Transform shootableTransform)
        {
            return _offScreenIndicatorManager.ShowHazardIndicator(shootableTransform);
        }

        /// <summary>
        ///     No Longer need the offscreen indicator
        /// </summary>
        /// <param name="indicator"> </param>
        public void NoLongerOffscreen(OffScreenIndicator indicator)
        {
            _offScreenIndicatorManager.HideIndicator(indicator);
        }

        public bool IsCoreGameLoopRunning()
        {
            return _controller.CoreGameLoopRunning;
        }

        public Vector2 GetPlayerPosition()
        {
            return _controller.HeroPosition;
        }
    }
}