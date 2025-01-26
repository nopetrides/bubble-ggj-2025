using P3T.Scripts.Gameplay.Survivor.Drivers;
using UnityEngine;

namespace P3T.Scripts.Gameplay.Survivor.ScriptableObjects
{
	[CreateAssetMenu(fileName = "NewPointsConfig", menuName = "P3T/Survivor/PointsConfig", order = 4)]
	public class SurvivorPointsPickupConfig : ScriptableObject
	{
		public SurvivorAnimatedAsset PickupPrefab;
#region Audio
		public AudioClip[] CollectedSounds;
#endregion
	}
}