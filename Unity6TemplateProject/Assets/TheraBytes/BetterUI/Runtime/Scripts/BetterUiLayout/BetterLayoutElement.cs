using System;
using UnityEngine;
using UnityEngine.UI;

namespace TheraBytes.BetterUi
{
#if UNITY_2018_3_OR_NEWER
	[ExecuteAlways]
#else
    [ExecuteInEditMode]
#endif
	[HelpURL("https://documentation.therabytes.de/better-ui/BetterLayoutElement.html")]
	[AddComponentMenu("Better UI/Layout/Better Layout Element", 30)]
	public class BetterLayoutElement : LayoutElement, IResolutionDependency
	{
		[Serializable]
		public class Settings : IScreenConfigConnection
		{
			public bool IgnoreLayout;
			public bool MinWidthEnabled, MinHeightEnabled;
			public bool PreferredWidthEnabled, PreferredHeightEnabled;
			public bool FlexibleWidthEnabled, FlexibleHeightEnabled;
			public float FlexibleWidth = 1;
			public float FlexibleHeight = 1;

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

		public FloatSizeModifier MinWidthSizer => customMinWidthSizers.GetCurrentItem(minWidthSizerFallback);
		public FloatSizeModifier MinHeightSizer => customMinHeightSizers.GetCurrentItem(minHeightSizerFallback);

		public FloatSizeModifier PreferredWidthSizer =>
			customPreferredWidthSizers.GetCurrentItem(preferredWidthSizerFallback);

		public FloatSizeModifier PreferredHeightSizer =>
			customPreferredHeightSizers.GetCurrentItem(preferredHeightSizerFallback);

		public new bool ignoreLayout
		{
			get => base.ignoreLayout;
			set { Config.Set(value, o => base.ignoreLayout = o, o => CurrentSettings.IgnoreLayout = o); }
		}

		public new float flexibleWidth
		{
			get => base.flexibleWidth;
			set { Config.Set(value, o => base.flexibleWidth = o, o => CurrentSettings.FlexibleWidth = o); }
		}

		public new float flexibleHeight
		{
			get => base.flexibleHeight;
			set { Config.Set(value, o => base.flexibleHeight = o, o => CurrentSettings.FlexibleHeight = o); }
		}

		public new float minWidth
		{
			get => base.minWidth;
			set { Config.Set(value, o => base.minWidth = o, o => MinWidthSizer.SetSize(this, o)); }
		}

		public new float minHeight
		{
			get => base.minHeight;
			set { Config.Set(value, o => base.minHeight = o, o => MinHeightSizer.SetSize(this, o)); }
		}

		public new float preferredWidth
		{
			get => base.preferredWidth;
			set { Config.Set(value, o => base.preferredWidth = o, o => PreferredWidthSizer.SetSize(this, o)); }
		}

		public new float preferredHeight
		{
			get => base.preferredHeight;
			set { Config.Set(value, o => base.preferredHeight = o, o => PreferredHeightSizer.SetSize(this, o)); }
		}

		[SerializeField] private FloatSizeModifier minWidthSizerFallback = new(0, 0, 5000);
		[SerializeField] private FloatSizeConfigCollection customMinWidthSizers = new();

		[SerializeField] private FloatSizeModifier minHeightSizerFallback = new(0, 0, 5000);
		[SerializeField] private FloatSizeConfigCollection customMinHeightSizers = new();

		[SerializeField] private FloatSizeModifier preferredWidthSizerFallback = new(100, 0, 5000);
		[SerializeField] private FloatSizeConfigCollection customPreferredWidthSizers = new();

		[SerializeField] private FloatSizeModifier preferredHeightSizerFallback = new(100, 0, 5000);
		[SerializeField] private FloatSizeConfigCollection customPreferredHeightSizers = new();


		protected override void OnEnable()
		{
			base.OnEnable();
			Apply();
		}

		public void OnResolutionChanged()
		{
			Apply();
		}

		private void Apply()
		{
			var s = CurrentSettings;

			base.ignoreLayout = s.IgnoreLayout;

			base.minWidth = s.MinWidthEnabled ? MinWidthSizer.CalculateSize(this) : -1;
			base.minHeight = s.MinHeightEnabled ? MinHeightSizer.CalculateSize(this) : -1;
			base.preferredWidth = s.PreferredWidthEnabled ? PreferredWidthSizer.CalculateSize(this) : -1;
			base.preferredHeight = s.PreferredHeightEnabled ? PreferredHeightSizer.CalculateSize(this) : -1;
			base.flexibleWidth = s.FlexibleWidthEnabled ? s.FlexibleWidth : -1;
			base.flexibleHeight = s.FlexibleHeightEnabled ? s.FlexibleHeight : -1;
		}

#if UNITY_EDITOR
		protected override void OnValidate()
		{
			base.OnValidate();
			Apply();
		}
#endif
	}
}