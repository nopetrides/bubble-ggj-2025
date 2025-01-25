using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

#pragma warning disable 0649 // disable "never assigned" warnings

namespace TheraBytes.BetterUi
{
	[Serializable]
	public class ColorTransitions : TransitionStateCollection<Color>
	{
		public enum AffectedColor
		{
			ColorMixedIn = 0,

			MainColorDirect = 1,
			SecondColorDirect = 2
		}

		private static Dictionary<ColorTransitions, Coroutine> activeCoroutines = new();

		private static List<ColorTransitions> keysToRemove = new();


		[SerializeField] private Graphic target;

		[Range(1, 5)] [SerializeField] private float colorMultiplier = 1;

		[SerializeField] private float fadeDuration = 0.1f;

		[SerializeField] private AffectedColor affectedColor;

		[SerializeField] private List<ColorTransitionState> states = new();


		public ColorTransitions(params string[] stateNames)
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

			// Backwards compatibility: colorMultiplyer is a new field. 
			// It is 0 for upgrades from 1.x versions of Better UI.
			if (colorMultiplier <= float.Epsilon) colorMultiplier = 1;

			switch (affectedColor)
			{
				case AffectedColor.ColorMixedIn:
					target.CrossFadeColor(state.StateObject * colorMultiplier, instant ? 0f : fadeDuration, true, true);
					break;

				case AffectedColor.MainColorDirect:
					CrossFadeColor(target.color, state.StateObject * colorMultiplier, instant ? 0 : fadeDuration);
					break;

				case AffectedColor.SecondColorDirect:
					Color start;
					if (target is IImageAppearanceProvider img)
						start = img.SecondColor;
					else
						throw new NotSupportedException("SecondaryColor transition not suppoted for "
														+ target.GetType().Name);

					CrossFadeColor(start, state.StateObject * colorMultiplier, instant ? 0 : fadeDuration);
					break;


				default:
					throw new NotImplementedException();
			}
		}

		internal override void AddStateObject(string stateName)
		{
			var obj = new ColorTransitionState(stateName, Color.white);
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

		private void CrossFadeColor(Color startValue, Color targetValue, float duration)
		{
			// Stop clashing coroutines
			foreach (var key in activeCoroutines.Keys)
				if (key.target == target && key.affectedColor == affectedColor)
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
				ApplyColor(targetValue);
			}
			else
			{
				var coroutine = target.StartCoroutine(CoCrossFadeColorDirect(startValue, targetValue, duration));
				activeCoroutines.Add(this, coroutine);
			}
		}

		private IEnumerator CoCrossFadeColorDirect(Color startValue, Color targetValue, float duration)
		{
			// animate
			var startTime = Time.unscaledTime;
			var endTime = startTime + duration;

			while (Time.unscaledTime < endTime)
			{
				var amount = (Time.unscaledTime - startTime) / duration;
				var value = Color.Lerp(startValue, targetValue, amount);
				ApplyColor(value);
				yield return null;
			}

			ApplyColor(targetValue);
		}


		private void ApplyColor(Color color)
		{
			if (target is IImageAppearanceProvider img)
				switch (affectedColor)
				{
					case AffectedColor.MainColorDirect:
						img.color = color;
						break;
					case AffectedColor.SecondColorDirect:
						img.SecondColor = color;
						break;
					default: throw new ArgumentException("MainColorDirect or GradientSecondColor expected.");
				}
			else if (affectedColor == AffectedColor.MainColorDirect)
				target.color = color;
			else
				throw new ArgumentException("affected object doesn't have a secondary color.");
		}

		[Serializable]
		public class ColorTransitionState : TransitionState
		{
			public ColorTransitionState(string name, Color stateObject)
				: base(name, stateObject)
			{
			}
		}
	}
}