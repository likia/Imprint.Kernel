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

    public static class Algorithm
    {
        /// <summary>
        /// »úÆ÷Âë
        /// </summary>
        /// <returns></returns>
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
            var algo = new Security.SecureHash("SHA256");
            return algo.Hash("Imprint.HardwareID<" + HID + ",HASH = " + algo.Hash(HID) + ">");
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
            string Text = File.ReadAllText(FileName);
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

        /// <summary>
        /// unixÊ±¼ä´Á
        /// </summary>
        /// <returns></returns>
        public static int TimeStamp()
        {
            return (int)((DateTime.Now - new DateTime(1970, 1, 1, 8, 0, 0)).TotalSeconds);
        }
    }
}