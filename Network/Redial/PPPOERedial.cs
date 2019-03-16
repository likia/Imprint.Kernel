using Imprint.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace Imprint.Network
{
    public class PPPOERedial : IRedial
    {
        public RedialStatus GetStatus()
        {
            var client = new WebClient();
            try
            {
                client.DownloadData("http://baidu.com");

                return RedialStatus.Connected;
            }
            catch
            {
                return RedialStatus.Disconnected;
            }
        }

        public void Offline()
        {
            var proc = Process.Start(Environment.CurrentDirectory + "\\disconnect.bat");
            proc.WaitForExit();
            Thread.Sleep(1000);
        }

        public void Online()
        {
            var proc = Process.Start(Environment.CurrentDirectory + "\\connect.bat");
            proc.WaitForExit();
        }
    }
}
