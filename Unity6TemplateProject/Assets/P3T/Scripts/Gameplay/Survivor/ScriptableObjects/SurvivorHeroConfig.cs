using P3T.Scripts.Gameplay.Survivor.Drivers;
using UnityEngine;

namespace P3T.Scripts.Gameplay.Survivor.ScriptableObjects
{
	/// <summary>
	///     A unique config for a specific hero
	/// </summary>
	[CreateAssetMenu(fileName = "NewHeroConfig", menuName = "P3T/Survivor/HeroConfig", order = 1)]
	public class SurvivorHeroConfig : ScriptableObject
	{
		public SurvivorAnimatedAsset SurvivorPrefab;
		public SurvivorAdvancedTrailFx TrailFx;
#region Audio

		public AudioClip[] HeroProjectileFiredSound;

		public AudioClip[] HeroShieldActivateSound;

		public AudioClip[] HeroShieldHitSound ;

		public AudioClip[] HeroDamagedSound ;
		
		// Don't need both of these if rotation and always happens together.
		// Can be separate if using RotateAndThrustTowardsPoint for hero movement
		// public AudioClip HeroRotationSound = new("", false);
		// public AudioClip HeroThrustSound = new("", false);

#endregion
	}
}