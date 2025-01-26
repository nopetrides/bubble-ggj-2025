using UnityEngine;

namespace P3T.Scripts.Gameplay.Survivor
{
	public abstract class PooledParticleBase<T> : MonoBehaviour
	{
		[SerializeField] private float Lifetime = 2.1f;

		[SerializeField] private ParticleSystem Particles;

		private float _timer;
		private bool _playing;

		private void FixedUpdate()
		{
			if (_playing == false) return;
			if (_timer > Lifetime) Release();

			_timer += Time.fixedDeltaTime;
		}

		private void OnDisable()
		{
			_playing = false;
			_timer = 0;
		}

		protected abstract void Release();
		public abstract void SetManager(T manager);

		public void SetParent(Transform parentTransform)
		{
			transform.SetParent(parentTransform);
		}

		public void SetPosition(Vector3 position)
		{
			transform.position = position;
		}

		public void Play()
		{
			Particles.Play();
			_playing = true;
		}
	}
}