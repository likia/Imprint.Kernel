using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Imprint.Db.Pool
{
    /// <summary>
    /// 连接池
    /// </summary>
    public class ConnectionPool<T> where T : new()
    {
        ConcurrentQueue<T> pool = new ConcurrentQueue<T>();
        Thread worker;
        IConnector<T> connector;
        readonly Object sync = new Object();
        Object connectorConfig;
        Hashtable map = new Hashtable();

        // 当前连接数 
        private int connCount = 0;

        /// <summary>
        /// 连接等待时间
        /// </summary>
        public int WaitTime
        {
            get;
            set;
        }

        /// <summary>
        /// 空闲检测时间
        /// </summary>
        public int IdleCheckTime
        {
            get;
            set;
        }

        /// <summary>
        /// 最大空闲时间
        /// </summary>
        public int MaxIdleTime
        {
            get;
            set;
        }

        /// <summary>
        /// 最大活跃连接
        /// </summary>
        public int MaxActive
        {
            get;
            set;
        }

        /// <summary>
        /// 最小活跃连接数
        /// </summary>
        public int MinActive
        {
            get;
            set;
        }

        public ConnectionPool(IConnector<T> connector, Object connectorConfig)
        {
            this.connector = connector;
            this.connectorConfig = connectorConfig;
            // 启动线程
            worker = new Thread(() =>
            {
                while (true)
                {
                    if (connCount > MinActive && pool.Count > 0)
                    {

                        var list = pool.ToArray();
                        var toRemove = new List<T>();
                        // 找出空闲连接
                        foreach (T conn in list)
                        {
                            var lastActive = map[conn];
                            if (lastActive == null)
                            {
                                lastActive = DateTime.MinValue;
                            }
                            if (DateTime.Now.Subtract((DateTime)lastActive).TotalMilliseconds > MaxIdleTime)
                            {
                                toRemove.Add(conn);
                            }
                        }
                        // 移除空闲连接
                        list = pool.Where(i => !toRemove.Contains(i)).ToArray();
                        pool = new ConcurrentQueue<T>();
                        foreach (var i in list)
                        {
                            pool.Enqueue(i);
                        }
                        foreach (var i in toRemove)
                        {
                            removeConnection(i);
                        }
                    }

                    Thread.Sleep(IdleCheckTime);
                }
            })
            {
                IsBackground = true
            };
            worker.Start();
        }

        /// <summary>
        /// 创建连接
        /// </summary>
        /// <returns></returns>
        private T createConnection()
        {
            connCount++;
            var connection = connector.Connect(connectorConfig);
            lock (map.SyncRoot)
            {
                map[connection] = DateTime.Now;
            }
            return connection;
        }

        /// <summary>
        /// 移除连接
        /// </summary>
        private void removeConnection(T conn)
        {
            connCount--;
            new Thread(() =>
            {
                try
                {
                    lock (map.SyncRoot)
                    {
                        map.Remove(conn);
                    }
                    connector.Disconnect(conn);
                }
                catch (Exception ex)
                {
                }
            })
            { IsBackground = true }.Start();
        }

        ///// <summary>
        ///// 消费一个链接
        ///// </summary>
        ///// <returns></returns>
        public T Borrow()
        {
            if (connCount < MaxActive)
            {
                // 增加新连接 
                return createConnection();
            }
            T connection = default(T);

            int retry = WaitTime;
            var flag = false;
            do
            {
                if (pool.Count > 0)
                {
                    flag = pool.TryDequeue(out connection);
                }

                if (!flag)
                {
                    Thread.Sleep(64);
                }
            }
            while (
                !flag &&
                (retry -= 64) > 0
            );
            // 超时
            if (!flag)
            {
                throw new Exception("borrow timeout.");
            }

            if (connector.IsConnected(connection))
            {
                // 初始化连接
                connector.Reset(connection);
            }
            else
            {
                removeConnection(connection);
                connection = createConnection();
            }
            return connection;
        }

        /// <summary>
        /// 关闭连接池
        /// </summary>
        public void Close()
        {
            worker.Abort();
            new Thread(() =>
            {
                while (pool.Count > 0)
                {
                    pool.TryDequeue(out T conn);
                    if (conn != null)
                    {
                        connector.Disconnect(conn);
                    }
                }
            })
            { IsBackground = true }.Start();
        }

        /// <summary>
        /// 归还连接
        /// </summary>
        /// <param name="connection"></param>
        public void Return(T connection)
        {

            if (pool.Count >= MaxActive)
            {
                removeConnection(connection);
            }
            else
            {
                lock (map.SyncRoot)
                {
                    map[connection] = DateTime.Now;
                }
                pool.Enqueue(connection);
            }
        }
    }
}