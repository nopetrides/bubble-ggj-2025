using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheraBytes.BetterUi
{
#if UNITY_2018_3_OR_NEWER
	[ExecuteAlways]
#else
    [ExecuteInEditMode]
#endif
	public abstract class ResolutionSizer<T> : UIBehaviour, ILayoutController, ILayoutSelfController,
		IResolutionDependency
	{
		protected abstract ScreenDependentSize<T> sizer { get; }

		public virtual void SetLayoutHorizontal()
		{
			UpdateSize();
		}

		public virtual void SetLayoutVertical()
		{
			UpdateSize();
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			UpdateSize();
		}

#if UNITY_EDITOR
		protected override void OnValidate()
		{
			SetDirty();
		}
#endif

		protected void SetDirty()
		{
			if (!isActiveAndEnabled) return;

			UpdateSize();
		}

		protected override void OnRectTransformDimensionsChange()
		{
			base.OnRectTransformDimensionsChange();
			UpdateSize();
		}

		private void UpdateSize()
		{
			if (!isActiveAndEnabled) return;

			var newSize = sizer.CalculateSize(this);
			ApplySize(newSize);
		}

		protected abstract void ApplySize(T newSize);

		public void OnResolutionChanged()
		{
			UpdateSize();
		}
	}
}