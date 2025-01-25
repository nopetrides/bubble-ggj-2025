using System.Collections.Generic;
using UnityEngine;

namespace P3T.Scripts.Gameplay.Survivor
{
	public class SurvivorBombPower : MonoBehaviour
	{
		public Rigidbody2D Rigidbody;
		[SerializeField] private GameObject BombParent;
		[SerializeField] private SurvivorController Controller;
		[SerializeField] private Animator Animator;

		public void OnTriggerEnter2D(Collider2D col)
		{
			if (col.attachedRigidbody != null && Controller.DoesRigidbodyBelongToShootable(col.attachedRigidbody))
				Controller.EliminateHazard(col.attachedRigidbody);
		}

		public void ActivateBomb(Collider2D colliderToIgnore)
		{
			BombParent.SetActive(true);
			var colliders = new List<Collider2D>();
			Rigidbody.GetAttachedColliders(colliders);
			foreach (var col in colliders) Physics2D.IgnoreCollision(colliderToIgnore, col);

			Rigidbody.position = colliderToIgnore.attachedRigidbody.position;
		}

		public void BombComplete()
		{
			BombParent.SetActive(false);
		}
	}
}