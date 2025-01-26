using UnityEngine;

namespace P3T.Scripts.Gameplay.Survivor
{
	/// <summary>
	///     A unique config for a specific hero
	/// </summary>
	[CreateAssetMenu(fileName = "NewHeroConfig", menuName = "P3T/Survivor/HeroConfig", order = 1)]
	public class SurvivorHeroConfig : ScriptableObject
	{
		public SurvivorAdvancedTrailFx TrailFx;
#region Audio

		public AudioClip[] HeroProjectileFiredSound;

		public AudioClip[] HeroShieldActivateSound;

		public AudioClip[] HeroShieldHitSound ;

		public AudioClip[] HeroDamagedSound ;
		
		// Don't need both of these if rotation and always happens together.
		// Can be separate if using RotateAndThrustTowardsPoint for hero movement
		// public ModSound HeroRotationSound = new("SFX_Echo_Musical_BonusPointsScale_02", false);
		// public ModSound HeroThrustSound = new("SFX_Get_Coin_Musical", false);

#endregion
	}
}