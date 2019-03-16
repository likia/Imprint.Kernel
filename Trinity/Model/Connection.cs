using System;

namespace Trinity.Model
{
	[Serializable]
	public struct Connection
	{
		public int ToNeuronIndex;

		public int ToWeightIndex;

		public Connection(int toNeuronIndex, int toWeightIndex)
		{
			this = default(Connection);
			ToNeuronIndex = toNeuronIndex;
			ToWeightIndex = toWeightIndex;
		}
	}
}
