using System;
using UnityEngine;

namespace TheraBytes.BetterUi
{
	[Serializable]
	public class ScreenInfo
	{
		[SerializeField] private Vector2 resolution = new(1980, 1020);

		[SerializeField] private float dpi = 96;

		public ScreenInfo()
		{
		}

		public ScreenInfo(Vector2 resolution, float dpi)
		{
			this.resolution = resolution;
			this.dpi = dpi;
		}

		public Vector2 Resolution
		{
			get => resolution;
			set => resolution = value;
		}

		public float Dpi
		{
			get => dpi;
			set => dpi = value;
		}
	}
}