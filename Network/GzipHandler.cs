using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imprint.Network
{
    public class GzipHandler :  ReflectionHandler
    {
        protected ReflectionHandler baseHandler;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseHandler">基础handler解压缩后使用这个handler处理</param>
        public GzipHandler(ReflectionHandler baseHandler)
        {
            this.baseHandler = baseHandler;
        }

        public object DataToObject(byte[] Data)
        {
            var mem = new MemoryStream(Data);
            // 解压缩
            using (var stream = new GZipStream(mem, CompressionMode.Decompress))
            {
                using (var resultStream = new MemoryStream())
                {
                    var buffer = new byte[10240];
                    int c = 0;
                    while ((c = stream.Read(buffer, 0, 10240)) != 0)
                    {
                        resultStream.Write(buffer, 0, c);
                    }
                    buffer = resultStream.ToArray();
                    return baseHandler.DataToObject(buffer);
                }
            }
        }


        public byte[] ObjectToData(object Obj)
        {
            var buf = baseHandler.ObjectToData(Obj);
            // 压缩
            using (var resultStream = new MemoryStream())
            {
                using (var stream = new GZipStream(resultStream, CompressionMode.Compress))
                {
                    stream.Write(buf, 0, buf.Length);
                }
                return resultStream.ToArray();
            }
        }
    }
}
