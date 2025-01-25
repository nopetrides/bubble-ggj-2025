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
	[HelpURL("https://documentation.therabytes.de/better-ui/BetterAxisAlignedLayoutGroup.html")]
	[AddComponentMenu("Better UI/Layout/Better Axis Aligned Layout Group", 30)]
	public class BetterAxisAlignedLayoutGroup
		: HorizontalOrVerticalLayoutGroup, IBetterHorizontalOrVerticalLayoutGroup, IResolutionDependency
	{
		[Serializable]
		public class Settings : IScreenConfigConnection
		{
			public TextAnchor ChildAlignment;

			public bool ReverseArrangement;

			public bool ChildForceExpandHeight;
			public bool ChildForceExpandWidth;

			public bool ChildScaleWidth;
			public bool ChildScaleHeight;

			public bool ChildControlWidth = true;
			public bool ChildControlHeight = true;

			public Axis Orientation;

			[SerializeField] private string screenConfigName;


			public Settings(TextAnchor childAlignment, bool expandWidth, bool expandHeight, Axis orientation)
			{
				ChildAlignment = childAlignment;
				ChildForceExpandWidth = expandWidth;
				ChildForceExpandHeight = expandHeight;
				Orientation = orientation;
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

		public enum Axis
		{
			Horizontal,
			Vertical
		}

		public MarginSizeModifier PaddingSizer => customPaddingSizers.GetCurrentItem(paddingSizerFallback);
		public FloatSizeModifier SpacingSizer => customSpacingSizers.GetCurrentItem(spacingSizerFallback);
		public Settings CurrentSettings => customSettings.GetCurrentItem(settingsFallback);

		public Axis Orientation
		{
			get => orientation;
			set => orientation = value;
		}

		private bool isVertical => orientation == Axis.Vertical;

		[SerializeField] private MarginSizeModifier paddingSizerFallback =
			new(new Margin(), new Margin(), new Margin(1000, 1000, 1000, 1000));

		[SerializeField] private MarginSizeConfigCollection customPaddingSizers = new();

		[SerializeField] private FloatSizeModifier spacingSizerFallback = new(0, 0, 300);

		[SerializeField] private FloatSizeConfigCollection customSpacingSizers = new();

		[SerializeField] private Settings settingsFallback;

		[SerializeField] private SettingsConfigCollection customSettings = new();

		[SerializeField] private Axis orientation;

#region new base setters

		public new RectOffset padding
		{
			get => base.padding;
			set { Config.Set(value, o => base.padding = value, o => PaddingSizer.SetSize(this, new Margin(o))); }
		}

		public new float spacing
		{
			get => base.spacing;
			set { Config.Set(value, o => base.spacing = value, o => SpacingSizer.SetSize(this, o)); }
		}

		public new TextAnchor childAlignment
		{
			get => base.childAlignment;
			set { Config.Set(value, o => base.childAlignment = o, o => CurrentSettings.ChildAlignment = o); }
		}


		public new bool childForceExpandHeight
		{
			get => base.childForceExpandHeight;
			set
			{
				Config.Set(value, o => base.childForceExpandHeight = o,
					o => CurrentSettings.ChildForceExpandHeight = o);
			}
		}

		public new bool childForceExpandWidth
		{
			get => base.childForceExpandWidth;
			set
			{
				Config.Set(value, o => base.childForceExpandWidth = o, o => CurrentSettings.ChildForceExpandWidth = o);
			}
		}

#if UNITY_2020_1_OR_NEWER
		public new bool reverseArrangement
		{
			get => base.reverseArrangement;
			set { Config.Set(value, o => base.reverseArrangement = o, o => CurrentSettings.ReverseArrangement = o); }
		}
#endif
#if UNITY_2019_1_OR_NEWER
		public new bool childScaleWidth
		{
			get => base.childScaleWidth;
			set { Config.Set(value, o => base.childScaleWidth = o, o => CurrentSettings.ChildScaleWidth = o); }
		}

		public new bool childScaleHeight
		{
			get => base.childScaleHeight;
			set { Config.Set(value, o => base.childScaleHeight = o, o => CurrentSettings.ChildScaleHeight = o); }
		}
#endif
#if !(UNITY_5_4) && !(UNITY_5_3)
		public new bool childControlWidth
		{
			get => base.childControlWidth;
			set { Config.Set(value, o => base.childControlWidth = o, o => CurrentSettings.ChildControlWidth = o); }
		}

		public new bool childControlHeight
		{
			get => base.childControlHeight;
			set { Config.Set(value, o => base.childControlHeight = o, o => CurrentSettings.ChildControlHeight = o); }
		}
#endif

#endregion


		protected override void OnEnable()
		{
			base.OnEnable();

			if (settingsFallback == null || string.IsNullOrEmpty(settingsFallback.ScreenConfigName))
				StartCoroutine(InitDelayed());
			else
				CalculateCellSize();
		}

		protected override void OnTransformChildrenChanged()
		{
			base.OnTransformChildrenChanged();

			if (isActiveAndEnabled) StartCoroutine(SetDirtyDelayed());
		}

		private IEnumerator SetDirtyDelayed()
		{
			yield return null;

			SetDirty();
		}

		protected override void OnRectTransformDimensionsChange()
		{
			base.OnRectTransformDimensionsChange();
			SetDirty();
		}


		private IEnumerator InitDelayed()
		{
			yield return null;

			settingsFallback = new Settings(childAlignment, childForceExpandWidth, childForceExpandHeight, orientation)
			{
#if !(UNITY_5_4) && !(UNITY_5_3)
				ChildControlWidth = childControlWidth,
				ChildControlHeight = childControlHeight,
#endif
#if UNITY_2019_1_OR_NEWER
				ChildScaleWidth = childScaleWidth,
				ChildScaleHeight = childScaleHeight,
#endif
#if UNITY_2020_1_OR_NEWER
				ReverseArrangement = reverseArrangement,
#endif
				ScreenConfigName = "Fallback"
			};

			CalculateCellSize();
		}

		public override void CalculateLayoutInputHorizontal()
		{
			base.CalculateLayoutInputHorizontal();
			CalcAlongAxis(0, isVertical);
		}

		public override void CalculateLayoutInputVertical()
		{
			CalcAlongAxis(1, isVertical);
		}

		public override void SetLayoutHorizontal()
		{
			SetChildrenAlongAxis(0, isVertical);
		}

		public override void SetLayoutVertical()
		{
			SetChildrenAlongAxis(1, isVertical);
		}

		public void OnResolutionChanged()
		{
			CalculateCellSize();
		}

		public void CalculateCellSize()
		{
			var r = rectTransform.rect;
			if (r.width == float.NaN || r.height == float.NaN)
				return;

			ApplySettings(CurrentSettings);

			m_Spacing = SpacingSizer.CalculateSize(this);

			var pad = PaddingSizer.CalculateSize(this);
			pad.CopyValuesTo(m_Padding);
		}

		private void ApplySettings(Settings settings)
		{
			if (settingsFallback == null)
				return;

			m_ChildAlignment = settings.ChildAlignment;
			orientation = settings.Orientation;
			m_ChildForceExpandWidth = settings.ChildForceExpandWidth;
			m_ChildForceExpandHeight = settings.ChildForceExpandHeight;

#if !(UNITY_5_4) && !(UNITY_5_3)
			m_ChildControlWidth = settings.ChildControlWidth;
			m_ChildControlHeight = settings.ChildControlHeight;
#endif
#if UNITY_2019_1_OR_NEWER
			childScaleWidth = settings.ChildScaleWidth;
			childScaleHeight = settings.ChildScaleHeight;
#endif
#if UNITY_2020_1_OR_NEWER
			reverseArrangement = settings.ReverseArrangement;
#endif
		}


#if UNITY_EDITOR
		protected override void OnValidate()
		{
			CalculateCellSize();
			base.OnValidate();
		}
#endif
	}
}