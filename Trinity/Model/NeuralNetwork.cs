using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;

namespace Trinity.Model
{
	[Serializable]
	public class NeuralNetwork : IDisposable
	{
        public ThreadSafeRandom RandomGenerator { get; private set; }
        public ParallelOptions ParallelOption { get; private set; }

        public string PatternMapping;

		public double TrainToValue
		{
			get;
			set;
		}

		public List<Layer> Layers
		{
			get;
			private set;
		}

		public int MaxDegreeOfParallelism
		{
			get
			{
				return Utils.ParallelOption.MaxDegreeOfParallelism;
			}
			set
			{
				if (value != Utils.ParallelOption.MaxDegreeOfParallelism)
				{
					Utils.ParallelOption.MaxDegreeOfParallelism = value;
				}
			}
		}

		public NeuralNetwork()
		{
			Layers = new List<Layer>();
			Utils.RandomGenerator = new ThreadSafeRandom();
			Utils.ParallelOption = new ParallelOptions();
			Utils.ParallelOption.TaskScheduler = null;
			Utils.ParallelOption.MaxDegreeOfParallelism = MaxDegreeOfParallelism;
		}

		~NeuralNetwork()
		{
			Dispose(disposing: false);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Layers != null)
				{
					Layers.Clear();
				}
				Layers = null;
			}
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

        /// <summary>
        /// 加载权值
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public bool LoadWeights(byte[] buffer)
        {
            int indexSize = sizeof(byte);
            int weightSize = sizeof(double);
            int recordSize = indexSize + weightSize;
            int totalWeightCount = buffer.Length / recordSize;
            int checkWeightCount = 0;

            // 检测权值总数是否相等
            for (int l = 1; l < Layers.Count; l++)
                checkWeightCount += Layers[l].WeightCount;

            if (totalWeightCount == checkWeightCount)
            {
                byte oldLayerIdx = 0;
                int weightIdx = 0;
                for (int index = 0; index < buffer.Length; index += recordSize)
                {
                    if (buffer[index] != oldLayerIdx)
                    {
                        weightIdx = 0;
                        oldLayerIdx = buffer[index];
                    }
                    byte[] temp = new byte[weightSize];
                    for (int j = 0; j < weightSize; j++)
                        temp[j] = buffer[index + j + indexSize];
                    Layers[buffer[index]].Weights[weightIdx++].Value = BitConverter.ToDouble(temp, 0);
                }
            }
            else
            {
                return false;
            }

            buffer = null;
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            GC.WaitForPendingFinalizers();
            GC.Collect();

            return true;
        }

        /// <summary>
        /// 反序结构+权值
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static List<NeuralNetwork> Unserialize(Stream stream)
		{
			List<NeuralNetwork> list = new List<NeuralNetwork>();
			GZipStream gZipStream = new GZipStream(stream, CompressionMode.Decompress);
			byte[] buffer = new byte[2048000];
			int num = 0;
			MemoryStream memoryStream = new MemoryStream();
			while ((num = gZipStream.Read(buffer, 0, 2048000)) != 0)
			{
				memoryStream.Write(buffer, 0, num);
			}
			memoryStream.Seek(0L, SeekOrigin.Begin);
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			binaryFormatter.Binder = new CNNBinder();
			object obj = binaryFormatter.Deserialize(memoryStream);
			return obj as List<NeuralNetwork>;
		}

        /// <summary>
        /// 用随机数初始化网络权值
        /// </summary>
        /// <param name="useNeuronCount"></param>
        /// <param name="weightScope"></param>
        /// <param name="weightFactor"></param>
		public void InitWeights(bool useNeuronCount = true, double weightScope = 2.0, double weightFactor = 1.0)
		{
			foreach (Layer item in Layers.Skip(1))
			{
				item.SetInitalWeights(useNeuronCount, weightScope, weightFactor);
			}
		}

        /// <summary>
        /// 根据第一层输入正向传播计算网络输出
        /// </summary>
		public void Calculate(double[] input = null)
		{
			Layer layer = Layers.First();
            if (input != null)
            {
                for (int i = 0; i < input.Length; i++)
                {
                    layer.Neurons[i].Output = input[i];
                }
            }
			while (layer.NextLayer != null)
			{
				layer = layer.NextLayer;
				layer.Calculate();
			}
		}
	}
}
