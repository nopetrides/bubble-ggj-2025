using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

#pragma warning disable 0649 // disable "never assigned" warnings

namespace TheraBytes.BetterUi
{
	[Serializable]
	public class LocationAnimationTransitions : TransitionStateCollection<string>
	{
		[SerializeField] private LocationAnimations target;

		[SerializeField] private List<LocationAnimationTransitionState> states = new();


		public LocationAnimationTransitions(params string[] stateNames)
			: base(stateNames)
		{
		}


		public override Object Target => target;

		protected override void ApplyState(TransitionState state, bool instant)
		{
			if (Target == null)
				return;

			target.StartAnimation(state.StateObject);
		}

		internal override void AddStateObject(string stateName)
		{
			var obj = new LocationAnimationTransitionState(stateName, "");
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
		public class LocationAnimationTransitionState : TransitionState
		{
			public LocationAnimationTransitionState(string name, string stateObject)
				: base(name, stateObject)
			{
			}
		}
	}
}