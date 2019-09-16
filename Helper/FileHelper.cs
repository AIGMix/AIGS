using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AIGS.Common;
using System.IO;
namespace AIGS.Helper
{

    public class FileHelper
    {
        public static bool Write(string sContent, bool IsClearBeforeWrite, string sFilePath)
        {
            if (sFilePath.IsBlank() || sContent.IsBlank())
                return false;
            try
            {
                //创建目录
                var di = new DirectoryInfo(Path.GetDirectoryName(sFilePath));
                if (!di.Exists)
                    di.Create();
                StreamWriter FD = new StreamWriter(sFilePath, IsClearBeforeWrite ? false : true);
                FD.Write(sContent);
                FD.Flush();
                FD.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool Write(byte[] sContent, bool IsClearBeforeWrite, string sFilePath)
        {
            if (sFilePath.IsBlank())
                return false;
            try
            {
                //创建目录
                var di = new DirectoryInfo(Path.GetDirectoryName(sFilePath));
                if (!di.Exists)
                    di.Create();
                FileStream FD = new FileStream(sFilePath, IsClearBeforeWrite ? FileMode.Create : FileMode.Append);
                FD.Write(sContent, 0, sContent.Length);
                FD.Flush();
                FD.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }



        public static string Read(string sFilePath)
        {
            if (sFilePath.IsBlank())
                return null;
            try
            {
                StreamReader FD = new StreamReader(sFilePath);
                string sBuf = FD.ReadToEnd();
                FD.Close();
                return sBuf;
            }
            catch
            {
                return null;
            }
        }

        public static string[] ReadLines(string sFilePath)
        {
            if (sFilePath.IsBlank())
                return null;
            try
            {
                StreamReader FD = new StreamReader(sFilePath);
                List<string> aLines = new List<string>();
                while(!FD.EndOfStream)
                {
                    string sBuf = FD.ReadLine();
                    aLines.Add(sBuf);
                }
                FD.Close();
                return aLines.ToArray();
            }
            catch
            {
                return null;
            }
        }

    }
}
