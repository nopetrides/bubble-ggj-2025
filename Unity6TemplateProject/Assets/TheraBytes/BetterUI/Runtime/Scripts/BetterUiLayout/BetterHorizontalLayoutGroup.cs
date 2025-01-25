using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TheraBytes.BetterUi
{
	[HelpURL("https://documentation.therabytes.de/better-ui/BetterHorizontalLayoutGroup.html")]
	[Obsolete("Beter use BetterAxisAlignedLayoutGroup.")]
	[AddComponentMenu("Better UI/Obsolete/Better Horizontal Layout Group", 30)]
	public class BetterHorizontalLayoutGroup
		: HorizontalLayoutGroup, IBetterHorizontalOrVerticalLayoutGroup, IResolutionDependency
	{
		[FormerlySerializedAs("paddingSizer")] [SerializeField]
		private MarginSizeModifier paddingSizerFallback =
			new(new Margin(), new Margin(), new Margin(1000, 1000, 1000, 1000));

		[FormerlySerializedAs("spacingSizer")] [SerializeField]
		private FloatSizeModifier spacingSizerFallback = new(0, 0, 300);

		protected override void OnEnable()
		{
			base.OnEnable();
			CalculateCellSize();
		}

#if UNITY_EDITOR
		protected override void OnValidate()
		{
			CalculateCellSize();
			base.OnValidate();
		}
#endif
		public MarginSizeModifier PaddingSizer => paddingSizerFallback;
		public FloatSizeModifier SpacingSizer => spacingSizerFallback;

		public void OnResolutionChanged()
		{
			CalculateCellSize();
		}

		public void CalculateCellSize()
		{
			var r = rectTransform.rect;
			if (r.width == float.NaN || r.height == float.NaN)
				return;

			m_Spacing = SpacingSizer.CalculateSize(this);

			var pad = PaddingSizer.CalculateSize(this);
			pad.CopyValuesTo(m_Padding);
		}
	}
}