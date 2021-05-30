using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Imprint.Network.Tcp
{
    public class TcpServer<T>
    {
        Thread acceptTask, readTask;

        TcpListener server;

        bool flag = true;

        List<Socket> connections = new List<Socket>();

        public event Action<Socket, Packet<T>> OnMessage;

        public event Action<Socket> OnConnect;

        public event Action<Socket> OnError;

        public event Action<Socket> OnDisconnect;

        public TcpServer(int port, string addr = "0.0.0.0")
        {
            server = new TcpListener(IPAddress.Parse(addr), port);
            server.Start();

            Thread thread(ThreadStart ts)
            {
                var task = new Thread(ts) { IsBackground = true };
                task.Start();
                return task;
            }

            acceptTask = thread(accept);
            readTask = thread(readData);
        }

        /// <summary>
        /// 关闭服务
        /// </summary>
        public void Stop()
        {
            flag = false;
            server.Stop();
        }


        async void accept()
        {
            while (flag)
            {
                try
                {
                    var socket = await server.AcceptSocketAsync();
                    connections.Add(socket);
                    OnConnect?.Invoke(socket);
                }
                catch (Exception ex)
                {

                }
            }
        }

        async void readData()
        {
            while(flag)
            {
                // 复制一份来select
                if (connections.Count > 0)
                {
                    var readList = connections.ToList();
                    var errList = connections.ToList();

                    Socket.Select(readList, null, errList, 64);

                    var err = errList.Where(i => i != null)
                                     .ToArray();
                    foreach (var item in err)
                    {
                        connections.Remove(item);
                        OnError?.Invoke(item);
                    }

                    try
                    {
                        var read = readList.Where(i => i != null)
                                           .ToArray();
                        foreach (var item in read)
                        {
                            var stream = new NetworkStream(item, false);

                            var data = await Packet<T>.FromStream(stream);
                            OnMessage?.Invoke(item, data);
                        }
                    } 
                    catch(Exception ex)
                    {

                    }
                }
                else
                    await Task<object>.Delay(64);
            }
        }
    }
}