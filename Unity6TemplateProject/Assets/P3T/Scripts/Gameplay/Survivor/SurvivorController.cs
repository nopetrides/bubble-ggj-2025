using System;
using System.Collections;
using DG.Tweening;
using P3T.Scripts.Gameplay.Survivor.Pooled;
using P3T.Scripts.Managers;
using UnityEngine;

namespace P3T.Scripts.Gameplay.Survivor
{
    /// <summary>
    ///     Prototype for the arcade top down shooter
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public class SurvivorController : MonoBehaviour, IProcessPowerUp
    {
        public struct PointsTracker
        {
            public int SurvivalPoints;
            public int HazardPoints;
            public int PickupPoints;
            public int PickupPowers;
        }

        [SerializeField] private SurvivorConfig GameConfig;
        
        /// <summary>
        /// A timer object used during gameplay to signify the duration of <b>total</b> game time
        /// </summary>
        [SerializeField] private StopwatchComponent GameTimer;
        [SerializeField] private AnimatedCounter ScoreCounter;
        [SerializeField] private SurvivorPointsDisplayManager SurvivorPointsManager;
        
        /// <summary>
        ///     How big each object (hero, hazards, etc) should be
        /// </summary>
        [SerializeField] private int BaseSpriteSize = 64;

        // Exposed values for testing
        [SerializeField] private float TimeScalingMultiplier = 1f;

        [Header("Hero")] 
        [SerializeField] private SurvivorHero SurvivorHero;
        [SerializeField] private SurvivorBulletManager BulletManager;

        [Header("Obstacles")] 
        [SerializeField] private SurvivorHazardManager HazardManager;

        [Header("Camera")]
        [SerializeField] private Camera GameCamera;
        [SerializeField] private VirtualJoystick Joystick;

        [Header("Bounds")] 
        [SerializeField] private PlayerBounds PlayerBounds;
        [SerializeField] private OffScreenIndicatorManager OffscreenIndicatorManager;

        [Header("Pickups/Power Ups")] 
        [SerializeField] private SurvivorPowerUpManager PowerUpManager;
        [SerializeField] private SurvivorPointsPickupManager PointsPickupManager;
        [SerializeField] private SurvivorSummaryWidget LevelSummaryWidget;
        [SerializeField] private SurvivorPointsVfx PointBubbleTemplate;

        /// <summary>
        /// Backing field for the <inheritdoc cref="ScoreEarned"/>
        /// </summary>
        private int _trueScore;
        public int ScoreEarned 
        {
            get => _trueScore;
            protected set
            {
                _trueScore = value;
            }
        }
        
        private Coroutine _mainRunRoutine;
        private bool _playing;
        private bool _waitForInit;
        private bool _gamePaused;
        private bool _endSequence;
        private float _pointsTimer;
        private bool _introComplete;
        private bool _waitForPreload;
        private bool _gameIsCompleted;
        private int _totalHazardsSpawned;
        private PointsTracker _pointsTracker;

        public float TimeScaling => TimeScalingMultiplier;
        public bool CoreGameLoopRunning => _playing && _introComplete && !_gameIsCompleted && !_endSequence;
        public Vector3 HeroPosition => SurvivorHero.transform.position;

        
        
        private void Awake()
        {
            Init();
        }

        /// <summary>
        ///     Game logic on Fixed Update, especially physics related
        ///     Handle input
        ///     Update hazard manager
        ///     Track survival time
        /// </summary>
        private void FixedUpdate()
        {
            if (CoreGameLoopRunning == false) return;

            SurvivorHero.ProcessInput(Joystick.InputVector);

            HazardManager.FixedUpdateHazardManager();

            TrackSurvivalTime();
        }

        /// <summary>
        ///     Trigger a picked up power and award points
        /// </summary>
        /// <param name="triggeredPowerUp"> </param>
        /// <exception cref="ArgumentOutOfRangeException"> </exception>
        public void ProcessPowerUp(MonoTriggeredGamePower triggeredPowerUp)
        {
            var powerUp = triggeredPowerUp as SurvivorPowerUp;
            if (powerUp == null)
            {
                UnityEngine.Debug.LogError("MG_ArcadeSurvivor ProcessPowerUp expected a SurvivorPowerUp type");
                return;
            }

            switch (powerUp.SurvivorPower)
            {
                case SurvivorPowerUpType.FireRateUp:
                    SurvivorHero.ActivateRapidShot();
                    break;
                case SurvivorPowerUpType.Shield:
                    SurvivorHero.ActivateShield();
                    break;
                case SurvivorPowerUpType.SpreadShot:
                    SurvivorHero.ActivateSpreadShot();
                    break;
                case SurvivorPowerUpType.PierceShot:
                    SurvivorHero.ActivatePierceShot();
                    break;
                case SurvivorPowerUpType.RegainHealth:
                    SurvivorHero.RegainHealth();
                    break;
                case SurvivorPowerUpType.Bomb:
                    SurvivorHero.ActivateBomb();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(powerUp), powerUp,
                        "MG_ArcadeSurvivor ProcessPowerUp was given a bad PowerUpType");
            }

            var powerUpPointsValue =
                Mathf.RoundToInt(GameConfig.PointsPickupValue * GameConfig.PowerUpPickupPointsMultiplier);
            _pointsTracker.PickupPowers += powerUpPointsValue;
            UpdatePoints(powerUpPointsValue, powerUp.transform.position);
            PowerUpManager.OnPowerUpProcessed(triggeredPowerUp);
        }

        /// <summary>
        ///     Track the elapsed time the player has been alive
        /// </summary>
        private void TrackSurvivalTime()
        {
            _pointsTimer += Time.fixedDeltaTime;
            if (_pointsTimer < GameConfig.PointsUpdateInterval) return;
            _pointsTimer = 0;
            var pointsEarned = Mathf.RoundToInt(GameConfig.PointsPerSecond*GameConfig.PointsUpdateInterval);
            _pointsTracker.SurvivalPoints += pointsEarned;
            ScoreEarned += pointsEarned;
            ScoreCounter.Animate(pointsEarned);
        }

        /// <summary>
        ///     Handle the player being defeated
        /// </summary>
        public void OnPlayerLose()
        {
            // Player is dead 
            _endSequence = true;
            //AddSurvivalPoints();
            StartCoroutine(GameEndDelay());
        }

        /// <summary>
        ///     Delay to allow game over vfx to play
        /// </summary>
        /// <returns> </returns>
        private IEnumerator GameEndDelay()
        {
            SurvivorHero.PlayerLose();
            yield return new WaitForSeconds(1f);
            ShowScoreSummary();
        }

        /// <summary>
        ///     Call back to the manager with necessary shared info
        /// </summary>
        public void SpawnHazard()
        {
            _totalHazardsSpawned++;
            var (spawnOnDestroy, nextPower) = PowerUpManager.GetNextPower(_totalHazardsSpawned);
            HazardManager.SpawnHazard(GameCamera, spawnOnDestroy, nextPower);
        }

        public void SpawnHazardWave(int hazardsInWave, int waveAngle)
        {
            for (int i = 0; i < hazardsInWave; i++)
            {
                _totalHazardsSpawned++;
                var (spawnOnDestroy, nextPower) = PowerUpManager.GetNextPower(_totalHazardsSpawned);
            
                HazardManager.SpawnWaveBasedHazard(waveAngle, i, GameCamera, spawnOnDestroy, nextPower);
            }
        }

        public void OnHazardDestroyed(Vector3 position, bool spawnsPickupOnDestroy,
            StreakAssist.Item streakItem = null)
        {
            position = ClampPositionWithinLevelBounds(position);
            AddHazardPoints(GameConfig.PointsOnHazardDestroy, position);

            if (spawnsPickupOnDestroy == false) return;
            // spawn the power or points that the shootable drops
            if (streakItem != null)
                // if this hazard has predetermined power up, spawn it
                PowerUpManager.SpawnPowerUp(position, streakItem);
            else
                // otherwise just spawn a points pickup
                PointsPickupManager.SpawnPointsPickup(position);
        }

        public Vector3 ClampPositionWithinLevelBounds(Vector3 position)
        {
            var bounds = PlayerBounds.PlayerMovementBounds.bounds;
            var minX = -1 * bounds.extents.x + BaseSpriteSize;
            var maxX = bounds.extents.x - BaseSpriteSize;
            var minY = -1 * bounds.extents.y + BaseSpriteSize;
            var maxY = bounds.extents.y - BaseSpriteSize;
            var x = Mathf.Clamp(position.x, minX, maxX);
            var z = Mathf.Clamp(position.z, minY, maxY);
            position.x = x;
            position.y = 0;
            position.z = z;
            return position;
        }

        private void ShowScoreSummary()
        {
            LevelSummaryWidget.gameObject.SetActive(true);
            LevelSummaryWidget.Setup(_pointsTracker, ScoreEarned,
                () => _gameIsCompleted = true);
        }

        public bool DoesRigidbodyBelongToHazard(Rigidbody rb)
        {
            return HazardManager.IsRigidbodyActiveHazard(rb);
        }

        /// <summary>
        ///     Something  has damaged a hazard object
        /// </summary>
        /// <param name="hazardRigidbody"> </param>
        /// <param name="damagingCollider"> Leave null if the collider should be able to damage the same hazard twice (i.e. player) </param>
        public void DamageHazard(Rigidbody hazardRigidbody, Collider damagingCollider = null)
        {
            HazardManager.DamageHazard(hazardRigidbody, damagingCollider);
        }

        /// <summary>
        ///     Something damaged the hazard by completely obliterating it.
        ///     No pickups will spawn
        /// </summary>
        /// <param name="hazardRigidbody"></param>
        public void EliminateHazard(Rigidbody hazardRigidbody)
        {
            HazardManager.EliminateHazard(hazardRigidbody);
        }

        public Rigidbody[] GetActiveHazardRigidbodies()
        {
            return HazardManager.GetActiveHazardRigidbodies();
        }
        
        private void Init() // todo call from some kind of countdown ui?
        {
            UnityEngine.Random.InitState((int)DateTime.Now.Ticks);
            
            _gamePaused = false;
            _gameIsCompleted = false;
            _playing = false;

            SetStreak();
            
            RunMiniGame();
        }

        private void RunMiniGame()
        {
            PreloadAssets();

            // Top bar display
            ScoreCounter.SetCountTextImmediate(0);

            gameObject.SetActive(true);
            _mainRunRoutine = StartCoroutine(RunRoutine());
        }
        
        private async void PreloadAssets()
        {
            _waitForPreload = true;
            PreloadAssetsRoutine();
            _waitForPreload = false;
        }
        
        /// <summary>
        ///     Set the streak system
        /// </summary>
        private void SetStreak()
        {
            PowerUpManager.SetupStreakAssist(this, OffscreenIndicatorManager);
        }

        /// <summary>
        ///     Load game assets so they are ready for the game to start
        /// </summary>
        /// <returns> </returns>
        private void PreloadAssetsRoutine()
        {
            // Process the configs

            // Spawn background art asset

            // Spawn enemy poolable object
            HazardManager.SetUseWaveSpawnSystem(GameConfig.UseWaveSpawning);
            HazardManager.SetConfigurableAsset(GameConfig.HazardConfig);

            // Setup points poolable object
            PointsPickupManager.SetConfigurableAsset(this, GameConfig.PointsConfig);

            // Setup hero
            SurvivorHero.SetLives(GameConfig.LifeCount);
            SurvivorHero.Setup(GameConfig.HeroConfig);
            // Wait for the hero to be setup before attaching the trail. It needs the hero config to be setup first

            // Setup the hero trail
            SurvivorHero.SetupTrail(GameConfig.HeroConfig.TrailFx);
            
            AudioMgr.Instance.PlayMusic(GameConfig.BackgroundConfig.Music, 1f);
        }
        
        /// <summary>
        ///     Entry point for the game controller
        /// </summary>
        /// <returns> </returns>
        private void StartGame()
        {
            // move start game logic?
            _playing = true;
            _introComplete = true;

            var cameraMax = GameCamera.ViewportToWorldPoint(new Vector2(1f, 1f));
            var cameraMin = GameCamera.ViewportToWorldPoint(new Vector2(0f, 0f));
            var cameraSize = new Vector2(Math.Abs(cameraMin.x) + Math.Abs(cameraMax.x),
                Math.Abs(cameraMin.y) + Math.Abs(cameraMax.y));

            BulletManager.Setup(this, SurvivorHero.gameObject, PlayerBounds.PlayerMovementBounds);
            HazardManager.Setup(this, PlayerBounds, OffscreenIndicatorManager);

            // prototype controls
            Joystick.gameObject.SetActive(true);

            // Set player bounds to the camera viewport
            OffscreenIndicatorManager.SetBoundsToCamera(GameCamera);

            
        }

        private IEnumerator RunRoutine()
        {
            var canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup != null) canvasGroup.alpha = 1f;

            GameTimer.ResetTimer();

            while (_waitForInit)
                yield return null;

            while (_waitForPreload) yield return null;

            StartGame();
			
            GameTimer.StartTimer();
            //AudioMgr.PlaySound(GameStartSound);

            _playing = true;
        }


        #region PointsHandling

        /// <summary>
        ///     Presuming the vector3 position is already adjusted to the canvas local space
        /// </summary>
        /// <param name="points"> </param>
        /// <param name="position"> </param>
        private void UpdatePoints(int points, Vector3 position)
        {
            //Visual
            if (points != 0)
                (SurvivorPointsManager.GetPooledObject(PointBubbleTemplate) as SurvivorPointsVfx)?
                    .SetIsBonus(points > GameConfig.PointsOnHazardDestroy)
                    .SetWorldPosition(position, GameCamera)
                    .SetPointsValue(points)
                    .Animate();

            //Add to total
            ScoreEarned += points;
            AnimateScoreChanged();
            ScoreCounter.Animate(points);
        }

        /// <summary>
        ///     Add points from a collected power up
        /// </summary>
        /// <param name="points"> </param>
        /// <param name="position"> </param>
        private void AddHazardPoints(int points, Vector3 position)
        {
            _pointsTracker.HazardPoints += points;
            UpdatePoints(points, position);
        }

        /// <summary>
        ///     Add score for Points items being collected
        /// </summary>
        /// <param name="position"> </param>
        public void AddPointsPickupPoints(Vector3 position)
        {
            _pointsTracker.PickupPoints += GameConfig.PointsPickupValue;
            UpdatePoints(GameConfig.PointsPickupValue, position);
        }

        /// <summary>
        /// Default score changed animator
        /// Calls a basic "you earned these many points" quick popup
        /// </summary>
        private void AnimateScoreChanged()
        {
            // Scale bounce for the actual score label TODO do we still want this?
            var root = ScoreCounter.transform;
            root.DOKill(true);
            root.DOPunchScale(0.3f * Vector3.one, 0.6f, 2, 0.5f).OnComplete(() =>
            {
                root.localScale = Vector3.one;
            });
        }
        
        #endregion
    }
}