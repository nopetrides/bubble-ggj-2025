using UnityEngine;

namespace TheraBytes.BetterUi
{
	public static class RectTransformExtensions
	{
		public static Rect ToScreenRect(this RectTransform self, bool startAtBottom = false, Canvas canvas = null,
			bool localTransform = false)
		{
			var corners = new Vector3[4];
			var screenCorners = new Vector3[2];

			if (localTransform)
				self.GetLocalCorners(corners);
			else
				self.GetWorldCorners(corners);

			var idx1 = startAtBottom ? 0 : 1;
			var idx2 = startAtBottom ? 2 : 3;


			if (canvas != null && (canvas.renderMode == RenderMode.ScreenSpaceCamera ||
									canvas.renderMode == RenderMode.WorldSpace))
			{
				screenCorners[0] = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, corners[idx1]);
				screenCorners[1] = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, corners[idx2]);
			}
			else
			{
				screenCorners[0] = RectTransformUtility.WorldToScreenPoint(null, corners[idx1]);
				screenCorners[1] = RectTransformUtility.WorldToScreenPoint(null, corners[idx2]);
			}

			if (!startAtBottom)
			{
				screenCorners[0].y = Screen.height - screenCorners[0].y;
				screenCorners[1].y = Screen.height - screenCorners[1].y;
			}

			return new Rect(screenCorners[0], screenCorners[1] - screenCorners[0]);
		}
	}
}