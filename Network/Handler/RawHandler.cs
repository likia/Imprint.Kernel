using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imprint.Network.Handler
{
    public class RawHandler : ReflectionHandler
    {
        public object DataToObject(byte[] Data)
        {
            return Data;
        }

        public byte[] ObjectToData(object Obj)
        {
            return (byte[])Obj;
        }
    }
}
