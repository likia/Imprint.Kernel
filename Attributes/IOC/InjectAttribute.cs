using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imprint.Attributes.IOC
{
    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter)]
   public  class InjectAttribute : Attribute
    {
        public InjectAttribute(string name = "")
        {
            Name = name;
        }

        public string Name { get; }
    }
}
