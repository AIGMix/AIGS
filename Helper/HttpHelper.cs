using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AIGS.Helper
{
    public class HttpHelper
    {
        /// <summary>
        /// 错误信息
        /// </summary>
        public static string Errmsg;

        #region get/post

        public static object GetOrPost(string sUrl,
                                Dictionary<string, string> PostData = null,
                                bool    IsRetByte                   = false,
                                int     Timeout                     = 5*1000, 
                                bool    KeepAlive                   = true, 
                                string  UserAgent                   = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_13_1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.84 Safari/537.36",
                                string  ContentType                 = "application/x-www-form-urlencoded; charset=UTF-8",
                                int     Retry                       = 0,
                                CookieContainer Cookie              = null,
                                string  Header                      = null,
                                string  ElseMethod                  = null)
        {
            //获取重试的次数
            int iTryNum = Retry > 0 ? Retry + 1 : 1;
            if (iTryNum > 100)
                iTryNum = 100;

            RETRY_POINT:
            try
            {
                ServicePointManager.Expect100Continue = false;

                HttpWebRequest request  = (HttpWebRequest)WebRequest.Create(sUrl);
                request.Method          = PostData == null ? "GET" : "POST";
                request.ContentType     = ContentType;
                request.Timeout         = Timeout;
                request.KeepAlive       = KeepAlive;
                request.CookieContainer = Cookie == null ? new CookieContainer() : Cookie;
                request.UserAgent       = UserAgent;

                if (ElseMethod != null)
                    request.Method = ElseMethod;

                if (!string.IsNullOrEmpty(Header))
                {
                    request.Headers = new WebHeaderCollection();
                    request.Headers.Add(Header);
                }

                //装载要POST数据  
                if (!(PostData == null || PostData.Count == 0))
                {
                    StringBuilder str = new StringBuilder();
                    foreach (string key in PostData.Keys)
                    {
                        str.AppendFormat("&{0}={1}", key, PostData[key]);
                    }

                    byte[] data = Encoding.ASCII.GetBytes(str.ToString().Substring(1));
                    using (Stream stream = request.GetRequestStream())
                    {
                        stream.Write(data, 0, data.Length);
                        stream.Close();
                    }
                }

                //垃圾回收，将一些已经废掉的正在保持的http链接丢掉
                //System.Net.ServicePointManager.DefaultConnectionLimit设置的http链接有一定上限
                System.GC.Collect();

                //开始请求
                HttpWebResponse response    = (HttpWebResponse)request.GetResponse();
                Stream myResponseStream     = response.GetResponseStream();

                byte[] retByte = null;
                string retString = null;
                if (IsRetByte)
                {
                    MemoryStream myMemoryStream = GetMemoryStream(response.GetResponseStream());
                    retByte = myMemoryStream.ToArray();
                    myMemoryStream.Close();
                }
                else
                {
                    StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
                    retString = myStreamReader.ReadToEnd();
                    myStreamReader.Close();
                }


                myResponseStream.Close();
                if (request.Method == "HEAD")
                    return response.Headers;

                //在最后需要关掉请求,要不然有太多alive
                if (!request.KeepAlive)
                {
                    if (response != null)
                        response.Close();
                    if (request != null)
                        request.Abort();
                }

                if(IsRetByte)
                    return retByte;
                return retString;
            }
            catch(WebException e)
            {
                if(--iTryNum > 0)
                    goto RETRY_POINT;
                Errmsg = e.Message;
                return null;
            }
        }

        /// <summary>
        /// 4.0以下.net版本取数据使用
        /// </summary>
        /// <param name="streamResponse">流</param>
        private static MemoryStream GetMemoryStream(Stream streamResponse)
        {
            MemoryStream memoryStream = new MemoryStream();
            int length                = 256;
            Byte[] buffer             = new Byte[length];
            int count                 = streamResponse.Read(buffer, 0, length);
            while (count > 0)
            {
                memoryStream.Write(buffer, 0, count);
                count = streamResponse.Read(buffer, 0, length);
            }
            return memoryStream;
        }


        /// <summary>
        /// 下载Jason并转换为对象
        /// </summary>
        public static T GetJsonToObject<T>(string sUrl, 
                                            int     Timeout        = 5*1000, 
                                            bool    KeepAlive      = true, 
                                            string  UserAgent      = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_13_1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.84 Safari/537.36",
                                            string  ContentType    = "application/x-www-form-urlencoded; charset=UTF-8",
                                            int     Retry          = 0,
                                            CookieContainer Cookie = null)
        {
            object oValue = GetOrPost(sUrl, null, false, Timeout, KeepAlive, UserAgent, ContentType, Retry, Cookie);
            if (oValue == null)
                return default(T);

            return AIGS.Helper.JsonHelper.ConverStringToObject<T>(oValue.ToString());
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="sUrl"></param>
        /// <param name="sPath"></param>
        /// <param name="Retry"></param>
        /// <returns></returns>
        public static bool GetFile(string sUrl,
                                    string  sPath,
                                    int     Timeout        = 10*1000, 
                                    bool    KeepAlive      = true, 
                                    string  UserAgent      = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_13_1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.84 Safari/537.36",
                                    string  ContentType    = "application/x-www-form-urlencoded; charset=UTF-8",
                                    int     Retry          = 0,
                                    CookieContainer Cookie = null)
        {
            object oValue = GetOrPost(sUrl, null, false, Timeout, KeepAlive, UserAgent, ContentType, Retry, Cookie);
            if (oValue == null)
                return false;

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(sPath));
                File.WriteAllText(sPath, oValue.ToString());
                return true;
            }
            catch(Exception e)
            {
                Errmsg = e.Message;
                return false;
            }

        }

        #endregion
    }


}
