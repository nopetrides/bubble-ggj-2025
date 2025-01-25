using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

#pragma warning disable 0649 // disable "never assigned" warnings

namespace TheraBytes.BetterUi
{
	[Serializable]
	public class Color32Transitions : TransitionStateCollection<Color32>
	{
		[SerializeField] private Graphic target;

		[SerializeField] private float fadeDuration = 0.1f;

		[SerializeField] private List<Color32TransitionState> states = new();


		public Color32Transitions(params string[] stateNames)
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

			target.CrossFadeColor(state.StateObject, instant ? 0f : fadeDuration, true, true);
		}

		internal override void AddStateObject(string stateName)
		{
			var obj = new Color32TransitionState(stateName, new Color32(255, 255, 255, 255));
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
		public class Color32TransitionState : TransitionState
		{
			public Color32TransitionState(string name, Color32 stateObject)
				: base(name, stateObject)
			{
			}
		}
	}
}