using Imprint.Attributes.IOC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;



namespace Imprint.IOC
{
    /// <summary>
    /// IOC容器
    /// </summary>
    public class Container
    {
        HashSet<Type> registerType = new HashSet<Type>();
        Dictionary<Type, object> registeInstance = new Dictionary<Type, object>();
        Dictionary<string, Type> namedType = new Dictionary<string, Type>();
        Dictionary<Type, Type> abstractImplMap = new Dictionary<Type, Type>();
        
        // 最后处理注入的队列
        Queue<Tuple<Type, object>> injectQueue = new Queue<Tuple<Type, object>>();

        // 延迟注册bean队列
        Queue<Tuple<MethodInfo, Type, string>> beanQueue = new Queue<Tuple<MethodInfo, Type, string>>();

        public Container()
        {
        }

        /// <summary>
        /// 扫描注解 注册服务
        /// </summary>
        /// <param name="container"></param>
        /// <param name="assemblies"></param>
        public void ScanAttribute(Assembly[] assemblies)
        {
            foreach (var ass in assemblies)
            {
                var typeList = ass.GetTypes()
                    .Where(i => i.GetCustomAttribute<ServiceAttribute>(true) != null)
                    .ToArray();
                foreach (var type in typeList)
                {
                    var attr = type.GetCustomAttribute<ServiceAttribute>(true);
                    // 注册type
                    var interfaces = type.GetInterfaces();
                    if (interfaces.Length > 0)
                    {
                        if (attr.Name != "")
                        {
                            registerType.Add(type);
                            namedType.Add(attr.Name, type);
                        }

                        // 注册抽象/实现map
                        foreach (var _interface in interfaces)
                        {
                            Set(type, _interface);
                        }
                    }
                    else
                    {
                        Set(type, null, attr.Name);
                    }

                    // 注册bean
                    var beanMakerList = type.GetMethods()
                        .Where(i => i.GetCustomAttribute<ServiceAttribute>() != null)
                        .ToArray();

                    if (beanMakerList.Length > 0)
                    {
                        foreach (var method in beanMakerList)
                        {
                            // 执行并注入实例
                            var mattr = method.GetCustomAttribute<ServiceAttribute>();
                            var name = !string.IsNullOrEmpty(mattr.Name) ?
                                      mattr.Name : method.Name;

                            beanQueue.Enqueue((method, type, name).ToTuple());
                        }
                    }
                }
            }

            // 注册bean
            while (beanQueue.Count > 0)
            {
                var (method, type, name) = beanQueue.Dequeue();
                registerBean(method, type, name);
            }
        }

        /// <summary>
        /// 注册bean
        /// </summary>
        void registerBean(MethodInfo method, Type type, string name)
        {
            var paramList = method.GetParameters();
            object[] realParamList = new object[paramList.Length];
            int i = 0;
            foreach (var p in paramList)
            {
                var pattr = p.GetCustomAttribute<InjectAttribute>();

                if (pattr != null && !string.IsNullOrEmpty(pattr.Name))
                {
                    // 指定注入
                    realParamList[i++] = Get(p.ParameterType, pattr.Name);
                } 
                else
                {
                    realParamList[i++] = Get(p.ParameterType);
                }
            }
            var service = Get(type);
            var obj = method.Invoke(service, realParamList);
            Set(obj, name);
        }


        /// <summary>
        /// 注册类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        public void Set<T>(string key = "")
        {
            var type = typeof(T);
            Set(type, null, key);
        }

        /// <summary>
        /// 注册接口 & 实现
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="I"></typeparam>
        /// <param name="key"></param>
        public void Set<T, I>(string key = "")
        {
            var _interface = typeof(I);
            var type = typeof(T);

            Set(type, _interface, key);
        }

        public void Set(Type type, Type _interface = null, string key = "")
        {
            registerType.Add(type);
            if (!string.IsNullOrEmpty(key))
            {
                namedType.Add(key, type);
            }
            if (_interface != null)
            {
                abstractImplMap.Add(_interface, type);
            }
        }

        /// <summary>
        /// 注册单例
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="key"></param>
        public void Set(object instance, string key = "")
        {
            Type type = instance.GetType();
            if (!string.IsNullOrEmpty(key))
            {
                registerType.Add(type);
                namedType.Add(key, type);
            }
            registeInstance.Add(type, instance);
        }

        /// <summary>
        /// 获取实例
        /// </summary>
        /// <param name="type"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public object Get(Type type, string key = "") 
        {
            if (!string.IsNullOrEmpty(key) && namedType.ContainsKey(key))
            {
                type = namedType[key];
            }

            // 如果单例0.
            if (registeInstance.ContainsKey(type))
            {
                return registeInstance[type];
            }
            // 如果是接口
            if (type.IsInterface)
            {
                // 获取接口实现
                if (abstractImplMap.ContainsKey(type))
                {
                    type = abstractImplMap[type];
                }
            }
            else
            {
                // 有就创建 没有给null
                type = registerType.Contains(type) ?
                          type : null;
            }
            if (type == null)
                return null;

            return containerNew(type);
        }

        /// <summary>
        /// 获取实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Get<T>(string key = "")
        {
            Type type = typeof(T);
            
            return (T)Get(type, key);
        }

        /// <summary>
        /// 容器创建对象
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        protected object containerNew(Type type)
        {
            // 选择无参数造函数
            object obj = Activator.CreateInstance(type);

            // 最后操作
            injectQueue.Enqueue((type, obj).ToTuple());

            propertyInject(type, obj);
            fieldInject(type, obj);

            return obj;
        }


        /// <summary>
        /// 属性注入
        /// </summary>
        /// <param name="type"></param>
        /// <param name="instance"></param>
        void propertyInject(Type type, object instance)
        {
            // 搜索属性进行注入
            var propList = type.GetProperties()
                 .Where(i => i.GetCustomAttributes(typeof(InjectAttribute), true).Length > 0)
                 .ToList();
            foreach (var item in propList)
            {
                var ptype = item.PropertyType;
                var pattr = item.GetCustomAttributes(typeof(InjectAttribute), true)
                                .First() as InjectAttribute;
                item.SetValue(instance, Get(ptype, pattr.Name));
            }
        }

        /// <summary>
        /// 字段注入
        /// </summary>
        /// <param name="type"></param>
        /// <param name="instance"></param>
        void fieldInject(Type type, object instance)
        {
            // 搜索字段进行注入
            var fieldList = type.GetFields()
                .Where(i => i.GetCustomAttributes(typeof(InjectAttribute), true).Length > 0)
                .ToArray();
            foreach (var item in fieldList)
            {
                var ftype = item.FieldType;
                var fattr = item.GetCustomAttributes(typeof(InjectAttribute), true)
                                .First() as InjectAttribute;

                item.SetValue(instance, Get(ftype, fattr.Name));
            }
        }
    }
}