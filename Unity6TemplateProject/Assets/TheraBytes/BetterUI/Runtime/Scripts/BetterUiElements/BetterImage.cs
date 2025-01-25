using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Sprites;
using UnityEngine.UI;

namespace TheraBytes.BetterUi
{
	public enum ColorMode
	{
		Color,
		HorizontalGradient,
		VerticalGradient
	}

#if UNITY_2018_3_OR_NEWER
	[ExecuteAlways]
#else
    [ExecuteInEditMode]
#endif
	[HelpURL("https://documentation.therabytes.de/better-ui/BetterImage.html")]
	[AddComponentMenu("Better UI/Controls/Better Image", 30)]
	public class BetterImage : Image, IResolutionDependency, IImageAppearanceProvider
	{
		private static readonly Vector2[] vertScratch = new Vector2[4];
		private static readonly Vector2[] uvScratch = new Vector2[4];

#region Nested Types

		[Serializable]
		public class SpriteSettings : IScreenConfigConnection
		{
			public Sprite Sprite;
			public ColorMode ColorMode;
			public Color PrimaryColor;
			public Color SecondaryColor;

			[SerializeField] private string screenConfigName;


			public SpriteSettings(Sprite sprite, ColorMode colorMode, Color primary, Color secondary)
			{
				Sprite = sprite;
				ColorMode = colorMode;
				PrimaryColor = primary;
				SecondaryColor = secondary;
			}

			public string ScreenConfigName
			{
				get => screenConfigName;
				set => screenConfigName = value;
			}
		}

		[Serializable]
		public class SpriteSettingsConfigCollection : SizeConfigCollection<SpriteSettings>
		{
		}

#endregion

#if !UNITY_2019_1_OR_NEWER
        float multipliedPixelsPerUnit => base.pixelsPerUnit;
#endif

		public bool KeepBorderAspectRatio
		{
			get => keepBorderAspectRatio;
			set
			{
				keepBorderAspectRatio = value;
				SetVerticesDirty();
			}
		}

		public Vector2SizeModifier SpriteBorderScale => customBorderScales.GetCurrentItem(spriteBorderScaleFallback);

		public string MaterialType
		{
			get => materialType;
			set => ImageAppearanceProviderHelper.SetMaterialType(value, this, materialProperties, ref materialEffect,
				ref materialType);
		}

		public MaterialEffect MaterialEffect
		{
			get => materialEffect;
			set => ImageAppearanceProviderHelper.SetMaterialEffect(value, this, materialProperties, ref materialEffect,
				ref materialType);
		}

		public VertexMaterialData MaterialProperties => materialProperties;

		public ColorMode ColoringMode
		{
			get => colorMode;
			set
			{
				Config.Set(value, o => colorMode = value, o => CurrentSpriteSettings.ColorMode = value);
				SetVerticesDirty();
			}
		}

		public Color SecondColor
		{
			get => secondColor;
			set
			{
				Config.Set(value, o => secondColor = value, o => CurrentSpriteSettings.SecondaryColor = value);
				SetVerticesDirty();
			}
		}

		public override Color color
		{
			get => base.color;
			set { Config.Set(value, o => base.color = value, o => CurrentSpriteSettings.PrimaryColor = value); }
		}

		public new Sprite sprite
		{
			get => base.sprite;
			set { Config.Set(value, o => base.sprite = value, o => CurrentSpriteSettings.Sprite = value); }
		}

		[SerializeField] private ColorMode colorMode = ColorMode.Color;

		[SerializeField] private Color secondColor = Color.white;


		[SerializeField] private VertexMaterialData materialProperties = new();

		[SerializeField] private string materialType;

		[SerializeField] private MaterialEffect materialEffect;

		[SerializeField] private float materialProperty1, materialProperty2, materialProperty3;

		[SerializeField] private bool keepBorderAspectRatio;

		[FormerlySerializedAs("spriteBorderScale")] [SerializeField]
		private Vector2SizeModifier spriteBorderScaleFallback = new(Vector2.one, Vector2.zero, 3 * Vector2.one);

		[SerializeField] private Vector2SizeConfigCollection customBorderScales = new();

		[SerializeField] private SpriteSettings fallbackSpriteSettings;

		[SerializeField] private SpriteSettingsConfigCollection customSpriteSettings = new();

		public SpriteSettings CurrentSpriteSettings
		{
			get
			{
				DoValidation();
				return customSpriteSettings.GetCurrentItem(fallbackSpriteSettings);
			}
		}

		private Animator animator;

		private Sprite activeSprite => overrideSprite != null ? overrideSprite : sprite;

		protected override void OnEnable()
		{
			base.OnEnable();

			AssignSpriteSettings();

			animator = GetComponent<Animator>();

			if (MaterialProperties.FloatProperties != null)
			{
				if (MaterialProperties.FloatProperties.Length > 0)
					materialProperty1 = MaterialProperties.FloatProperties[0].Value;

				if (MaterialProperties.FloatProperties.Length > 1)
					materialProperty2 = MaterialProperties.FloatProperties[1].Value;

				if (MaterialProperties.FloatProperties.Length > 2)
					materialProperty3 = MaterialProperties.FloatProperties[2].Value;
			}
		}

		public float GetMaterialPropertyValue(int propertyIndex)
		{
			return ImageAppearanceProviderHelper.GetMaterialPropertyValue(propertyIndex,
				ref materialProperty1, ref materialProperty2, ref materialProperty3);
		}

		public void SetMaterialProperty(int propertyIndex, float value)
		{
			ImageAppearanceProviderHelper.SetMaterialProperty(propertyIndex, value, this, materialProperties,
				ref materialProperty1, ref materialProperty2, ref materialProperty3);
		}

		protected override void OnPopulateMesh(VertexHelper toFill)
		{
			if (animator != null
				&& MaterialProperties.FloatProperties != null
				&& Application.isPlaying == animator.isActiveAndEnabled)
			{
				if (MaterialProperties.FloatProperties.Length > 0)
					MaterialProperties.FloatProperties[0].Value = materialProperty1;

				if (MaterialProperties.FloatProperties.Length > 1)
					MaterialProperties.FloatProperties[1].Value = materialProperty2;

				if (MaterialProperties.FloatProperties.Length > 2)
					MaterialProperties.FloatProperties[2].Value = materialProperty3;
			}

			if (activeSprite == null)
			{
				GenerateSimpleSprite(toFill, false);
				return;
			}

			switch (type)
			{
				case Type.Simple:
					GenerateSimpleSprite(toFill, preserveAspect);
					break;

				case Type.Sliced:
					GenerateSlicedSprite(toFill);
					break;

				case Type.Tiled:
					GenerateTiledSprite(toFill);
					break;

				case Type.Filled:
				default:
					base.OnPopulateMesh(toFill);
					break;
			}
		}

#region Simple

		private void GenerateSimpleSprite(VertexHelper vh, bool preserveAspect)
		{
			var rect = GetDrawingRect(preserveAspect);
			var uv = activeSprite == null
				? Vector4.zero
				: DataUtility.GetOuterUV(activeSprite);

			vh.Clear();
			AddQuad(vh, rect,
				rect.min, rect.max,
				colorMode, color, secondColor,
				new Vector2(uv.x, uv.y), new Vector2(uv.z, uv.w));
		}

		private Rect GetDrawingRect(bool shouldPreserveAspect)
		{
			var padding = activeSprite == null ? Vector4.zero : DataUtility.GetPadding(activeSprite);
			var spriteSize = activeSprite == null
				? Vector2.zero
				: new Vector2(activeSprite.rect.width, activeSprite.rect.height);
			var rect = GetPixelAdjustedRect();

			if (activeSprite == null)
				return rect;

			float w = Mathf.RoundToInt(spriteSize.x);
			float h = Mathf.RoundToInt(spriteSize.y);
			var paddingFraction = new Vector4(
				padding.x / w,
				padding.y / h,
				(w - padding.z) / w,
				(h - padding.w) / h);

			if (shouldPreserveAspect && spriteSize.sqrMagnitude > 0f) PreserveSpriteAspectRatio(ref rect, spriteSize);

			var v = new Vector4(
				rect.x + rect.width * paddingFraction.x,
				rect.y + rect.height * paddingFraction.y,
				rect.x + rect.width * paddingFraction.z,
				rect.y + rect.height * paddingFraction.w);

			return new Rect(v.x, v.y, v.z - v.x, v.w - v.y);
		}

		private void PreserveSpriteAspectRatio(ref Rect rect, Vector2 spriteSize)
		{
			var spriteAspect = spriteSize.x / spriteSize.y;
			var rectAspect = rect.width / rect.height;
			if (spriteAspect > rectAspect)
			{
				var height = rect.height;
				rect.height = rect.width * (1f / spriteAspect);
				rect.y += (height - rect.height) * rectTransform.pivot.y;
			}
			else
			{
				var width = rect.width;
				rect.width = rect.height * spriteAspect;
				rect.x += (width - rect.width) * rectTransform.pivot.x;
			}
		}

#endregion

#region Sliced

		private void GenerateSlicedSprite(VertexHelper toFill)
		{
			if (!hasBorder)
			{
				base.OnPopulateMesh(toFill);
				return;
			}

			Vector4 outer, inner, padding, border;

			if (activeSprite != null)
			{
				outer = DataUtility.GetOuterUV(activeSprite);
				inner = DataUtility.GetInnerUV(activeSprite);
				padding = DataUtility.GetPadding(activeSprite);
				border = activeSprite.border;
			}
			else
			{
				outer = Vector4.zero;
				inner = Vector4.zero;
				padding = Vector4.zero;
				border = Vector4.zero;
			}

			var scale = SpriteBorderScale.CalculateSize(this) / multipliedPixelsPerUnit;
			var rect = GetPixelAdjustedRect();

			border = new Vector4(
				scale.x * border.x,
				scale.y * border.y,
				scale.x * border.z,
				scale.y * border.w);

			border = GetAdjustedBorders(border, rect, KeepBorderAspectRatio,
				new Vector2(
					activeSprite.rect.width * scale.x,
					activeSprite.rect.height * scale.y));


			if (border.x + border.z > rect.width)
			{
				var s = rect.width / (border.x + border.z);
				border.x *= s;
				border.z *= s;
			}

			if (border.y + border.w > rect.height)
			{
				var s = rect.height / (border.y + border.w);
				border.y *= s;
				border.w *= s;
			}


			padding = padding / multipliedPixelsPerUnit;

			vertScratch[0] = new Vector2(padding.x, padding.y);
			vertScratch[3] = new Vector2(rect.width - padding.z, rect.height - padding.w);

			vertScratch[1].x = border.x;
			vertScratch[1].y = border.y;
			vertScratch[2].x = rect.width - border.z;
			vertScratch[2].y = rect.height - border.w;

			for (var i = 0; i < 4; i++)
			{
				vertScratch[i].x += rect.x;
				vertScratch[i].y += rect.y;
			}

			uvScratch[0] = new Vector2(outer.x, outer.y);
			uvScratch[1] = new Vector2(inner.x, inner.y);
			uvScratch[2] = new Vector2(inner.z, inner.w);
			uvScratch[3] = new Vector2(outer.z, outer.w);

			toFill.Clear();

			for (var x = 0; x < 3; x++)
			{
				var xIdx = x + 1;

				for (var y = 0; y < 3; y++)
					if (fillCenter || x != 1 || y != 1)
					{
						var yIdx = y + 1;


						AddQuad(toFill,
							posMin: new Vector2(vertScratch[x].x, vertScratch[y].y),
							posMax: new Vector2(vertScratch[xIdx].x, vertScratch[yIdx].y),
							bounds: rect,
							mode: colorMode, colorA: color, colorB: secondColor,
							uvMin: new Vector2(uvScratch[x].x, uvScratch[y].y),
							uvMax: new Vector2(uvScratch[xIdx].x, uvScratch[yIdx].y));
					}
			}
		}

#endregion

#region Tiled

		private void GenerateTiledSprite(VertexHelper toFill)
		{
			Vector4 outerUV, innerUV, border;
			Vector2 spriteSize;

			if (activeSprite == null)
			{
				outerUV = Vector4.zero;
				innerUV = Vector4.zero;
				border = Vector4.zero;
				spriteSize = Vector2.one * 100f;
			}
			else
			{
				outerUV = DataUtility.GetOuterUV(activeSprite);
				innerUV = DataUtility.GetInnerUV(activeSprite);
				border = activeSprite.border;
				spriteSize = activeSprite.rect.size;
			}

			var rect = GetPixelAdjustedRect();

			var tileWidth = (spriteSize.x - border.x - border.z) / multipliedPixelsPerUnit;
			var tileHeight = (spriteSize.y - border.y - border.w) / multipliedPixelsPerUnit;

			border = GetAdjustedBorders(border / multipliedPixelsPerUnit, rect, false,
				new Vector2(
					activeSprite.textureRect.width,
					activeSprite.textureRect.height));

			var scale = SpriteBorderScale.CalculateSize(this);
			tileWidth *= scale.x;
			tileHeight *= scale.y;

			var uvMin = new Vector2(innerUV.x, innerUV.y);
			var uvMax = new Vector2(innerUV.z, innerUV.w);

			UIVertex.simpleVert.color = color;

			// Min to max max range for tiled region in coordinates relative to lower left corner.
			var xMin = scale.x * border.x;
			var xMax = rect.width - scale.x * border.z;
			var yMin = scale.y * border.y;
			var yMax = rect.height - scale.y * border.w;

			toFill.Clear();

			var uvMax2 = uvMax;
			var pos = rect.position;

			if (tileWidth <= 0f) tileWidth = xMax - xMin;
			if (tileHeight <= 0f) tileHeight = yMax - yMin;
			if (fillCenter)
				for (var y1 = yMin; y1 < yMax; y1 = y1 + tileHeight)
				{
					var y2 = y1 + tileHeight;
					if (y2 > yMax)
					{
						uvMax2.y = uvMin.y + (uvMax.y - uvMin.y) * (yMax - y1) / (y2 - y1);
						y2 = yMax;
					}

					uvMax2.x = uvMax.x;
					for (var x1 = xMin; x1 < xMax; x1 = x1 + tileWidth)
					{
						var x2 = x1 + tileWidth;
						if (x2 > xMax)
						{
							uvMax2.x = uvMin.x + (uvMax.x - uvMin.x) * (xMax - x1) / (x2 - x1);
							x2 = xMax;
						}

						AddQuad(toFill, rect,
							new Vector2(x1, y1) + pos,
							new Vector2(x2, y2) + pos,
							colorMode, color, secondColor,
							uvMin, uvMax2);
					}
				}

			if (hasBorder)
			{
				uvMax2 = uvMax;
				for (var y1 = yMin; y1 < yMax; y1 = y1 + tileHeight)
				{
					var y2 = y1 + tileHeight;
					if (y2 > yMax)
					{
						uvMax2.y = uvMin.y + (uvMax.y - uvMin.y) * (yMax - y1) / (y2 - y1);
						y2 = yMax;
					}

					AddQuad(toFill, rect,
						new Vector2(0f, y1) + pos,
						new Vector2(xMin, y2) + pos,
						colorMode, color, secondColor,
						new Vector2(outerUV.x, uvMin.y), new Vector2(uvMin.x, uvMax2.y));

					AddQuad(toFill, rect,
						new Vector2(xMax, y1) + pos,
						new Vector2(rect.width, y2) + pos,
						colorMode, color, secondColor,
						new Vector2(uvMax.x, uvMin.y), new Vector2(outerUV.z, uvMax2.y));
				}

				uvMax2 = uvMax;
				for (var x1 = xMin; x1 < xMax; x1 = x1 + tileWidth)
				{
					var x2 = x1 + tileWidth;
					if (x2 > xMax)
					{
						uvMax2.x = uvMin.x + (uvMax.x - uvMin.x) * (xMax - x1) / (x2 - x1);
						x2 = xMax;
					}

					AddQuad(toFill, rect,
						new Vector2(x1, 0f) + pos,
						new Vector2(x2, yMin) + pos,
						colorMode, color, secondColor,
						new Vector2(uvMin.x, outerUV.y), new Vector2(uvMax2.x, uvMin.y));

					AddQuad(toFill, rect,
						new Vector2(x1, yMax) + pos,
						new Vector2(x2, rect.height) + pos,
						colorMode, color, secondColor,
						new Vector2(uvMin.x, uvMax.y), new Vector2(uvMax2.x, outerUV.w));
				}

				AddQuad(toFill, rect,
					new Vector2(0f, 0f) + pos,
					new Vector2(xMin, yMin) + pos,
					colorMode, color, secondColor,
					new Vector2(outerUV.x, outerUV.y), new Vector2(uvMin.x, uvMin.y));

				AddQuad(toFill, rect,
					new Vector2(xMax, 0f) + pos,
					new Vector2(rect.width, yMin) + pos,
					colorMode, color, secondColor,
					new Vector2(uvMax.x, outerUV.y), new Vector2(outerUV.z, uvMin.y));

				AddQuad(toFill, rect,
					new Vector2(0f, yMax) + pos,
					new Vector2(xMin, rect.height) + pos,
					colorMode, color, secondColor,
					new Vector2(outerUV.x, uvMax.y), new Vector2(uvMin.x, outerUV.w));

				AddQuad(toFill, rect,
					new Vector2(xMax, yMax) + pos,
					new Vector2(rect.width, rect.height) + pos,
					colorMode, color, secondColor,
					new Vector2(uvMax.x, uvMax.y), new Vector2(outerUV.z, outerUV.w));
			}
		}

#endregion

		private void AddQuad(
			VertexHelper vertexHelper, Rect bounds,
			Vector2 posMin, Vector2 posMax,
			ColorMode mode, Color colorA, Color colorB,
			Vector2 uvMin, Vector2 uvMax)
		{
			ImageAppearanceProviderHelper.AddQuad(vertexHelper, bounds, posMin, posMax, mode, colorA, colorB, uvMin,
				uvMax, materialProperties);
		}


		private Vector4 GetAdjustedBorders(Vector4 border, Rect rect, bool keepAspect, Vector2 texSize)
		{
			float scale = 1;
			for (var axis = 0; axis <= 1; axis++)
			{
				// If the rect is smaller than the combined borders, then there's not room for the borders at their normal size.
				// In order to avoid artefacts with overlapping borders, we scale the borders down to fit.
				var combinedBorders = border[axis] + border[axis + 2];
				if (rect.size[axis] < combinedBorders) // && combinedBorders != 0)
				{
					if (keepAspect)
					{
						scale = Mathf.Min(scale, rect.size[axis] / combinedBorders);
					}
					else
					{
						var borderScaleRatio = rect.size[axis] / combinedBorders;
						border[axis] *= borderScaleRatio;
						border[axis + 2] *= borderScaleRatio;
					}
				}
				else if (combinedBorders == 0 && keepAspect)
				{
					var o = (axis + 1) % 2;
					combinedBorders = border[o] + border[o + 2];

					scale = rect.size[axis] / texSize[axis];
					if (scale * combinedBorders > rect.size[o]) scale = rect.size[o] / combinedBorders;
				}
			}

			if (keepAspect) border = scale * border;

			return border;
		}

		public void OnResolutionChanged()
		{
			SetVerticesDirty();
			AssignSpriteSettings();
		}

		private void AssignSpriteSettings()
		{
			var settings = CurrentSpriteSettings;
			sprite = settings.Sprite;
			colorMode = settings.ColorMode;
			color = settings.PrimaryColor;
			secondColor = settings.SecondaryColor;
		}

#if UNITY_EDITOR

		protected override void OnValidate()
		{
			base.OnValidate();
			DoValidation();
			AssignSpriteSettings();
		}

#endif

		private void DoValidation()
		{
			var isUnInitialized = fallbackSpriteSettings == null
								|| (fallbackSpriteSettings.Sprite == null
									&& fallbackSpriteSettings.ColorMode == ColorMode.Color
									&& fallbackSpriteSettings.PrimaryColor == new Color());

			if (isUnInitialized) fallbackSpriteSettings = new SpriteSettings(sprite, colorMode, color, secondColor);
		}
	}
}