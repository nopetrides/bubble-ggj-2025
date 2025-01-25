using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace TheraBytes.BetterUi
{
	[HelpURL("https://documentation.therabytes.de/better-ui/BetterToggle.html")]
	[AddComponentMenu("Better UI/Controls/Better Toggle", 30)]
	public class BetterToggle : Toggle, IBetterTransitionUiElement
	{
		[SerializeField] [DefaultTransitionStates]
		private List<Transitions> betterTransitions = new();

		[SerializeField] [TransitionStates("On", "Off")]
		private List<Transitions> betterToggleTransitions = new();

		[SerializeField] [DefaultTransitionStates]
		private List<Transitions> betterTransitionsWhenOn = new();

		[SerializeField] [DefaultTransitionStates]
		private List<Transitions> betterTransitionsWhenOff = new();

		private bool wasOn;
		public List<Transitions> BetterTransitionsWhenOn => betterTransitionsWhenOn;
		public List<Transitions> BetterTransitionsWhenOff => betterTransitionsWhenOff;
		public List<Transitions> BetterToggleTransitions => betterToggleTransitions;

		private void Update()
		{
			if (wasOn != isOn) ValueChanged(isOn);
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			ValueChanged(isOn, true);
			DoStateTransition(SelectionState.Normal, true);
		}

		public List<Transitions> BetterTransitions => betterTransitions;

		protected override void DoStateTransition(SelectionState state, bool instant)
		{
			base.DoStateTransition(state, instant);

			if (!gameObject.activeInHierarchy)
				return;

			var stateTransitions = isOn
				? betterTransitionsWhenOn
				: betterTransitionsWhenOff;

			foreach (var info in stateTransitions) info.SetState(state.ToString(), instant);

			foreach (var info in betterTransitions)
			{
				if (state != SelectionState.Disabled && isOn)
				{
					var tglTr = betterToggleTransitions.FirstOrDefault(
						o => o.TransitionStates != null && info.TransitionStates != null
														&& o.TransitionStates.Target == info.TransitionStates.Target
														&& o.Mode == info.Mode);

					if (tglTr != null) continue;
				}

				info.SetState(state.ToString(), instant);
			}
		}

		private void ValueChanged(bool on)
		{
			ValueChanged(on, false);
		}

		private void ValueChanged(bool on, bool immediate)
		{
			wasOn = on;
			foreach (var state in betterToggleTransitions) state.SetState(on ? "On" : "Off", immediate);

			var stateTransitions = on
				? betterTransitionsWhenOn
				: betterTransitionsWhenOff;

			foreach (var state in stateTransitions) state.SetState(currentSelectionState.ToString(), immediate);
		}
	}
}