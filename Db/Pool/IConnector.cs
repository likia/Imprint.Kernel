using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imprint.Db.Pool
{
    /// <summary>
    /// 连接池连接 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IConnector<T>
    {
        /// <summary>
        /// 连接
        /// </summary>
        /// <param name="param">配置 </param>
        /// <returns>连接对象</returns>
        T Connect(Object param);

        /// <summary>
        /// 断开连接,释放资源
        /// </summary>
        void Disconnect(T conn);

        /// <summary>
        /// 连接状态
        /// </summary>
        bool IsConnected(T connection);

        /// <summary>
        /// 重置当前连接至初始状态
        /// </summary>
        /// <param name="connection"></param>
        void Reset(T connection);
    }
}
