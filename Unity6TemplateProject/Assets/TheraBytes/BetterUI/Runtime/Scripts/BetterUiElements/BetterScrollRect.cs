using UnityEngine;
using UnityEngine.UI;

namespace TheraBytes.BetterUi
{
	[HelpURL("https://documentation.therabytes.de/better-ui/BetterScrollRect.html")]
	[AddComponentMenu("Better UI/Controls/Better Scroll Rect", 30)]
	public class BetterScrollRect : ScrollRect, IResolutionDependency
	{
		[SerializeField] [Range(0, 1)] private float horizontalStartPosition;

		[SerializeField] [Range(0, 1)] private float verticalStartPosition = 1;

		[SerializeField] private FloatSizeModifier horizontalSpacingFallback = new(-3, -500, 500);

		[SerializeField] private FloatSizeConfigCollection customHorizontalSpacingSizers = new();


		[SerializeField] private FloatSizeModifier verticalSpacingFallback = new(-3, -500, 500);

		[SerializeField] private FloatSizeConfigCollection customVerticalSpacingSizers = new();

		public float HorizontalStartPosition
		{
			get => horizontalStartPosition;
			set => horizontalStartPosition = value;
		}

		public float VerticalStartPosition
		{
			get => verticalStartPosition;
			set => verticalStartPosition = value;
		}

		public new float horizontalScrollbarSpacing
		{
			get => base.horizontalScrollbarSpacing;
			set
			{
				Config.Set(value, o => base.horizontalScrollbarSpacing = o,
					o => HorizontalSpacingSizer.SetSize(this, o));
			}
		}

		public new float verticalScrollbarSpacing
		{
			get => base.verticalScrollbarSpacing;
			set
			{
				Config.Set(value, o => base.verticalScrollbarSpacing = o, o => VerticalSpacingSizer.SetSize(this, o));
			}
		}

		public FloatSizeModifier HorizontalSpacingSizer =>
			customHorizontalSpacingSizers.GetCurrentItem(horizontalSpacingFallback);

		public FloatSizeModifier VerticalSpacingSizer =>
			customVerticalSpacingSizers.GetCurrentItem(verticalSpacingFallback);


		protected override void Start()
		{
			base.Start();

			if (Application.isPlaying) ResetToStartPosition();
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			CalculateSize();
		}


#if UNITY_EDITOR
		protected override void OnValidate()
		{
			CalculateSize();
			base.OnValidate();
		}
#endif

		public void OnResolutionChanged()
		{
			CalculateSize();
		}

		public void ResetToStartPosition()
		{
			if (horizontalScrollbar != null)
				horizontalScrollbar.value = horizontalStartPosition;
			else if (horizontal) horizontalNormalizedPosition = horizontalStartPosition;

			if (verticalScrollbar != null)
				verticalScrollbar.value = verticalStartPosition;
			else if (vertical) verticalNormalizedPosition = verticalStartPosition;
		}

		private void CalculateSize()
		{
			base.horizontalScrollbarSpacing = HorizontalSpacingSizer.CalculateSize(this);
			base.verticalScrollbarSpacing = VerticalSpacingSizer.CalculateSize(this);
		}
#if UNITY_5_5_OR_NEWER
        /// <summary>
        ///     Exposes the "m_ContentStartPosition" variable which is used as reference position during drag.
        ///     It is a variable of the base ScrollRect class which is not accessible by default.
        ///     Use the setter at your own risk.
        /// </summary>
        public Vector2 DragStartPosition
		{
			get => m_ContentStartPosition;
			set => m_ContentStartPosition = value;
		}

        /// <summary>
        ///     Exposes the "m_ContentBounds" variable which is used to evaluate the size of the content.
        ///     It is a variable of the base ScrollRect class which is not accessible by default.
        ///     Use ther setter at your own risk.
        /// </summary>
        public Bounds ContentBounds
		{
			get => m_ContentBounds;
			set => m_ContentBounds = value;
		}
#endif
	}
}