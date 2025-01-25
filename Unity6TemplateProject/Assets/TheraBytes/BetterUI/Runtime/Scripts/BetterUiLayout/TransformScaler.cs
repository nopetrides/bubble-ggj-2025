using UnityEngine;
using UnityEngine.Serialization;

namespace TheraBytes.BetterUi
{
#if UNITY_2018_3_OR_NEWER
	[ExecuteAlways]
#else
    [ExecuteInEditMode]
#endif
	[HelpURL("https://documentation.therabytes.de/better-ui/TransformScaler.html")]
	[AddComponentMenu("Better UI/Helpers/Transform Scaler", 30)]
	public class TransformScaler : ResolutionSizer<Vector3>
	{
		public Vector3SizeModifier ScaleSizer => customScaleSizers.GetCurrentItem(scaleSizerFallback);

		protected override ScreenDependentSize<Vector3> sizer => customScaleSizers.GetCurrentItem(scaleSizerFallback);

		[FormerlySerializedAs("scaleSizer")] [SerializeField]
		private Vector3SizeModifier scaleSizerFallback = new(Vector3.one, Vector3.zero, 4 * Vector3.one);

		[SerializeField] private Vector3SizeConfigCollection customScaleSizers = new();

		private DrivenRectTransformTracker rectTransformTracker;

		protected override void OnDisable()
		{
			base.OnDisable();
			rectTransformTracker.Clear();
		}

		protected override void ApplySize(Vector3 newSize)
		{
			rectTransformTracker.Clear();

			var rt = transform as RectTransform;
			if (rt != null)
			{
				rectTransformTracker.Clear();
				rectTransformTracker.Add(this, rt, DrivenTransformProperties.Scale);
			}

			transform.localScale = newSize;
		}
	}
}