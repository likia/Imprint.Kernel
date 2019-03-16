using System;
using System.Collections.Generic;

namespace Trinity.Model
{
	[Serializable]
	public class Mappings
	{
		public List<bool> Mapping;

		public Mappings(List<bool> mapping)
		{
			Mapping = mapping;
		}

		public Mappings(int previousLayerMapCount, int currentLayerMapCount, int density, int randomSeed = 0)
		{
			if (previousLayerMapCount < 1 || currentLayerMapCount < 1)
			{
				throw new ArgumentException("Invalid Mappings parameter(s)");
			}
			bool[] array = new bool[previousLayerMapCount * currentLayerMapCount];
			Random random = new Random(randomSeed);
			for (int i = 0; i < previousLayerMapCount; i++)
			{
				for (int j = 0; j < currentLayerMapCount; j++)
				{
					array[i * currentLayerMapCount + j] = (random.Next(100) < density);
				}
			}
			Mapping = new List<bool>(array);
		}

		public bool IsMapped(int map, int prevMap, int mapCount)
		{
			return Mapping[map + mapCount * prevMap];
		}
	}
}
