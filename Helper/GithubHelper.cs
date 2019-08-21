using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AIGS.Common;
namespace AIGS.Helper
{
    public class GithubHelper
    {
        /// <summary>
        /// 获取最新的版本
        /// </summary>
        /// <param name="sAuthor">作者名</param>
        /// <param name="sProjectName">项目名</param>
        /// <returns></returns>
        public static string getLastReleaseVersion(string sAuthor, string sProjectName)
        {
            try
            {
                string sUrl = string.Format("https://api.github.com/repos/{0}/{1}/releases/latest", sAuthor, sProjectName);
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                string sErr = null;
                object oObj = HttpHelper.GetOrPost(sUrl, out sErr);
                if (oObj == null)
                    return null;

                string sVer = JsonHelper.GetValue(oObj.ToString(), "tag_name");
                return sVer;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 获取文件
        /// </summary>
        /// <param name="sAuthor"></param>
        /// <param name="sProjectName"></param>
        /// <param name="sOnlineFileName"></param>
        /// <param name="sOutputPath"></param>
        /// <returns></returns>
        public static bool getLastReleaseFile(string sAuthor, string sProjectName, string sOnlineFileName, string sOutputPath)
        {
            string sVer = getLastReleaseVersion(sAuthor, sProjectName);
            if (sVer.IsBlank())
                return false;

            string sUrl = string.Format("https://github.com/{0}/{1}/releases/download/{2}/{3}", sAuthor, sProjectName, sVer, sOnlineFileName);
            bool bRet = (bool)DownloadFileHepler.Start(sUrl, sOutputPath,RetryNum:5);
            return bRet;
        }
    }
}
