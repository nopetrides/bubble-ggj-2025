using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace TheraBytes.BetterUi
{
#if UNITY_2018_3_OR_NEWER
	[ExecuteAlways]
#else
    [ExecuteInEditMode]
#endif
	[HelpURL("https://documentation.therabytes.de/better-ui/BetterTextMeshProUGUI.html")]
	[AddComponentMenu("Better UI/TextMeshPro/Better TextMeshPro Text", 30)]
	public class BetterTextMeshProUGUI : TextMeshProUGUI, IResolutionDependency
	{
		public BetterText.FittingMode Fitting
		{
			get => fitting;
			set
			{
				if (fitting == value)
					return;

				fitting = value;
				CalculateSize();
			}
		}

		public MarginSizeModifier MarginSizer => customMarginSizers.GetCurrentItem(marginSizerFallback);

		public FloatSizeModifier FontSizer => customFontSizers.GetCurrentItem(fontSizerFallback);
		public FloatSizeModifier MinFontSizer => customMinFontSizers.GetCurrentItem(minFontSizerFallback);
		public FloatSizeModifier MaxFontSizer => customMaxFontSizers.GetCurrentItem(maxFontSizerFallback);

		public bool IgnoreFontSizerOptions { get; set; }

		public new float fontSize
		{
			get => base.fontSize;
			set { Config.Set(value, o => base.fontSize = o, o => FontSizer.SetSize(this, o)); }
		}

		public new float fontSizeMin
		{
			get => base.fontSizeMin;
			set { Config.Set(value, o => base.fontSizeMin = o, o => MinFontSizer.SetSize(this, o)); }
		}

		public new float fontSizeMax
		{
			get => base.fontSizeMax;
			set { Config.Set(value, o => base.fontSizeMax = o, o => MaxFontSizer.SetSize(this, o)); }
		}

		public new Vector4 margin
		{
			get => base.margin;
			set { Config.Set(value, o => base.margin = o, o => MarginSizer.SetSize(this, new Margin(o))); }
		}

		[SerializeField] private BetterText.FittingMode fitting;

		[FormerlySerializedAs("marginSizer")] [SerializeField]
		private MarginSizeModifier marginSizerFallback =
			new(new Margin(), new Margin(), new Margin(1000, 1000, 1000, 1000));

		[SerializeField] private MarginSizeConfigCollection customMarginSizers = new();


		[FormerlySerializedAs("fontSizer")] [SerializeField]
		private FloatSizeModifier fontSizerFallback = new(36, 10, 500);

		[SerializeField] private FloatSizeConfigCollection customFontSizers = new();


		[FormerlySerializedAs("minFontSizer")] [SerializeField]
		private FloatSizeModifier minFontSizerFallback = new(10, 10, 500);

		[SerializeField] private FloatSizeConfigCollection customMinFontSizers = new();


		[FormerlySerializedAs("maxFontSizer")] [SerializeField]
		private FloatSizeModifier maxFontSizerFallback = new(500, 500, 500);

		[SerializeField] private FloatSizeConfigCollection customMaxFontSizers = new();

		protected override void OnEnable()
		{
			CalculateSize();
			base.OnEnable();
		}

		public void OnResolutionChanged()
		{
			CalculateSize();
		}

		protected override void OnRectTransformDimensionsChange()
		{
			base.OnRectTransformDimensionsChange();
			CalculateSize();
		}


		public void CalculateSize()
		{
			if (IgnoreFontSizerOptions)
				enableAutoSizing = false;
			else
				switch (fitting)
				{
					case BetterText.FittingMode.SizerOnly:

						enableAutoSizing = false;
						base.fontSize = FontSizer.CalculateSize(this);
						break;

					case BetterText.FittingMode.StayInBounds:

						enableAutoSizing = true;
						base.fontSizeMin = MinFontSizer.CalculateSize(this);
						base.fontSizeMax = FontSizer.CalculateSize(this);
						break;

					case BetterText.FittingMode.BestFit:

						enableAutoSizing = true;
						base.fontSizeMin = MinFontSizer.CalculateSize(this);
						base.fontSizeMax = MaxFontSizer.CalculateSize(this);
						break;
				}

			base.margin = MarginSizer.CalculateSize(this).ToVector4();
		}

		public void RegisterMaterials(Material[] materials)
		{
			base.GetMaterials(materials);
		}

#if UNITY_EDITOR
		protected override void OnValidate()
		{
			CalculateSize();
			base.OnValidate();
		}
#endif
	}
}