using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

#pragma warning disable 0649 // never assigned warning

namespace TheraBytes.BetterUi
{
#if UNITY_2018_3_OR_NEWER
	[ExecuteAlways]
#else
    [ExecuteInEditMode]
#endif

	[HelpURL("https://documentation.therabytes.de/better-ui/OverrideScreenProperties.html")]
	[AddComponentMenu("Better UI/Layout/Override Screen Properties", 30)]
	public class OverrideScreenProperties : UIBehaviour, IResolutionDependency
	{
		public enum ScreenProperty
		{
			Width,
			Height,
			Dpi
		}

		public enum OverrideMode
		{
			Override,
			Inherit,
			ActualScreenProperty
		}

		[Serializable]
		public class Settings : IScreenConfigConnection
		{
			public OverrideProperty OptimizedWidthOverride;
			public OverrideProperty OptimizedHeightOverride;
			public OverrideProperty OptimizedDpiOverride;

			[SerializeField] private string screenConfigName;

			public OverrideProperty this[ScreenProperty property]
			{
				get
				{
					switch (property)
					{
						case ScreenProperty.Width: return OptimizedWidthOverride;
						case ScreenProperty.Height: return OptimizedHeightOverride;
						case ScreenProperty.Dpi: return OptimizedDpiOverride;
						default: throw new ArgumentException();
					}
				}
			}

			public string ScreenConfigName
			{
				get => screenConfigName;
				set => screenConfigName = value;
			}

			public IEnumerable<OverrideProperty> PropertyIterator()
			{
				yield return OptimizedWidthOverride;
				yield return OptimizedHeightOverride;
				yield return OptimizedDpiOverride;
			}

			[Serializable]
			public class OverrideProperty
			{
				[SerializeField] private OverrideMode mode;

				[SerializeField] private float value;

				public OverrideMode Mode => mode;
				public float Value => value;
			}
		}

		[Serializable]
		public class SettingsConfigCollection : SizeConfigCollection<Settings>
		{
		}


		[SerializeField] private Settings settingsFallback = new();

		[SerializeField] private SettingsConfigCollection customSettings = new();

		public Settings CurrentSettings => customSettings.GetCurrentItem(settingsFallback);

#if UNITY_EDITOR
		public SettingsConfigCollection SettingsList => customSettings;
		public Settings FallbackSettings => settingsFallback;
#endif

		public ScreenInfo OptimizedOverride { get; } = new();

		public ScreenInfo CurrentSize { get; } = new();

		protected override void OnEnable()
		{
			base.OnEnable();
			OnResolutionChanged();
		}

		protected override void OnTransformParentChanged()
		{
			base.OnTransformParentChanged();
			OnResolutionChanged();
		}

		protected override void OnRectTransformDimensionsChange()
		{
			base.OnRectTransformDimensionsChange();
			OnResolutionChanged();
		}

		public void OnResolutionChanged()
		{
			// unfortunately the dimension is updated delayed and the UI-interfaces do not work (at least not for all supported unity versions)
			// so we simply wait a frame, before updating the sizes.

			StopAllCoroutines();
			StartCoroutine(RecalculateRoutine());
		}

		private IEnumerator RecalculateRoutine()
		{
			yield return null;

			var settings = customSettings.GetCurrentItem(settingsFallback);
			Recalculate(settings);

			// let all children recalculate now
			InformChildren();
		}

		private void Recalculate(Settings settings)
		{
			var parent = settings.PropertyIterator().Any(o => o.Mode == OverrideMode.Inherit)
				? GetComponentInParent<OverrideScreenProperties>()
				: null;

			var optimizedWidth = CalculateOptimizedValue(settings, ScreenProperty.Width, parent);
			var optimizedHeight = CalculateOptimizedValue(settings, ScreenProperty.Height, parent);
			var optimizedDpi = CalculateOptimizedValue(settings, ScreenProperty.Dpi, parent);

			OptimizedOverride.Resolution = new Vector2(optimizedWidth, optimizedHeight);
			OptimizedOverride.Dpi = optimizedDpi;

			var rect = new Rect();
			if (settings.PropertyIterator().Any(o => o.Mode == OverrideMode.Override))
				rect = (transform as RectTransform).rect;
			;


			var currentWidth = CalculateCurrentValue(settings, ScreenProperty.Width, parent, rect);
			var currentHeight = CalculateCurrentValue(settings, ScreenProperty.Height, parent, rect);
			var currentDpi = CalculateCurrentValue(settings, ScreenProperty.Dpi, parent, rect);

			CurrentSize.Resolution = new Vector2(currentWidth, currentHeight);
			CurrentSize.Dpi = currentDpi;
		}

		public float CalculateOptimizedValue(Settings settings, ScreenProperty property,
			OverrideScreenProperties parent)
		{
			switch (settings[property].Mode)
			{
				case OverrideMode.Override:
					return settings[property].Value;

				case OverrideMode.Inherit:
					if (parent != null)
						switch (parent.CurrentSettings[property].Mode)
						{
							case OverrideMode.Override:
								return parent.CurrentSettings[property].Value;

							case OverrideMode.Inherit:
								var parentParent = parent.GetComponentsInParent<OverrideScreenProperties>()
									.FirstOrDefault(o => o.gameObject != gameObject);
								return parent.CalculateOptimizedValue(parent.CurrentSettings, property, parentParent);

							case OverrideMode.ActualScreenProperty: break;
						}

					// If parent is null or parent uses actual screen property: 
					// Fall through!
					goto case OverrideMode.ActualScreenProperty;

				case OverrideMode.ActualScreenProperty:
					var info = ResolutionMonitor.GetOpimizedScreenInfo(settings.ScreenConfigName);
					switch (property)
					{
						case ScreenProperty.Width: return info.Resolution.x;
						case ScreenProperty.Height: return info.Resolution.y;
						case ScreenProperty.Dpi: return info.Dpi;
						default:
							throw new ArgumentException();
							;
					}
			}

			throw new ArgumentException();
		}

		private float CalculateCurrentValue(Settings settings, ScreenProperty property, OverrideScreenProperties parent,
			Rect rect)
		{
			switch (settings[property].Mode)
			{
				case OverrideMode.Override:
					switch (property)
					{
						case ScreenProperty.Width: return rect.width;
						case ScreenProperty.Height: return rect.height;
						case ScreenProperty.Dpi: break;
					}

					;

					// DPI case: Fall through!
					goto case OverrideMode.ActualScreenProperty;

				case OverrideMode.Inherit:
					if (parent != null)
						switch (parent.CurrentSettings[property].Mode)
						{
							case OverrideMode.Override:
								var parentRect = (parent.transform as RectTransform).rect;
								return parent.CalculateCurrentValue(parent.CurrentSettings, property, null, parentRect);

							case OverrideMode.Inherit:
								var parentParent = parent.GetComponentsInParent<OverrideScreenProperties>()
									.FirstOrDefault(o => o.gameObject != gameObject);
								return parent.CalculateCurrentValue(parent.CurrentSettings, property, parentParent,
									new Rect());

							case OverrideMode.ActualScreenProperty: break;
						}

					// If parent is null or parent uses actual screen property: 
					// Fall through!
					goto case OverrideMode.ActualScreenProperty;

				case OverrideMode.ActualScreenProperty:
					switch (property)
					{
						case ScreenProperty.Width: return ResolutionMonitor.CurrentResolution.x;
						case ScreenProperty.Height: return ResolutionMonitor.CurrentResolution.y;
						case ScreenProperty.Dpi: return ResolutionMonitor.CurrentDpi;
						default:
							throw new ArgumentException();
							;
					}
			}

			throw new ArgumentException();
		}


		public void InformChildren()
		{
			var resDeps = GetComponentsInChildren<Component>().OfType<IResolutionDependency>();
			foreach (var comp in resDeps)
			{
				if (comp.Equals(this))
					continue;

				comp.OnResolutionChanged();
			}
		}
	}
}

#pragma warning restore 0649