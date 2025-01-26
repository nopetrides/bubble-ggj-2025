using System.Collections.Generic;
using System.Linq;
using P3T.Scripts.Managers;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;

namespace P3T.Scripts.Gameplay.Survivor
{
    public class SurvivorPowerUpManager : MonoBehaviour
    {
        [SerializeField] private List<StreakAssist.Item> RandomPowers;

        [SerializeField] private StreakAssist StreakAssist;

        [SerializeField] private Transform PowerUpParent;

        [SerializeField] private SurvivorPowerCollectedParticle CollectParticle;

        [SerializeField] private float AnyPickupSpawnChance = 50f;
        
        /// <summary>
        /// Out of 100, the chance of a power spawning on destroy
        /// </summary>
        [SerializeField] private float StartingPowerSpawnChance = 20f;

        [FormerlySerializedAs("_defensivePowerUps")] [SerializeField] private GameObject[] SpawnOncePowers;
        
        private SurvivorController _controller;
        private ObjectPool<SurvivorPowerCollectedParticle> _fxPool;
        private OffScreenIndicatorManager _offScreenIndicatorManager;
        private Dictionary<ShooterPickup.PowerUpType, ObjectPool<ShooterPickup>> _poolDictionary;

        private float _powerSpawnTimer;

        public void SetupStreakAssist(SurvivorController controller,
            OffScreenIndicatorManager offScreenIndicatorManager)
        {
            _controller = controller;
            _offScreenIndicatorManager = offScreenIndicatorManager;
            StreakAssist.SetUp(RandomPowers, 50, 20);
            _poolDictionary = new Dictionary<ShooterPickup.PowerUpType, ObjectPool<ShooterPickup>>();

            foreach (var pickup in RandomPowers)
            {
                var power = pickup.GameItem.GetComponent<ShooterPickup>();
                var pool = new ObjectPool<ShooterPickup>(
                    () =>
                    {
                        var pooledObject = Instantiate(power);
                        pooledObject.Setup(_controller);
                        pooledObject.SetParent(PowerUpParent)
                            .SetManager(this);

                        pooledObject.name = pickup.GameItem.name + _poolDictionary[power.Power]?.CountAll;
                        return pooledObject;
                    },
                    pooledObject => { pooledObject.gameObject.SetActive(true); },
                    pooledObject => { pooledObject.gameObject.SetActive(false); },
                    pooledObject => Destroy(pooledObject.gameObject)
                );

                _poolDictionary.Add(power.Power, pool);
            }

            _fxPool = new ObjectPool<SurvivorPowerCollectedParticle>(
                () =>
                {
                    var pooledObject = Instantiate(CollectParticle);
                    pooledObject.SetManager(this);
                    pooledObject.SetParent(PowerUpParent);

                    pooledObject.name = CollectParticle.name + _fxPool.CountAll;
                    return pooledObject;
                },
                pooledObject => { pooledObject.gameObject.SetActive(true); },
                pooledObject => { pooledObject.gameObject.SetActive(false); },
                pooledObject => Destroy(pooledObject.gameObject)
            );
        }

        /// <summary>
        /// Determine what power, if any, should appear when a hazard is destroyed
        /// </summary>
        /// <returns>Item with power data. Null if no power selected, so hazard will drop a points instead</returns>
        public (bool, StreakAssist.Item) GetNextPower(int totalSpawned)
        {
            // Is there any pickup (points or power)
            if (Random.value > 0.01f * (AnyPickupSpawnChance)) return (false, null);
            
            float powerSpawnChance = StartingPowerSpawnChance - (0.1f * totalSpawned);
            if (powerSpawnChance <= 0 || Random.value > 0.01f * (powerSpawnChance)) return (true, null);
            
            return (true, ValidateNextStreakItem());
        }

        /// <summary>
        ///     Only allows one defensive type power to drop per game
        /// </summary>
        /// <returns></returns>
        private StreakAssist.Item ValidateNextStreakItem()
        {
            var item = StreakAssist.GetItem();
            if (item != null)
            {
                if (SpawnOncePowers.Contains(item.GameItem)) StreakAssist.InvalidateItem(item.GameItem);
            }
            return item;
        }
        
        /// <summary>
        /// Create the instance of the predetermined power and spawn it into the world
        /// </summary>
        /// <param name="spawnPoint"></param>
        /// <param name="power"></param>
        public void SpawnPowerUp(Vector3 spawnPoint, StreakAssist.Item power)
        {
            // Get the game object connected to the streak item
            var shooterPickup = power.GameItem;
            // todo, replace with spacial sound
            if (shooterPickup.TryGetComponent(out ShooterPickup pickup))
                AudioMgr.Instance.PlaySound(pickup.AppearSound);
            else
                UnityEngine.Debug.LogError("ArcadeSurvivorPowerUpManager failed to instantiate ShooterPickup from StreakAssist");

            // Get an instance of it from the pool spawned into the world
            _poolDictionary[pickup.Power].Get(out var nextShootable);
            if (nextShootable == null) return;

            var pickupTransform = nextShootable.transform;

            pickupTransform.position = spawnPoint;
        }

        public void OnPowerUpProcessed(MonoTriggeredGamePower triggeredPowerUp)
        {
            if (triggeredPowerUp.TryGetComponent(out ShooterPickup pickup))
                AudioMgr.Instance.PlaySound(pickup.CollectedSound);

            var particle = _fxPool.Get();
            particle.SetPosition(triggeredPowerUp.transform.position);
            particle.Play();
        }

        public void Release(ShooterPickup pickup)
        {
            _poolDictionary[pickup.Power].Release(pickup);
        }

        public void Release(SurvivorPowerCollectedParticle particle)
        {
            _fxPool.Release(particle);
        }

        /// <summary>
        ///     Get an indicator for an offscreen hazard
        /// </summary>
        /// <param name="hazardTransform"> </param>
        /// <returns> </returns>
        public OffScreenIndicator IndicateOffscreen(Transform hazardTransform)
        {
            return _offScreenIndicatorManager.ShowPowerIndicator(hazardTransform);
        }

        /// <summary>
        ///     No Longer need the offscreen indicator
        /// </summary>
        /// <param name="indicator"> </param>
        public void NoLongerOffscreen(OffScreenIndicator indicator)
        {
            _offScreenIndicatorManager.HideIndicator(indicator);
        }
    }
}