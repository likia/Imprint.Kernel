using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imprint.Network
{
    public interface ReflectionHandler
    {
        object DataToObject(byte[] Data);
        byte[] ObjectToData(object Obj);
    }
}
