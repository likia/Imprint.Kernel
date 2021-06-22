using Imprint.Attributes.IOC;
using Imprint.IOC;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest.IOC
{
    [TestClass]
    public class ContainerTest
    {
        Container container = new Container();

        [TestMethod]
        public void testAttribute()
        {
            container.ScanAttribute(new Assembly[] { Assembly.GetAssembly(this.GetType()) });
            Assert.IsNotNull(container.Get<IServiceA>());

            Assert.IsNotNull(container.Get<ServiceD>());
            Assert.AreEqual(Config.obj, container.Get<ServiceB>("myServiceB"));
            Assert.IsNotNull(container.Get<ServiceC>("svcC"));
            Assert.IsNotNull(container.Get<IServiceC>("svcC"));


            var d = container.Get<ServiceD>();
            d.test();
        }

    }


    interface IServiceC { }

    [Service("svcC")]
    class ServiceC : IServiceC { }

    interface IServiceA { }

    [Service]
    class ServiceA : IServiceA { 
    }

    interface IServiceB { }

    class ServiceB : IServiceB
    {
       
    }

    [Service]
    class Config
    {
        public static ServiceB obj = new ServiceB();

        [Service]
        public IServiceB myServiceB(IServiceA svcA, [Inject("svcC")] IServiceC c)
        {
            Assert.IsNotNull(svcA);
            Assert.IsNotNull(c);

            return obj;
        }

    }


    [Service]
    class ServiceD
    {
        [Inject]
        public IServiceA a { get; set; }

        [Inject]
        public IServiceA aa;

        public void test()
        {
            Assert.IsNotNull(a);
            Assert.IsNotNull(aa);
        }
    }
}