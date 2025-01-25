using System;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace TheraBytes.BetterUi
{
	[Serializable]
	public class Transitions
	{
		public enum TransitionMode
		{
			None = 0,
			ColorTint = 1,
			SpriteSwap = 2,
			Animation = 3,
			ObjectActiveness = 4,
			Alpha = 5,
			MaterialProperty = 6,
			Color32Tint = 7,
			LocationAnimationTransition = 8,
			CustomCallback = 9
		}

		public static readonly string[] OnOffStateNames = { "On", "Off" };
		public static readonly string[] ShowHideStateNames = { "Show", "Hide" };

		public static readonly string[] SelectionStateNames =
		{
			"Normal", "Highlighted", "Pressed",
#if UNITY_2019_1_OR_NEWER
			"Selected",
#endif
			"Disabled"
		};

		[SerializeField] private TransitionMode mode;


		[SerializeField] private string[] stateNames;

		[SerializeField] private ColorTransitions colorTransitions;

		[SerializeField] private Color32Transitions color32Transitions;

		[SerializeField] private SpriteSwapTransitions spriteSwapTransitions;

		[SerializeField] private AnimationTransitions animationTransitions;

		[SerializeField] private ObjectActivenessTransitions activenessTransitions;

		[SerializeField] private AlphaTransitions alphaTransitions;

		[SerializeField] private MaterialPropertyTransition materialPropertyTransitions;

		[SerializeField] private LocationAnimationTransitions locationAnimationTransitions;

		[SerializeField] private CustomTransitions customTransitions;

		public Transitions(params string[] stateNames)
		{
			this.stateNames = stateNames;
		}

		public TransitionMode Mode => mode;
		public ReadOnlyCollection<string> StateNames => stateNames.ToList().AsReadOnly();


		public TransitionStateCollection TransitionStates
		{
			get
			{
				switch (mode)
				{
					case TransitionMode.ColorTint: return colorTransitions;
					case TransitionMode.Color32Tint: return color32Transitions;
					case TransitionMode.SpriteSwap: return spriteSwapTransitions;
					case TransitionMode.Animation: return animationTransitions;
					case TransitionMode.ObjectActiveness: return activenessTransitions;
					case TransitionMode.Alpha: return alphaTransitions;
					case TransitionMode.MaterialProperty: return materialPropertyTransitions;
					case TransitionMode.LocationAnimationTransition: return locationAnimationTransitions;
					case TransitionMode.CustomCallback: return customTransitions;
					default: return null;
				}
			}
		}

		public void SetState(string stateName, bool instant)
		{
			if (TransitionStates == null)
				return;

			if (!stateNames.Contains(stateName))
				return;

			TransitionStates.Apply(stateName, instant);
		}

		public void SetMode(TransitionMode mode)
		{
			this.mode = mode;

			colorTransitions = null;
			color32Transitions = null;
			spriteSwapTransitions = null;
			animationTransitions = null;
			activenessTransitions = null;
			alphaTransitions = null;
			locationAnimationTransitions = null;
			customTransitions = null;

			switch (mode)
			{
				case TransitionMode.None:
					break;
				case TransitionMode.ColorTint:
					colorTransitions = new ColorTransitions(stateNames);
					break;
				case TransitionMode.Color32Tint:
					color32Transitions = new Color32Transitions(stateNames);
					break;
				case TransitionMode.SpriteSwap:
					spriteSwapTransitions = new SpriteSwapTransitions(stateNames);
					break;
				case TransitionMode.Animation:
					animationTransitions = new AnimationTransitions(stateNames);
					break;
				case TransitionMode.ObjectActiveness:
					activenessTransitions = new ObjectActivenessTransitions(stateNames);
					break;
				case TransitionMode.Alpha:
					alphaTransitions = new AlphaTransitions(stateNames);
					break;
				case TransitionMode.MaterialProperty:
					materialPropertyTransitions = new MaterialPropertyTransition(stateNames);
					break;
				case TransitionMode.LocationAnimationTransition:
					locationAnimationTransitions = new LocationAnimationTransitions(stateNames);
					break;
				case TransitionMode.CustomCallback:
					customTransitions = new CustomTransitions(stateNames);
					break;

				default: throw new NotImplementedException();
			}
		}

		public void ComplementStateNames(string[] stateNames)
		{
			foreach (var name in stateNames)
			{
				if (this.stateNames.Contains(name))
					continue;

				switch (mode)
				{
					case TransitionMode.None:
						break;
					case TransitionMode.ColorTint:
						colorTransitions.AddStateObject(name);
						colorTransitions.SortStates(stateNames);
						break;
					case TransitionMode.Color32Tint:
						color32Transitions.AddStateObject(name);
						color32Transitions.SortStates(stateNames);
						break;
					case TransitionMode.SpriteSwap:
						spriteSwapTransitions.AddStateObject(name);
						spriteSwapTransitions.SortStates(stateNames);
						break;
					case TransitionMode.Animation:
						animationTransitions.AddStateObject(name);
						animationTransitions.SortStates(stateNames);
						break;
					case TransitionMode.ObjectActiveness:
						activenessTransitions.AddStateObject(name);
						activenessTransitions.SortStates(stateNames);
						break;
					case TransitionMode.Alpha:
						alphaTransitions.AddStateObject(name);
						alphaTransitions.SortStates(stateNames);
						break;
					case TransitionMode.MaterialProperty:
						materialPropertyTransitions.AddStateObject(name);
						materialPropertyTransitions.SortStates(stateNames);
						break;
					case TransitionMode.LocationAnimationTransition:
						locationAnimationTransitions.AddStateObject(name);
						locationAnimationTransitions.SortStates(stateNames);
						break;
					case TransitionMode.CustomCallback:
						customTransitions.AddStateObject(name);
						customTransitions.SortStates(stateNames);
						break;

					default: throw new NotImplementedException();
				}
			}

			this.stateNames = stateNames;
		}
	}
}