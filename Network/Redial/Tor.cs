using Imprint.Core;
using System;
using System.Net;

namespace Imprint.Kernel.Network.Redial
{
	public class Tor : IRedial
	{
		public RedialStatus GetStatus()
		{
			try
			{
				WebClient webClient = new WebClient();
				webClient.Proxy = new WebProxy("4cat.org:48888");
				webClient.DownloadData("http://baidu.com/");
				return RedialStatus.Connected;
			}
			catch
			{
				return RedialStatus.Disconnected;
			}
		}

		public void Init(dynamic param)
		{
			throw new NotImplementedException();
		}

		public void Offline()
		{
		}

		public void Online()
		{
			while (true)
			{
				try
				{
					WebClient webClient = new WebClient();
					webClient.DownloadData("https://4spc.us/ctl.php");
					return;
				}
				catch
				{
				}
			}
		}
	}
}
