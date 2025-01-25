using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TheraBytes.BetterUi
{
#if UNITY_2018_3_OR_NEWER
	[ExecuteAlways]
#else
    [ExecuteInEditMode]
#endif
	[HelpURL("https://documentation.therabytes.de/better-ui/GameObjectActivator.html")]
	[AddComponentMenu("Better UI/Helpers/Game Object Activator", 30)]
	public class GameObjectActivator : UIBehaviour, IResolutionDependency
	{
		[Serializable]
		public class Settings : IScreenConfigConnection
		{
			public List<GameObject> ActiveObjects = new();
			public List<GameObject> InactiveObjects = new();

			[SerializeField] private string screenConfigName;

			public string ScreenConfigName
			{
				get => screenConfigName;
				set => screenConfigName = value;
			}
		}

		[Serializable]
		public class SettingsConfigCollection : SizeConfigCollection<Settings>
		{
		}

		public Settings CurrentSettings => customSettings.GetCurrentItem(settingsFallback);

		[SerializeField] private Settings settingsFallback = new();

		[SerializeField] private SettingsConfigCollection customSettings = new();

		protected override void OnEnable()
		{
			base.OnEnable();
			Apply();
		}

		public void OnResolutionChanged()
		{
			Apply();
		}

		public void Apply()
		{
#if UNITY_EDITOR
			if (!EditorPreview && !EditorApplication.isPlaying)
				return;
#endif
			foreach (var go in CurrentSettings.ActiveObjects)
				if (go != null)
					go.SetActive(true);

			foreach (var go in CurrentSettings.InactiveObjects)
				if (go != null)
					go.SetActive(false);
		}

#if UNITY_EDITOR
		public bool EditorPreview { get; set; }

		protected override void OnValidate()
		{
			base.OnValidate();

			if (EditorPreview) Apply();
		}
#endif
	}
}