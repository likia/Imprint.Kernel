using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Management;
using System.Diagnostics;


namespace Imprint.Util
{
    public class ConfigRow
    {
        public string Key;
        public List<string> Fields;
        public ConfigRow(string _Key, List<string> _Fields)
        {
            Key = _Key;
            Fields = _Fields;
        }
    }

    public static class Information
    {
        public static string Version()
        {
            return "2.1.1";
        }
    }

    public interface JammerMaker
    {
        string Get(int Count);
        string GetSingle();
    }

    public class JammerElement
    {
        public int RangeStart;
        public int RangeEnd;
        public int ASCIIStart;
    }
    public class FixedJammer : JammerMaker
    {
        string[] Rows;

        public FixedJammer(string FileName)
        {
            string Content = Algorithm.LoadText(FileName);
            if (Content == null) return;
            Rows = StrHelper.Explode("\n", Content);
        }

        public string Get(int Count)
        {
            return GetSingle();
        }

        public string GetSingle()
        {
            if (Rows == null || Rows.Length == 0) return null;
            return Rows[Algorithm.Rand() % Rows.Length];
        }
    }
    public class LyricsJammer : CharJammer
    {
        string LyricsText = "";


        public void AddElements(string Chars)
        {
            LyricsText += Chars;
        }
        public override string Get(int Length)
        {
            int pos = new Random().Next(0, LyricsText.Length - Length);
            string rtn = LyricsText.Substring(pos, Length);
            return rtn;
        }
    }
    public class CharJammer : JammerMaker
    {
        private List<JammerElement> Elements;
        private int RangeCapacity;

        public CharJammer()
        {
            RangeCapacity = 0;
            Elements = new List<JammerElement>(255);
        }

        public virtual void AddRange(int Start, int End)
        {
            JammerElement NewElement = new JammerElement();
            NewElement.RangeStart = RangeCapacity;
            RangeCapacity += End - Start + 1;
            NewElement.RangeEnd = RangeCapacity - 1;
            NewElement.ASCIIStart = Start;
            Elements.Add(NewElement);
        }

        public virtual void AddElements(char[] Chars)
        {
            for (int i = 0; i < Chars.Length; i++)
            {
                AddRange((int)Chars[i], (int)Chars[i]);
            }
        }

        public string GetSingle()
        {
            if (RangeCapacity == 0) return null;
            int Position = Algorithm.Rand() % RangeCapacity;
            for (int i = 0; i < Elements.Count; i++)
            {
                if (Elements[i].RangeStart <= Position && Elements[i].RangeEnd >= Position)
                {
                    return ((char)(Elements[i].ASCIIStart + (Position - Elements[i].RangeStart))).ToString();
                }
            }
            return null;
        }

        public virtual string Get(int Length)
        {
            string Mix = "";
            for (int i = 0; i < Length; i++)
            {
                Mix += GetSingle();
            }
            return Mix;
        }
    }
    public static class Url
    {
        private static bool IsSafe(char ch)
        {
            /*   if ((((ch >= 'a') && (ch <= 'z')) || ((ch >= 'A') && (ch <= 'Z'))) || ((ch >= '0') && (ch <= '9')))
               {
                   return true;
               }
               switch (ch)
               {
                   case '\'':
                   case '(':
                   case ')':
                   case '*':
                   case '-':
                   case '.':
                   case '_':
                   case '!':
                       return true;
               }
               return false;
               */
            return false;
        }

        private static char IntToHex(int n)
        {
            if (n <= 9)
            {
                return (char)(n + 0x30);
            }
            return (char)((n - 10) + 0x61);
        }

        public static byte[] UrlEncodeBytesToBytes(byte[] bytes, int offset, int count, bool alwaysCreateReturnValue)
        {
            int num = 0;
            int num2 = 0;
            for (int i = 0; i < count; i++)
            {
                char ch = (char)bytes[offset + i];
                if (ch == ' ')
                {
                    num++;
                }
                else if (!IsSafe(ch))
                {
                    num2++;
                }
            }
            if ((!alwaysCreateReturnValue && (num == 0)) && (num2 == 0))
            {
                return bytes;
            }
            byte[] buffer = new byte[count + (num2 * 2)];
            int num4 = 0;
            for (int j = 0; j < count; j++)
            {
                byte num6 = bytes[offset + j];
                char ch2 = (char)num6;
                if (IsSafe(ch2))
                {
                    buffer[num4++] = num6;
                }
                else if (ch2 == ' ')
                {
                    buffer[num4++] = 0x2b;
                }
                else
                {
                    buffer[num4++] = 0x25;
                    buffer[num4++] = (byte)IntToHex((num6 >> 4) & 15);
                    buffer[num4++] = (byte)IntToHex(num6 & 15);
                }
            }
            return buffer;
        }

        public static byte[] UrlEncodeToBytes(string str)
        {
            if (str == null)
            {
                return null;
            }
            return UrlEncodeToBytes(str, Encoding.UTF8);
        }

        public static byte[] UrlEncodeToBytes(string str, Encoding e)
        {
            if (str == null)
            {
                return null;
            }
            byte[] bytes = e.GetBytes(str);
            return UrlEncodeToBytes(bytes);
        }

        public static byte[] UrlEncodeToBytes(byte[] bytes)
        {
            if (bytes == null)
            {
                return null;
            }
            return UrlEncodeBytesToBytes(bytes, 0, bytes.Length, false);
        }

        public static string UrlEncode(string str, Encoding e)
        {
            if (str == null)
            {
                return null;
            }
            return Encoding.ASCII.GetString(UrlEncodeToBytes(str, e));
        }

        public static string UrlEncode(string str)
        {
            if (str == null)
            {
                return null;
            }
            return UrlEncode(str, Encoding.UTF8);
        }

        public static string UrlEncode(byte[] bytes)
        {
            if (bytes == null)
            {
                return null;
            }
            return Encoding.ASCII.GetString(UrlEncodeToBytes(bytes));
        }
    }

    public static class Algorithm
    {
        public static readonly byte[] _md5_padding_header = new byte[8] { 0x0A, 0x0f, 0x1d, 0xcf, 0x9e, 0x3b, 0xce, 0x48 };
        public static readonly byte[] _md5_padding_tail = new byte[8] { 0xAd, 0xb1, 0x33, 0x12, 0x25, 0x99, 0x7e, 0x2b };

        public static bool BytesEqual(byte[] buf1, byte[] buf2)
        {
            if (buf1.Length == buf2.Length)
            {
                for (int i = 0; i < buf1.Length; i++)
                {
                    if (!buf1[i].Equals(buf2[i]))
                        return false;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 自制分块异或加密
        /// </summary>
        public class Crypt
        {
            private byte[] mKey = { 0x7d, 0xff, 0x0d, 0x0a, 0x66, 0x12, 0x35, 0x67, 0x98, 0xfa, 0x9f, 0xfb, 0xba, 0x8c, 0xcc, 0x1a };
            private long nGourp = 6184;
            private string SALT = "-=)(*&^%$%$%!@,></><][{}{\":|.-*/;'][+->.<;.s49(*^][:;/.,+-=*()(&^$%$#";
            private int P = 256;

            private void ChangeKey()
            {
                mKey = Algorithm.MD5(Encoding.UTF8.GetBytes(BitConverter.ToString(mKey).Replace("-", "").ToUpper() + SALT + nGourp.ToString()));
                nGourp++;
            }
            public byte[] Encrypt(byte[] src)
            {
                List<byte> rtn = new List<byte>();
                for (int i = 0; i < src.Length; i++)
                {
                    if (i % P == 0) ChangeKey();
                    rtn.Add((byte)(src[i] ^ mKey[i % 16]));
                }
                return rtn.ToArray();
            }
            public byte[] Decrypt(byte[] enc)
            {
                List<byte> rtn = new List<byte>();
                for (int i = 0; i < enc.Length; i++)
                {
                    if (i % P == 0) ChangeKey();
                    rtn.Add((byte)(enc[i] ^ mKey[i % 16]));
                }
                return rtn.ToArray();
            }
        }


        static private Random Seed = null;
        static object RndLock = new object();
        static public int Rand()
        {
            lock (RndLock)
            {
                if (Seed == null)
                {
                    Seed = new Random((int)((DateTime.Now - new DateTime(1970, 1, 1, 8, 0, 0)).TotalSeconds) / (DateTime.Now.Millisecond + 1));
                    System.Threading.Thread.Sleep(Seed.Next(1, 65));
                }
                return Seed.Next();
            }
        }
        public static byte[] MD5(byte[] src)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] mdByte = md5.ComputeHash(src);
            return mdByte;
        }
        static public string MD5(string str)
        {
            if (str == null) str = "";
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            var buf = System.Text.Encoding.UTF8.GetBytes(str);
            byte[] mdByte = md5.ComputeHash(buf, 0, buf.Length);
            return System.BitConverter.ToString(mdByte).Replace("-", "").ToLower();
        }

        static public string HardwareID()
        {
            string HID = "";
            ManagementObjectSearcher Searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_NetworkAdapter");
            foreach (ManagementObject obj in Searcher.Get())
            {
                string MAC = (string)obj["MACAddress"];
                string PNPID = (string)obj["PNPDeviceID"];
                if (MAC != null && PNPID != null && PNPID.IndexOf("PCI") > -1)
                {
                    HID += "\"" + MAC + "\", ";
                }
            }

            /* That's for DiskDrive*/
            ManagementClass cimobject = new ManagementClass("Win32_DiskDrive");
            ManagementObjectCollection moc = cimobject.GetInstances();
            foreach (ManagementObject mo in moc)
            {
                string HDid = (string)mo.Properties["Model"].Value;
                HID += HDid + "|";
            }
            HID += "|CPUID|" + GetCpuID();
            return MD5("Imprint.HardwareID<" + HID + ",HASH = " + MD5(HID) + ">");
        }
        public static String GetCpuID()
        {
            try
            {
                ManagementClass mc = new ManagementClass("Win32_Processor");
                ManagementObjectCollection moc = mc.GetInstances();
                String strCpuID = null;
                foreach (ManagementObject mo in moc)
                {
                    strCpuID = mo.Properties["ProcessorId"].Value.ToString();
                    break;
                }
                return strCpuID;
            }
            catch
            {
                return "";
            }
        }//end method
        public static string LoadText(string FileName)
        {
            if (System.IO.File.Exists(FileName) == false) return null;
            System.IO.StreamReader SReader = new System.IO.StreamReader(FileName);
            string Content = SReader.ReadToEnd();
            SReader.Close();
            SReader.Dispose();
            return Content;
        }

        public static List<ConfigRow> GetConfigRow(string Text)
        {
            if (Text == null) return null;
            List<ConfigRow> Rows = new List<ConfigRow>();
            string[] Lines = StrHelper.Explode("\n", Text);
            for (int i = 0; i < Lines.Length; i++)
            {
                ConfigRow CurrentRow = ParseConfigRow(Lines[i]);
                if (CurrentRow != null) Rows.Add(CurrentRow);
            }
            return Rows;
        }

        private static ConfigRow ParseConfigRow(string Raw)
        {
            string Line = Raw.Trim();
            if (Line.Length == 0) return null;
            if (Line.StartsWith("#")) return null;
            string Key = Line.Substring(0, Line.IndexOf("=")).Trim();
            string Value = Line.Substring(Line.IndexOf("=") + 1).Trim();
            List<string> Fields = new List<string>();
            string Buffer = "";
            bool InQuote = false;
            for (int j = 0; j < Value.Length; j++)
            {
                if (Value[j] == '"')
                {
                    InQuote = !InQuote;
                    continue;
                }
                if (InQuote)
                {
                    if (Value[j] == '\\')
                    {
                        char ch = Value[++j];
                        if (ch == 'r')
                            Buffer += "\r";
                        else if (ch == 'n')
                            Buffer += "\n";
                        else if (ch == 't')
                            Buffer += "\t";
                        else
                            Buffer += ch;
                        continue;
                    }
                    Buffer += Value[j];
                    continue;
                }
                if (Value[j] == ':')
                {
                    Fields.Add(Buffer);
                    Buffer = "";
                }
                else
                {
                    Buffer += Value[j];
                }
            }
            if (Buffer.Length > 0)
            {
                Fields.Add(Buffer);
            }
            return new ConfigRow(Key, Fields);
        }

        public static List<ConfigRow> ParseConfig(string FileName)
        {
            string Text = LoadText(FileName);
            if (Text == null) return null;
            List<ConfigRow> Rows = new List<ConfigRow>();
            string[] Lines = StrHelper.Explode("\n", Text);
            for (int i = 0; i < Lines.Length; i++)
            {
                ConfigRow CurrentRow = ParseConfigRow(Lines[i]);
                if (CurrentRow != null)
                {
                    if (CurrentRow.Key == "+include")
                    {
                        string IncludeFile = FileName;
                        IncludeFile = IncludeFile.Substring(0, IncludeFile.LastIndexOf(@"\")) + @"\" + CurrentRow.Fields[0];
                        List<ConfigRow> Include = ParseConfig(IncludeFile);
                        if (Include != null)
                            Rows.AddRange(Include);
                    }
                    else
                        Rows.Add(CurrentRow);
                }
            }
            return Rows;
        }

        public static int TimeStamp()
        {
            return (int)((DateTime.Now - new DateTime(1970, 1, 1, 8, 0, 0)).TotalSeconds);
        }
    }
}