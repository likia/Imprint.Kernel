using Newtonsoft.Json;
using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Imprint.Model
{
    [Serializable]
    public class Result<T>
    {
        /// <summary>
        /// 状态码
        /// </summary>
        [JsonProperty("code")]
        public int Code { get; set; } = 200;

        /// <summary>
        /// 信息
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }

        /// <summary>
        /// 数据
        /// </summary>
        [JsonProperty("data")]
        public T Data { get; set; }

        /// <summary>
        /// 是否成功
        /// </summary>
        [JsonProperty("success")]
        public bool Success { get; set; }

        public static Result<T> Succeed(string msg = "", int code = 200, T data = default)
        {
            return new Result<T>() { Success = true, Code = code, Data = data, Message = msg };
        }

        public static Result<T> Failed(string msg = "", int code = -1, T data = default)
        {
            return new Result<T>() { Success = false, Code = code, Data = data, Message = msg };
        }
    }
}
