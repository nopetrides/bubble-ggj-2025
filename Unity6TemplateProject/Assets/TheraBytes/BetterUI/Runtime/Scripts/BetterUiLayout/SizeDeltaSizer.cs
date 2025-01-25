using System;
using UnityEngine;

namespace TheraBytes.BetterUi
{
#if UNITY_2018_3_OR_NEWER
	[ExecuteAlways]
#else
    [ExecuteInEditMode]
#endif
	[HelpURL("https://documentation.therabytes.de/better-ui/SizeDeltaSizer.html")]
	[RequireComponent(typeof(RectTransform))]
	[AddComponentMenu("Better UI/Layout/Size Delta Sizer", 30)]
	public class SizeDeltaSizer : ResolutionSizer<Vector2>
	{
		[Serializable]
		public class Settings : IScreenConfigConnection
		{
			[SerializeField] private bool applyWidth, applyHeight;

			[SerializeField] private string screenConfigName;

			public bool ApplyWidth
			{
				get => applyWidth;
				set => applyWidth = value;
			}

			public bool ApplyHeight
			{
				get => applyHeight;
				set => applyHeight = value;
			}

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


		public Vector2SizeModifier DeltaSizer => customDeltaSizers.GetCurrentItem(deltaSizerFallback);


		protected override ScreenDependentSize<Vector2> sizer => customDeltaSizers.GetCurrentItem(deltaSizerFallback);

		[SerializeField]
		private Vector2SizeModifier deltaSizerFallback = new(100 * Vector2.one, Vector2.zero, 1000 * Vector2.one);

		[SerializeField] private Vector2SizeConfigCollection customDeltaSizers = new();

		private DrivenRectTransformTracker rectTransformTracker;

		protected override void OnDisable()
		{
			base.OnDisable();
			rectTransformTracker.Clear();
		}


		protected override void ApplySize(Vector2 newSize)
		{
			var rt = transform as RectTransform;
			var size = rt.sizeDelta;

			var settings = CurrentSettings;
			rectTransformTracker.Clear();

			if (settings.ApplyWidth)
			{
				size.x = newSize.x;
				rectTransformTracker.Add(this, transform as RectTransform, DrivenTransformProperties.SizeDeltaX);
			}

			if (settings.ApplyHeight)
			{
				size.y = newSize.y;
				rectTransformTracker.Add(this, transform as RectTransform, DrivenTransformProperties.SizeDeltaY);
			}

			rt.sizeDelta = size;
		}
	}
}