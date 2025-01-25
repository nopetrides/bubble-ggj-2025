using System.Collections.Generic;
using TMPro;
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
	[HelpURL("https://documentation.therabytes.de/better-ui/BetterTextMeshPro-InputField.html")]
	[AddComponentMenu("Better UI/TextMeshPro/Better TextMeshPro - Input Field", 30)]
	public class BetterTextMeshProInputField : TMP_InputField, IBetterTransitionUiElement, IResolutionDependency
	{
		public List<Transitions> BetterTransitions => betterTransitions;
		public List<Graphic> AdditionalPlaceholders => additionalPlaceholders;
		public FloatSizeModifier PointSizeScaler => pointSizeScaler;

		public bool OverridePointSizeSettings
		{
			get => overridePointSize;
			set => overridePointSize = value;
		}

		[SerializeField] [DefaultTransitionStates]
		private List<Transitions> betterTransitions = new();

		[SerializeField] private List<Graphic> additionalPlaceholders = new();

		[SerializeField] private FloatSizeModifier pointSizeScaler = new(36, 10, 500);

		[SerializeField] private bool overridePointSize;

		public new float pointSize
		{
			get => base.pointSize;
			set { Config.Set(value, o => base.pointSize = o, o => PointSizeScaler.SetSize(this, o)); }
		}

		protected override void DoStateTransition(SelectionState state, bool instant)
		{
			base.DoStateTransition(state, instant);

			if (!gameObject.activeInHierarchy)
				return;

			foreach (var info in betterTransitions) info.SetState(state.ToString(), instant);
		}

		public override void OnUpdateSelected(BaseEventData eventData)
		{
			base.OnUpdateSelected(eventData);
			DisplayPlaceholders(text);
		}

		private void DisplayPlaceholders(string input)
		{
			var show = string.IsNullOrEmpty(input);

			if (Application.isPlaying)
				foreach (var ph in additionalPlaceholders)
					ph.enabled = show;
		}

		protected override void OnEnable()
		{
			CalculateSize();
			base.OnEnable();
		}

		protected override void OnRectTransformDimensionsChange()
		{
			base.OnRectTransformDimensionsChange();
			CalculateSize();
		}

		public void OnResolutionChanged()
		{
			CalculateSize();
		}

		public void CalculateSize()
		{
			if (overridePointSize)
				base.pointSize = pointSizeScaler.CalculateSize(this);

			OverrideBetterTextMeshSize(m_Placeholder as BetterTextMeshProUGUI, pointSize);
			OverrideBetterTextMeshSize(m_TextComponent as BetterTextMeshProUGUI, pointSize);

			foreach (var p in additionalPlaceholders) OverrideBetterTextMeshSize(p as BetterTextMeshProUGUI, pointSize);
		}

		private void OverrideBetterTextMeshSize(BetterTextMeshProUGUI better, float size)
		{
			if (better == null)
				return;

			better.IgnoreFontSizerOptions = overridePointSize;

			if (overridePointSize)
			{
				better.FontSizer.OverrideLastCalculatedSize(size);
				better.fontSize = size;
			}
			else
			{
				better.FontSizer.CalculateSize(this);
			}
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