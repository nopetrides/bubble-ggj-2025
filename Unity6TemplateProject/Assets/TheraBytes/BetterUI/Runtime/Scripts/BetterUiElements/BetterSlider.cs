using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TheraBytes.BetterUi
{
	[HelpURL("https://documentation.therabytes.de/better-ui/BetterSlider.html")]
	[AddComponentMenu("Better UI/Controls/Better Slider", 30)]
	public class BetterSlider : Slider, IBetterTransitionUiElement
	{
		[SerializeField] [DefaultTransitionStates]
		private List<Transitions> betterTransitions = new();

		public List<Transitions> BetterTransitions => betterTransitions;

		protected override void DoStateTransition(SelectionState state, bool instant)
		{
			base.DoStateTransition(state, instant);

			if (!gameObject.activeInHierarchy)
				return;

			foreach (var info in betterTransitions) info.SetState(state.ToString(), instant);
		}
	}
}