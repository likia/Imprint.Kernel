using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imprint.Network.Handler
{
    public class PlainTextHandler : ReflectionHandler
    {
        private Encoding TextEncoding;

        public PlainTextHandler(Encoding Coding)
        {
            TextEncoding = Coding;
        }

        public PlainTextHandler()
        {
            TextEncoding = Encoding.GetEncoding("utf-8");
        }

        public object DataToObject(byte[] Data)
        {
            return TextEncoding.GetString(Data);
        }

        public byte[] ObjectToData(object Obj)
        {
            return TextEncoding.GetBytes((string)Obj);
        }
    }
}
