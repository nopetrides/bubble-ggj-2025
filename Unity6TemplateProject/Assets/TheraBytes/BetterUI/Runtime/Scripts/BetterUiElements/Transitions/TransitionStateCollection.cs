using System;
using System.Collections.Generic;
using System.Linq;
using Object = UnityEngine.Object;

namespace TheraBytes.BetterUi
{
	//
	// GENERIC CLASS
	//
	public abstract class TransitionStateCollection<T> : TransitionStateCollection
	{
		protected TransitionStateCollection(string[] stateNames)
		{
			foreach (var name in stateNames) AddStateObject(name);
		}

		public IEnumerable<TransitionState> GetStates()
		{
			foreach (var s in GetTransitionStates()) yield return s;
		}

		public override void Apply(string stateName, bool instant)
		{
			var s = GetTransitionStates().FirstOrDefault(o => o.Name == stateName);
			if (s != null) ApplyState(s, instant);
		}

		protected abstract IEnumerable<TransitionState> GetTransitionStates();
		protected abstract void ApplyState(TransitionState state, bool instant);
		internal abstract void AddStateObject(string stateName);

		[Serializable]
		public abstract class TransitionState : TransitionStateBase
		{
			public T StateObject;

			public TransitionState(string name, T stateObject)
				: base(name)
			{
				StateObject = stateObject;
			}
		}
	}

	//
	// NON GENERIC CLASS
	//
	[Serializable]
	public abstract class TransitionStateCollection
	{
		public abstract Object Target { get; }

		public abstract void Apply(string stateName, bool instant);

		internal abstract void SortStates(string[] sortedOrder);


		protected void SortStatesLogic<T>(List<T> states, string[] sortedOrder)
			where T : TransitionStateBase
		{
			states.Sort((a, b) =>
			{
				var idxA = -1;
				var idxB = -1;

				for (var i = 0; i < sortedOrder.Length; i++)
				{
					if (sortedOrder[i] == a.Name)
						idxA = i;

					if (sortedOrder[i] == b.Name)
						idxB = i;
				}

				return idxA.CompareTo(idxB);
			});
		}

		[Serializable]
		public abstract class TransitionStateBase
		{
			public string Name;

			public TransitionStateBase(string name)
			{
				Name = name;
			}
		}
	}
}