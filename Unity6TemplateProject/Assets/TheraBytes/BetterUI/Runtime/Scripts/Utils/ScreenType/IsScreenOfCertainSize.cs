using System;
using UnityEngine;

namespace TheraBytes.BetterUi
{
	[Serializable]
	public class IsScreenOfCertainSize : IScreenTypeCheck
	{
		public enum ScreenMeasure
		{
			Width,
			Height,
			Diagonal
		}

		public enum UnitType
		{
			Inches,
			Centimeters
		}

		public const float DEFAULT_SMALL_THRESHOLD = 4.7f;
		public const float DEFAULT_LARGE_THRESHOLD = 7.6f;

		[SerializeField] private ScreenMeasure measureType = ScreenMeasure.Height;

		[SerializeField] private UnitType unitType;

		[SerializeField] private float minSizeInInches = DEFAULT_SMALL_THRESHOLD;

		[SerializeField] private float maxSizeInInches = DEFAULT_LARGE_THRESHOLD;

		[SerializeField] private bool isActive;

		public IsScreenOfCertainSize()
		{
		}

		public IsScreenOfCertainSize(float minHeighInInches, float maxHeightInInches)
		{
			minSizeInInches = minHeighInInches;
			maxSizeInInches = maxHeightInInches;
		}

		public ScreenMeasure MeasureType
		{
			get => measureType;
			set => measureType = value;
		}

		public UnitType Units
		{
			get => unitType;
			set => unitType = value;
		}

		public float MinSize
		{
			get => unitType == UnitType.Centimeters ? 2.54f * minSizeInInches : minSizeInInches;
			set => minSizeInInches = unitType == UnitType.Centimeters ? value / 2.54f : value;
		}

		public float MaxSize
		{
			get => unitType == UnitType.Centimeters ? 2.54f * maxSizeInInches : maxSizeInInches;
			set => maxSizeInInches = unitType == UnitType.Centimeters ? value / 2.54f : value;
		}

		public bool IsActive
		{
			get => isActive;
			set => isActive = value;
		}

		public bool IsScreenType()
		{
			var res = ResolutionMonitor.CurrentResolution;
			var dpi = ResolutionMonitor.CurrentDpi;

			float size = 0;
			switch (measureType)
			{
				case ScreenMeasure.Width:
					size = res.x / dpi;
					break;
				case ScreenMeasure.Height:
					size = res.y / dpi;
					break;
				case ScreenMeasure.Diagonal:
					size = Mathf.Sqrt(res.x * res.x + res.y * res.y) / dpi;
					break;
				default:
					throw new NotImplementedException();
			}

			return size >= minSizeInInches
					&& size < maxSizeInInches;
		}
	}
}