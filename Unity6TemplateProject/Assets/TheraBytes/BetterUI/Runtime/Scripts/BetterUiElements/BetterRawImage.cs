using System;
using UnityEngine;
using UnityEngine.UI;

namespace TheraBytes.BetterUi
{
	[HelpURL("https://documentation.therabytes.de/better-ui/BetterRawImage.html")]
	[AddComponentMenu("Better UI/Controls/Better Raw Image", 30)]
	public class BetterRawImage : RawImage, IImageAppearanceProvider, IResolutionDependency
	{
		[SerializeField] private ColorMode colorMode = ColorMode.Color;

		[SerializeField] private Color secondColor = Color.white;

		[SerializeField] private VertexMaterialData materialProperties = new();

		[SerializeField] private string materialType;

		[SerializeField] private MaterialEffect materialEffect;

		[SerializeField] private float materialProperty1, materialProperty2, materialProperty3;


		[SerializeField] private TextureSettings fallbackTextureSettings;

		[SerializeField] private TextureSettingsConfigCollection customTextureSettings = new();

		public new Texture texture
		{
			get => base.texture;
			set { Config.Set(value, o => base.texture = value, o => CurrentTextureSettings.Texture = value); }
		}

		public new Rect uvRect
		{
			get => base.uvRect;
			set { Config.Set(value, o => base.uvRect = value, o => CurrentTextureSettings.UvRect = value); }
		}

		public TextureSettings CurrentTextureSettings
		{
			get
			{
				DoValidation();
				return customTextureSettings.GetCurrentItem(fallbackTextureSettings);
			}
		}

		protected override void OnEnable()
		{
			base.OnEnable();

			AssignTextureSettings();

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

#if UNITY_EDITOR

		protected override void OnValidate()
		{
			base.OnValidate();
			DoValidation();
			AssignTextureSettings();
		}

#endif

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
				Config.Set(value, o => colorMode = value, o => CurrentTextureSettings.ColorMode = value);
				SetVerticesDirty();
			}
		}

		public Color SecondColor
		{
			get => secondColor;
			set
			{
				Config.Set(value, o => secondColor = value, o => CurrentTextureSettings.SecondaryColor = value);
				SetVerticesDirty();
			}
		}

		public override Color color
		{
			get => base.color;
			set { Config.Set(value, o => base.color = value, o => CurrentTextureSettings.PrimaryColor = value); }
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

		public void OnResolutionChanged()
		{
			AssignTextureSettings();
		}

		protected override void OnPopulateMesh(VertexHelper vh)
		{
			var rect = GetPixelAdjustedRect();

			var pMin = new Vector2(rect.x, rect.y);
			var pMax = new Vector2(rect.x + rect.width, rect.y + rect.height);

			var w = texture != null ? texture.width * texture.texelSize.x : 1;
			var h = texture != null ? texture.height * texture.texelSize.y : 1;
			var uvMin = new Vector2(uvRect.xMin * w, uvRect.yMin * h);
			var uvMax = new Vector2(uvRect.xMax * w, uvRect.yMax * h);

			vh.Clear();
			ImageAppearanceProviderHelper.AddQuad(vh, rect,
				pMin, pMax,
				colorMode, color, secondColor,
				uvMin, uvMax,
				materialProperties);
		}

		private void AssignTextureSettings()
		{
			var settings = CurrentTextureSettings;

			texture = settings.Texture;
			colorMode = settings.ColorMode;
			color = settings.PrimaryColor;
			secondColor = settings.SecondaryColor;
			uvRect = settings.UvRect;
		}

		private void DoValidation()
		{
			var isUnInitialized = fallbackTextureSettings == null
								|| (fallbackTextureSettings.Texture == null
									&& fallbackTextureSettings.ColorMode == ColorMode.Color
									&& fallbackTextureSettings.PrimaryColor == new Color()
									&& uvRect == new Rect());

			if (isUnInitialized)
				fallbackTextureSettings = new TextureSettings(texture, colorMode, color, secondColor, uvRect);
		}

#region Nested Types

		[Serializable]
		public class TextureSettings : IScreenConfigConnection
		{
			public Texture Texture;
			public ColorMode ColorMode;
			public Color PrimaryColor;
			public Color SecondaryColor;
			public Rect UvRect;

			[SerializeField] private string screenConfigName;


			public TextureSettings(Texture texture, ColorMode colorMode, Color primary, Color secondary, Rect uvRect)
			{
				Texture = texture;
				ColorMode = colorMode;
				PrimaryColor = primary;
				SecondaryColor = secondary;
				UvRect = uvRect;
			}

			public string ScreenConfigName
			{
				get => screenConfigName;
				set => screenConfigName = value;
			}
		}

		[Serializable]
		public class TextureSettingsConfigCollection : SizeConfigCollection<TextureSettings>
		{
		}

#endregion
	}
}