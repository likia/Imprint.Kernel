using Imprint.Attributes.IOC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

public static class Extensions
{
    /// <summary>
    /// 扫描标记类
    /// </summary>
    /// <param name="assembly"></param>
    /// <returns></returns>
    public static Type[] GetAllServices(this Assembly assembly)
    {
        return assembly.GetTypes()
             .Where(i => i.GetCustomAttribute<ServiceAttribute>() != null)
             .ToArray();
    }
}