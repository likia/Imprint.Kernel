using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace Trinity
{
    public class ThreadSafeRandom : Random, IDisposable
    {
        private static readonly RNGCryptoServiceProvider _global = new RNGCryptoServiceProvider();

        private ThreadLocal<Random> _local = new ThreadLocal<Random>(delegate
        {
            byte[] array = new byte[4];
            _global.GetBytes(array);
            return new Random(BitConverter.ToInt32(array, 0));
        });

        public override int Next()
        {
            return _local.Value.Next();
        }

        public override int Next(int maxValue)
        {
            return _local.Value.Next(maxValue);
        }

        public override int Next(int minValue, int maxValue)
        {
            return _local.Value.Next(minValue, maxValue);
        }

        public int NextPercentage()
        {
            return _local.Value.Next(101);
        }

        public override double NextDouble()
        {
            double num = _local.Value.NextDouble();
            while (num < 0.0 && num >= 1.0)
            {
                num = _local.Value.NextDouble();
            }
            return num;
        }

        public double NextDouble(double stdDev, double mean)
        {
            double num = _local.Value.NextDouble();
            while (num < 0.0 && num >= 1.0)
            {
                num = _local.Value.NextDouble();
            }
            num *= stdDev;
            return num + mean;
        }

        public double NextDouble(double stdDev)
        {
            double num = _local.Value.NextDouble();
            while (num < 0.0 && num >= 1.0)
            {
                num = _local.Value.NextDouble();
            }
            num *= 2.0;
            num -= 1.0;
            return num * stdDev;
        }

        public override void NextBytes(byte[] buffer)
        {
            _local.Value.NextBytes(buffer);
        }

        ~ThreadSafeRandom()
        {
            Dispose(disposing: false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && _local != null)
            {
                _local.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }



    public class Utils
	{
		public static ThreadSafeRandom RandomGenerator
		{
			get;
			set;
		}

		public static ParallelOptions ParallelOption
		{
			get;
			set;
		}

		static Utils()
		{
			ParallelOption = new ParallelOptions();
			ParallelOption.TaskScheduler = null;
			ParallelOption.MaxDegreeOfParallelism = Environment.ProcessorCount;
		}
	}
}
