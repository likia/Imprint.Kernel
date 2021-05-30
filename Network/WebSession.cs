using Imprint.Network.Handler;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Imprint.Network
{    
    public delegate void RequestFinishedHandler(object sender);

    /// <summary>
    /// Web会话类
    /// 封装一次会话具有的属性（主要是 Cookie）
    /// </summary>
    public class WebSession
    {
        private Object sync = new Object();

        // 数据处理器，对象和字符串直接进行转换的处理器
        // 子类有  JsonHandler： dynamic对象
        //        RawHandler : byte[]
        //        PlainTextHandler : string           
        private ReflectionHandler DataHandler;

        // 除了默认header还需要添加的header
        private NameValueCollection Headers;

        // 会话积累的Cookie
        public CookieCollection Cookies;

        /// <summary>
        /// 是否允许30x跳转
        /// </summary>
        public bool AllowRedirect
        {
            get;
            set;
        }
        /// <summary>
        /// 超时时间
        /// </summary>
        public int Timeout
        {
            get;
            set;
        }
        /// <summary>
        /// 最多重试次数
        /// </summary>
        public int Retry
        {
            get;
            set;
        }
        /// <summary>
        /// http代理
        /// </summary>
        public string Proxy
        {
            get;
            set;
        }

        /// <summary>
        /// 上一从请求返回头
        /// </summary>
        public NameValueCollection ResponseHeader
        {
            get;
            set;
        }


        public WebSession(ReflectionHandler handler = null, NameValueCollection customHeader = null, bool AllowRedirect = false, int Timeout = 10000, string Proxy = "")
        {
            if (handler != null)
            {
                DataHandler = handler;
            }
            else
            {
                DataHandler = new PlainTextHandler(Encoding.UTF8);
            }
            if (customHeader != null)
            {
                Headers = customHeader;
            }
            else
            {
                Headers = new NameValueCollection();
                Headers.Add("User-Agent", UserAgent.PC);
            }
            this.AllowRedirect = AllowRedirect;
            this.Timeout = Timeout;
            this.Proxy = Proxy;
        }


        /// <summary>
        /// 创建WebConnection对象
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        WebConnection buildRequest(string uri)
        {
            var req = new WebConnection(uri, DataHandler);
            lock (sync)
            {
                foreach (var header in Headers.Keys)
                {
                    req.SetHeader(header.ToString(), Headers[header.ToString()]);
                }

                if (Cookies == null)
                {
                    Cookies = new CookieCollection();
                }
                req.SetCookie(Cookies);
            }
            req.SetTimeout(Timeout);
            req.SetAllowAutoRedirect(AllowRedirect);
            req.Retry = Retry;
            if (!string.IsNullOrEmpty(Proxy))
            {
                req.SetProxy(Proxy);
            }
            return req;
        }

        /// <summary>
        /// 创建并发送GET
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="referer"></param>
        /// <returns></returns>
        public Object Get(string uri, string referer = null)
        {
            var conn = buildRequest(uri);
            if (referer != null)
                conn.SetHeader(HttpRequestHeader.Referer, referer);
            conn.SetMethod(RequestMethod.GET);

            return SendRequest(conn);
        }

        /// <summary>
        /// 创建并发送POST
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="data"></param>
        /// <param name="referer"></param>
        /// <returns></returns>
        public Object Post(string uri, Object data, string referer = null)
        {
            var conn = buildRequest(uri);
            conn.SetMethod(RequestMethod.POST);
            if (string.IsNullOrEmpty(Headers["Content-Type"]))
            {
                // 未设置默认表单类型
                conn.SetHeader("Content-Type", "application/x-www-form-urlencoded");
            }
            if (referer != null)
            {
                conn.SetHeader(HttpRequestHeader.Referer, referer);
            }
            return SendRequest(conn, data);
        }

        /// <summary>
        /// 异步
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="data"></param>
        /// <param name="referer"></param>
        /// <returns></returns>
        public async Task<Object> PostAsync(string uri, Object data, string referer = null)
        {
            var conn = buildRequest(uri);
            conn.SetMethod(RequestMethod.POST);
            if (string.IsNullOrEmpty(Headers["Content-Type"]))
            {
                // 未设置默认表单类型
                conn.SetHeader("Content-Type", "application/x-www-form-urlencoded");
            }
            if (referer != null)
            {
                conn.SetHeader(HttpRequestHeader.Referer, referer);
            }
            return await SendRequestAsync(conn, data);
        }



        /// <summary>
        /// 异步GET
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="referer"></param>
        /// <returns></returns>
        public async Task<Object> GetAsync(string uri, string referer = null)
        {
            var conn = buildRequest(uri);
            if (referer != null)
                conn.SetHeader(HttpRequestHeader.Referer, referer);
            conn.SetMethod(RequestMethod.GET);

            return await SendRequestAsync(conn);
        }


        /// <summary>
        /// GET获取原始字节数组
        /// </summary>
        /// <param name="url"></param>
        /// <param name="referer"></param>
        /// <returns></returns>
        public async Task<byte[]> GetRawAsync(string url, string referer = null)
        {
            var rawHandler = new RawHandler();
            var conn = buildRequest(url);
            if (referer != null)
            {
                conn.SetHeader(HttpRequestHeader.Referer, referer);
            }
            conn.SetProcessor(rawHandler);
            return await SendRequestAsync(conn) as byte[];
        }



        /// <summary>
        /// GET获取原始字节数组
        /// </summary>
        /// <param name="url"></param>
        /// <param name="referer"></param>
        /// <returns></returns>
        public byte[] GetRaw(string url, string referer = null)
        {
            var rawHandler = new RawHandler();
            var conn = buildRequest(url);
            if (referer != null)
            {
                conn.SetHeader(HttpRequestHeader.Referer, referer);
            }
            conn.SetProcessor(rawHandler);
            return SendRequest(conn) as byte[];
        }

        /// <summary>
        /// 发送一次请求
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public Object SendRequest(WebConnection connection, Object data = null)
        {
            if (data != null)
            {
                connection.SetData(data);
            }
            var resp = connection.SendRequest();
            if (resp != null)
            {
                lock (sync)
                {
                    // 请求成功, 累加cookie
                    var respCookies = connection.ResponseCookie;

                    Cookies.Add(respCookies);
                }
                ResponseHeader = connection.ResponseHeaders;
            }
            return resp;
        }

        /// <summary>
        /// 异步请求
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<Object> SendRequestAsync(WebConnection connection, Object data = null)
        {
            if (data != null)
            {
                connection.SetData(data);
            }
            var resp = await connection.SendRequestAsync();
            if (resp != null)
            {
                lock (sync)
                {
                    // 请求成功, 累加cookie
                    var respCookies = connection.ResponseCookie;

                    Cookies.Add(respCookies);
                }
                ResponseHeader = connection.ResponseHeaders;
            }
            return resp;
        }

    }
}