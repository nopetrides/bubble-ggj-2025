using System.Collections.Generic;
using UnityEngine;

namespace P3T.Scripts.Gameplay.Survivor
{
	public class SurvivorBombPower : MonoBehaviour
	{
		[SerializeField] private Rigidbody Rb;
		[SerializeField] private GameObject BombParent;
		[SerializeField] private SurvivorController Controller;
		[SerializeField] private Animator Animator;
		
		public Rigidbody Rigidbody => Rb;

		public void OnTriggerEnter(Collider col)
		{
			if (col.attachedRigidbody != null && Controller.DoesRigidbodyBelongToHazard(col.attachedRigidbody))
				Controller.EliminateHazard(col.attachedRigidbody);
		}

		public void ActivateBomb(Collider colliderToIgnore)
		{
			BombParent.SetActive(true);
			
			// todo optimize GetComponents
			foreach (var col in Rigidbody.GetComponentsInChildren<Collider>()) Physics.IgnoreCollision(colliderToIgnore, col);

			Rigidbody.position = colliderToIgnore.attachedRigidbody.position;
		}

		public void BombComplete()
		{
			BombParent.SetActive(false);
		}
	}
}