using System;
using JetBrains.Annotations;
using UnityEngine;

namespace P3T.Scripts.Gameplay.Survivor
{
	public class SurvivorPickupAnimationEventListener : MonoBehaviour
	{
		public Action OnAnimationComplete;

		[UsedImplicitly] // Called by an Animation Event
		public void CollectionAnimationComplete()
		{
			OnAnimationComplete?.Invoke();
		}
	}
}