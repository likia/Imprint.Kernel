using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imprint.Network.Tcp
{
    [Serializable]
    public class Packet<T>
    {
        // 头部魔数
        public const UInt32 MAGIC = 0xcafecafe;

        public T Data
        {
            get;
            set;
        }


        /// <summary>
        /// 转换成二进制数据
        /// 
        /// Json + gzip压缩
        /// </summary>
        /// <returns></returns>
        public byte[] ToBuffer()
        {
            var jsonStr = JsonConvert.SerializeObject(this);
            var buf = Encoding.UTF8.GetBytes(jsonStr);
            using (var ms = new MemoryStream())
            {
                using (var gzipStream = new GZipStream(ms, CompressionLevel.Optimal))
                {
                    gzipStream.Write(buf, 0, buf.Length);
                }
                return ms.ToArray();
            }
        }

        /// <summary>
        /// 字节数据写入流中
        /// </summary>
        /// <param name="stream"></param>
        public async System.Threading.Tasks.Task WriteToStream(Stream stream)
        {
            var buf = ToBuffer();
            var len = BitConverter.GetBytes((uint)buf.Length);
            var head = BitConverter.GetBytes(MAGIC);

            await stream.WriteAsync(head, 0, head.Length);
            await stream.WriteAsync(len, 0, len.Length);
            await stream.WriteAsync(buf, 0, buf.Length);
        }

        /// <summary>
        /// 从二进制数据中反序
        /// 
        /// Json + gzip压缩
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public async static Task<Packet<T>> FromStream(Stream bufferStream)
        {
            var header = new byte[sizeof(uint)];
            await bufferStream.ReadAsync(header, 0, header.Length);
            var magic = BitConverter.ToUInt32(header,0);
            
            // 标识
            if (magic == MAGIC)
            {
                // 读长度
                await bufferStream.ReadAsync(header, 0, header.Length);
                var len = BitConverter.ToUInt32(header, 0);
                var buf = new byte[len];
                await bufferStream.ReadAsync(buf, 0, (int)len);

                using (var ms = new MemoryStream(buf))
                {
                    using (var resStream = new MemoryStream())
                    {
                        using (var gzipStream = new GZipStream(ms, CompressionMode.Decompress))
                        {
                            var l = 0;
                            
                            byte[] tmp = new byte[2048];

                            while ((l = await gzipStream.ReadAsync(tmp, 0, 2048)) > 0)
                            {
                                resStream.Write(tmp, 0, l);
                            }
                            var res = resStream.ToArray();
                            var jsonStr = Encoding.UTF8.GetString(res);
                            return JsonConvert.DeserializeObject<Packet<T>>(jsonStr);
                        }
                    }
                }
            }
            return null;
        }
    }
}