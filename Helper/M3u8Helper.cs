using AIGS.Common;
using System.Collections.Generic;
using System.Linq;

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

        struct DownloadData
        {
            public long AllSize;
            public long AlreadyDownloadSize;
            public ProgressNotify Func;
        }

        public delegate bool ProgressNotify(long lTotalSize, long lAlreadyDownloadSize, long lIncreSize, object indata);

        public static bool Download(string[] pTsUrls, string sOutFile, ProgressNotify pFunc, ref string errmsg, HttpHelper.ProxyInfo Proxy = null)
        {
            if (pTsUrls == null || pTsUrls.Count() <= 0)
                return false;

            if (System.IO.File.Exists(sOutFile))
                System.IO.File.Delete(sOutFile);

            int iCount = pTsUrls.Count();
            long []fileLengths = DownloadFileHepler.GetAllFileLengths(pTsUrls);
            long allSize = 0;
            foreach (long item in fileLengths)
            {
                allSize += item;
                if (item <= 0)
                {
                    allSize = 0;
                    break;
                }
            }
            if (allSize <= 0)
            {
                errmsg = "Get file length failed.";
                return false;
            }

            if (!pFunc(allSize, 0, 0, null))
                return false;

            DownloadData data = new DownloadData();
            data.AllSize = allSize;
            data.AlreadyDownloadSize = 0;
            data.Func = pFunc;

            for (int i = 0; i < iCount; i++)
            {
                bool bRet = true;
                for (int j = 0; j < 3; j++)
                {
                    bRet = (bool)DownloadFileHepler.Start(pTsUrls[i], sOutFile, data, bAppendFile: true, Timeout: 3 * 1000, Proxy: Proxy, RetryNum:3, UpdateFunc: UpdateDownloadNotify);
                    if (!bRet)
                        continue;

                    data.AlreadyDownloadSize += fileLengths[i];
                    break;
                }
                if (!bRet)
                {
                    errmsg = "Download some part-files failed.";
                    return false;
                }
            }
            pFunc(allSize, allSize, 0, null);
            return true;
        }

        private static bool UpdateDownloadNotify(long lTotalSize, long lAlreadyDownloadSize, long lIncreSize, object indata)
        {
            DownloadData data = (DownloadData)indata;
            return data.Func(data.AllSize, data.AlreadyDownloadSize + lAlreadyDownloadSize, lIncreSize, null);
        }
    }
}
