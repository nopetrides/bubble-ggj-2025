using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TheraBytes.BetterUi
{
	[HelpURL("https://documentation.therabytes.de/better-ui/BetterTextMeshPro-Dropdown.html")]
	[AddComponentMenu("Better UI/TextMeshPro/Better TextMeshPro - Dropdown", 30)]
	public class BetterTextMeshProDropdown : TMP_Dropdown, IBetterTransitionUiElement
	{
		[SerializeField] [DefaultTransitionStates]
		private List<Transitions> betterTransitions = new();

		[SerializeField] [TransitionStates("Show", "Hide")]
		private List<Transitions> showHideTransitions = new();

		public List<Transitions> ShowHideTransitions => showHideTransitions;
		public List<Transitions> BetterTransitions => betterTransitions;

		protected override void DoStateTransition(SelectionState state, bool instant)
		{
			base.DoStateTransition(state, instant);

			if (!gameObject.activeInHierarchy)
				return;

			foreach (var info in betterTransitions) info.SetState(state.ToString(), instant);
		}

		protected override GameObject CreateDropdownList(GameObject template)
		{
			foreach (var tr in showHideTransitions) tr.SetState("Show", false);

			return base.CreateDropdownList(template);
		}

		protected override void DestroyDropdownList(GameObject dropdownList)
		{
			foreach (var tr in showHideTransitions) tr.SetState("Hide", false);

			base.DestroyDropdownList(dropdownList);
		}
	}
}