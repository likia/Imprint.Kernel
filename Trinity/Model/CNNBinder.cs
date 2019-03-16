using System;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace Trinity.Model
{
	public class CNNBinder : SerializationBinder
	{
		public override Type BindToType(string assemblyName, string typeName)
		{
            Assembly ass = Assembly.GetExecutingAssembly();
            return ass.GetType(typeName);
		}
	}
}
