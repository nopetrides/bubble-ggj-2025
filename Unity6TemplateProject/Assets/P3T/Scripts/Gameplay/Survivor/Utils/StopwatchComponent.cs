using UnityEngine;

namespace P3T.Scripts.Gameplay.Survivor
{
	/// <summary>
	/// A self handling stopwatch timer with public methods to start, pause, stop, reset
	/// Also automatically handles app focus change while running
	/// </summary>
	public class StopwatchComponent : MonoBehaviour {
		/// <summary>
		/// the actual time value
		/// </summary>
		private float _timer;
		/// <summary>
		/// is the timer enabled
		/// </summary>
		private bool _running;

		/// <summary>
		/// Public accessor to ask <inheritdoc cref="_running"/>
		/// </summary>
		public bool IsRunning => _running;

		/// <summary>
		/// Public accessor for <inheritdoc cref="_timer"/>
		/// </summary>
		public float CurrentTime => _timer;

		private float _appPauseTime;

		private void Update() {
			if (_running)
				_timer += Time.deltaTime;
		}

		public void StartTimer() {
			_running = true;
		}

		public void ResumeTimer() {
			_running = true;
		}

		public void PauseTimer() {
			_running = false;
		}

		public void StopTimer() {
			_running = false;
		}

		public void ResetTimer() {
			_timer = 0.0f;
		}

		private void OnApplicationPause(bool paused) {
			if (paused) {
				_appPauseTime = Time.realtimeSinceStartup;
			}
			else {
				_timer += Time.realtimeSinceStartup - _appPauseTime;
				UnityEngine.Debug.Log("Seconds passed while paused added to timer: " + (Time.realtimeSinceStartup - _appPauseTime));
			}
		}
	}
}