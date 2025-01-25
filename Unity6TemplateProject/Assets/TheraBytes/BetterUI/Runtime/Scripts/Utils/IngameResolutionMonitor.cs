using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheraBytes.BetterUi
{
	[HelpURL("https://documentation.therabytes.de/better-ui/IngameResolutionMonitor.html")]
	[AddComponentMenu("Better UI/In-Game Resolution Monitor", 30)]
	public class IngameResolutionMonitor : MonoBehaviour
	{
		private static IngameResolutionMonitor instance;

		[SerializeField] private bool onlyPresentInThisScene;

		public static GameObject Create()
		{
			var go = new GameObject("IngameResolutionMonitor");
			go.AddComponent<IngameResolutionMonitor>();

			return go;
		}

		private void OnEnable()
		{
			if (instance != null)
			{
				Debug.LogWarning(
					"There already is an Ingame Resolution Monitor. One is enough. Destroying the previous one now...");
				Destroy(instance.gameObject);
			}

			instance = this;

			if (!onlyPresentInThisScene) DontDestroyOnLoad(gameObject);

			SceneManager.sceneLoaded += SceneLoaded;
		}

		private void OnDisable()
		{
			instance = null;
			SceneManager.sceneLoaded -= SceneLoaded;
		}

		private void SceneLoaded(Scene scene, LoadSceneMode mode)
		{
			ResolutionMonitor.MarkDirty();
			ResolutionMonitor.Update();
		}

#if !(UNITY_EDITOR)
        void Update()
        {
            ResolutionMonitor.Update();
        }
#endif
	}
}