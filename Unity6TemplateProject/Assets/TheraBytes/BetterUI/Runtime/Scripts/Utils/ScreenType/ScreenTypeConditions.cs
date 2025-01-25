using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TheraBytes.BetterUi
{
	[Serializable]
	public class ScreenTypeConditions
	{
		[SerializeField] private string name = "Screen";

		[SerializeField] private IsCertainScreenOrientation checkOrientation;

		[SerializeField] private IsScreenOfCertainSize checkScreenSize;

		[SerializeField] private IsCertainAspectRatio checkAspectRatio;

		[SerializeField] private IsScreenOfCertainDeviceInfo checkDeviceType;

		[SerializeField] private IsScreenTagPresent checkScreenTag;

		[SerializeField] private ScreenInfo optimizedScreenInfo;

		[SerializeField] private List<string> fallbacks = new();

		public ScreenTypeConditions(string displayName, params Type[] enabledByDefault)
		{
			name = displayName;
			optimizedScreenInfo = new ScreenInfo(new Vector2(1920, 1080), 96);
			EnsureScreenConditions(enabledByDefault);
		}

		public string Name
		{
			get => name;
			set => name = value;
		}

		public bool IsActive { get; private set; }

		public List<string> Fallbacks => fallbacks;

		public Vector2 OptimizedResolution => optimizedScreenInfo != null
			? optimizedScreenInfo.Resolution
			: ResolutionMonitor.OptimizedResolutionFallback;

		public int OptimizedWidth => (int)OptimizedResolution.x;
		public int OptimizedHeight => (int)OptimizedResolution.y;

		public float OptimizedDpi => optimizedScreenInfo != null
			? optimizedScreenInfo.Dpi
			: ResolutionMonitor.OptimizedDpiFallback;

		public IsCertainScreenOrientation CheckOrientation => checkOrientation;

		public IsScreenOfCertainSize CheckScreenSize => checkScreenSize;

		public IsCertainAspectRatio CheckAspectRatio => checkAspectRatio;

		public IsScreenOfCertainDeviceInfo CheckDeviceType => checkDeviceType;

		public IsScreenTagPresent CheckScreenTag => checkScreenTag;


		public ScreenInfo OptimizedScreenInfo => optimizedScreenInfo;

		private void EnsureScreenConditions(params Type[] enabledByDefault)
		{
			EnsureScreenCondition(ref checkOrientation,
				() => new IsCertainScreenOrientation(IsCertainScreenOrientation.Orientation.Landscape),
				enabledByDefault);
			EnsureScreenCondition(ref checkScreenSize, () => new IsScreenOfCertainSize(), enabledByDefault);
			EnsureScreenCondition(ref checkAspectRatio, () => new IsCertainAspectRatio(), enabledByDefault);
			EnsureScreenCondition(ref checkDeviceType, () => new IsScreenOfCertainDeviceInfo(), enabledByDefault);
			EnsureScreenCondition(ref checkScreenTag, () => new IsScreenTagPresent(), enabledByDefault);
		}

		private void EnsureScreenCondition<T>(ref T screenCondition, Func<T> instantiatoMethod, Type[] enabledTypes)
			where T : IIsActive
		{
			if (screenCondition != null)
				return;

			screenCondition = instantiatoMethod();
			screenCondition.IsActive = enabledTypes.Contains(typeof(T));
		}

		public bool IsScreenType()
		{
			EnsureScreenConditions();

			IsActive = (!checkOrientation.IsActive || checkOrientation.IsScreenType())
						&& (!checkScreenSize.IsActive || checkScreenSize.IsScreenType())
						&& (!checkAspectRatio.IsActive || checkAspectRatio.IsScreenType())
						&& (!checkDeviceType.IsActive || checkDeviceType.IsScreenType())
						&& (!checkScreenTag.IsActive || checkScreenTag.IsScreenType());

			return IsActive;
		}
	}
}