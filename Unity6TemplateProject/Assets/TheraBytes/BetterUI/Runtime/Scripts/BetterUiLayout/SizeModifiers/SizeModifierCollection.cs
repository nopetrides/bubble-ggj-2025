using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TheraBytes.BetterUi
{
	[Serializable]
	public class SizeModifierCollection
	{
		public float ExponentialScale = 1;


		public List<SizeModifier> SizeModifiers = new() { new() };

		public SizeModifierCollection(params SizeModifier[] mods)
			: this(1, mods)
		{
		}

		public SizeModifierCollection(float exponentialScale, params SizeModifier[] mods)
		{
			//this.screenConfig = screenConfig;
			ExponentialScale = exponentialScale;
			SizeModifiers = mods.ToList();
		}

		public float CalculateFactor(Component caller, string screenConfig)
		{
			var parentOverride = caller.GetComponentInParent<OverrideScreenProperties>();

			var info = parentOverride != null
				? parentOverride.OptimizedOverride
				: ResolutionMonitor.GetOpimizedScreenInfo(screenConfig);

			var optimizedRes = info.Resolution;
			var optimizedDpi = info.Dpi;

			var actualRes = parentOverride != null
				? parentOverride.CurrentSize.Resolution
				: ResolutionMonitor.CurrentResolution;

			var actualDpi = parentOverride != null
				? parentOverride.CurrentSize.Dpi
				: ResolutionMonitor.CurrentDpi;


			float factor = 0;

			var max = SizeModifiers.Count > 0 ? SizeModifiers.Max(o => o.Impact) : 1;
			float scale = 0;

			foreach (var entry in SizeModifiers)
			{
				factor += entry.GetFactor(caller, optimizedRes, actualRes, optimizedDpi, actualDpi);
				scale += entry.Impact / max;
			}

			if (factor == 0) // nothing in list
			{
				factor = 1;
			}
			else
			{
				factor /= scale;
				factor = 1 - factor;
			}

			if (ExponentialScale != 1) factor = Mathf.Pow(factor, ExponentialScale);

			return factor;
		}

		public SizeModifierCollection Clone()
		{
			var clone = new SizeModifierCollection();

			CopyTo(clone);

			return Clone();
		}

		public void CopyTo(SizeModifierCollection other)
		{
			other.SizeModifiers.Clear();

			other.ExponentialScale = ExponentialScale;

			foreach (var mod in SizeModifiers) other.SizeModifiers.Add(new SizeModifier(mod.Mode, mod.Impact));
		}

		[Serializable]
		public class SizeModifier
		{
			public ImpactMode Mode;

			[Range(0, 1)] public float Impact = 1;

			public SizeModifier()
			{
			}

			public SizeModifier(ImpactMode mode, float impact)
			{
				Mode = mode;
				Impact = impact;
			}

			internal float GetFactor(Component caller, Vector2 optimizedResolution, Vector2 actualResolution,
				float optimizedDpi, float actualDpi)
			{
				float result;
				switch (Mode)
				{
					case ImpactMode.PixelWidth:
						result = CalculateSize(optimizedResolution.x, actualResolution.x, Impact);
						break;
					case ImpactMode.PixelHeight:
						result = CalculateSize(optimizedResolution.y, actualResolution.y, Impact);
						break;
					case ImpactMode.AspectRatio:
					{
						var optRatio = optimizedResolution.x / optimizedResolution.y;
						var actRatio = actualResolution.x / actualResolution.y;
						result = CalculateSize(optRatio, actRatio, Impact);
					}
						break;
					case ImpactMode.InverseAspectRatio:
					{
						var optRatio = optimizedResolution.y / optimizedResolution.x;
						var actRatio = actualResolution.y / actualResolution.x;
						result = CalculateSize(optRatio, actRatio, Impact);
					}
						break;
					case ImpactMode.Dpi:
						result = CalculateSize(optimizedDpi, actualDpi, Impact);
						break;

					case ImpactMode.StaticMethod1:
					case ImpactMode.StaticMethod2:
					case ImpactMode.StaticMethod3:
					case ImpactMode.StaticMethod4:
					case ImpactMode.StaticMethod5:
						result = 1 - ResolutionMonitor.InvokeStaticMethod(Mode, caller, optimizedResolution,
							actualResolution, optimizedDpi, actualDpi);
						break;

					default:
						result = 0;

						break;
				}

				return result;
			}

			private float CalculateSize(float optimizedValue, float actualValue, float impact)
			{
				if (impact == 0 || optimizedValue == actualValue)
					return 0;

				var val = actualValue / optimizedValue;
				return 1 - Mathf.Lerp(1, val, impact);
			}
		}
	}
}