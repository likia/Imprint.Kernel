using Imprint.Network;
using Imprint.Task;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Threading;

namespace UnitTest.Task
{
    [TestClass]
    public class DispatcherTest
    {
        

        public TestContext TestContext { get; set; }

        class WebTimeJob : WorkerJob
        {
            public TestContext TestContext { get; set; }

            WebSession session;

            public WebTimeJob()
            {
                var kv = new System.Collections.Specialized.NameValueCollection();
                kv.Add("User-Agent", UserAgent.PC);
                session = new WebSession(customHeader: kv);
            }

            public override void Run()
            {
                var html = session.Get("http://time.tianqi.com/") as string;
                if (html != null)
                {
                    var time = StrHelper.GetStrBetween(html, "<p id=\"times\">", "</p>");
                    Debug.WriteLine(time);
                }
            }
        }

        bool flag = false;

        WorkerDispatcher dispatcher;

        [TestInitialize]
        public void Init()
        {
            dispatcher = new WorkerDispatcher();
            dispatcher.Start(10, 0);
            flag = false;
        }

        [TestMethod]
        public void TestEvent()
        {
            dispatcher.Append(new WebTimeJob()
            {
                TestContext = TestContext,
                Repeat = -1
            });
            dispatcher.DispatchFinished += Dispatcher_DispatchFinished;

            new Thread(() =>
            {
                Thread.Sleep(20 * 1000);
                dispatcher.Halt();
            }).Start();

            while(!flag)
            {
                Thread.Sleep(100);
            }
        }


        [TestMethod]
        public void Test2()
        {

        }



        private void Dispatcher_DispatchFinished(object sender)
        {
            Debug.WriteLine("FINISHED...");
            flag = true;
        }
    }


}
