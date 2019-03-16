using System;
using System.Collections.Generic;

namespace Trinity.Model
{
	public class Schema
	{
		private NeuralNetwork model;

		public NeuralNetwork Model => model;

		public Schema(string conf = null)
		{
			if (conf != null)
			{
				Load(conf);
			}
		}

		private T getEnumByName<T>(string name)
		{
			return (T)Enum.Parse(typeof(T), name);
		}

       
		public void Load(string config)
		{
			dynamic cfg = DynamicJson.Parse(config);
			if (cfg == null)
			{
				throw new Exception("配置错误!");
			}
			dynamic layerList = cfg.layers;
			NeuralNetwork neuralNetwork = new NeuralNetwork();
			foreach (dynamic item2 in layerList)
			{
				dynamic val3 = this.getEnumByName<LayerType>(item2.type);
				int num = (int)item2.map;
				if ((!item2.IsDefined("width")))
				{
					neuralNetwork.Layers.Add(new Layer(neuralNetwork, val3, this.getEnumByName<ActivationFunction>(item2.function), num));
				}
				else
				{
					int num2 = (int)item2.width;
					int num3 = (int)item2.height;
					ActivationFunction activationFunction = ActivationFunction.Tanh;
					if (item2.IsDefined("function"))
					{
						activationFunction = (ActivationFunction)this.getEnumByName<ActivationFunction>(item2.function);
						int num4 = (int)item2.filterWidth;
						int num5 = (int)item2.filterHeight;
						if (item2.IsDefined("mapping"))
						{
							List<bool> list = new List<bool>();
							foreach (object item3 in item2.mapping)
							{
								bool item = (byte)item3 != 0;
								list.Add(item);
							}
							neuralNetwork.Layers.Add(new Layer(neuralNetwork, val3, activationFunction, num, num2, num3, num4, num5, new Mappings(list)));
						}
						else
						{
							neuralNetwork.Layers.Add(new Layer(neuralNetwork, val3, activationFunction, num, num2, num3, num4, num5));
						}
					}
					else
					{
						neuralNetwork.Layers.Add(new Layer(neuralNetwork, val3, num, num2, num3, false));
					}
				}
			}
			model = neuralNetwork;
		}
	}
}
