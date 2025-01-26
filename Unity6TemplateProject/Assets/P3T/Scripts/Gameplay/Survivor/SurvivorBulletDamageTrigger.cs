using UnityEngine;

namespace P3T.Scripts.Gameplay.Survivor
{
	public class SurvivorBulletDamageTrigger : MonoBehaviour
	{
		[SerializeField] private SurvivorBullet Bullet;

		private void OnTriggerEnter(Collider other)
		{
			Bullet.OnDamageTriggerEnter(other);
		}
	}
}