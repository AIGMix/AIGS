using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AIGS.Common;

namespace AIGS.Helper
{
    public class EncryptHelper
    {
        /// <summary>
        /// 格式化key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        private static string FormatKey(string key, int length = 8)
        {
            if (key.Length > length)
                return key.Substring(0, length);

            while (key.Length < length)
                key += '@';
            return key;
        }

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="data"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string Encode(string data, string key)
        {
            if (data.IsBlank())
                return null;
            if (key.IsBlank())
                return data;

            key = FormatKey(key, 8);
            byte[] byKey = System.Text.ASCIIEncoding.ASCII.GetBytes(key);
            byte[] byIV  = System.Text.ASCIIEncoding.ASCII.GetBytes(key);

            DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
            MemoryStream ms  = new MemoryStream();
            CryptoStream cst = new CryptoStream(ms, cryptoProvider.CreateEncryptor(byKey, byIV), CryptoStreamMode.Write);
            StreamWriter sw  = new StreamWriter(cst);
            sw.Write(data);
            sw.Flush();
            cst.FlushFinalBlock();
            sw.Flush();
            return System.Convert.ToBase64String(ms.GetBuffer(), 0, (int)ms.Length);
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="data"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string Decode(string data, string key)
        {
            if (data.IsBlank())
                return null;
            if (key.IsBlank())
                return data;

            byte[] byKey = System.Text.ASCIIEncoding.ASCII.GetBytes(key);
            byte[] byIV = System.Text.ASCIIEncoding.ASCII.GetBytes(key);

            byte[] byEnc;
            try
            {
                byEnc = System.Convert.FromBase64String(data);
            }
            catch
            {
                return null;
            }

            DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
            MemoryStream ms  = new MemoryStream(byEnc);
            CryptoStream cst = new CryptoStream(ms, cryptoProvider.CreateDecryptor(byKey, byIV), CryptoStreamMode.Read);
            StreamReader sr  = new StreamReader(cst);
            return sr.ReadToEnd();
        }

        /// <summary>
        /// 加密不可逆
        /// </summary>
        /// <param name="data"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public string EncodeIrreversible(string data, string key)
        {
            MD5 md5          = new MD5CryptoServiceProvider();
            byte[] t         = md5.ComputeHash(Encoding.GetEncoding(key).GetBytes(data));
            StringBuilder sb = new StringBuilder(32);
            for (int i = 0; i < t.Length; i++)
            {
                sb.Append(t[i].ToString("x").PadLeft(2, '0'));
            }
            return sb.ToString();
        }
    }
}
