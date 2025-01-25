using System;
using UnityEngine;

namespace TheraBytes.BetterUi
{
	[Serializable]
	public class IsCertainScreenOrientation : IScreenTypeCheck
	{
		public enum Orientation
		{
			Portrait,
			Landscape
		}

		[SerializeField] private Orientation expectedOrientation;

		[SerializeField] private bool isActive;

		public IsCertainScreenOrientation(Orientation expectedOrientation)
		{
			this.expectedOrientation = expectedOrientation;
		}

		public Orientation ExpectedOrientation
		{
			get => expectedOrientation;
			set => expectedOrientation = value;
		}

		public bool IsActive
		{
			get => isActive;
			set => isActive = value;
		}

		public bool IsScreenType()
		{
			var res = ResolutionMonitor.CurrentResolution;

			switch (expectedOrientation)
			{
				case Orientation.Portrait:
					return res.x < res.y;

				case Orientation.Landscape:
					return res.x >= res.y;

				default:
					throw new NotImplementedException();
			}
		}
	}
}