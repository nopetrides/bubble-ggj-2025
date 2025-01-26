using System;
using UnityEngine;

namespace P3T.Scripts.Gameplay.Survivor
{
	[CreateAssetMenu(fileName = "NewPointsConfig", menuName = "P3T/Survivor/PointsConfig", order = 4)]
	public class SurvivorPointsPickupConfig : ScriptableObject
	{
		public AudioClip[] CollectedSounds;
	}
}