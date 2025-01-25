using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace TheraBytes.BetterUi
{
#if UNITY_2018_3_OR_NEWER
	[ExecuteAlways]
#else
    [ExecuteInEditMode]
#endif
	[HelpURL("https://documentation.therabytes.de/better-ui/BetterContentSizeFitter.html")]
	[AddComponentMenu("Better UI/Layout/Better Content Size Fitter", 30)]
	public class BetterContentSizeFitter : ContentSizeFitter, IResolutionDependency, ILayoutChildDependency,
		ILayoutElement, ILayoutIgnorer
	{
		[Serializable]
		public class Settings : IScreenConfigConnection
		{
			public FitMode HorizontalFit;
			public FitMode VerticalFit;


			public bool IsAnimated;
			public float AnimationTime = 0.2f;

			public bool HasMinWidth;
			public bool HasMinHeight;

			public bool HasMaxWidth;
			public bool HasMaxHeight;

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

		private RectTransform rectTransform => transform as RectTransform;

		public Settings CurrentSettings => customSettings.GetCurrentItem(settingsFallback);

		public RectTransform Source
		{
			get => source != null ? source : rectTransform;
			set
			{
				source = value;
				SetDirty();
			}
		}

		public bool TreatAsLayoutElement
		{
			get => treatAsLayoutElement;
			set => treatAsLayoutElement = value;
		}


		public FloatSizeModifier CurrentMinWidth => minWidthSizers.GetCurrentItem(minWidthSizerFallback);
		public FloatSizeModifier CurrentMinHeight => minHeightSizers.GetCurrentItem(minHeightSizerFallback);
		public FloatSizeModifier CurrentMaxWidth => maxWidthSizers.GetCurrentItem(maxWidthSizerFallback);
		public Vector2SizeModifier CurrentPadding => paddingSizers.GetCurrentItem(paddingFallback);

		public new FitMode horizontalFit
		{
			get => base.horizontalFit;
			set { Config.Set(value, o => base.horizontalFit = value, o => CurrentSettings.HorizontalFit = value); }
		}

		public new FitMode verticalFit
		{
			get => base.verticalFit;
			set { Config.Set(value, o => base.verticalFit = value, o => CurrentSettings.VerticalFit = value); }
		}

		[SerializeField] private RectTransform source;

		[SerializeField] private Settings settingsFallback = new();

		[SerializeField] private SettingsConfigCollection customSettings = new();

		[SerializeField] private FloatSizeModifier minWidthSizerFallback = new(0, 0, 4000);
		[SerializeField] private FloatSizeConfigCollection minWidthSizers = new();


		[SerializeField] private FloatSizeModifier minHeightSizerFallback = new(0, 0, 4000);
		[SerializeField] private FloatSizeConfigCollection minHeightSizers = new();

		[SerializeField] private FloatSizeModifier maxWidthSizerFallback = new(1000, 0, 4000);
		[SerializeField] private FloatSizeConfigCollection maxWidthSizers = new();


		[SerializeField] private FloatSizeModifier maxHeightSizerFallback = new(1000, 0, 4000);
		[SerializeField] private FloatSizeConfigCollection maxHeightSizers = new();


		[SerializeField] private Vector2SizeModifier paddingFallback =
			new(new Vector2(), new Vector2(-5000, -5000), new Vector2(5000, 5000));

		[SerializeField] private Vector2SizeConfigCollection paddingSizers = new();

		[SerializeField] private bool treatAsLayoutElement = true;

		private readonly RectTransformData start = new();
		private readonly RectTransformData end = new();

		private bool isAnimating;
		private Vector2 lastCalculatedSize;

		protected override void OnEnable()
		{
			base.OnEnable();
			Apply();
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			isAnimating = false;
		}

		public void OnResolutionChanged()
		{
			Apply();
		}

		private void Apply()
		{
			var settings = CurrentSettings;
			m_HorizontalFit = settings.HorizontalFit;
			m_VerticalFit = settings.VerticalFit;

			SetDirty();
		}

		public override void SetLayoutHorizontal()
		{
			SetLayout(0);
		}

		public override void SetLayoutVertical()
		{
			SetLayout(1);
		}

		private void SetLayout(int axis)
		{
			if (axis == 0 && CurrentSettings.HorizontalFit == FitMode.Unconstrained)
				return;

			if (axis == 1 && CurrentSettings.VerticalFit == FitMode.Unconstrained)
				return;

			if (isAnimating)
				return;


			if (CurrentSettings.IsAnimated) start.PullFromTransform(transform as RectTransform);

			// disable layout element functionality to prevent wrong size calculation for itself.
			var wasLayoutElement = treatAsLayoutElement;
			treatAsLayoutElement = false;

			if (axis == 0)
				base.SetLayoutHorizontal();
			else
				base.SetLayoutVertical();

			ApplyOffsetToDefaultSize(axis, axis == 0 ? m_HorizontalFit : m_VerticalFit);

			if (CurrentSettings.IsAnimated)
			{
				end.PullFromTransform(transform as RectTransform);
				start.PushToTransform(transform as RectTransform);

				Animate();
			}

			// restore layout element functionality to prevent wrong size calculation for parent layout groups.
			treatAsLayoutElement = wasLayoutElement;
		}

		private void ApplyOffsetToDefaultSize(int axis, FitMode fitMode)
		{
			var padding = paddingSizers.GetCurrentItem(paddingFallback).CalculateSize(this);
			var hasMax = axis == 0 ? CurrentSettings.HasMaxWidth : CurrentSettings.HasMaxHeight;
			var hasMin = axis == 0 ? CurrentSettings.HasMinWidth : CurrentSettings.HasMinHeight;

			if (hasMax || hasMin || !Mathf.Approximately(padding[axis], 0) || source != null)
			{
				var size = fitMode == FitMode.MinSize
					? LayoutUtility.GetMinSize(Source, axis)
					: LayoutUtility.GetPreferredSize(Source, axis);


				size += padding[axis];

				size = ClampSize((RectTransform.Axis)axis, size);

				lastCalculatedSize[axis] = size;
				rectTransform.SetSizeWithCurrentAnchors((RectTransform.Axis)axis, size);
			}
		}


		private float ClampSize(RectTransform.Axis axis, float size)
		{
			switch (axis)
			{
				case RectTransform.Axis.Horizontal:

					if (CurrentSettings.HasMinWidth)
						size = Mathf.Max(size,
							minWidthSizers.GetCurrentItem(minWidthSizerFallback).CalculateSize(this));

					if (CurrentSettings.HasMaxWidth)
						size = Mathf.Min(size,
							maxWidthSizers.GetCurrentItem(maxWidthSizerFallback).CalculateSize(this));
					break;

				case RectTransform.Axis.Vertical:

					if (CurrentSettings.HasMinHeight)
						size = Mathf.Max(size,
							minHeightSizers.GetCurrentItem(minHeightSizerFallback).CalculateSize(this));

					if (CurrentSettings.HasMaxHeight)
						size = Mathf.Min(size,
							maxHeightSizers.GetCurrentItem(maxHeightSizerFallback).CalculateSize(this));
					break;
			}

			return size;
		}

		private Bounds GetChildBounds()
		{
			var rt = transform as RectTransform;
			var bounds = new Bounds();
			for (var i = 0; i < transform.childCount; i++)
			{
				var child = transform.GetChild(i);
				if (!child.gameObject.activeSelf)
					continue;

				var b = RectTransformUtility.CalculateRelativeRectTransformBounds(rt, child);
				bounds.Encapsulate(b);
			}

			return bounds;
		}

		private void Animate()
		{
			if (!CurrentSettings.IsAnimated)
				return;


			StopAllCoroutines();

			StartCoroutine(CoAnimate());
		}

		private IEnumerator CoAnimate()
		{
			float t = 0;
			isAnimating = true;

			yield return null;

			while (t < CurrentSettings.AnimationTime)
			{
				t += Time.unscaledDeltaTime;
				var amount = Mathf.SmoothStep(0, 1, t / CurrentSettings.AnimationTime);
				var data = RectTransformData.Lerp(start, end, amount);
				data.PushToTransform(transform as RectTransform);

				yield return null;
			}

			end.PushToTransform(transform as RectTransform);

			isAnimating = false;

			// In case that we missed something during animation
			// simply apply the changes without animation
			CurrentSettings.IsAnimated = false;
			SetLayoutHorizontal();
			SetLayoutVertical();
			CurrentSettings.IsAnimated = true;
		}

#region ILayoutChildDependency

		public void ChildSizeChanged(Transform child)
		{
			ChildChanged();
		}

		public void ChildAddedOrEnabled(Transform child)
		{
			ChildChanged();
		}

		public void ChildRemovedOrDisabled(Transform child)
		{
			ChildChanged();
		}

		private void ChildChanged()
		{
			var tmp = CurrentSettings.IsAnimated;
			CurrentSettings.IsAnimated = false;
			SetLayoutHorizontal();
			SetLayoutVertical();
			CurrentSettings.IsAnimated = tmp;
		}

#endregion

#region ILayoutElement & ILayoutIgnorer

		float ILayoutElement.minWidth => treatAsLayoutElement && CurrentSettings.HasMinWidth
			? CurrentMinWidth.LastCalculatedSize
			: -1;

		float ILayoutElement.minHeight => treatAsLayoutElement && CurrentSettings.HasMinHeight
			? CurrentMinHeight.LastCalculatedSize
			: -1;


		float ILayoutElement.preferredWidth
		{
			get
			{
				if (!treatAsLayoutElement)
					return -1;

				SetLayoutHorizontal();
				return lastCalculatedSize.x;
			}
		}

		float ILayoutElement.preferredHeight
		{
			get
			{
				if (!treatAsLayoutElement)
					return -1;

				SetLayoutVertical();
				return lastCalculatedSize.y;
			}
		}

		float ILayoutElement.flexibleWidth => -1;
		float ILayoutElement.flexibleHeight => -1;

		int ILayoutElement.layoutPriority => 1;

		bool ILayoutIgnorer.ignoreLayout => !treatAsLayoutElement;

		void ILayoutElement.CalculateLayoutInputHorizontal()
		{
			SetLayoutHorizontal();
		}

		void ILayoutElement.CalculateLayoutInputVertical()
		{
			SetLayoutVertical();
		}

#endregion

#if UNITY_EDITOR
		protected override void OnValidate()
		{
			base.OnValidate();
			Apply();
		}


#endif
	}
}