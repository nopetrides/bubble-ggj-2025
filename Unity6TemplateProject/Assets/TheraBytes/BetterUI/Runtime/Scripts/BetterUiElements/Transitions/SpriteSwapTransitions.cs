using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

#pragma warning disable 0649 // disable "never assigned" warnings

namespace TheraBytes.BetterUi
{
	[Serializable]
	public class SpriteSwapTransitions : TransitionStateCollection<Sprite>
	{
		[SerializeField] private Image target;

		[SerializeField] private List<SpriteSwapTransitionState> states = new();


		public SpriteSwapTransitions(params string[] stateNames)
			: base(stateNames)
		{
		}


		public override Object Target => target;

		protected override void ApplyState(TransitionState state, bool instant)
		{
			if (Target == null)
				return;

			target.overrideSprite = state.StateObject;
		}

		internal override void AddStateObject(string stateName)
		{
			var obj = new SpriteSwapTransitionState(stateName, null);
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
		public class SpriteSwapTransitionState : TransitionState
		{
			public SpriteSwapTransitionState(string name, Sprite stateObject)
				: base(name, stateObject)
			{
			}
		}
	}
}