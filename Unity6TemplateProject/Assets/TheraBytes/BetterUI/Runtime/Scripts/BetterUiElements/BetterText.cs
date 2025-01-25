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
	[HelpURL("https://documentation.therabytes.de/better-ui/BetterText.html")]
	[AddComponentMenu("Better UI/Controls/Better Text", 30)]
	public class BetterText : Text, IResolutionDependency
	{
		public enum FittingMode
		{
			SizerOnly,
			StayInBounds,
			BestFit
		}

		public FloatSizeModifier FontSizer => customFontSizers.GetCurrentItem(fontSizerFallback);

		public FittingMode Fitting
		{
			get => fitting;
			set
			{
				fitting = value;
				CalculateSize();
			}
		}

		public new float fontSize
		{
			get => base.fontSize;
			set { Config.Set(value, o => base.fontSize = Mathf.RoundToInt(o), o => FontSizer.SetSize(this, o)); }
		}

		[SerializeField] private FittingMode fitting = FittingMode.StayInBounds;

		[FormerlySerializedAs("fontSizer")] [SerializeField]
		private FloatSizeModifier fontSizerFallback = new(40, 0, 500);

		[SerializeField] private FloatSizeConfigCollection customFontSizers = new();

		private bool isCalculatingSize;

		protected override void OnEnable()
		{
			base.OnEnable();
			CalculateSize();
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

		public override void SetVerticesDirty()
		{
			base.SetVerticesDirty();
			CalculateSize();
		}

		private void CalculateSize()
		{
			if (isCalculatingSize)
				return;

			isCalculatingSize = true;

			switch (fitting)
			{
				case FittingMode.SizerOnly:

					resizeTextForBestFit = false;
					base.fontSize = Mathf.RoundToInt(FontSizer.CalculateSize(this));
					break;

				case FittingMode.StayInBounds:

					resizeTextMinSize = Mathf.RoundToInt(FontSizer.MinSize);
					resizeTextMaxSize = Mathf.RoundToInt(FontSizer.MaxSize);
					resizeTextForBestFit = true;
					var size = Mathf.RoundToInt(FontSizer.CalculateSize(this));

					base.fontSize = size;
					base.Rebuild(CanvasUpdate.PreRender);

					var bestFit = cachedTextGenerator.fontSizeUsedForBestFit;
					resizeTextForBestFit = false;

					fontSize = bestFit < size ? bestFit : size;
					FontSizer.OverrideLastCalculatedSize(base.fontSize);

					break;

				case FittingMode.BestFit:

					resizeTextMinSize = Mathf.RoundToInt(FontSizer.MinSize);
					resizeTextMaxSize = Mathf.RoundToInt(FontSizer.MaxSize);
					resizeTextForBestFit = true;

					base.Rebuild(CanvasUpdate.PreRender);

					FontSizer.OverrideLastCalculatedSize(cachedTextGenerator.fontSizeUsedForBestFit);
					break;
			}

			isCalculatingSize = false;
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