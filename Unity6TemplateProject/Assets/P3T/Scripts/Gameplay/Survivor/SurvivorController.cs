using System;
using System.Collections;
using System.Threading.Tasks;
using DG.Tweening;
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

        [Header("Background")] 
        [SerializeField] private Transform BackgroundParent;

        [Header("Camera")]
        // The Rect that all the parent objects anchor and stretch with
        [SerializeField] private RectTransform GameParentTransform;
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
                AnimateScoreChanged();
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
        public Vector2 HeroPosition => SurvivorHero.transform.position;

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
            var powerUp = triggeredPowerUp as ShooterPickup;
            if (powerUp == null)
            {
                UnityEngine.Debug.LogError("MG_ArcadeSurvivor ProcessPowerUp expected a ShooterPickup type");
                return;
            }

            switch (powerUp.Power)
            {
                case ShooterPickup.PowerUpType.FireRateUp:
                    SurvivorHero.ActivateRapidShot();
                    break;
                case ShooterPickup.PowerUpType.Shield:
                    SurvivorHero.ActivateShield();
                    break;
                case ShooterPickup.PowerUpType.SpreadShot:
                    SurvivorHero.ActivateSpreadShot();
                    break;
                case ShooterPickup.PowerUpType.PierceShot:
                    SurvivorHero.ActivatePierceShot();
                    break;
                case ShooterPickup.PowerUpType.RegainHealth:
                    SurvivorHero.RegainHealth();
                    break;
                case ShooterPickup.PowerUpType.Bomb:
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

        public void OnShootableDestroyed(Vector3 position, bool spawnsPickupOnDestroy,
            StreakAssist.Item streakItem = null)
        {
            position = ClampPositionWithinLevelBounds(position);
            AddShootablePoints(GameConfig.PointsOnHazardDestroy, position);

            if (spawnsPickupOnDestroy == false) return;
            // spawn the power or points that the shootable drops
            if (streakItem != null)
                // if this shootable has predetermined power up, spawn it
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
            var y = Mathf.Clamp(position.y, minY, maxY);
            position.x = x;
            position.y = y;
            return position;
        }

        private void ShowScoreSummary()
        {
            LevelSummaryWidget.gameObject.SetActive(true);
            LevelSummaryWidget.Setup(_pointsTracker, ScoreEarned,
                () => _gameIsCompleted = true);
        }

        public bool DoesRigidbodyBelongToShootable(Rigidbody2D rb)
        {
            return HazardManager.IsRigidbodyActiveShootable(rb);
        }

        /// <summary>
        ///     Something  has damaged a hazard object
        /// </summary>
        /// <param name="shootableRigidbody"> </param>
        /// <param name="damagingCollider"> Leave null if the collider should be able to damage the same hazard twice (i.e. player) </param>
        public void DamageHazard(Rigidbody2D shootableRigidbody, Collider2D damagingCollider = null)
        {
            HazardManager.DamageHazard(shootableRigidbody, damagingCollider);
        }

        /// <summary>
        ///     Something damaged the hazard by completely obliterating it.
        ///     No pickups will spawn
        /// </summary>
        public void EliminateHazard(Rigidbody2D shootableRigidbody)
        {
            HazardManager.EliminateHazard(shootableRigidbody);
        }

        public Rigidbody2D[] GetActiveHazardRigidbodies()
        {
            return HazardManager.GetActiveHazardRigidbodies();
        }





        public void Init()
        {
            _gamePaused = false;
            _gameIsCompleted = false;
            _playing = false;

            SetStreak();
            
            RunMiniGame();
        }
        
        protected virtual void RunMiniGame()
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
            await PreloadAssetsRoutine();
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
        protected async Task PreloadAssetsRoutine()
        {
            // Process ConfigurableMinigameAssetInfo from the config (defaults or mod panel assigned)

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
            SurvivorHero.SetupTrail(GameConfig.BackgroundConfig.TrailFx);
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

            GameParentTransform.sizeDelta = cameraSize;

            BulletManager.Setup(this, SurvivorHero.SpawnedArtAsset, PlayerBounds.PlayerMovementBounds);
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
        private void UpdatePoints(int points, Vector2 position)
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
        }

        /// <summary>
        ///     Add points from a collected power up
        /// </summary>
        /// <param name="points"> </param>
        /// <param name="position"> </param>
        private void AddShootablePoints(int points, Vector3 position)
        {
            _pointsTracker.HazardPoints += points;
            UpdatePoints(points, position);
        }

        /// <summary>
        ///     Bonus score for overall survival time
        /// </summary>
        [Obsolete]
        private void AddSurvivalPoints()
        {
            var totalIntervals =
                Mathf.FloorToInt(_pointsTimer / GameConfig.PointsUpdateInterval); // Calculate intervals passed
            var survivalPoints = totalIntervals * GameConfig.PointsPerSecond;
            _pointsTracker.SurvivalPoints = survivalPoints;
            // Don't animate a bubble
            ScoreEarned += totalIntervals * GameConfig.PointsPerSecond;
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
        public void AnimateScoreChanged()
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