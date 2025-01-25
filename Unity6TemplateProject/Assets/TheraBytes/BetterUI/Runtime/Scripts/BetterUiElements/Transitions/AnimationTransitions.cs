using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

#pragma warning disable 0649 // disable "never assigned" warnings

namespace TheraBytes.BetterUi
{
	[Serializable]
	public class AnimationTransitions : TransitionStateCollection<string>
	{
		[SerializeField] private Animator target;

		[SerializeField] private List<AnimationTransitionState> states = new();


		public AnimationTransitions(params string[] stateNames)
			: base(stateNames)
		{
		}


		public override Object Target => target;

		protected override void ApplyState(TransitionState state, bool instant)
		{
			if (Target == null
				|| !target.isActiveAndEnabled
				|| target.runtimeAnimatorController == null
				|| string.IsNullOrEmpty(state.StateObject))
				return;

			foreach (var s in states) target.ResetTrigger(s.StateObject);

			target.SetTrigger(state.StateObject);
		}

		internal override void AddStateObject(string stateName)
		{
			var obj = new AnimationTransitionState(stateName, null);
			states.Add(obj);
		}

		protected override IEnumerable<TransitionState> GetTransitionStates()
		{
			foreach (var s in states)
				yield return s;
		}

		internal override void SortStates(string[] sortedOrder)
		{
			SortStatesLogic(states, sortedOrder);
		}

		[Serializable]
		public class AnimationTransitionState : TransitionState
		{
			public AnimationTransitionState(string name, string stateObject)
				: base(name, stateObject)
			{
			}
		}
	}
}