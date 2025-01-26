using UnityEngine;

namespace P3T.Scripts.Gameplay.Survivor
{
	/// <summary>
	///     A unique config for a specific hero
	/// </summary>
	[CreateAssetMenu(fileName = "NewHazardConfig", menuName = "P3T/Survivor/HazardConfig", order = 2)]
	public class SurvivorHazardConfig : ScriptableObject
	{
#region Audio

		public AudioClip[] HazardDamagedSounds;

#endregion
	}
}