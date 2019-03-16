using System;

namespace Trinity.Model
{

    /// <summary>
    /// Éñ¾­Ôª
    /// </summary>
	[Serializable]
	public class Neuron
	{
		public double Output;

		public Connection[] Connections;

		public Neuron()
		{
			Output = 0.0;
			Connections = new Connection[0];
		}

		public void AddConnection(int neuronIndex, int weightIndex)
		{
			Array.Resize(ref Connections, Connections.Length + 1);
			Connections[Connections.Length - 1] = new Connection(neuronIndex, weightIndex);
		}

		public void AddBias(int weightIndex)
		{
			Array.Resize(ref Connections, Connections.Length + 1);
			Connections[Connections.Length - 1] = new Connection(2147483647, weightIndex);
		}
	}
}
