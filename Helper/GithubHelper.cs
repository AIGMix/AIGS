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
        public static async Task<string> getLastReleaseVersionAsync(string sAuthor, string sProjectName)
        {
            try
            {
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                string sUrl = string.Format("https://api.github.com/repos/{0}/{1}/releases/latest", sAuthor, sProjectName);

                HttpHelper.Result result = await HttpHelper.GetOrPostAsync(sUrl);
                if (result.Success == false)
                    return null;

                string sVer = JsonHelper.GetValue(result.sData, "tag_name");
                return sVer;
            }
            catch
            {
                return null;
            }
        }

        public static string getLastReleaseVersion(string sAuthor, string sProjectName)
        {

            var ret = getLastReleaseVersionAsync(sAuthor, sProjectName);
            string ver = ret.Result;
            return ver;
        }

        /// <summary>
        /// 获取文件链接
        /// </summary>
        /// <param name="sAuthor">作者名</param>
        /// <param name="sProjectName">项目名</param>
        /// <param name="sVer">版本</param>
        /// <param name="sOnlineFileName">文件名</param>
        /// <returns></returns>
        public static string getFileUrl(string sAuthor, string sProjectName, string sVer, string sOnlineFileName)
        {
            string sUrl = string.Format("https://github.com/{0}/{1}/releases/download/{2}/{3}", sAuthor, sProjectName, sVer, sOnlineFileName);
            return sUrl;
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
