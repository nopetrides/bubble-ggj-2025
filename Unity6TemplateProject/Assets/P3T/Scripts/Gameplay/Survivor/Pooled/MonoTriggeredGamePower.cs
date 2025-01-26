using UnityEngine;

namespace P3T.Scripts.Gameplay.Survivor.Pooled
{
	public class MonoTriggeredGamePower : MonoBehaviour, IGamePower
	{
		private IProcessPowerUp _controller;

		protected virtual void OnTriggerEnter(Collider col)
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