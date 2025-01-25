using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TheraBytes.BetterUi
{
#if UNITY_2018_3_OR_NEWER
	[ExecuteAlways]
#else
    [ExecuteInEditMode]
#endif
	[HelpURL("https://documentation.therabytes.de/better-ui/BetterGridLayoutGroup.html")]
	[AddComponentMenu("Better UI/Layout/Better Grid Layout Group", 30)]
	public class BetterGridLayoutGroup : GridLayoutGroup, IResolutionDependency
	{
		[Serializable]
		public class Settings : IScreenConfigConnection
		{
			public Constraint Constraint;
			public int ConstraintCount;
			public TextAnchor ChildAlignment;
			public Axis StartAxis;
			public Corner StartCorner;
			public bool Fit;

			[SerializeField] private string screenConfigName;

			public Settings(BetterGridLayoutGroup grid)
			{
				Constraint = grid.m_Constraint;
				ConstraintCount = grid.m_ConstraintCount;
				ChildAlignment = grid.childAlignment;
				StartAxis = grid.m_StartAxis;
				StartCorner = grid.m_StartCorner;
				Fit = grid.fit;
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


		public MarginSizeModifier PaddingSizer => customPaddingSizers.GetCurrentItem(paddingSizerFallback);
		public Vector2SizeModifier CellSizer => customCellSizers.GetCurrentItem(cellSizerFallback);
		public Vector2SizeModifier SpacingSizer => customSpacingSizers.GetCurrentItem(spacingSizerFallback);
		public Settings CurrentSettings => customSettings.GetCurrentItem(settingsFallback);

		public new RectOffset padding
		{
			get => base.padding;
			set { Config.Set(value, o => base.padding = value, o => PaddingSizer.SetSize(this, new Margin(o))); }
		}

		public new Vector2 spacing
		{
			get => base.spacing;
			set { Config.Set(value, o => base.spacing = value, o => SpacingSizer.SetSize(this, o)); }
		}

		public new Vector2 cellSize
		{
			get => base.cellSize;
			set { Config.Set(value, o => base.cellSize = value, o => CellSizer.SetSize(this, o)); }
		}

		public new Constraint constraint
		{
			get => base.constraint;
			set { Set(value, o => base.constraint = o, (s, o) => s.Constraint = o); }
		}

		public new int constraintCount
		{
			get => base.constraintCount;
			set { Set(value, o => base.constraintCount = o, (s, o) => s.ConstraintCount = o); }
		}

		public new TextAnchor childAlignment
		{
			get => base.childAlignment;
			set { Set(value, o => base.childAlignment = o, (s, o) => s.ChildAlignment = o); }
		}

		public new Axis startAxis
		{
			get => base.startAxis;
			set { Set(value, o => base.startAxis = o, (s, o) => s.StartAxis = o); }
		}

		public new Corner startCorner
		{
			get => base.startCorner;
			set { Set(value, o => base.startCorner = o, (s, o) => s.StartCorner = o); }
		}

		public bool Fit
		{
			get => fit;
			set { Set(value, o => fit = o, (s, o) => s.Fit = o); }
		}

		[FormerlySerializedAs("paddingSizer")] [SerializeField]
		private MarginSizeModifier paddingSizerFallback =
			new(new Margin(), new Margin(), new Margin(1000, 1000, 1000, 1000));

		[SerializeField] private MarginSizeConfigCollection customPaddingSizers = new();

		[FormerlySerializedAs("cellSizer")] [SerializeField]
		private Vector2SizeModifier cellSizerFallback =
			new(new Vector2(100, 100), new Vector2(10, 10), new Vector2(300, 300));

		[SerializeField] private Vector2SizeConfigCollection customCellSizers = new();

		[FormerlySerializedAs("spacingSizer")] [SerializeField]
		private Vector2SizeModifier spacingSizerFallback = new(Vector2.zero, Vector2.zero, new Vector2(300, 300));

		[SerializeField] private Vector2SizeConfigCollection customSpacingSizers = new();

		[SerializeField] private Settings settingsFallback;

		[SerializeField] private SettingsConfigCollection customSettings = new();

		[SerializeField] private bool fit;

		protected override void OnRectTransformDimensionsChange()
		{
			base.OnRectTransformDimensionsChange();
			CalculateCellSize();
		}

		protected override void OnEnable()
		{
			base.OnEnable();

			if (settingsFallback == null || string.IsNullOrEmpty(settingsFallback.ScreenConfigName))
				StartCoroutine(InitDelayed());
			else
				CalculateCellSize();
		}

		private IEnumerator InitDelayed()
		{
			yield return null;

			settingsFallback = new Settings(this)
			{
				ScreenConfigName = "Fallback"
			};

			CalculateCellSize();
		}

		public void OnResolutionChanged()
		{
			CalculateCellSize();

			// for fit mode we need to calculate it again because of unity internal stuff...
			if (fit)
			{
				SetDirty();
				CalculateCellSize();
			}
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

			// cell size
			CellSizer.CalculateSize(this);

			if (fit)
			{
				var size = CellSizer.LastCalculatedSize;

				switch (base.constraint)
				{
					case Constraint.FixedColumnCount:

						size.x = GetCellWidth();
						break;

					case Constraint.FixedRowCount:

						size.y = GetCellHeight();
						break;
				}

				CellSizer.OverrideLastCalculatedSize(size);
			}

			m_CellSize = CellSizer.LastCalculatedSize;
		}


		public float GetCellWidth()
		{
			var space = rectTransform.rect.width
						- base.padding.horizontal
						- base.constraintCount * base.spacing.x;

			return space / constraintCount;
		}

		public float GetCellHeight()
		{
			var space = rectTransform.rect.height
						- base.padding.vertical
						- base.constraintCount * base.spacing.y;

			return space / constraintCount;
		}


		private void ApplySettings(Settings settings)
		{
			if (settingsFallback == null)
				return;

			m_Constraint = settings.Constraint;
			m_ConstraintCount = settings.ConstraintCount;
			m_ChildAlignment = settings.ChildAlignment;
			m_StartAxis = settings.StartAxis;
			m_StartCorner = settings.StartCorner;
			fit = settings.Fit;
		}

		private void Set<T>(T value, Action<T> setBase, Action<Settings, T> setSettings)
		{
			Config.Set(value, setBase, o => setSettings(CurrentSettings, value));
			CalculateCellSize();
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