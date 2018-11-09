using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AIGS.Helper;
using System.Net;

namespace AIGS.Helper
{
    public class DownloadFileHepler
    {
        int m_ThreadNum;
        ThreadHelper m_Thread;
        public DownloadFileHepler(int iThreadNum = 1)
        {
            m_ThreadNum = iThreadNum < 1 ? 1 : iThreadNum;
            m_Thread    = new ThreadHelper(m_ThreadNum);
        }

        public int Start(string sUrl, string sSavePath)
        {
            if (String.IsNullOrWhiteSpace(sUrl) || String.IsNullOrWhiteSpace(sSavePath))
                return -1;

            try
            {
                HttpWebRequest aRequest = (HttpWebRequest)WebRequest.Create(sUrl);
                HttpWebResponse aResponse = (HttpWebResponse)aRequest.GetResponse();
                long lFileSize = aResponse.ContentLength;

                int iSingelNum = (int)(lFileSize / m_ThreadNum);  //平均分配
                int remainder = (int)(lFileSize % m_ThreadNum);   //获取剩余的

            }
            catch { return -1; }

            return 0;
        }




        private static readonly string DefaultUserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";
        /// <summary>  
        /// 创建GET方式的HTTP请求  
        /// </summary>  
        /// <param name="url">请求的URL</param>  
        /// <param name="timeout">请求的超时时间</param>  
        /// <param name="userAgent">请求的客户端浏览器信息，可以为空</param>  
        /// <param name="cookies">随同HTTP请求发送的Cookie信息，如果不需要身份验证可以为空</param>  
        /// <returns></returns>  
        public static HttpWebResponse CreateGetHttpResponse(string url, int? timeout, string userAgent, CookieCollection cookies)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException("url");
            }
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = "GET";
            request.UserAgent = DefaultUserAgent;
            if (!string.IsNullOrEmpty(userAgent))
            {
                request.UserAgent = userAgent;
            }
            if (timeout.HasValue)
            {
                request.Timeout = timeout.Value;
            }
            if (cookies != null)
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(cookies);
            }
            return request.GetResponse() as HttpWebResponse;
        }  

    }



}
