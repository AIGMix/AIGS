using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIGS.Common;
using System.IO;

namespace AIGS.Helper
{
    public class M3u8Helper
    {
        public static string[] GetTsUrls(string sUrl)
        {
            string sTxt = NetHelper.DownloadString(sUrl);
            if (sTxt.IsBlank())
                return null;

            List<string> pList = new List<string>();
            string[] sArray = sTxt.Split("#EXTINF");
            foreach (string item in sArray)
            {
                if (item.IndexOf("http") < 0)
                    continue;
                string sValue = "http" + StringHelper.GetSubString(item, "http", "\n");
                pList.Add(sValue);
            }
            return pList.ToArray();
        }

        public delegate bool ProgressNotify(long lCurSize, long lAllSize);
        public static bool Download(string[] pTsUrls, string sOutFile, ProgressNotify pFunc)
        {
            if (pTsUrls == null || pTsUrls.Count() <= 0)
                return false;

            if (System.IO.File.Exists(sOutFile))
                System.IO.File.Delete(sOutFile);

            int iCount = pTsUrls.Count();
            for (int i = 0; i < iCount; i++)
            {
                if (!pFunc(i, iCount))
                    return false;

                bool bRet = true;
                for (int j = 0; j < 100; j++)
                {
                    bRet = (bool)DownloadFileHepler.Start(pTsUrls[i], sOutFile, bAppendFile: true, Timeout: 3 * 1000);
                    if (!pFunc(i, iCount))
                        return false;
                    if (bRet)
                        break;
                }
                if (!bRet)
                    return false;
            }
            pFunc(iCount, iCount);
            return true;
        }
    }
}
