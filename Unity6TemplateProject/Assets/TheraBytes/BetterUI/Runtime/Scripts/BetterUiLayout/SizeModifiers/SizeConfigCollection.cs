using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TheraBytes.BetterUi
{
	public interface ISizeConfigCollection
	{
		bool IsDirty { get; }
		string GetCurrentConfigName();
		void MarkDirty();
		void Sort();
	}

	[Serializable]
	public class SizeConfigCollection<T> : ISizeConfigCollection
		where T : class, IScreenConfigConnection
	{
		[SerializeField] private List<T> items = new();

		public IReadOnlyList<T> Items => items;
		public bool IsDirty { get; private set; } = true;

		public void Sort()
		{
			if (!IsDirty)
				return;

			var order = ResolutionMonitor.Instance.OptimizedScreens.Select(o => o.Name).ToList();
			items.Sort((a, b) => order.IndexOf(a.ScreenConfigName).CompareTo(order.IndexOf(b.ScreenConfigName)));

			IsDirty = false;
		}

		public string GetCurrentConfigName()
		{
			var result = GetCurrentItem(null);

			if (result != null)
				return result.ScreenConfigName;

			return null;
		}

		public void MarkDirty()
		{
			IsDirty = true;
		}

		public void AddItem(T item)
		{
			items.Add(item);
			MarkDirty();
		}

		public T GetItemForConfig(string configName, T fallback)
		{
			foreach (var itm in items)
				if (itm.ScreenConfigName == configName)
					return itm;

			return fallback;
		}

		public T GetCurrentItem(T fallback)
		{
			// if there is no config matching the screen
			if (ResolutionMonitor.CurrentScreenConfiguration == null)
				return fallback;

			Sort();
#if UNITY_EDITOR

			// simulation
			var config = ResolutionMonitor.SimulatedScreenConfig;
			if (config != null)
				if (Items.Any(o => o.ScreenConfigName == config.Name))
					return Items.First(o => o.ScreenConfigName == config.Name);
#endif

			// search for screen config
			foreach (var item in items)
			{
				if (string.IsNullOrEmpty(item.ScreenConfigName))
					return fallback;

				var c = ResolutionMonitor.GetConfig(item.ScreenConfigName);
				if (c != null && c.IsActive) return item;
			}

			// fallback logic
			foreach (var conf in ResolutionMonitor.GetCurrentScreenConfigurations())
			{
				foreach (var c in conf.Fallbacks)
				{
					var matchingItem = items.FirstOrDefault(o => o.ScreenConfigName == c);
					if (matchingItem != null)
						return matchingItem;
				}
			}

			// final fallback
			return fallback;
		}
	}
}