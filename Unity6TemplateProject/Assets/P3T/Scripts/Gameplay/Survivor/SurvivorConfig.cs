using System;

namespace P3T.Scripts.Gameplay.Survivor
{
	/// <summary>
	///     Config for all game config information
	/// </summary>
	[Serializable]
	public class SurvivorConfig
	{
		// Hero
		public SurvivorHeroConfig HeroConfig;

		// Hazards
		public SurvivorHazardConfig HazardConfig;

		// Background
		public SurvivorBackgroundConfig BackgroundConfig;

		// Points
		public SurvivorPointsPickupConfig PointsConfig;
		
		// Number of points earned per second of survival
		public int PointsPerSecond = 10;

		// Interval to track points earned (in seconds)
		// 0.1 tracks points every 1/10th of a second elapsed
		public float PointsUpdateInterval = 0.1f;

		// Per hazard destroyed
		public int PointsOnHazardDestroy = 10;

		// Per points pickup collected
		public int PointsPickupValue = 100;

		public float PowerUpPickupPointsMultiplier = 2.0f;

		// total life of the player (tied to the number of hearts widget)
		public int LifeCount = 3;

		public bool UseWaveSpawning = true;
            
	}
}