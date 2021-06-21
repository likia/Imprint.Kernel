using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imprint.Attributes.IOC
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ServiceAttribute : Attribute
    {
        public ServiceAttribute(string name = "")
        {
            Name = name;
        }

        public string Name { get; }
    }
}
