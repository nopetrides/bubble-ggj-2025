using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

#pragma warning disable 0649 // disable "never assigned" warnings

namespace TheraBytes.BetterUi
{
	[Serializable]
	public class ObjectActivenessTransitions : TransitionStateCollection<bool>
	{
		[SerializeField] private GameObject target;

		[SerializeField] private List<ActiveTransitionState> states = new();


		public ObjectActivenessTransitions(params string[] stateNames)
			: base(stateNames)
		{
		}


		public override Object Target => target;

		protected override void ApplyState(TransitionState state, bool instant)
		{
			if (Target == null)
				return;

			if (Application.isPlaying) target.SetActive(state.StateObject);
			//else
			//{
			//    Debug.LogWarning("Active State Transitions cannot be previewed outside play mode.");
			//}
		}

		internal override void AddStateObject(string stateName)
		{
			var obj = new ActiveTransitionState(stateName, true);
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
		public class ActiveTransitionState : TransitionState
		{
			public ActiveTransitionState(string name, bool stateObject)
				: base(name, stateObject)
			{
			}
		}
	}
}