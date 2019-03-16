using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Trinity.Model
{
    [Serializable()]
    public sealed class Layer
    {
        public NeuralNetwork Network { get; private set; }
        public int LayerIndex { get; private set; }
        public string Name { get; private set; }
        public LayerType LayerType { get; private set; }
        public ActivationFunction ActivationFunction { get; private set; }
        public int NeuronCount { get; private set; }
        public int WeightCount { get; private set; }
        public bool UseMapInfo { get; private set; }
        public int MapCount { get; private set; }
        public int MapWidth { get; private set; }
        public int MapHeight { get; private set; }
        public bool IsFullyMapped { get; private set; }
        public int ReceptiveFieldWidth { get; private set; }
        public int ReceptiveFieldHeight { get; private set; }
        public double SubsamplingScalingFactor { get; private set; }
        public Layer PreviousLayer { get; private set; }
        public Layer NextLayer { get; private set; }
        public Neuron[] Neurons { get; private set; }
        public Weight[] Weights { get; private set; }
        public Mappings Mappings { get; private set; }
        public bool LockedWeights { get; set; }

        //private OrderablePartitioner<Tuple<int,int>> WeightPartitioner;
        //private OrderablePartitioner<Tuple<int,int>> NeuronPartitioner;
        private bool useWeightPartitioner;
        private bool useNeuronPartitioner;
        public Layer(NeuralNetwork network, LayerType layerType, int mapCount, int mapWidth, int mapHeight, bool lockedWeights = false) : this(network, ((network.Layers.Count == 0) ? (0) : (network.Layers.Count)), layerType, ActivationFunction.Tanh, mapCount * mapWidth * mapHeight, true, mapCount, mapWidth, mapHeight, true, 0, 0, ((network.Layers.Count == 0) ? (null) : (network.Layers[network.Layers.Count - 1])), null, lockedWeights) { }
        public Layer(NeuralNetwork network, LayerType layerType, ActivationFunction activationFunction, int mapCount, int mapWidth, int mapHeight, bool lockedWeights = false) : this(network, ((network.Layers.Count == 0) ? (0) : (network.Layers.Count)), layerType, activationFunction, mapCount * mapWidth * mapHeight, true, mapCount, mapWidth, mapHeight, true, 0, 0, ((network.Layers.Count == 0) ? (null) : (network.Layers[network.Layers.Count - 1])), null, lockedWeights) { }
        public Layer(NeuralNetwork network, LayerType layerType, ActivationFunction activationFunction, int neuronCount, bool lockedWeights = false) : this(network, ((network.Layers.Count == 0) ? (0) : (network.Layers.Count)), layerType, activationFunction, neuronCount, false, 1, 1, 1, true, 0, 0, ((network.Layers.Count == 0) ? (null) : (network.Layers[network.Layers.Count - 1])), null, lockedWeights) { }
        public Layer(NeuralNetwork network, LayerType layerType, ActivationFunction activationFunction, int neuronCount, Mappings mappings, bool lockedWeights = false) : this(network, ((network.Layers.Count == 0) ? (0) : (network.Layers.Count)), layerType, activationFunction, neuronCount, false, 1, 1, 1, false, 0, 0, ((network.Layers.Count == 0) ? (null) : (network.Layers[network.Layers.Count - 1])), mappings, lockedWeights) { }
        public Layer(NeuralNetwork network, LayerType layerType, ActivationFunction activationFunction, int mapCount, int mapWidth, int mapHeight, Mappings mappings, bool lockedWeights = false) : this(network, ((network.Layers.Count == 0) ? (0) : (network.Layers.Count)), layerType, activationFunction, mapCount * mapWidth * mapHeight, true, mapCount, mapWidth, mapHeight, false, 1, 1, ((network.Layers.Count == 0) ? (null) : (network.Layers[network.Layers.Count - 1])), mappings, lockedWeights) { }
        public Layer(NeuralNetwork network, LayerType layerType, ActivationFunction activationFunction, int mapCount, int mapWidth, int mapHeight, int receptiveFieldWidth, int receptiveFieldHeight, bool lockedWeights = false) : this(network, ((network.Layers.Count == 0) ? (0) : (network.Layers.Count)), layerType, activationFunction, mapCount * mapWidth * mapHeight, true, mapCount, mapWidth, mapHeight, true, receptiveFieldWidth, receptiveFieldHeight, network.Layers[network.Layers.Count - 1], null, lockedWeights) { }
        public Layer(NeuralNetwork network, LayerType layerType, ActivationFunction activationFunction, int mapCount, int mapWidth, int mapHeight, int receptiveFieldWidth, int receptiveFieldHeight, Mappings mappings, bool lockedWeights = false) : this(network, ((network.Layers.Count == 0) ? (0) : (network.Layers.Count)), layerType, activationFunction, mapCount * mapWidth * mapHeight, true, mapCount, mapWidth, mapHeight, false, receptiveFieldWidth, receptiveFieldHeight, network.Layers[network.Layers.Count - 1], mappings, lockedWeights) { }
        public Layer(NeuralNetwork network, int layerIndex, LayerType layerType, ActivationFunction activationFunction, int neuronCount, bool useMapInfo, int mapCount, int mapWidth, int mapHeight, bool isFullyMapped, int receptiveFieldWidth, int receptiveFieldHeight, Layer previousLayer, Mappings mappings, bool lockedWeights = false)
        {
            Network = network;
            LayerIndex = layerIndex;
            LayerType = layerType;
            ActivationFunction = activationFunction;
            NeuronCount = neuronCount;
            UseMapInfo = useMapInfo;
            MapCount = mapCount;
            MapWidth = mapWidth;
            MapHeight = mapHeight;
            IsFullyMapped = isFullyMapped;
            ReceptiveFieldWidth = receptiveFieldWidth;
            ReceptiveFieldHeight = receptiveFieldHeight;
            PreviousLayer = previousLayer;
            LockedWeights = lockedWeights;


            Neurons = new Neuron[NeuronCount];
            for (int i = 0; i < NeuronCount; i++)
            {
                Neurons[i] = new Neuron();
            }
            //NeuronPartitioner = Partitioner.Create(0, NeuronCount);
            useNeuronPartitioner = NeuronCount > 500;

            int[] kernelTemplate;
            int iNumWeight = 0;
            int position = 0;

            switch (LayerType)
            {
                case LayerType.Input:
                    ActivationFunction = ActivationFunction.None;
                    WeightCount = 0;
                    Weights = null;
                    break;

                case LayerType.Convolutional:
                    int totalMappings;
                    if (UseMapInfo)
                    {
                        if (IsFullyMapped)
                        {
                            totalMappings = PreviousLayer.MapCount * MapCount;
                        }
                        else
                        {
                            Mappings = mappings;
                            if (Mappings != null)
                            {
                                if (Mappings.Mapping.Count() == PreviousLayer.MapCount * MapCount)
                                    totalMappings = Mappings.Mapping.Count(p => p == true);
                                else
                                    throw new ArgumentException("Invalid mappings definition");
                            }
                            else
                                throw new ArgumentException("Empty mappings definition");
                        }

                        WeightCount = (totalMappings * ReceptiveFieldWidth * ReceptiveFieldHeight) + MapCount;
                        Weights = new Weight[WeightCount];

                        kernelTemplate = new int[ReceptiveFieldWidth * ReceptiveFieldHeight];
                        Parallel.For(0, ReceptiveFieldHeight, Network.ParallelOption, row =>
                        {
                            for (int column = 0; column < ReceptiveFieldWidth; column++)
                            {
                                kernelTemplate[column + (row * ReceptiveFieldWidth)] = column + (row * PreviousLayer.MapWidth);
                            }
                        });

                        int positionPrevMap = 0;
                        iNumWeight = 0;
                        int mapping = 0;
                        int prevCurMap = -1;
                        if (!IsFullyMapped) // not fully mapped
                        {
                            for (int curMap = 0; curMap < MapCount; curMap++)
                            {
                                for (int prevMap = 0; prevMap < PreviousLayer.MapCount; prevMap++)
                                {
                                    positionPrevMap = prevMap * PreviousLayer.MapWidth * PreviousLayer.MapHeight;

                                    if (mappings.IsMapped(curMap, prevMap, MapCount))
                                    {
                                        for (int y = 0; y < MapHeight; y++)
                                        {
                                            for (int x = 0; x < MapWidth; x++)
                                            {
                                                position = x + (y * MapWidth) + (curMap * MapWidth * MapHeight);
                                                iNumWeight = (mapping * (ReceptiveFieldWidth * ReceptiveFieldHeight)) + curMap;
                                                if (prevCurMap != curMap)
                                                    Neurons[position].AddBias(iNumWeight++);

                                                for (int i = 0; i < (ReceptiveFieldWidth * ReceptiveFieldHeight); i++)
                                                {
                                                    Neurons[position].AddConnection(x + (y * PreviousLayer.MapWidth) + kernelTemplate[i] + positionPrevMap, iNumWeight++);
                                                }
                                            }
                                        }
                                        mapping++;
                                        prevCurMap = curMap;
                                    }
                                }
                            }
                        }
                        else // Fully mapped
                        {
                            if (totalMappings > MapCount)
                            {
                                for (int curMap = 0; curMap < MapCount; curMap++)
                                {
                                    for (int prevMap = 0; prevMap < PreviousLayer.MapCount; prevMap++)
                                    {
                                        positionPrevMap = prevMap * PreviousLayer.MapWidth * PreviousLayer.MapHeight;

                                        for (int y = 0; y < MapHeight; y++)
                                        {
                                            for (int x = 0; x < MapWidth; x++)
                                            {
                                                position = x + (y * MapWidth) + (curMap * MapWidth * MapHeight);
                                                iNumWeight = (mapping * ReceptiveFieldWidth * ReceptiveFieldHeight) + curMap;

                                                if (prevCurMap != curMap)
                                                    Neurons[position].AddBias(iNumWeight++);

                                                for (int i = 0; i < (ReceptiveFieldWidth * ReceptiveFieldHeight); i++)
                                                {
                                                    Neurons[position].AddConnection(x + (y * PreviousLayer.MapWidth) + kernelTemplate[i] + positionPrevMap, iNumWeight++);
                                                }
                                            }
                                        }
                                        mapping++;
                                        prevCurMap = curMap;
                                    }
                                }
                            }
                            else // PreviousLayer has only one map
                            {
                                for (int curMap = 0; curMap < MapCount; curMap++)
                                {
                                    for (int y = 0; y < MapHeight; y++)
                                    {
                                        for (int x = 0; x < MapWidth; x++)
                                        {
                                            position = x + (y * MapWidth) + (curMap * MapWidth * MapHeight);
                                            iNumWeight = curMap * ((ReceptiveFieldWidth * ReceptiveFieldHeight) + 1);

                                            Neurons[position].AddBias(iNumWeight++);

                                            for (int i = 0; i < (ReceptiveFieldWidth * ReceptiveFieldHeight); i++)
                                            {
                                                Neurons[position].AddConnection(x + (y * PreviousLayer.MapWidth) + kernelTemplate[i], iNumWeight++);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        throw new ArgumentException("Inadequate mapping information provided");
                    }
                    break;

                case LayerType.ConvolutionalSubsampling:  // Simard's implementation
                    if (UseMapInfo)
                    {
                        if (IsFullyMapped)
                        {
                            totalMappings = PreviousLayer.MapCount * MapCount;
                        }
                        else
                        {
                            Mappings = mappings;
                            if (Mappings != null)
                            {
                                if (Mappings.Mapping.Count() == PreviousLayer.MapCount * MapCount)
                                    totalMappings = Mappings.Mapping.Count(p => p == true);
                                else
                                    throw new ArgumentException("Invalid mappings definition");
                            }
                            else
                                throw new ArgumentException("Empty mappings definition");
                        }

                        WeightCount = (totalMappings * ReceptiveFieldWidth * ReceptiveFieldHeight) + MapCount;
                        Weights = new Weight[WeightCount];

                        kernelTemplate = new int[ReceptiveFieldWidth * ReceptiveFieldHeight];
                        Parallel.For(0, ReceptiveFieldHeight, Network.ParallelOption, row =>
                        {
                            for (int column = 0; column < ReceptiveFieldWidth; column++)
                            {
                                kernelTemplate[column + (row * ReceptiveFieldWidth)] = column + (row * PreviousLayer.MapWidth);
                            }
                        });

                        int positionPrevMap = 0;
                        iNumWeight = 0;
                        int mapping = 0;
                        int prevCurMap = -1;
                        if (!IsFullyMapped) // not fully mapped
                        {
                            for (int curMap = 0; curMap < MapCount; curMap++)
                            {
                                for (int prevMap = 0; prevMap < PreviousLayer.MapCount; prevMap++)
                                {
                                    positionPrevMap = prevMap * PreviousLayer.MapWidth * PreviousLayer.MapHeight;

                                    if (mappings.IsMapped(curMap, prevMap, MapCount))
                                    {
                                        for (int y = 0; y < MapHeight; y++)
                                        {
                                            for (int x = 0; x < MapWidth; x++)
                                            {
                                                position = x + (y * MapWidth) + (curMap * MapWidth * MapHeight);
                                                iNumWeight = (mapping * (ReceptiveFieldWidth * ReceptiveFieldHeight)) + curMap;
                                                if (prevCurMap != curMap)
                                                    Neurons[position].AddBias(iNumWeight++);

                                                for (int i = 0; i < (ReceptiveFieldWidth * ReceptiveFieldHeight); i++)
                                                {
                                                    Neurons[position].AddConnection((x * 2) + (y * 2 * PreviousLayer.MapWidth) + kernelTemplate[i] + positionPrevMap, iNumWeight++);
                                                }
                                            }
                                        }
                                        mapping++;
                                        prevCurMap = curMap;
                                    }
                                }
                            }
                        }
                        else // Fully mapped
                        {
                            if (totalMappings > MapCount)
                            {
                                for (int curMap = 0; curMap < MapCount; curMap++)
                                {
                                    for (int prevMap = 0; prevMap < PreviousLayer.MapCount; prevMap++)
                                    {
                                        positionPrevMap = prevMap * PreviousLayer.MapWidth * PreviousLayer.MapHeight;

                                        for (int y = 0; y < MapHeight; ++y)
                                        {
                                            for (int x = 0; x < MapWidth; ++x)
                                            {
                                                position = x + (y * MapWidth) + (curMap * MapWidth * MapHeight);
                                                iNumWeight = (mapping * ReceptiveFieldWidth * ReceptiveFieldHeight) + curMap;

                                                if (prevCurMap != curMap)
                                                    Neurons[position].AddBias(iNumWeight++);

                                                for (int i = 0; i < (ReceptiveFieldWidth * ReceptiveFieldHeight); i++)
                                                {
                                                    Neurons[position].AddConnection((x * 2) + (y * 2 * PreviousLayer.MapWidth) + kernelTemplate[i] + positionPrevMap, iNumWeight++);
                                                }
                                            }
                                        }
                                        mapping++;
                                        prevCurMap = curMap;
                                    }
                                }
                            }
                            else // PreviousLayer has only one map
                            {
                                for (int curMap = 0; curMap < MapCount; curMap++)
                                {
                                    for (int y = 0; y < MapHeight; y++)
                                    {
                                        for (int x = 0; x < MapWidth; x++)
                                        {
                                            position = x + (y * MapWidth) + (curMap * MapWidth * MapHeight);
                                            iNumWeight = curMap * ((ReceptiveFieldWidth * ReceptiveFieldHeight) + 1);

                                            Neurons[position].AddBias(iNumWeight++);

                                            for (int i = 0; i < (ReceptiveFieldWidth * ReceptiveFieldHeight); i++)
                                            {
                                                Neurons[position].AddConnection((x * 2) + (y * 2 * PreviousLayer.MapWidth) + kernelTemplate[i], iNumWeight++);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        throw new ArgumentException("Inadequate mapping information provided");
                    }
                    break;

                case LayerType.Subsampling:
                    if (UseMapInfo)
                    {
                        if (IsFullyMapped)
                        {
                            // Symmetrical mapping
                            List<bool> mapCombinations = new List<bool>(PreviousLayer.MapCount * MapCount);
                            for (int x = 0; x < MapCount; x++)
                            {
                                for (int y = 0; y < PreviousLayer.MapCount; y++)
                                {
                                    mapCombinations.Add(x == y);
                                }
                            }
                            mappings = new Mappings(mapCombinations);
                        }

                        Mappings = mappings;
                        if (Mappings != null)
                        {
                            if (Mappings.Mapping.Count() == PreviousLayer.MapCount * MapCount)
                                totalMappings = Mappings.Mapping.Count(p => p == true);
                            else
                                throw new ArgumentException("Invalid mappings definition");
                        }
                        else
                            throw new ArgumentException("Empty mappings definition");

                        WeightCount = MapCount * 2;
                        Weights = new Weight[WeightCount];

                        SubsamplingScalingFactor = 1D / (receptiveFieldWidth * ReceptiveFieldHeight);

                        kernelTemplate = new int[ReceptiveFieldWidth * ReceptiveFieldHeight];
                        Parallel.For(0, ReceptiveFieldHeight, Network.ParallelOption, row =>
                        {
                            for (int column = 0; column < ReceptiveFieldWidth; column++)
                            {
                                kernelTemplate[column + (row * ReceptiveFieldWidth)] = column + (row * PreviousLayer.MapWidth);
                            }
                        });

                        int positionPrevMap = 0;
                        iNumWeight = 0;
                        if (PreviousLayer.MapCount > 1) //fully symmetrical mapped
                        {
                            for (int curMap = 0; curMap < MapCount; curMap++)
                            {
                                for (int prevMap = 0; prevMap < PreviousLayer.MapCount; prevMap++)
                                {
                                    positionPrevMap = prevMap * PreviousLayer.MapWidth * PreviousLayer.MapHeight;

                                    if (mappings.IsMapped(curMap, prevMap, MapCount))
                                    {
                                        for (int y = 0; y < MapHeight; y++)
                                        {
                                            for (int x = 0; x < MapWidth; x++)
                                            {
                                                position = x + (y * MapWidth) + (curMap * MapWidth * MapHeight);
                                                iNumWeight = curMap * 2;
                                                Neurons[position].AddBias(iNumWeight++);

                                                for (int i = 0; i < (ReceptiveFieldWidth * ReceptiveFieldHeight); i++)
                                                {
                                                    Neurons[position].AddConnection((x * ReceptiveFieldWidth) + (y * ReceptiveFieldHeight * PreviousLayer.MapWidth) + kernelTemplate[i] + positionPrevMap, iNumWeight);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else // only one previous layer
                        {
                            for (int curMap = 0; curMap < MapCount; curMap++)
                            {
                                for (int y = 0; y < MapHeight; y++)
                                {
                                    for (int x = 0; x < MapWidth; x++)
                                    {
                                        position = x + (y * MapWidth) + (curMap * MapWidth * MapHeight);
                                        iNumWeight = curMap * 2;

                                        Neurons[position].AddBias(iNumWeight++);

                                        for (int i = 0; i < (ReceptiveFieldWidth * ReceptiveFieldHeight); i++)
                                        {
                                            Neurons[position].AddConnection((x * ReceptiveFieldWidth) + (y * ReceptiveFieldHeight * PreviousLayer.MapWidth) + kernelTemplate[i], iNumWeight);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    break;

                case LayerType.FullyConnected:
                    WeightCount = (PreviousLayer.NeuronCount + 1) * NeuronCount;
                    Weights = new Weight[WeightCount];

                    iNumWeight = 0;
                    if (UseMapInfo)
                    {
                        for (int curMap = 0; curMap < MapCount; curMap++)
                        {
                            for (int yc = 0; yc < MapHeight; yc++)
                            {
                                for (int xc = 0; xc < MapWidth; xc++)
                                {
                                    position = xc + (yc * MapWidth) + (curMap * MapWidth * MapHeight);
                                    Neurons[position].AddBias(iNumWeight++);

                                    for (int prevMaps = 0; prevMaps < PreviousLayer.MapCount; prevMaps++)
                                    {
                                        for (int y = 0; y < PreviousLayer.MapHeight; y++)
                                        {
                                            for (int x = 0; x < PreviousLayer.MapWidth; x++)
                                            {
                                                Neurons[position].AddConnection((x + (y * PreviousLayer.MapWidth) + (prevMaps * PreviousLayer.MapWidth * PreviousLayer.MapHeight)), iNumWeight++);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int y = 0; y < NeuronCount; y++)
                        {
                            Neurons[y].AddBias(iNumWeight++);
                            for (int x = 0; x < PreviousLayer.NeuronCount; x++)
                            {
                                Neurons[y].AddConnection(x, iNumWeight++);
                            }
                        }
                    }
                    break;

                case LayerType.RBF:
                    WeightCount = PreviousLayer.NeuronCount * NeuronCount; // no biasses
                    Weights = new Weight[WeightCount];

                    iNumWeight = 0;
                    if (UseMapInfo)
                    {
                        for (int n = 0; n < NeuronCount; n++)
                        {
                            for (int prevMaps = 0; prevMaps < PreviousLayer.MapCount; prevMaps++)
                            {
                                for (int y = 0; y < PreviousLayer.MapHeight; y++)
                                {
                                    for (int x = 0; x < PreviousLayer.MapWidth; x++)
                                    {
                                        Neurons[n].AddConnection((x + (y * PreviousLayer.MapWidth) + (prevMaps * PreviousLayer.MapWidth * PreviousLayer.MapHeight)), iNumWeight++);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int y = 0; y < NeuronCount; y++)
                        {
                            for (int x = 0; x < PreviousLayer.NeuronCount; x++)
                            {
                                Neurons[y].AddConnection(x, iNumWeight++);
                            }
                        }
                    }
                    break;
            };

            //if (WeightCount > 0)
            //{
            //    WeightPartitioner = Partitioner.Create(0, WeightCount);
            //}
            useWeightPartitioner = WeightCount > 1000;

            int conn = 0;
            foreach (Neuron neuron in Neurons)
            {
                conn += neuron.Connections.Count();
            }

            Name += "Layer: " + LayerIndex.ToString(CultureInfo.CurrentCulture) + "\r\n";
            Name += "Layer Type: " + LayerType.ToString() + "\r\n" +
                    ((LayerType == LayerType.Input) ? ("") : ("Activation Function: " + ActivationFunction.ToString() + "\r\n")) +
                    ((LayerType == LayerType.Convolutional || LayerType == LayerType.Subsampling) ? ("Receptive Field: " + ReceptiveFieldWidth.ToString(CultureInfo.CurrentCulture) + "x" + ReceptiveFieldHeight.ToString(CultureInfo.CurrentCulture) + "\r\n") : "") +
                    ((UseMapInfo) ? ("Maps: " + MapCount.ToString(CultureInfo.CurrentCulture) + "x(" + MapWidth.ToString(CultureInfo.CurrentCulture) + "x" + MapHeight.ToString(CultureInfo.CurrentCulture) + ")" + "\r\n") : ("")) +
                    "Neurons: " + NeuronCount.ToString(CultureInfo.CurrentCulture) + "\r\n" +
                    ((LayerType != LayerType.Input) ? ("Weights: " + Weights.Count().ToString(CultureInfo.CurrentCulture) + "\r\n") : ("")) +
                    ((LayerType == LayerType.Input) ? ("") : ("Connections: " + conn.ToString(CultureInfo.CurrentCulture) + "\r\n"));


            if (PreviousLayer != null)
            {
                PreviousLayer.NextLayer = this;
            }
        }

        private static double Sigmoid(double value)
        {
            return Math.Tanh(value);
            //return ((1.7159D * Math.Tanh(twodivthree * value)));
        }

        private static double DSigmoid(double value)
        {
            return 1D - (value * value);
        }

        private static double Logistic(double value)
        {
            double a = Math.Exp(value);
            return (a / (a + 1D));
        }

        private static double DLogistic(double value)
        {
            return (value * (1D - value));
        }

        private static double Gaussian(double value)
        {
            return value;

            //return Math.Exp(-0.5D*value);
        }
        private static double SoftReLU(double value)   // aka SoftPlus
        {
            if (value > 4D)
                return value;
            else
                return Math.Log(1D + Math.Exp(value));
        }
        private static double DReLU(double value)
        {
            //return (value > 0D) && (value <= 6) ? 1D : 0D;

            return value > 0D ? 1D : 0D;
        }
        private static double ReLU(double value)
        {
            //return value < 0D ? 0D : value > 6 ? 6 : value;
            return value < 0D ? 0D : value;
        }
        //public void CalculateL2PoolingTanh()
        //{
        //    Parallel.For(0, NeuronCount, Network.ParallelOption, i =>
        //    {
        //        double dSum = 0D;
        //        for (int c = 0; c < Connections[i].Length; c++)
        //            dSum += MathUtil.Pow2(PreviousLayer.Neurons[Connections[i][c].ToNeuronIndex].Output);

        //        Neurons[i].Output = Sigmoid((dSum / Connections[i].Length));
        //    });
        //}

        private static double DGaussian(double value)
        {
            return value;

            //return -(Math.Exp(-0.5D*value) / 2);
        }

        private static double Sech(double value)
        {
            return (2D * Math.Exp(value)) / (Math.Exp(2D * value) + 1D);
        }

        public void CalculateSoftMax()
        {
            Parallel.For(0, NeuronCount, Network.ParallelOption, i =>
            {
                //double bias = Weights[i * (PreviousLayer.NeuronCount+1)].Value;
                int idx = i * (PreviousLayer.NeuronCount + 1);
                double dSum = Weights[idx++].Value;
                for (int c = 0; c < PreviousLayer.NeuronCount; c++)
                    dSum += Weights[c + idx].Value * PreviousLayer.Neurons[c].Output;
                //foreach (Connection connection in Connections[i])
                //{
                //    if (connection.ToNeuronIndex == int.MaxValue)
                //        dSum += Weights[connection.ToWeightIndex].Value;
                //    else
                //        dSum += Weights[connection.ToWeightIndex].Value * PreviousLayer.Neurons[connection.ToNeuronIndex].Output;
                //}
                Neurons[i].Output = dSum;
            });

            //Parallel.For(0, NeuronCount, Network.ParallelOption, i =>
            //{
            //    double dSum = 0D;
            //    foreach (Connection connection in Connections[i])
            //    {
            //        if (connection.ToNeuronIndex == int.MaxValue)
            //            dSum += Weights[connection.ToWeightIndex].Value;
            //        else
            //            dSum += Weights[connection.ToWeightIndex].Value * PreviousLayer.Neurons[connection.ToNeuronIndex].Output;
            //    }
            //    Neurons[i].Output = dSum;
            //});

            double hi = Neurons[0].Output;
            for (int i = 1; i < NeuronCount; i++)
                if (Neurons[i].Output > hi)
                    hi = Neurons[i].Output;
            //double hi = Neurons.Max(neuron => neuron.Output);

            double total = 0D;
            for (int i = 0; i < NeuronCount; i++)
                total += Math.Exp(Neurons[i].Output - hi);

            double logsumexp = hi + Math.Log(total);
            for (int i = 0; i < NeuronCount; i++)
                Neurons[i].Output = Math.Exp(Neurons[i].Output - logsumexp);
        }

        private static double Rectification(double value)
        {
            return Math.Abs(Math.Tanh(value));
        }

        private static double DRectification(double value)
        {
            //double x = Math.Pow(Sech(value), 2D);
            //double y = Math.Tanh(value);
            //return (x * y / Math.Abs(y));
            return DSigmoid(Math.Sign(value));  // Is this correct???
            //return (Math.Sign(Math.Tanh(value)) * DSigmoid(value));
        }

        private static double Median(List<double> values)
        {
            values.Sort();

            return (values[(values.Count - 1) / 2]);
        }

        public void SetInitalWeights(bool useFannIn = true, double weightScope = 2D, double weightFactor = 1D)
        {
            if (!useFannIn)
            {
                for (int i = 0; i < WeightCount; i++)
                {
                    Weights[i].Value = ((Network.RandomGenerator.NextDouble() * weightScope) - (weightScope / 2D)) * weightFactor;
                }
            }
            else
            {
                switch (LayerType)
                {
                    //case LayerType.RBF:
                    //    int index = 0;
                    //    foreach (Neuron neuron in Neurons)
                    //    {
                    //        byte[] weightImage = new byte[12];
                    //        weightImage = Network.RbfWeights[index++].ToArray();
                    //        double[] realWeights = new double[(7 * 12)];
                    //        int row = 0;
                    //        for (int y = 0; y < 12; y++)
                    //        {
                    //            row = (int)weightImage[y];
                    //            realWeights[0 + (7 * y)] = (((128 & ~row) / 128) * 2) - 1;
                    //            realWeights[1 + (7 * y)] = (((64 & ~row) / 64) * 2) - 1;
                    //            realWeights[2 + (7 * y)] = (((32 & ~row) / 32) * 2) - 1;
                    //            realWeights[3 + (7 * y)] = (((16 & ~row) / 16) * 2) - 1;
                    //            realWeights[4 + (7 * y)] = (((8 & ~row) / 8) * 2) - 1;
                    //            realWeights[5 + (7 * y)] = (((4 & ~row) / 4) * 2) - 1;
                    //            realWeights[6 + (7 * y)] = (((2 & ~row) / 2) * 2) - 1;
                    //        }
                    //        foreach (Connection connection in neuron.Connections) //84x
                    //        {
                    //            Weights[connection.ToWeightIndex].Value = (realWeights[connection.ToNeuronIndex] == 1D) ? Network.TrainToValue : -Network.TrainToValue;
                    //        }
                    //    };
                    //    break;

                    default:
                        int windowNeuronCount = 0;
                        foreach (Connection connection in Neurons[0].Connections)
                        {
                            if (connection.ToNeuronIndex != int.MaxValue)
                                windowNeuronCount++;
                        }

                        //weightFactor = Math.Pow((double)windowNeuronCount, -0.5D);
                        weightFactor = 1D;
                        weightScope = 1D / Math.Sqrt((double)windowNeuronCount);
                        if ((Weights != null) && (WeightCount > 0))
                        {
                            Parallel.ForEach(Neurons, Network.ParallelOption, neuron =>
                            {
                                foreach (Connection connection in neuron.Connections)
                                {
                                    Weights[connection.ToWeightIndex].Value = ((Network.RandomGenerator.NextDouble() * weightScope) - (weightScope / 2D)) * weightFactor;
                                }
                            });
                        }
                        break;
                }
            }
        }

        public void Calculate()
        {
            switch (LayerType)
            {
                case LayerType.Convolutional:
                case LayerType.ConvolutionalSubsampling:
                    switch (ActivationFunction)
                    {
                        case ActivationFunction.ReLU:
                            Parallel.ForEach(Neurons, Network.ParallelOption, neuron =>
                            {
                                double dSum = 0D;
                                foreach (Connection connection in neuron.Connections)
                                {
                                    if (connection.ToNeuronIndex == int.MaxValue)
                                        dSum += Weights[connection.ToWeightIndex].Value;
                                    else
                                        dSum += Weights[connection.ToWeightIndex].Value * PreviousLayer.Neurons[connection.ToNeuronIndex].Output;
                                }
                                neuron.Output = ReLU(dSum);
                            });
                            break;
                        case ActivationFunction.Tanh:
                            Parallel.ForEach(Neurons, Network.ParallelOption, neuron =>
                            {
                                double dSum = 0D;
                                foreach (Connection connection in neuron.Connections)
                                {
                                    if (connection.ToNeuronIndex == int.MaxValue)
                                        dSum += Weights[connection.ToWeightIndex].Value;
                                    else
                                        dSum += Weights[connection.ToWeightIndex].Value * PreviousLayer.Neurons[connection.ToNeuronIndex].Output;
                                }
                                neuron.Output = Sigmoid(dSum);
                            });
                            break;

                        case ActivationFunction.AbsTanh:
                            Parallel.ForEach(Neurons, Network.ParallelOption, neuron =>
                            {
                                double dSum = 0D;
                                foreach (Connection connection in neuron.Connections)
                                {
                                    if (connection.ToNeuronIndex == int.MaxValue)
                                        dSum += Weights[connection.ToWeightIndex].Value;
                                    else
                                        dSum += Weights[connection.ToWeightIndex].Value * PreviousLayer.Neurons[connection.ToNeuronIndex].Output;
                                }
                                neuron.Output = Rectification(dSum);
                            });
                            break;
                    }
                    break;

                case LayerType.FullyConnected:
                    Parallel.ForEach(Neurons, Network.ParallelOption, neuron =>
                    {
                        double dSum = 0D;
                        foreach (Connection connection in neuron.Connections)
                        {
                            if (connection.ToNeuronIndex == int.MaxValue)
                                dSum += Weights[connection.ToWeightIndex].Value;
                            else
                                dSum += Weights[connection.ToWeightIndex].Value * PreviousLayer.Neurons[connection.ToNeuronIndex].Output;
                        }
                        neuron.Output = Sigmoid(dSum);
                    });
                    break;

                case LayerType.Subsampling:
                    switch (ActivationFunction)
                    {
                        case ActivationFunction.AveragePoolingTanh:
                            Parallel.ForEach(Neurons, Network.ParallelOption, neuron =>
                            {
                                double dSum = 0D;
                                foreach (Connection connection in neuron.Connections)
                                {
                                    if (connection.ToNeuronIndex == int.MaxValue)
                                        dSum += Weights[connection.ToWeightIndex].Value;
                                    else
                                        dSum += Weights[connection.ToWeightIndex].Value * PreviousLayer.Neurons[connection.ToNeuronIndex].Output * SubsamplingScalingFactor;
                                }
                                neuron.Output = Sigmoid(dSum);
                            });
                            break;
                        case ActivationFunction.L2PoolingTanh:
                            Parallel.ForEach(Neurons, Network.ParallelOption, neuron =>
                            {
                                double dSum = 0D;
                                foreach (Connection connection in neuron.Connections)
                                {
                                    if (connection.ToNeuronIndex == int.MaxValue)
                                        dSum += Weights[connection.ToWeightIndex].Value;
                                    else
                                        dSum += Weights[connection.ToWeightIndex].Value
                                            * PreviousLayer.Neurons[connection.ToNeuronIndex].Output;

                                    dSum = Math.Pow(dSum, 2);
                                }
                                neuron.Output = Sigmoid(dSum * SubsamplingScalingFactor);
                            });
                            break;
                        case ActivationFunction.MaxPoolingTanh:
                            Parallel.ForEach(Neurons, Network.ParallelOption, neuron =>
                            {
                                double bias = 0D;
                                double weight = 1D;
                                List<double> previousOutputs = new List<double>(4);
                                foreach (Connection connection in neuron.Connections)
                                {

                                    if (connection.ToNeuronIndex == int.MaxValue)
                                        bias = Weights[connection.ToWeightIndex].Value;
                                    else
                                    {
                                        weight = Weights[connection.ToWeightIndex].Value;
                                        previousOutputs.Add(PreviousLayer.Neurons[connection.ToNeuronIndex].Output);
                                    }
                                }
                                neuron.Output = Sigmoid((previousOutputs.Max() * weight) + bias);
                            });
                            break;

                        case ActivationFunction.MedianPoolingTanh:
                            Parallel.ForEach(Neurons, Network.ParallelOption, neuron =>
                            {
                                double bias = 0D;
                                double weight = 1D;
                                List<double> Outputs = new List<double>(3);
                                List<double> Q = new List<double>(3);
                                foreach (Connection connection in neuron.Connections)
                                {

                                    if (connection.ToNeuronIndex == int.MaxValue)
                                        bias = Weights[connection.ToWeightIndex].Value;
                                    else
                                    {
                                        weight = Weights[connection.ToWeightIndex].Value;
                                        Outputs.Add(PreviousLayer.Neurons[connection.ToNeuronIndex].Output);
                                    }
                                }

                                List<double> rOutputs = new List<double>();
                                foreach (double output in Outputs)
                                {
                                    rOutputs.Add(output);
                                }

                                foreach (double output in Outputs)
                                {
                                    rOutputs.Remove(output);
                                    Q.Add(Math.Pow(output - Median(rOutputs), 2D));
                                    rOutputs.Add(output);
                                }

                                int bestIndex = Q.IndexOf(Q.Min());

                                neuron.Output = Sigmoid((Outputs[bestIndex] * weight) + bias);
                            });
                            break;
                    }
                    break;

                case LayerType.RBF:
                    Parallel.ForEach(Neurons, Network.ParallelOption, neuron =>
                    {
                        double dSum = 0;
                        double x = 0D;
                        foreach (Connection connection in neuron.Connections)
                        {
                            x = PreviousLayer.Neurons[connection.ToNeuronIndex].Output - Weights[connection.ToWeightIndex].Value;
                            dSum += x * x;
                        }
                        neuron.Output = Gaussian(dSum);
                    });
                    break;
            }
        }
    }
}
