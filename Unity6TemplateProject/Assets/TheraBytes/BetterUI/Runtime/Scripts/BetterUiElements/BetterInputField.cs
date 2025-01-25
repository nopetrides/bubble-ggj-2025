using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheraBytes.BetterUi
{
	[HelpURL("https://documentation.therabytes.de/better-ui/BetterInputField.html")]
	[AddComponentMenu("Better UI/Controls/Better Input Field", 30)]
	public class BetterInputField : InputField, IBetterTransitionUiElement
	{
		[SerializeField] [DefaultTransitionStates]
		private List<Transitions> betterTransitions = new();

		[SerializeField] private List<Graphic> additionalPlaceholders = new();

		public List<Graphic> AdditionalPlaceholders => additionalPlaceholders;
		public List<Transitions> BetterTransitions => betterTransitions;

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
	}
}