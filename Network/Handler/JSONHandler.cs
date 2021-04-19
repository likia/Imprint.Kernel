using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imprint.Network.Handler
{

    public class JsonHandler : ReflectionHandler
    {
        bool FormSubmit = false;

        public JsonHandler(bool submitFormData = false)
        {
            FormSubmit = submitFormData;
        }

        public object DataToObject(byte[] Data)
        {
            var str = Encoding.Default.GetString(Data);
            var jsonObj = JObject.Parse(str);
            return jsonObj;
        }

        public byte[] ObjectToData(object Obj)
        {
            return Encoding.Default.GetBytes(
                JObject.FromObject(Obj).ToString());
        }
    }
}
