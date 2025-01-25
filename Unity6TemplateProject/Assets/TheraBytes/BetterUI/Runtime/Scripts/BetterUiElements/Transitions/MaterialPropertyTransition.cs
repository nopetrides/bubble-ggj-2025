using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

#pragma warning disable 0649 // disable "never assigned" warnings

namespace TheraBytes.BetterUi
{
	[Serializable]
	public class MaterialPropertyTransition : TransitionStateCollection<float>
	{
		private static Dictionary<MaterialPropertyTransition, Coroutine> activeCoroutines = new();
		private static List<MaterialPropertyTransition> keysToRemove = new();

		[SerializeField] private BetterImage target;

		[SerializeField] private float fadeDuration = 0.1f;

		[SerializeField] private List<MaterialPropertyTransitionState> states = new();

		[SerializeField] private int propertyIndex;


		public MaterialPropertyTransition(params string[] stateNames)
			: base(stateNames)
		{
		}

		public override Object Target => target;

		public float FadeDurtaion
		{
			get => fadeDuration;
			set => fadeDuration = value;
		}

		public int PropertyIndex
		{
			get => propertyIndex;
			set => propertyIndex = value;
		}

		protected override void ApplyState(TransitionState state, bool instant)
		{
			if (Target == null)
				return;

			if (!Application.isPlaying) instant = true;

			var start = target.GetMaterialPropertyValue(propertyIndex);
			CrossFadeProperty(start, state.StateObject, instant ? 0 : fadeDuration);
		}

		internal override void AddStateObject(string stateName)
		{
			var obj = new MaterialPropertyTransitionState(stateName, 1f);
			states.Add(obj);
		}

		protected override IEnumerable<TransitionState> GetTransitionStates()
		{
			foreach (var s in states)
				yield return s;
		}

		private void CrossFadeProperty(float startValue, float targetValue, float duration)
		{
			// Stop clashing coroutines
			foreach (var key in activeCoroutines.Keys)
				if (key.target == target && key.propertyIndex == propertyIndex)
				{
					if (key.target != null)
						key.target.StopCoroutine(activeCoroutines[key]);

					keysToRemove.Add(key);
				}

			foreach (var key in keysToRemove) activeCoroutines.Remove(key);

			keysToRemove.Clear();

			// trigger value changes
			if (duration == 0 || !target.enabled || !target.gameObject.activeInHierarchy)
			{
				target.SetMaterialProperty(propertyIndex, targetValue);
			}
			else
			{
				var coroutine = target.StartCoroutine(CoCrossFadeProperty(startValue, targetValue, duration));
				activeCoroutines.Add(this, coroutine);
			}
		}

		private IEnumerator CoCrossFadeProperty(float startValue, float targetValue, float duration)
		{
			// animate
			var startTime = Time.unscaledTime;
			var endTime = startTime + duration;

			while (Time.unscaledTime < endTime)
			{
				var amount = (Time.unscaledTime - startTime) / duration;
				var value = Mathf.Lerp(startValue, targetValue, amount);
				target.SetMaterialProperty(propertyIndex, value);
				yield return null;
			}

			target.SetMaterialProperty(propertyIndex, targetValue);
		}

		internal override void SortStates(string[] sortedOrder)
		{
			SortStatesLogic(states, sortedOrder);
		}

		[Serializable]
		public class MaterialPropertyTransitionState : TransitionState
		{
			public MaterialPropertyTransitionState(string name, float stateObject)
				: base(name, stateObject)
			{
			}
		}
	}
}