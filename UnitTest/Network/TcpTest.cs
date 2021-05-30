using Imprint.Model;
using Imprint.Network.Tcp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UnitTest.Network
{
    [TestClass]
    public class TcpTest
    {
        string time;

        [TestMethod]
        public async System.Threading.Tasks.Task TestServer()
        {
            var list = new List<Socket>();

            time = DateTime.Now.ToString();
            var server = new TcpServer<JObject>(33145);
            server.OnMessage += Server_OnMessage;
            Thread thread(ThreadStart ts)
            {
                var task = new Thread(ts) { IsBackground = true };
                task.Start();
                return task;
            }


            for (int i = 0; i < 100; i++)
            {
                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(new IPEndPoint(IPAddress.Loopback, 33145));
                var pack = new Packet<JObject>()
                {
                    Data = JObject.FromObject(new
                    {
                        Jsb = ((IPEndPoint)socket.LocalEndPoint).Port
                    })
                };
                using (var stream = new NetworkStream(socket, false))
                {
                    await pack.WriteToStream(stream);
                }
                list.Add(socket);
            }
            await Task<Object>.Delay(20 * 1000);
            Assert.AreEqual(counter, 100);
        }

        int counter = 0;
        private void Server_OnMessage(Socket arg1, Packet<JObject> arg2)
        {
            Assert.AreEqual(arg2.Data["Jsb"], ((IPEndPoint)arg1.RemoteEndPoint).Port);
            ++counter;
        }

        [TestMethod]
        public void PackTest()
        {
            System.Threading.Tasks.Task.Run(async () =>
            {
                var pack = new Packet<Result<string[]>>()
                {
                    Data = new Result<string[]>()
                    {
                        Data = new string[]
                      {
                          "ssd",
                          "sfdasdasd"
                      }
                    }
                };
                var data = pack.ToBuffer();

                var len = BitConverter.GetBytes((uint)data.Length);
                var head = BitConverter.GetBytes(Packet<object>.MAGIC);
                var ms = new MemoryStream();
                ms.Write(head, 0, head.Length);
                ms.Write(len, 0, len.Length);
                ms.Write(data, 0, data.Length);
                ms.Seek(0, SeekOrigin.Begin);

                var deserial = await Packet<Result<string[]>>.FromStream(ms);

                Assert.AreEqual(pack.Data.Data[1], deserial.Data.Data[1]);
            
            }).GetAwaiter().GetResult();
            
        }
    }
}
