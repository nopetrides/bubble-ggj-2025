using P3T.Scripts.Gameplay.Survivor.Drivers;
using UnityEngine;

namespace P3T.Scripts.Gameplay.Survivor.ScriptableObjects
{
	/// <summary>
	///     A unique config for a specific hero
	/// </summary>
	[CreateAssetMenu(fileName = "NewHazardConfig", menuName = "P3T/Survivor/HazardConfig", order = 2)]
	public class SurvivorHazardConfig : ScriptableObject
	{
		public SurvivorAnimatedAsset HazardPrefab;
#region Audio

		public AudioClip[] HazardDamagedSounds;

#endregion
	}
}