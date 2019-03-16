using Imprint.Imaging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Trinity.Model;

namespace Trinity.Recogniser
{
	public class Recognizer
	{
		public NeuralNetwork CNN;

		public static string patternMapping = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

		public double[] LastOutputWeights
		{
			get;
			set;
		}

		public Recognizer(int oClass = 62)
		{
			CNN = new NeuralNetwork();
			CNN.Layers.Add(new Layer(CNN, LayerType.Input, 1, 32, 32));
			CNN.Layers.Add(new Layer(CNN, LayerType.Convolutional, ActivationFunction.Tanh, 6, 28, 28, 5, 5));
			CNN.Layers.Add(new Layer(CNN, LayerType.Subsampling, ActivationFunction.AveragePoolingTanh, 6, 14, 14, 2, 2));
			List<bool> mapping = new List<bool>(96)
			{
				true,
				false,
				false,
				false,
				true,
				true,
				true,
				false,
				false,
				true,
				true,
				true,
				true,
				false,
				true,
				true,
				true,
				true,
				false,
				false,
				false,
				true,
				true,
				true,
				false,
				false,
				true,
				true,
				true,
				true,
				false,
				true,
				true,
				true,
				true,
				false,
				false,
				false,
				true,
				true,
				true,
				false,
				false,
				true,
				false,
				true,
				true,
				true,
				false,
				true,
				true,
				true,
				false,
				false,
				true,
				true,
				true,
				true,
				false,
				false,
				true,
				false,
				true,
				true,
				false,
				false,
				true,
				true,
				true,
				false,
				false,
				true,
				true,
				true,
				true,
				false,
				true,
				true,
				false,
				true,
				false,
				false,
				false,
				true,
				true,
				true,
				false,
				false,
				true,
				true,
				true,
				true,
				false,
				true,
				true,
				true
			};
			Mappings mappings = new Mappings(mapping);
			CNN.Layers.Add(new Layer(CNN, LayerType.Convolutional, ActivationFunction.Tanh, 16, 10, 10, 5, 5, mappings));
			CNN.Layers.Add(new Layer(CNN, LayerType.Subsampling, ActivationFunction.AveragePoolingTanh, 16, 5, 5, 2, 2));
			CNN.Layers.Add(new Layer(CNN, LayerType.Convolutional, ActivationFunction.Tanh, 512, 1, 1, 5, 5));
			CNN.Layers.Add(new Layer(CNN, LayerType.FullyConnected, ActivationFunction.Tanh, oClass));
			CNN.InitWeights();
		}

		public Recognizer(NeuralNetwork ann)
		{
			CNN = ann;
		}

		public string Recognize(Bitmap img)
		{
			EffImage inputPattern = new EffImage(img);
			img.Dispose();
			SetInputPattern(inputPattern);
			int index = ForwardPropagation();
			return patternMapping[index].ToString();
		}

		public string Recognize(Stream ImgStream)
		{
			using (Image image = Image.FromStream(ImgStream))
			{
				EffImage inputPattern = new EffImage(image as Bitmap);
				SetInputPattern(inputPattern);
				int index = ForwardPropagation();
				return patternMapping[index].ToString();
			}
		}

		private void SetInputPattern(EffImage src)
		{
			Layer layer = CNN.Layers.First();
			int width = src.Width;
			int height = src.Height;
			double[] array = new double[width * height];
			double num = -1.0;
			double value = 1.0;
			double num2 = 255.0 / (Math.Abs(num) + Math.Abs(value));
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = num;
			}
			int num3 = 0;
			for (int j = 0; j < height; j++)
			{
				for (int k = 0; k < width; k++)
				{
					Color color = src.At(k, j);
					int num4 = color.R + color.G + color.B;
					num4 /= 3;
					array[num3] = (double)(255 - num4) / num2 - 1.0;
					num3++;
				}
			}
			for (int l = 0; l < layer.NeuronCount; l++)
			{
				layer.Neurons[l].Output = array[l];
			}
		}

		private int ForwardPropagation()
		{
			int result = 0;
			CNN.Calculate();
			Layer layer = CNN.Layers.Last();
			double[] array = new double[layer.NeuronCount];
			double num = -1.0;
			for (int i = 0; i < layer.NeuronCount; i++)
			{
				array[i] = layer.Neurons[i].Output;
				if (array[i] > num)
				{
					result = i;
					num = array[i];
				}
			}
			LastOutputWeights = array;
			return result;
		}

		public string Recognize(byte[] Imgbuf)
		{
			MemoryStream memoryStream = new MemoryStream(Imgbuf);
			string result = Recognize(memoryStream);
			memoryStream.Close();
			return result;
		}

		public int Unserilize(byte[] data)
		{
			try
			{
				CNN = NeuralNetwork.Unserialize(new MemoryStream(data))[0];
				return 1;
			}
			catch
			{
				return -1;
			}
		}

		public bool LoadWeights(byte[] buffer)
		{
			try
			{
				return CNN.LoadWeights(buffer);
			}
			catch
			{
				return false;
			}
		}
	}
}
