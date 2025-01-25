using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace TheraBytes.BetterUi
{
	[HelpURL("https://documentation.therabytes.de/better-ui/ValueDragger.html")]
	[AddComponentMenu("Better UI/Controls/Value Dragger", 30)]
	public class ValueDragger : BetterSelectable, IDragHandler, IBeginDragHandler, IPointerClickHandler
	{
		[SerializeField] private DragSettings fallbackDragSettings = new();

		[SerializeField] private DragSettingsConfigCollection customDragSettings = new();

		[SerializeField] private ValueSettings fallbackValueSettings = new();

		[SerializeField] private ValueSettingsConfigCollection customValueSettings = new();

		[SerializeField] private FloatSizeModifier fallbackDragDistance = new(1, float.Epsilon, 10000);

		[SerializeField] private FloatSizeConfigCollection customDragDistance = new();

		[SerializeField] private float value;

		[SerializeField] private ValueDragEvent onValueChanged = new();

		private float internalValue;

		public DragSettings CurrentDragSettings => customDragSettings.GetCurrentItem(fallbackDragSettings);

		public ValueSettings CurrentValueSettings => customValueSettings.GetCurrentItem(fallbackValueSettings);

		public FloatSizeModifier CurrentDragDistanceSizer => customDragDistance.GetCurrentItem(fallbackDragDistance);

		public float Value
		{
			get => value;
			set => ApplyValue(value);
		}

		public ValueDragEvent OnValueChanged
		{
			get => onValueChanged;
			set => onValueChanged = value;
		}

		void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
		{
			internalValue = value;
			CurrentDragDistanceSizer.CalculateSize(this);
		}

		void IDragHandler.OnDrag(PointerEventData eventData)
		{
			var dragSettings = CurrentDragSettings;

			var axis = (int)dragSettings.Direction;
			var delta = eventData.delta[axis];
			var divisor = CurrentDragDistanceSizer.LastCalculatedSize;

			internalValue += dragSettings.Invert
				? -delta / divisor
				: delta / divisor;

			ApplyValue(internalValue);
		}

		void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
		{
			// consume the click by implementing this interface.
			// we don't want to let the click pass through to a control higher up in the hierarchy.
		}

		private void ApplyValue(float val)
		{
			var valueSettings = CurrentValueSettings;
			if (valueSettings.HasMinValue && val < valueSettings.MinValue)
				val = valueSettings.MinValue;
			else if (valueSettings.HasMaxValue && val > valueSettings.MaxValue) val = valueSettings.MaxValue;

			if (valueSettings.WholeNumbers) val = (int)val;

			if (val != value)
			{
				value = val;
				onValueChanged.Invoke(value);
			}
		}

#region Nested Types

		[Serializable]
		public class ValueDragEvent : UnityEvent<float>
		{
		}

		public enum DragDirection
		{
			Horizontal = 0, // maps to Vector2's X index
			Vertical = 1 // maps to Vector2's Y index
		}

		[Serializable]
		public class DragSettings : IScreenConfigConnection
		{
			public DragDirection Direction = DragDirection.Horizontal;
			public bool Invert;

			[SerializeField] private string screenConfigName;

			public string ScreenConfigName
			{
				get => screenConfigName;
				set => screenConfigName = value;
			}
		}

		[Serializable]
		public class DragSettingsConfigCollection : SizeConfigCollection<DragSettings>
		{
		}

		[Serializable]
		public class ValueSettings : IScreenConfigConnection
		{
			public bool HasMinValue;
			public float MinValue;

			public bool HasMaxValue;
			public float MaxValue = 1f;

			public bool WholeNumbers;

			[SerializeField] private string screenConfigName;

			public string ScreenConfigName
			{
				get => screenConfigName;
				set => screenConfigName = value;
			}
		}

		[Serializable]
		public class ValueSettingsConfigCollection : SizeConfigCollection<ValueSettings>
		{
		}

#endregion
	}
}