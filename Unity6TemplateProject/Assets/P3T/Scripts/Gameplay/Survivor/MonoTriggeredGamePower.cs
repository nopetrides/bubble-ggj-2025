using UnityEngine;

namespace P3T.Scripts.Gameplay.Survivor
{
	public class MonoTriggeredGamePower : MonoBehaviour, IGamePower
	{
		private IProcessPowerUp _controller;

		protected virtual void OnTriggerEnter2D(Collider2D col)
		{
			_controller.ProcessPowerUp(this);
		}

		protected virtual void OnCollectionDone()
		{
			gameObject.SetActive(false);
		}

		public void Setup(IProcessPowerUp controller)
		{
			_controller = controller;
		}
	}
}