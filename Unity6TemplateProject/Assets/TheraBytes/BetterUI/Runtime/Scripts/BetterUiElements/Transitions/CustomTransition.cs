using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

#pragma warning disable 0649 // disable "never assigned" warnings

namespace TheraBytes.BetterUi
{
	[Serializable]
	public class CustomTransitions : TransitionStateCollection<UnityEvent>
	{
		[SerializeField] private List<CustomTransitionState> states = new();

		public CustomTransitions(params string[] stateNames)
			: base(stateNames)
		{
		}

		public override Object Target => null;

		protected override void ApplyState(TransitionState state, bool instant)
		{
			state.StateObject.Invoke();
		}

		internal override void AddStateObject(string stateName)
		{
			var obj = new CustomTransitionState(stateName);
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
		public class CustomTransitionState : TransitionState
		{
			public CustomTransitionState(string name)
				: base(name, new UnityEvent())
			{
			}
		}
	}
}