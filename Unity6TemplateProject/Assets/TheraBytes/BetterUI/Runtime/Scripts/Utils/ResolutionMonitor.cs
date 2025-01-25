using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

#pragma warning disable 0618 // disable "never assigned" warnings

namespace TheraBytes.BetterUi
{
	[HelpURL("https://documentation.therabytes.de/better-ui/ResolutionMonitor.html")]
	public class ResolutionMonitor : SingletonScriptableObject<ResolutionMonitor>
	{
		private static string FilePath => "TheraBytes/Resources/ResolutionMonitor";


#region Obsolete

		[Obsolete("Use 'GetOptimizedResolution()' instead.")]
		public static Vector2 OptimizedResolution
		{
			get => Instance.optimizedResolutionFallback;
			set
			{
				if (Instance.optimizedResolutionFallback == value)
					return;

				Instance.optimizedResolutionFallback = value;
				CallResolutionChanged();
			}
		}

		[Obsolete("Use 'GetOptimizedDpi()' instead.")]
		public static float OptimizedDpi
		{
			get => Instance.optimizedDpiFallback;
			set
			{
				if (Instance.optimizedDpiFallback == value)
					return;

				Instance.optimizedDpiFallback = value;
				CallResolutionChanged();
			}
		}

#endregion

		public static Vector2 CurrentResolution
		{
			get
			{
				if (lastScreenResolution == Vector2.zero)
					lastScreenResolution = new Vector2(Screen.width, Screen.height);

				return lastScreenResolution;
			}
		}

		public static float CurrentDpi
		{
			get
			{
				if (lastDpi == 0) lastDpi = Instance.dpiManager.GetDpi();

				return lastDpi;
			}
		}

		public string FallbackName
		{
			get => fallbackName;
			set => fallbackName = value;
		}

		public static Vector2 OptimizedResolutionFallback => Instance.optimizedResolutionFallback;
		public static float OptimizedDpiFallback => Instance.optimizedDpiFallback;

		[FormerlySerializedAs("optimizedResolution")] [SerializeField]
		private Vector2 optimizedResolutionFallback = new(1080, 1920);

		[FormerlySerializedAs("optimizedDpi")] [SerializeField]
		private float optimizedDpiFallback = 96;

		[SerializeField] private string fallbackName = "Portrait";

		[SerializeField] private StaticSizerMethod[] staticSizerMethods = new StaticSizerMethod[5];

		[SerializeField] private DpiManager dpiManager = new();

		private ScreenTypeConditions currentScreenConfig;

		[SerializeField] private List<ScreenTypeConditions> optimizedScreens = new()
		{
			new("Landscape", typeof(IsCertainScreenOrientation))
		};

		public List<ScreenTypeConditions> OptimizedScreens => optimizedScreens;

		private static readonly Dictionary<string, ScreenTypeConditions> lookUpScreens = new();

#region Screen Tags

		private static readonly HashSet<string> screenTags = new();
		public static IEnumerable<string> CurrentScreenTags => screenTags;

		public static bool AddScreenTag(string screenTag)
		{
			if (screenTags.Add(screenTag))
			{
				isDirty = true;
				Update();
				return true;
			}

			return false;
		}

		public static bool RemoveScreenTag(string screenTag)
		{
			if (screenTags.Remove(screenTag))
			{
				isDirty = true;
				Update();
				return true;
			}

			return false;
		}

		public static void ClearScreenTags()
		{
			screenTags.Clear();
			isDirty = true;
			Update();
		}

#endregion

#if UNITY_EDITOR && UNITY_2018_3_OR_NEWER
		private static StageHandle currentStage;
#endif

		public static ScreenTypeConditions CurrentScreenConfiguration
		{
			get
			{
#if UNITY_EDITOR
				if (simulatedScreenConfig != null) return simulatedScreenConfig;
#endif
				return Instance.currentScreenConfig;
			}
		}

		public static ScreenTypeConditions GetConfig(string name)
		{
			if (lookUpScreens.Count == 0)
				foreach (var config in Instance.optimizedScreens)
					lookUpScreens.Add(config.Name, config);

			if (!lookUpScreens.ContainsKey(name))
			{
				var config = Instance.optimizedScreens.FirstOrDefault(o => o.Name == name);

				if (config != null)
				{
					lookUpScreens.Add(name, config);
					return config;
				}

				return null;
			}

			return lookUpScreens[name];
		}


		public static ScreenInfo GetOpimizedScreenInfo(string name)
		{
			if (string.IsNullOrEmpty(name)) return new ScreenInfo(OptimizedResolutionFallback, OptimizedDpiFallback);

			return GetConfig(name).OptimizedScreenInfo;
		}


		public static IEnumerable<ScreenTypeConditions> GetCurrentScreenConfigurations()
		{
			foreach (var config in Instance.optimizedScreens)
				if (config.IsActive)
					yield return config;
		}


		private static Vector2 lastScreenResolution;
		private static float lastDpi;

		private static bool isDirty;

#if UNITY_EDITOR
		private static Type gameViewType;
		private static EditorWindow gameViewWindow;
		private static Version unityVersion;

		private static ScreenTypeConditions simulatedScreenConfig;

		public static ScreenTypeConditions SimulatedScreenConfig
		{
			get => simulatedScreenConfig;
			set
			{
				if (simulatedScreenConfig != value)
					isDirty = true;

				simulatedScreenConfig = value;
			}
		}

		private void OnEnable()
		{
			RegisterCallbacks();
		}

		private void OnDisable()
		{
			UnregisterCallbacks();
		}

		private static void RegisterCallbacks()
		{
			unityVersion = InternalEditorUtility.GetUnityVersion();

			isDirty = true;

			EditorApplication.update += Update;

#if UNITY_5_6_OR_NEWER
			EditorSceneManager.sceneOpened += SceneOpened;
#endif

#if UNITY_2018_0_OR_NEWER
            UnityEditor.EditorApplication.playModeStateChanged += PlayModeStateChanged;
#else
			EditorApplication.playmodeStateChanged += PlayModeStateChanged;
#endif
		}

		private static void UnregisterCallbacks()
		{
			EditorApplication.update -= Update;

#if UNITY_5_6_OR_NEWER
			EditorSceneManager.sceneOpened -= SceneOpened;
#endif

#if UNITY_2018_0_OR_NEWER
            UnityEditor.EditorApplication.playModeStateChanged -= PlayModeStateChanged;
#else
			EditorApplication.playmodeStateChanged -= PlayModeStateChanged;
#endif
		}


#if UNITY_5_6_OR_NEWER
		private static void SceneOpened(Scene scene, OpenSceneMode mode)
		{
			isDirty = true;
			Update();
		}
#endif


		private static void PlayModeStateChanged()
		{
			if (!EditorApplication.isPlaying) ClearScreenTags();

			Instance.ResolutionChanged();
		}
#else
        void OnEnable()
        {
            ResolutionChanged();
        }
#endif

		public static float InvokeStaticMethod(ImpactMode mode, Component caller, Vector2 optimizedResolution,
			Vector2 actualResolution, float optimizedDpi, float actualDpi)
		{
			var idx = 0;
			switch (mode)
			{
				case ImpactMode.StaticMethod1:
					idx = 0;
					break;
				case ImpactMode.StaticMethod2:
					idx = 1;
					break;
				case ImpactMode.StaticMethod3:
					idx = 2;
					break;
				case ImpactMode.StaticMethod4:
					idx = 3;
					break;
				case ImpactMode.StaticMethod5:
					idx = 4;
					break;
				default: throw new ArgumentException();
			}

			return HasInstance && Instance.staticSizerMethods[idx] != null
				? Instance.staticSizerMethods[idx]
					.Invoke(caller, optimizedResolution, actualResolution, optimizedDpi, actualDpi)
				: 1;
		}


		public static void MarkDirty()
		{
			isDirty = true;
		}

		public static float GetOptimizedDpi(string screenName)
		{
			if (string.IsNullOrEmpty(screenName) || screenName == Instance.fallbackName)
				return OptimizedDpiFallback;

			var s = Instance.optimizedScreens.FirstOrDefault(o => o.Name == screenName);
			if (s == null)
			{
				Debug.LogError("Screen Config with name " + screenName + " could not be found.");
				return OptimizedDpiFallback;
			}

			return s.OptimizedDpi;
		}


		public static Vector2 GetOptimizedResolution(string screenName)
		{
			if (string.IsNullOrEmpty(screenName) || screenName == Instance.fallbackName)
				return OptimizedResolutionFallback;

			var s = GetConfig(screenName);
			if (s == null)
				return OptimizedResolutionFallback;

			return s.OptimizedResolution;
		}

		public static bool IsOptimizedResolution(int width, int height)
		{
			if ((int)OptimizedResolutionFallback.x == width && (int)OptimizedResolutionFallback.y == height)
				return true;

			foreach (var config in Instance.optimizedScreens)
			{
				var si = config.OptimizedScreenInfo;
				if (si != null && (int)si.Resolution.x == width && (int)si.Resolution.y == height)
					return true;
			}

			return false;
		}

		public static void Update()
		{
#if UNITY_EDITOR
			// check if file was deleted
			if (!HasInstance)
			{
				UnregisterCallbacks();
				return;
			}
#endif

			isDirty = isDirty
#if UNITY_EDITOR // should never change in reality...
					|| Instance.GetCurrentDpi() != lastDpi
#endif
					|| GetCurrentResolution() != lastScreenResolution;

#if UNITY_EDITOR && UNITY_2018_3_OR_NEWER
			if (!isDirty)
			{
				var stage = StageUtility.GetCurrentStageHandle();
				if (stage != currentStage)
				{
					currentStage = stage;
					isDirty = true;
				}
			}
#endif

			if (isDirty)
			{
				CallResolutionChanged();
				isDirty = false;
			}
		}

		public static void CallResolutionChanged()
		{
			Instance.ResolutionChanged();
		}

		public void ResolutionChanged()
		{
			lastScreenResolution = GetCurrentResolution();
			lastDpi = GetCurrentDpi();

			currentScreenConfig = null;

			var foundConfig = false;
			foreach (var config in optimizedScreens)
				if (config.IsScreenType() && !foundConfig)
				{
					currentScreenConfig = config;
					foundConfig = true;
				}

			if (HasInstance) // preserve calling too early
				foreach (var rd in AllResolutionDependencies())
				{
					if (!(rd as Behaviour).isActiveAndEnabled)
						continue;

					rd.OnResolutionChanged();
				}

#if UNITY_EDITOR
			if (IsZoomPossible())
			{
				FindAndStoreGameView();
				if (gameViewWindow != null)
				{
					var method = gameViewType.GetMethod("UpdateZoomAreaAndParent",
						BindingFlags.Instance | BindingFlags.NonPublic);

					try
					{
						if (method != null)
							method.Invoke(gameViewWindow, null);
					}
					catch (Exception)
					{
					}
				}
			}
#endif
		}

		private static IEnumerable<IResolutionDependency> AllResolutionDependencies()
		{
			var allObjects = GetAllEditableObjects();

			// first update the "override screen properties", because other objects rely on them
			foreach (var go in allObjects)
			{
				var resDeps = go.GetComponents<OverrideScreenProperties>();
				foreach (IResolutionDependency comp in resDeps) yield return comp;
			}

			// then update all other objects
			foreach (var go in allObjects)
			{
				var resDeps = go.GetComponents<Behaviour>().OfType<IResolutionDependency>();
				foreach (var comp in resDeps)
				{
					if (comp is OverrideScreenProperties)
						continue;

					yield return comp;
				}
			}
		}

		private static IEnumerable<GameObject> GetAllEditableObjects()
		{
			var allObjects =
#if UNITY_2022_2_OR_NEWER
				FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
                UnityEngine.Object.FindObjectsOfType<GameObject>();
#endif

			foreach (var go in allObjects)
				yield return go;

#if UNITY_EDITOR && UNITY_2018_3_OR_NEWER
			var prefabStage =
#if UNITY_2021_2_OR_NEWER
				PrefabStageUtility.GetCurrentPrefabStage();
#else
                UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
#endif

			if (prefabStage != null)
				foreach (var root in prefabStage.scene.GetRootGameObjects())
				{
					foreach (var go in IterateHierarchy(root)) yield return go;
				}
#endif
		}

		private static IEnumerable<GameObject> IterateHierarchy(GameObject root)
		{
			yield return root;

			foreach (Transform child in root.transform)
			{
				foreach (var subChild in IterateHierarchy(child.gameObject)) yield return subChild;
			}
		}

		private static Vector2 GetCurrentResolution()
		{
#if UNITY_EDITOR
			FindAndStoreGameView();

			var GetSizeOfMainGameView = gameViewType.GetMethod("GetSizeOfMainGameView",
				BindingFlags.NonPublic | BindingFlags.Static);

			var res = GetSizeOfMainGameView.Invoke(null, null);
			return (Vector2)res;
#else
            return new Vector2(Screen.width, Screen.height);
#endif
		}

		private float GetCurrentDpi()
		{
#if UNITY_EDITOR

			if (IsZoomPossible())
			{
				var scale = Vector2.one;

				FindAndStoreGameView();

				if (gameViewWindow != null)
				{
					var zoomArea = gameViewType.GetField("m_ZoomArea",
							BindingFlags.Instance | BindingFlags.NonPublic)
						.GetValue(gameViewWindow);

					scale = (Vector2)zoomArea.GetType().GetField("m_Scale",
							BindingFlags.Instance | BindingFlags.NonPublic)
						.GetValue(zoomArea);
				}

				return Screen.dpi / scale.y;
			}
#endif
			return dpiManager.GetDpi();
		}

#if UNITY_EDITOR
		private static void FindAndStoreGameView()
		{
			if (gameViewType == null) gameViewType = Type.GetType("UnityEditor.GameView,UnityEditor");

			if (gameViewWindow == null)
				gameViewWindow = Resources.FindObjectsOfTypeAll(gameViewType)
					.FirstOrDefault() as EditorWindow;
		}

		public static bool IsZoomPossible()
		{
#if UNITY_2018_3_OR_NEWER // minimum officially supported version
			return true;
#else
            return unityVersion.Major > 5
                || (unityVersion.Major == 5 && unityVersion.Minor >= 4);
#endif
		}

		public void SetOptimizedResolutionFallback(Vector2 resolution)
		{
			optimizedResolutionFallback = resolution;
		}
#endif
	}
}

#pragma warning restore 0618