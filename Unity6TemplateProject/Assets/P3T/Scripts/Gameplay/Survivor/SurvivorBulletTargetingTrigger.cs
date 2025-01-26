using UnityEngine;

namespace P3T.Scripts.Gameplay.Survivor
{
	public class SurvivorBulletTargetingTrigger : MonoBehaviour
	{
		[SerializeField] private SurvivorBullet Bullet;

		private void OnTriggerEnter(Collider other)
		{
			Bullet.OnTargetTriggerEnter(other);
		}
	}
}