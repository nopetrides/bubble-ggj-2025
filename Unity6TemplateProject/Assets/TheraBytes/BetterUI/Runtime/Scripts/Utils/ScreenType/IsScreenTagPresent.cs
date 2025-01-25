using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheraBytes.BetterUi
{
	[Serializable]
	public class IsScreenTagPresent : IScreenTypeCheck
	{
		[SerializeField] private string screenTag;

		[SerializeField] private bool isActive;

		public string ScreenTag
		{
			get => screenTag;
			set => screenTag = value;
		}

		public bool IsActive
		{
			get => isActive;
			set => isActive = value;
		}

		public bool IsScreenType()
		{
			var curentTags = ResolutionMonitor.CurrentScreenTags as HashSet<string>;
			return curentTags.Contains(screenTag);
		}
	}
}