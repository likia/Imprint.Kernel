﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Imprint.Security
{
    public class SecureHash
    {
        private HashAlgorithm algorithm;

        public SecureHash(string Name)
        {
            algorithm = HashAlgorithm.Create(Name);
        }

        public string Hash(string data)
        {
            var buffer = Hash(Encoding.UTF8.GetBytes(data));
            var rtv = new StringBuilder();
            foreach (var _byte in buffer)
            {
                rtv.AppendFormat("{0:x2}", _byte);
            }
            return rtv.ToString();
        }

        public byte[] Hash(byte[] data)
        {
            var hashBuf = algorithm.ComputeHash(data);
            return hashBuf;
        }
    }
}
