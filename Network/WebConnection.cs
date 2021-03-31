using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Collections.Specialized;
using System.Collections;
using System.Threading;
using Imprint.Network.Handler;

namespace Imprint.Network
{
    public enum RequestMethod
    {
        GET = 1,
        POST = 2,
        PUT = 3,
        DELETE = 4,
        OPTIONS = 5
    }

    public class WebConnection
    {
        public event RequestFinishedHandler RequestFinished;
        public NameValueCollection ResponseHeaders;
        public string LastError;
        public int Retry = 3;
        public static bool EnableMultiServerAccelerate { get; set; }
        private ReflectionHandler Processor;
        private HttpWebRequest BaseRequest;
        public object ResponseObject;
        private RequestMethod ReqMethod = RequestMethod.GET;
        private object ReqObject;
        private bool AllowRedirect = false;
        public HttpWebResponse LastResponse;

        public WebConnection(string Url, ReflectionHandler Handle)
        {
            Processor = Handle;
            InitRequest(Url);
        }

        public WebConnection(string Url)
        {
            Processor = new PlainTextHandler();
            InitRequest(Url);
        }

        ~WebConnection()
        {
            try
            {
                BaseRequest.Abort();
            }
            catch
            {
            }
            BaseRequest = null;
        }

        private void InitRequest(string Url)
        {
            BaseRequest = (HttpWebRequest)HttpWebRequest.Create(Url);
            BaseRequest.Timeout = 10000;
            BaseRequest.ServicePoint.Expect100Continue = false;
            BaseRequest.ServicePoint.ConnectionLimit = 30000;
        }

        public WebConnection SetProcessor(ReflectionHandler handler)
        {
            Processor = handler;
            return this;
        }

        public string GetDomain()
        {
            return BaseRequest.RequestUri.Host;
        }

        /// <summary>
        /// 超时时间
        /// </summary>
        /// <param name="Timeout"></param>
        /// <returns></returns>
        public WebConnection SetTimeout(int Timeout)
        {
            BaseRequest.Timeout = Timeout;
            return this;
        }


        /// <summary>
        /// 设置代理
        /// </summary>
        /// <param name="ProxyAddr"></param>
        /// <returns></returns>
        public WebConnection SetProxy(string ProxyAddr)
        {
            if (string.IsNullOrEmpty(ProxyAddr))
            {
                BaseRequest.Proxy = null;
            }
            else
            {
                BaseRequest.Proxy = new WebProxy(ProxyAddr);
                BaseRequest.Timeout = 15000;
            }
            return this;
        }

        /// <summary>
        /// 设置标头
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public WebConnection SetHeader(HttpRequestHeader Name, string val)
        {
            return SetHeader(Name.ToString(), val);
        }

        public CookieCollection ResponseCookie
        {
            get;
            set;
        }

        /// <summary>
        /// 设置标头
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Value"></param>
        /// <returns></returns>
        public WebConnection SetHeader(string Name, string Value)
        {
            switch (Name)
            {
                case "Accept":
                    BaseRequest.Accept = Value;
                    break;
                case "Content-Type":
                    BaseRequest.ContentType = Value;
                    break;
                case "User-Agent":
                    BaseRequest.UserAgent = Value;
                    break;
                case "Connection":
                    BaseRequest.KeepAlive = Value.ToLower() == "keep-alive";
                    break;
                case "Referer":
                    BaseRequest.Referer = Value;
                    break;
                default:
                    BaseRequest.Headers[Name] = Value;
                    break;
            }
            return this;
        }

        /// <summary>
        /// 用cookiecollection设置请求cookie列表
        /// </summary>
        /// <param name="Cookies"></param>
        /// <returns></returns>
        public WebConnection SetCookie(CookieCollection Cookies)
        {
            CookieContainer BaseCookie = (BaseRequest.CookieContainer == null) ? new CookieContainer() : BaseRequest.CookieContainer;
            // 检测是否空域名的cookie
            // 指定的cookie有可能是空域名的
            foreach  (Cookie item in Cookies)
            {
                if (string.IsNullOrWhiteSpace(item.Domain))
                {
                    item.Domain = GetDomain();
                }
            }
            BaseCookie.Add(Cookies);
            BaseRequest.CookieContainer = BaseCookie;            
            return this;
        }

        /// <summary>
        /// 设置单个cookie
        /// </summary>
        /// <param name="Name">名称</param>
        /// <param name="Value">值</param>
        /// <returns></returns>
        public WebConnection SetCookie(string Name, string Value)
        {
            CookieContainer BaseCookie = (BaseRequest.CookieContainer == null) ? new CookieContainer() : BaseRequest.CookieContainer;
            BaseCookie.Add(new Cookie(Name, Value, "/", GetDomain()));
            BaseRequest.CookieContainer = BaseCookie;
            return this;
        }

        /// <summary>
        /// 用枚举类型设置请求动作
        /// </summary>
        /// <param name="Method"></param>
        /// <returns></returns>
        public WebConnection SetMethod(RequestMethod Method)
        {
            ReqMethod = Method;
            return this;
        }

        /// <summary>
        /// 设置请求报文
        /// </summary>
        /// <param name="Data"></param>
        /// <returns></returns>
        public WebConnection SetData(object Data)
        {
            ReqObject = Data;
            return this;
        }

        /// <summary>
        /// 启动异步请求
        /// </summary>
        /// <param name="Method"></param>
        /// <param name="Data"></param>
        /// <returns></returns>
        public bool SendRequestAsync(RequestMethod Method, object Data)
        {
            Thread RequestThread = new Thread(new ThreadStart(SendRequestProxy));
            SetMethod(Method);
            SetData(Data);
            RequestThread.Start();
            return true;
        }

        /// <summary>
        /// 发送请求
        /// </summary>
        /// <returns></returns>
        public object SendRequest()
        {
            return SendRequest(ReqMethod, ReqObject);
        }

        private void SendRequestProxy()
        {
            SendRequest();
        }

        /// <summary>
        /// 设置是否允许30x跳转
        /// </summary>
        /// <param name="isAllow"></param>
        public WebConnection SetAllowAutoRedirect(bool isAllow)
        {
            BaseRequest.AllowAutoRedirect = isAllow;
            AllowRedirect = isAllow;

            return this;
        }

        /// <summary>
        /// 发送请求
        /// </summary>
        /// <param name="Method"></param>
        /// <param name="Data"></param>
        /// <returns></returns>
        public object SendRequest(RequestMethod Method, object Data)
        {
            int retry_count = 1;
            do
            {
                try
                {
                    byte[] Body = null;
                    if (Data != null)
                    {
                        Body = Processor.ObjectToData(Data);
                    }
                    BaseRequest.Method = Enum.GetName(typeof(RequestMethod), Method);
                    BaseRequest.AllowAutoRedirect = AllowRedirect;
                    BaseRequest.Expect = null;
                    if (BaseRequest.CookieContainer == null)
                    {
                        // 初始化Cookies容器以获取返回的Cookie
                        BaseRequest.CookieContainer = new CookieContainer();
                    }
                    BaseRequest.ServicePoint.Expect100Continue = false;
                    BaseRequest.CachePolicy = new System.Net.Cache.HttpRequestCachePolicy(System.Net.Cache.HttpRequestCacheLevel.NoCacheNoStore);
                    if (Body != null)
                    {
                        Stream RequestStream = BaseRequest.GetRequestStream();
                        RequestStream.Write(Body, 0, Body.Length);
                        RequestStream.Close();
                    }
                    HttpWebResponse Response = (HttpWebResponse)BaseRequest.GetResponse();
                    LastResponse = Response;
                    ResponseCookie = Response.Cookies;
                    ResponseHeaders = new NameValueCollection();
                    for (int i = 0; i < Response.Headers.Count; i++)
                    {
                        ResponseHeaders.Add(Response.Headers.Keys[i], Response.Headers[i]);
                    }
                    Stream ResponseStream = Response.GetResponseStream();
                    MemoryStream Buffer = new MemoryStream();
                    int ReadLen = 0;
                    byte[] Part = new byte[409600];
                    while (true)
                    {
                        ReadLen = ResponseStream.Read(Part, 0, 409600);
                        if (ReadLen == 0) break;
                        Buffer.Write(Part, 0, ReadLen);
                    }
                    ResponseStream.Close();
                    ResponseStream.Dispose();
                    byte[] ResponseData = Buffer.ToArray();

                    Buffer.Close();
                    Buffer.Dispose();
                    ResponseObject = Processor.DataToObject(ResponseData);
                    LastError = null;
                    if (RequestFinished != null) RequestFinished(this);
                    return ResponseObject;
                }
                catch (Exception ex)
                {
                    LastError = "网络错误";
                    ResponseHeaders = null;
                    ResponseObject = null;
                    Thread.Sleep(500);
                }
            } while (retry_count++ <= Retry);

            return null;
        }
    }



    /// <summary>
    /// https 的解决方案：
    ///    参考地址：http://social.microsoft.com/Forums/zh-CN/wcfzhchs/thread/1591a00d-d431-4ad8-bbd5-34950c39d563
    /// </summary>
    public static class HttpsUtil
    {
        /// <summary>
        /// 调用这个方法信任所有证书
        /// </summary>
        public static void SetCertificatePolicy()
        {
            ServicePointManager.ServerCertificateValidationCallback
                       += RemoteCertificateValidate;
        }

        /// <summary>
        /// Remotes the certificate validate.
        /// </summary>
        private static bool RemoteCertificateValidate(
           object sender, X509Certificate cert,
            X509Chain chain, SslPolicyErrors error)
        {
            // trust any certificate!!!           
            return true;
        }
    }
}