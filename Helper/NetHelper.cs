using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Runtime.InteropServices;
using System.Net.NetworkInformation;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Web;

using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Diagnostics;
namespace AIGS.Helper
{
    public class NetHelper
    {
        /// <summary>
        /// 查看字符串是否为正确的IP地址
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsCorrectIP(string str)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(str) || str.Length < 7 || str.Length > 15)
                    return false;

                const string regformat = @"^\d{1,3}[\.]\d{1,3}[\.]\d{1,3}[\.]\d{1,3}";
                var regex = new Regex(regformat, RegexOptions.IgnoreCase);
                return regex.IsMatch(str);
            }
            catch (Exception)
            {
                return false;
            }
        }


        [DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(ref int Description, int ReservedValue);

        /// <summary>
        /// 检查本地连接状态
        /// </summary>
        /// <returns></returns>
        public static bool CheckInternetConnection()
        {
            System.Int32 dwFlag = new Int32();
            return InternetGetConnectedState(ref dwFlag, 0);
        }


        /// <summary>
        /// 用于检查IP地址或域名是否可以使用TCP/IP协议访问
        /// </summary>
        /// <param name="strIpOrDName">输入参数,表示IP地址或域名</param>
        /// <returns></returns>
        public static bool PingIpOrDomainName(string strIpOrDName, int iTimeOut = 0)
        {
            try
            {
                Ping aPing = new Ping();
                PingReply aPinReply = iTimeOut > 0 ? aPing.Send(strIpOrDName, iTimeOut) : aPing.Send(strIpOrDName);
                return aPinReply.Status == IPStatus.Success ? true : false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 用浏览器打开网页
        /// </summary>
        /// <param name="sUrl"></param>
        public static void OpenWeb(string sUrl)
        {
            if (String.IsNullOrWhiteSpace(sUrl))
                return;

            Process.Start(new ProcessStartInfo(sUrl));
        }

        #region 提供超时机制的WebClient

        /// <summary>
        /// 提供超时机制的WebClient
        /// </summary>
        public class WebClientEx : WebClient
        {
            public WebClientEx(int iTimeOut = 0)
            {
                if (iTimeOut > 0)
                    Timeout = iTimeOut;
            }

            /// <summary>
            /// 超时机制
            /// </summary>
            public int Timeout { get; set; }

            /// <summary>
            /// 重写
            /// </summary>
            /// <param name="address"></param>
            /// <returns></returns>
            protected override WebRequest GetWebRequest(Uri address)
            {
                HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(address);
                //var request = base.GetWebRequest(address);
                request.Timeout = Timeout;
                request.ReadWriteTimeout = Timeout;
                return request;
            }
        }
        #endregion


        #region 文件的下载和上传
        /// <summary>
        /// 下载至字符串
        /// </summary>
        /// <Param name="sUrl">链接</Param>
        public static string DownloadString(string sUrl, int iTimeOut = 5*1000)
        {
            object aObj = Download(DOWNLOAD_TYPE.STIRNG, sUrl, null, iTimeOut);
            if (aObj == null)
                return null;

            return aObj.ToString();
        }

        /// <summary>
        /// 下载Jason并转换为对象
        /// </summary>
        /// <param name="sUrl"></param>
        /// <returns></returns>
        public static T DownloadJsonToObject<T>(string sUrl, int iTimeOut = 5*1000)
        {
            string sValue = DownloadString(sUrl, iTimeOut);
            if (string.IsNullOrEmpty(sValue))
                return default(T);

            return AIGS.Helper.JsonHelper.ConverStringToObject<T>(sValue);
        }

        /// <summary>
        /// 下载至文件中
        /// </summary>
        /// <Param name="sUrl">链接</Param>
        /// <Param name="sFilePathName">文件路径文件名</Param>
        /// <Param name="bIsShowProgress">是否显示进度条</Param>
        /// <returns>0表示成功</returns>
        public static int DownloadFile(string sUrl, string sFilePathName, int iTimeOut = 10*60*1000)
        {
            object aObj = Download(DOWNLOAD_TYPE.FILE, sUrl, sFilePathName, iTimeOut);
            if (aObj == null)
                return -1;

            return 0;
        }

        /// <summary>
        /// 下载数据
        /// </summary>
        /// <param name="sUrl"></param>
        /// <returns></returns>
        public static byte[] DownloadData(string sUrl, int iTimeOut = 5*1000)
        {
            object aObj = Download(DOWNLOAD_TYPE.DATA, sUrl, null, iTimeOut);
            if (aObj == null)
                return null;

            return (byte[])aObj;
        }


        enum DOWNLOAD_TYPE
        {
            DATA,
            STIRNG,
            FILE,
        }

        /// <summary>
        /// 下载
        /// </summary>
        /// <param name="eType">类型</param>
        /// <param name="sUrl">连接</param>
        /// <param name="sFilePathName">文件名</param>
        /// <param name="iTimeOut">超时时间</param>
        /// <returns></returns>
        private static object Download(DOWNLOAD_TYPE eType, string sUrl, string sFilePathName, int iTimeOut)
        {
            object aRet = null;
            WebClientEx aClient = new WebClientEx(iTimeOut);

            try
            {
                WebRequest myre = WebRequest.Create(sUrl);
                if (eType == DOWNLOAD_TYPE.FILE)
                {
                    if (String.IsNullOrWhiteSpace(sFilePathName))
                        return null;
                    var di = new DirectoryInfo(Path.GetDirectoryName(sFilePathName));
                    if (!di.Exists)
                        di.Create();

                    aClient.DownloadFile(sUrl, sFilePathName);
                    aRet = 0;
                }
                if(eType == DOWNLOAD_TYPE.STIRNG)
                {
                    aClient.Encoding = System.Text.Encoding.UTF8;//定义对象语言
                    aRet = aClient.DownloadString(sUrl);
                }
                if(eType == DOWNLOAD_TYPE.DATA)
                {
                    aRet = aClient.DownloadData(sUrl);
                }
            }
            catch (System.Net.WebException e)
            {
                aClient.Dispose();
                return aRet;
            }

            aClient.Dispose();
            return aRet;
        }

        /// <summary>
        /// 上传数据
        /// </summary>
        /// <param name="eType"></param>
        /// <param name="sUrl"></param>
        /// <param name="sFilePathNameOrData"></param>
        /// <param name="iTimeOut"></param>
        /// <returns></returns>
        private static object Upload(DOWNLOAD_TYPE eType, string sUrl, string sFilePathNameOrData, int iTimeOut)
        {
            if (String.IsNullOrWhiteSpace(sFilePathNameOrData))
                return null;

            object aRet = null;
            WebClientEx aClient = new WebClientEx(iTimeOut);
            
            try
            {
                WebRequest myre = WebRequest.Create(sUrl);
                if (eType == DOWNLOAD_TYPE.FILE)
                {
                    var di = new DirectoryInfo(Path.GetDirectoryName(sFilePathNameOrData));
                    if (!di.Exists)
                        di.Create();

                    aClient.UploadFile(sUrl, sFilePathNameOrData);
                    aRet = 0;
                }
                if (eType == DOWNLOAD_TYPE.STIRNG)
                {
                    aClient.Encoding = System.Text.Encoding.UTF8;//定义对象语言
                    aRet = aClient.UploadString(sUrl, sFilePathNameOrData);
                }
                if (eType == DOWNLOAD_TYPE.DATA)
                {
                    byte[] byteArray = System.Text.Encoding.Default.GetBytes(sFilePathNameOrData);
                    aRet = aClient.UploadData(sUrl, byteArray);
                }
            }
            catch
            {
                
            }
            aClient.Dispose();
            return aRet;
        }

        #endregion


    }


 


}
