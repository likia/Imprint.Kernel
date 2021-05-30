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
        public async System.Threading.Tasks.Task TestGet()
        {
            var html = session.Get("http://time.tianqi.com/") as string;
            Assert.IsNotNull(
                html
            );
            var html2 = await session.GetAsync("http://time.tianqi.com/") as string;

           Assert.AreEqual(html.Substring(0,300), html2.Substring(0,300));
           Assert.IsTrue(html.IndexOf("北京时间校准") != -1);
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
