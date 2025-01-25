using System;
using System.Collections.Generic;

namespace TheraBytes.BetterUi
{
	[Serializable]
	public class VertexMaterialData
	{
		public FloatProperty[] FloatProperties = new FloatProperty[0];

		public void Apply(ref float uvX, ref float uvY, ref float tangentW)
		{
			Apply(FloatProperties, ref uvX, ref uvY, ref tangentW);
		}

		private static void Apply<T>(IEnumerable<Property<T>> prop,
			ref float uvX, ref float uvY, ref float tangentW)
		{
			if (prop == null)
				return;

			foreach (var item in prop) item.SetValue(ref uvX, ref uvY, ref tangentW);
		}

		public void Clear()
		{
			FloatProperties = new FloatProperty[0];
		}

		public void CopyTo(VertexMaterialData target)
		{
			target.FloatProperties = CloneArray<FloatProperty, float>(FloatProperties);
		}


		public VertexMaterialData Clone()
		{
			var result = new VertexMaterialData();
			CopyTo(result);

			return result;
		}

		private static T[] CloneArray<T, TValue>(T[] array)
			where T : Property<TValue>
		{
			var result = new T[array.Length];
			for (var i = 0; i < array.Length; i++) result[i] = array[i].Clone() as T;

			return result;
		}

#region Property Types

		[Serializable]
		public abstract class Property<T>
		{
			public string Name;
			public T Value;

			public abstract void SetValue(ref float uvX, ref float uvY, ref float tangentW);
			public abstract Property<T> Clone();
		}

		[Serializable]
		public class FloatProperty : Property<float>
		{
			public enum Mapping
			{
				TexcoordX,
				TexcoordY,
				TangentW
			}

			public Mapping PropertyMap;
			public float Min, Max;

			public bool IsRestricted => Min < Max;

			public override void SetValue(ref float uvX, ref float uvY, ref float tangentW)
			{
				switch (PropertyMap)
				{
					case Mapping.TexcoordX:
						uvX = Value;
						break;
					case Mapping.TexcoordY:
						uvY = Value;
						break;
					case Mapping.TangentW:
						tangentW = Value;
						break;
					default:
						throw new ArgumentException();
				}
			}

			public override Property<float> Clone()
			{
				return new FloatProperty
				{
					Name = Name,
					Value = Value,
					Min = Min,
					Max = Max,
					PropertyMap = PropertyMap
				};
			}
		}

#endregion
	}
}