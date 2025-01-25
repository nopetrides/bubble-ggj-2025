using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

#pragma warning disable 0649 // disable "never assigned" warnings
namespace TheraBytes.BetterUi
{
	[HelpURL("https://documentation.therabytes.de/better-ui/LocationAnimations.html")]
	[RequireComponent(typeof(RectTransform))]
	[AddComponentMenu("Better UI/Animation/Location Animations", 30)]
	public class LocationAnimations : MonoBehaviour
	{
		[SerializeField] private bool useRelativeLocations;

		[SerializeField] private List<LocationData> locations = new();

		[SerializeField] private List<Animation> animations = new();


		[SerializeField] private string startLocation;

		[SerializeField] private string startUpAnimation;

		[SerializeField] private LocationAnimationEvent actionOnInit;

		private RectTransformData referenceLocation;

		public RectTransform RectTransform => transform as RectTransform;

		public List<LocationData> Locations => locations;
		public List<Animation> Animations => animations;

		public string StartUpAnimation
		{
			get => startUpAnimation;
			set => startUpAnimation = value;
		}

		public string StartLocation
		{
			get => startLocation;
			set => startLocation = value;
		}

		public bool IsAnimating => RunningAnimation != null;

		public bool UseRelativeLocations
		{
			get { return useRelativeLocations; }
#if UNITY_EDITOR
			set { useRelativeLocations = value; }
#endif
		}

		public AnimationState RunningAnimation { get; private set; }

		public RectTransformData ReferenceLocation
		{
			get
			{
				EnsureReferenceLocation(true);
				return referenceLocation;
			}
		}

		private void Start()
		{
			ResetReferenceLocation();
			SetToLocation(startLocation);

			actionOnInit.Invoke();

			StartAnimation(startUpAnimation);
		}

		private void Update()
		{
			UpdateCurrentAnimation(Time.unscaledDeltaTime);
		}

#if UNITY_EDITOR
		public void OnValidate()
		{
			for (var i = 0; i < animations.Count; i++)
			{
				var ani = animations[i];

				if (ani.Curve == null || ani.Curve.keys.Length < 2)
					ani.Curve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
			}

			for (var i = 0; i < locations.Count; i++)
			{
				var loc = locations[i];
				var cur = loc.CurrentTransformData;

				if (cur == RectTransformData.Invalid)
				{
					cur.PullFromTransform(RectTransform);

					if (useRelativeLocations)
					{
						EnsureReferenceLocation();
						cur.PullFromData(RectTransformData.Separate(cur, referenceLocation));
					}
				}
			}
		}
#endif

		public void StopCurrentAnimation()
		{
			RunningAnimation = null;
		}

		public void StartAnimation(string name)
		{
			StartAnimation(GetAnimation(name), null, null);
		}

		public void StartAnimation(string name, float timeScale)
		{
			StartAnimation(GetAnimation(name), timeScale, null);
		}

		public void StartAnimation(string name, LocationAnimationEvent onFinish)
		{
			StartAnimation(GetAnimation(name), null, onFinish);
		}

		public void StartAnimation(string name, float timeScale, LocationAnimationEvent onFinish)
		{
			StartAnimation(GetAnimation(name), timeScale, onFinish);
		}

		public void StartAnimation(Animation ani, float? timeScale, LocationAnimationEvent onFinish)
		{
			if (ani == null || ani.To == null || (RunningAnimation != null && ani == RunningAnimation.Animation))
				return;

			if (RunningAnimation != null) StopCurrentAnimation();

			if (ani.Curve == null || ani.Curve.keys.Length <= 1)
			{
				SetToLocation(ani.To);
				return;
			}

			var speed = timeScale ?? ani.TimeScale;

			var forever =
				(speed > 0
				&& (ani.Curve.postWrapMode == WrapMode.Loop
					|| ani.Curve.postWrapMode == WrapMode.PingPong))
				|| (speed < 0
					&& (ani.Curve.preWrapMode == WrapMode.Loop
						|| ani.Curve.preWrapMode == WrapMode.PingPong));

			RunningAnimation = new AnimationState
			{
				Animation = ani,
				From = GetLocationTransformFallbackCurrent(ani.From),
				To = GetLocationTransformFallbackCurrent(ani.To),
				ActionAfterFinish = onFinish ?? ani.ActionAfterFinish,
				Duration = ani.Curve.keys[ani.Curve.keys.Length - 1].time,
				Loop = forever,
				TimeScale = speed,
				Time = 0
			};

			ani.ActionBeforeStart.Invoke();
		}

		public void UpdateCurrentAnimation(float deltaTime)
		{
			if (RunningAnimation == null || RunningAnimation.Animation == null ||
				RunningAnimation.Animation.Curve == null || RunningAnimation.Animation.Curve.length == 0)
				return;

			var animationTimeIsOver = !RunningAnimation.Loop && RunningAnimation.Time >= RunningAnimation.Duration;

			if (animationTimeIsOver)
				RunningAnimation.Time = RunningAnimation.Duration;

			var amount = RunningAnimation.Animation.Curve.Evaluate(RunningAnimation.Time);
			var rtd = RectTransformData.LerpUnclamped(RunningAnimation.From, RunningAnimation.To, amount,
				RunningAnimation.Animation.AnimateWithEulerRotation);
			rtd.PushToTransform(RectTransform);

			RunningAnimation.Animation.ActionOnUpdating.Invoke(amount);

			RunningAnimation.Time += deltaTime * RunningAnimation.TimeScale;
			if (animationTimeIsOver)
			{
				var cache = RunningAnimation;
				RunningAnimation = null;
				cache.ActionAfterFinish.Invoke();
			}
		}

		public void SetToLocation(string name)
		{
			var loc = GetLocation(name);
			if (loc == null)
				return;

			PushTransformData(loc);
		}

		public LocationData GetLocation(string name)
		{
			return locations.FirstOrDefault(o => o.Name == name);
		}

		public void ResetReferenceLocation()
		{
			ResetReferenceLocation(RectTransform);
		}

		public void ResetReferenceLocation(RectTransform rectTransform)
		{
			ResetReferenceLocation(new RectTransformData(rectTransform));
		}

		public void ResetReferenceLocation(RectTransformData reference)
		{
			referenceLocation = reference;
		}

		private void PushTransformData(LocationData loc)
		{
			if (useRelativeLocations)
			{
				EnsureReferenceLocation();

				var cur = loc.CurrentTransformData;
				var transformData = RectTransformData.Combine(cur, referenceLocation);
				transformData.PushToTransform(RectTransform);
			}
			else
			{
				loc.CurrentTransformData.PushToTransform(RectTransform);
			}
		}

		private void EnsureReferenceLocation(bool force = false)
		{
			if (referenceLocation == null
				|| ((force || useRelativeLocations) && referenceLocation == RectTransformData.Invalid))
				ResetReferenceLocation();
		}

		private RectTransformData GetLocationTransformFallbackCurrent(string name)
		{
			EnsureReferenceLocation();

			var loc = locations.FirstOrDefault(o => o.Name == name);
			var cur = loc == null
				? new RectTransformData(RectTransform)
				: loc.CurrentTransformData;

			var result = useRelativeLocations && loc != null
				? RectTransformData.Combine(cur, referenceLocation)
				: cur;

			result.SaveRotationAsEuler = true;

			return result;
		}

		public Animation GetAnimation(string name)
		{
			return animations.FirstOrDefault(o => o.Name == name);
		}

#region Nested types

		[Serializable]
		public class LocationAnimationEvent : UnityEvent
		{
			public LocationAnimationEvent()
			{
			}

			public LocationAnimationEvent(params UnityAction[] actions)
			{
				foreach (var act in actions) AddListener(act);
			}
		}

		[Serializable]
		public class LocationAnimationUpdateEvent : UnityEvent<float>
		{
			public LocationAnimationUpdateEvent()
			{
			}

			public LocationAnimationUpdateEvent(params UnityAction<float>[] actions)
			{
				foreach (var act in actions) AddListener(act);
			}
		}

		[Serializable]
		public class RectTransformDataConfigCollection : SizeConfigCollection<RectTransformData>
		{
		}

		[Serializable]
		public class LocationData
		{
			[SerializeField] private string name;

			[SerializeField] private RectTransformData transformFallback = new();

			[SerializeField] private RectTransformDataConfigCollection transformConfigs = new();

			public string Name
			{
				get => name;
				internal set => name = value;
			}

			public RectTransformData CurrentTransformData => transformConfigs.GetCurrentItem(transformFallback);
		}

		[Serializable]
		public class Animation
		{
			[SerializeField] private string name;

			[SerializeField] private string from;

			[SerializeField] private string to;

			[SerializeField] private AnimationCurve curve;

			[SerializeField] private LocationAnimationEvent actionBeforeStart = new();

			[SerializeField] private LocationAnimationEvent actionAfterFinish = new();

			[SerializeField] private LocationAnimationUpdateEvent actionOnUpdating = new();

			[SerializeField] private bool animateWithEulerRotation = true;

			[SerializeField] private float timeScale = 1;

			public string Name
			{
				get => name;
				internal set => name = value;
			}

			public string From
			{
				get => from;
				set => from = value;
			}

			public string To
			{
				get => to;
				set => to = value;
			}

			// advanced
			public AnimationCurve Curve
			{
				get => curve;
				internal set => curve = value;
			}

			public bool AnimateWithEulerRotation
			{
				get => animateWithEulerRotation;
				set => animateWithEulerRotation = value;
			}

			public float TimeScale
			{
				get => timeScale;
				set => timeScale = value;
			}

			public LocationAnimationEvent ActionBeforeStart => actionBeforeStart;
			public LocationAnimationEvent ActionAfterFinish => actionAfterFinish;
			public LocationAnimationUpdateEvent ActionOnUpdating => actionOnUpdating;
		}

		[Serializable]
		public class AnimationState
		{
			public Animation Animation { get; internal set; }
			public RectTransformData From { get; internal set; }
			public RectTransformData To { get; internal set; }
			public float Time { get; set; }
			public float Duration { get; set; }
			public bool Loop { get; internal set; }
			public float TimeScale { get; set; }
			public LocationAnimationEvent ActionAfterFinish { get; internal set; }
		}

#endregion
	}
}

#pragma warning restore 0649