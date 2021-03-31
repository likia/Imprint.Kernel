using Imprint.Network;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest.Network
{
    [TestClass]

    public class WebSessionTest
    {
        WebSession session = new WebSession();

        [TestMethod]
        public void TestGet()
        {
            Assert.IsNotNull(
                session.Get("https://www.baidu.com/")
            );
        }

        [TestMethod]
        public void TestGetRaw()
        {
            Assert.IsNotNull(
                session.GetRaw("https://www.baidu.com/img/flexible/logo/pc/result.png")
            );
        }
    }
}
