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
        /// ״̬��
        /// </summary>
        [JsonProperty("code")]
        public int Code { get; set; } = 200;

        /// <summary>
        /// ��Ϣ
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        [JsonProperty("data")]
        public T Data { get; set; }

        /// <summary>
        /// �Ƿ�ɹ�
        /// </summary>
        [JsonProperty("success")]
        public bool Success { get; set; }
    }
}
