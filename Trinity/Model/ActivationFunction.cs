namespace Trinity.Model
{
    /// <summary>
    /// 激活函数
    /// TODO: 只支持TANH
    /// </summary>
    public enum ActivationFunction
    {
        AbsTanh = 0,
        AveragePoolingTanh = 1,
        Gaussian = 2,
        Linear = 3,
        Logistics = 4,
        MaxPoolingTanh = 5,
        MedianPoolingTanh = 6,
        None = 7,
        Tanh = 8,
        L2PoolingTanh = 9,
        ReLU = 10,
        SoftPlus = 11,
    }
}
