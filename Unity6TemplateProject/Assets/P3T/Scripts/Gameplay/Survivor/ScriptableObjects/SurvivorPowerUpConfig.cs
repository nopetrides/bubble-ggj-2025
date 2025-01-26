using UnityEngine;
using UnityEngine.Serialization;

namespace P3T.Scripts.Gameplay.Survivor.ScriptableObjects
{
	[CreateAssetMenu(fileName = "NewPowerUpConfig", menuName = "P3T/Survivor/PowerUpConfig", order = 3)]
	public class SurvivorPowerUpConfig : ScriptableObject
	{
		[FormerlySerializedAs("PowerUp")] public SurvivorPowerUpType SurvivorPowerUp;
		public AudioClip PickupAppearSound;
		public AudioClip PickupCollectedSound;
	}
}