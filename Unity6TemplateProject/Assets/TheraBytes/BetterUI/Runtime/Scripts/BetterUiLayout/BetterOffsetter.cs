using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheraBytes.BetterUi
{
#if UNITY_2018_3_OR_NEWER
	[ExecuteAlways]
#else
    [ExecuteInEditMode]
#endif
	[HelpURL("https://documentation.therabytes.de/better-ui/BetterOffsetter.html")]
	[RequireComponent(typeof(RectTransform))]
	[AddComponentMenu("Better UI/Layout/Better Offsetter", 30)]
	public class BetterOffsetter : UIBehaviour, ILayoutController, ILayoutSelfController, IResolutionDependency
	{
		[Serializable]
		public class Settings : IScreenConfigConnection
		{
			[SerializeField] private bool applyPosX;

			[SerializeField] private bool applyPosY;

			[SerializeField] private bool applySizeX;

			[SerializeField] private bool applySizeY;

			[SerializeField] private string screenConfigName;

			public bool ApplyPosX
			{
				get => applyPosX;
				set => applySizeX = value;
			}

			public bool ApplyPosY
			{
				get => applyPosY;
				set => applyPosY = value;
			}

			public bool ApplySizeX
			{
				get => applySizeX;
				set => applySizeX = value;
			}

			public bool ApplySizeY
			{
				get => applySizeY;
				set => applySizeY = value;
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


		public FloatSizeModifier AnchoredPositionXSizer =>
			customAnchorPosXSizers.GetCurrentItem(anchorPosXSizerFallback);

		public FloatSizeModifier AnchoredPositionYSizer =>
			customAnchorPosYSizers.GetCurrentItem(anchorPosYSizerFallback);


		public FloatSizeModifier SizeDeltaXSizer => customSizeDeltaXSizers.GetCurrentItem(sizeDeltaXSizerFallback);

		public FloatSizeModifier SizeDeltaYSizer => customSizeDeltaYSizers.GetCurrentItem(sizeDeltaYSizerFallback);

		[SerializeField] private FloatSizeModifier anchorPosXSizerFallback = new(100, 0, 1000);
		[SerializeField] private FloatSizeConfigCollection customAnchorPosXSizers = new();

		[SerializeField] private FloatSizeModifier anchorPosYSizerFallback = new(100, 0, 1000);
		[SerializeField] private FloatSizeConfigCollection customAnchorPosYSizers = new();

		[SerializeField] private FloatSizeModifier sizeDeltaXSizerFallback = new(100, 0, 1000);
		[SerializeField] private FloatSizeConfigCollection customSizeDeltaXSizers = new();

		[SerializeField] private FloatSizeModifier sizeDeltaYSizerFallback = new(100, 0, 1000);
		[SerializeField] private FloatSizeConfigCollection customSizeDeltaYSizers = new();


		private DrivenRectTransformTracker rectTransformTracker;

		protected override void OnEnable()
		{
			base.OnEnable();
			ApplySize();
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			rectTransformTracker.Clear();
		}

#if UNITY_EDITOR
		protected override void OnValidate()
		{
			base.OnValidate();

			// NOTE: Unity sends a message when setting RectTransform.sizeDelta which is required here.
			//       This logs a warning: "SendMessage cannot be called during Awake, CheckConsistency, or OnValidate"
			//       However, everything seems to work anyway. Seems like there is no easy way to work around this problem.
			ApplySize();
		}
#endif

		private void ApplySize()
		{
			if (!isActiveAndEnabled)
				return;

			var rt = transform as RectTransform;
			var pos = rt.anchoredPosition;
			var size = rt.sizeDelta;

			var settings = CurrentSettings;
			rectTransformTracker.Clear();

			if (settings.ApplySizeX)
			{
				size.x = SizeDeltaXSizer.CalculateSize(this);
				rectTransformTracker.Add(this, transform as RectTransform, DrivenTransformProperties.SizeDeltaX);
			}

			if (settings.ApplySizeY)
			{
				size.y = SizeDeltaYSizer.CalculateSize(this);
				rectTransformTracker.Add(this, transform as RectTransform, DrivenTransformProperties.SizeDeltaY);
			}

			if (settings.ApplyPosX)
			{
				pos.x = AnchoredPositionXSizer.CalculateSize(this);
				rectTransformTracker.Add(this, transform as RectTransform, DrivenTransformProperties.AnchoredPositionX);
			}

			if (settings.ApplyPosY)
			{
				pos.y = AnchoredPositionYSizer.CalculateSize(this);
				rectTransformTracker.Add(this, transform as RectTransform, DrivenTransformProperties.AnchoredPositionY);
			}

			rt.anchoredPosition = pos;
			rt.sizeDelta = size;
		}

		public void OnResolutionChanged()
		{
			ApplySize();
		}

		public void SetLayoutHorizontal()
		{
			ApplySize();
		}

		public void SetLayoutVertical()
		{
			ApplySize();
		}

		protected override void OnRectTransformDimensionsChange()
		{
			base.OnRectTransformDimensionsChange();
			ApplySize();
		}
	}
}