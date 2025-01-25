using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

#pragma warning disable 0649 // disable "never assigned" warnings

namespace TheraBytes.BetterUi
{
	[Serializable]
	public class AlphaTransitions : TransitionStateCollection<float>
	{
		[SerializeField] private Graphic target;

		[SerializeField] private float fadeDuration = 0.1f;

		[SerializeField] private List<AlphaTransitionState> states = new();


		public AlphaTransitions(params string[] stateNames)
			: base(stateNames)
		{
		}

		public override Object Target => target;

		public float FadeDurtaion
		{
			get => fadeDuration;
			set => fadeDuration = value;
		}

		protected override void ApplyState(TransitionState state, bool instant)
		{
			if (Target == null)
				return;

			if (!Application.isPlaying) instant = true;

			target.CrossFadeAlpha(state.StateObject, instant ? 0f : fadeDuration, true);
		}

		internal override void AddStateObject(string stateName)
		{
			var obj = new AlphaTransitionState(stateName, 1f);
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
		public class AlphaTransitionState : TransitionState
		{
			public AlphaTransitionState(string name, float stateObject)
				: base(name, stateObject)
			{
			}
		}
	}
}