using System;
using UnityEngine;

namespace TheraBytes.BetterUi
{
	[Serializable]
	public class IsCertainAspectRatio : IScreenTypeCheck
	{
		[SerializeField] private float minAspect = 0.66f;

		[SerializeField] private float maxAspect = 1.5f;

		[SerializeField] private bool inverse;

		[SerializeField] private bool isActive;

		public float MinAspect
		{
			get => minAspect;
			set => minAspect = value;
		}

		public float MaxAspect
		{
			get => maxAspect;
			set => maxAspect = value;
		}

		public bool Inverse
		{
			get => inverse;
			set => inverse = value;
		}

		public bool IsActive
		{
			get => isActive;
			set => isActive = value;
		}

		public bool IsScreenType()
		{
			var realAspect = (float)Screen.width / Screen.height;

			return (!inverse
					&& realAspect >= minAspect
					&& realAspect <= maxAspect)
					|| (inverse
						&& realAspect < minAspect
						&& realAspect > maxAspect);
		}
	}
}