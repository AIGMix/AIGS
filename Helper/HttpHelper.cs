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
using AIGS.Common;
namespace AIGS.Helper
{
    public class HttpHelper
    {
        public class Result
        {
            public bool   Success { get; set; }
            public string Errmsg { get; set; }
            public string Errresponse { get; set; }
            public object oData { get; set; }
            public string sData { get; set; }
            public Result(bool IsSuccess, string err, object odata, string sdata, string sErrresponse=null)
            {
                Success = IsSuccess;
                Errmsg = err;
                oData = odata;
                sData = sdata;
                Errresponse = sErrresponse;
            }
            public Result()
            {
                Success = true;
                Errmsg = null;
                oData = null;
                sData = null;
                Errresponse = null;
            }
        }

        public class ProxyInfo
        {
            public string Host { get; set; }
            public int Port { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public ProxyInfo(string host, int port, string user=null, string pwd=null)
            {
                Host = host;
                Port = port;
                Username = user;
                Password = pwd;
            }
        }

        public class Record
        {
            public bool   IsHttp { get; set; }
            public string Host { get; set; } 
            public string Path { get; set; }
            public string FormatStr { get; set; }
            public string Method { get; set; } 
            public bool   IsRetByte { get; set; } 
            public bool   KeepAlive { get; set; } 
            public int    Retry { get; set; } 
            public int    Time { get; set; } 
            public string Accept { get; set; }
            public string Referer { get; set; }
            public string UserAgent { get; set; } 
            public string ContentType { get; set; } 
            public string PostJson { get; set; }
            public Dictionary<string, string> PostParas { get; set; }
            public ProxyInfo Proxy { get; set; }

            public Record()
            {
                Method = "GET";
                IsHttp = false;
                IsRetByte = false;
                KeepAlive = true;
                Retry = 3;
                Time = 5*1000;
                UserAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_13_1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.84 Safari/537.36";
                ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            }
        }

        public static async Task<Result> GetOrPostAsync(string sUrl,
                                Dictionary<string, string> PostData = null,
                                bool IsRetByte                      = false,
                                int Timeout                         = 5 * 1000,
                                bool KeepAlive                      = true,
                                string UserAgent                    = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_13_1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.84 Safari/537.36",
                                string ContentType                  = "application/x-www-form-urlencoded; charset=UTF-8",
                                int Retry                           = 0,
                                CookieContainer Cookie              = null,
                                string Header                       = null,
                                string ElseMethod                   = null,
                                string PostJson                     = null,
                                string Accept                       = null,
                                string Referer                      = null,
                                bool EnableAsync                    = true,
                                ProxyInfo Proxy                     = null,
                                bool AllowAutoRedirect              = true)
        {
            string Errmsg = null;
            string Errrep = null;
            //获取重试的次数
            int iTryNum = Retry > 0 ? Retry + 1 : 1;
            if (iTryNum > 100)
                iTryNum = 100;

            RETRY_POINT:
            try
            {
                ServicePointManager.Expect100Continue = false;

                HttpWebRequest request  = (HttpWebRequest)WebRequest.Create(sUrl);
                request.Method          = PostData == null && PostJson == null ? "GET" : "POST";
                request.ContentType     = ContentType;
                request.Timeout         = Timeout;
                request.KeepAlive       = KeepAlive;
                request.CookieContainer = Cookie == null ? new CookieContainer() : Cookie;
                request.UserAgent       = UserAgent;
                request.Accept          = Accept;
                request.Referer         = Referer;
                request.AllowAutoRedirect = AllowAutoRedirect;
                //request.Proxy           = null;

                if (ElseMethod != null)
                    request.Method = ElseMethod;
                request.Method = request.Method.ToUpper();

                if (!string.IsNullOrEmpty(Header))
                {
                    request.Headers = new WebHeaderCollection();
                    request.Headers.Add(Header);
                }

                if (Proxy != null && Proxy.Host.IsNotBlank() && Proxy.Port >= 0)
                {
                    WebProxy myProxy = new WebProxy(Proxy.Host, Proxy.Port);
                    if(Proxy.Username.IsNotBlank() && Proxy.Password.IsNotBlank())
                        myProxy.Credentials = new NetworkCredential(Proxy.Username, Proxy.Password);

                    request.Proxy = myProxy;
                    request.Credentials = CredentialCache.DefaultNetworkCredentials;
                }

                //装载要POST数据  
                byte[] data = null;
                if (PostData != null && PostData.Count > 0)
                {
                    StringBuilder str = new StringBuilder();
                    foreach (string key in PostData.Keys)
                        //str.AppendFormat("&{0}={1}", key, PostData[key]);
                        str.AppendFormat("&{0}={1}", key, System.Web.HttpUtility.UrlEncode(PostData[key]));
                    

                    data = Encoding.UTF8.GetBytes(str.ToString().Substring(1));

                    //string textstr = System.Text.Encoding.UTF8.GetString(data);
                }
                else if (PostJson != null)
                    data = Encoding.UTF8.GetBytes(PostJson);
                if (data != null)
                {
                    request.ContentLength = data.Length;
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
                HttpWebResponse response = null;
                if (EnableAsync)
                {
                    var resp = await request.GetResponseAsync();
                    response = (HttpWebResponse)resp;
                }
                else
                {
                    var resp = request.GetResponse();
                    response = (HttpWebResponse)resp;
                }

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
                    Stream myResponseStream = response.GetResponseStream();
                    StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
                    retString = myStreamReader.ReadToEnd();
                    myStreamReader.Close();
                    myResponseStream.Close();
                }

                if (request.Method == "HEAD")
                    return new Result(true, Errmsg, response.Headers, null);

                //在最后需要关掉请求,要不然有太多alive
                if (!request.KeepAlive)
                {
                    if (response != null)
                        response.Close();
                    if (request != null)
                        request.Abort();
                }

                if (IsRetByte)
                    return new Result(true, Errmsg, retByte, null);
                return new Result(true, Errmsg, retString, retString); 
            }
            catch (WebException e)
            {
                if (--iTryNum > 0)
                    goto RETRY_POINT;

                Errmsg = e.Message;
                Errrep = null;
                if (e.Response != null && e.Response.GetResponseStream().CanRead)
                {
                    StreamReader myReader = new StreamReader(e.Response.GetResponseStream(), Encoding.GetEncoding("utf-8"));
                    Errrep = myReader.ReadToEnd();
                    myReader.Close();
                }
                return new Result(false, Errmsg, null, null, Errrep);
            }
        }

        public static async Task<Result> GetOrPostAsync(Record Obj, CookieContainer Cookie = null)
        {
            string sPre = Obj.IsHttp ? "http://" : "https://";
            return await GetOrPostAsync(sPre + Obj.Host + Obj.Path, 
                                        PostData:Obj.PostParas,
                                        IsRetByte:Obj.IsRetByte, 
                                        Timeout:Obj.Time,
                                        KeepAlive:Obj.KeepAlive,
                                        UserAgent:Obj.UserAgent,
                                        ContentType:Obj.ContentType,
                                        Cookie:Cookie, 
                                        ElseMethod:Obj.Method,
                                        PostJson:Obj.PostJson,
                                        Accept:Obj.Accept,
                                        Referer:Obj.Referer,
                                        Proxy:Obj.Proxy);
        }

        #region get/post

        public static object GetOrPost(string sUrl,
                                out string Errmsg,
                                Dictionary<string, string> PostData = null,
                                bool    IsRetByte                   = false,
                                int     Timeout                     = 5*1000, 
                                bool    KeepAlive                   = true, 
                                string  UserAgent                   = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_13_1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.84 Safari/537.36",
                                string  ContentType                 = "application/x-www-form-urlencoded; charset=UTF-8",
                                int     Retry                       = 0,
                                CookieContainer Cookie              = null,
                                string  Header                      = null,
                                string  ElseMethod                  = null,
                                string  PostJson                    = null,
                                bool    IsErrResponse               = false,
                                ProxyInfo Proxy                     = null,
                                bool    AllowAutoRedirect           = true,
                                string Referer = null)
        {
            try
            {
                var Res = GetOrPostAsync(sUrl, PostData, IsRetByte, Timeout, KeepAlive, UserAgent, ContentType, Retry, Cookie, Header, ElseMethod, PostJson, EnableAsync: false, Proxy:Proxy, Referer: Referer ,AllowAutoRedirect: AllowAutoRedirect);
                Errmsg = Res.Result.Errmsg;
                if (IsErrResponse)
                    Errmsg = Res.Result.Errresponse;
                return Res.Result.oData;
            }
            catch
            {
                Errmsg = "Err!";
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
                                            out string Errmsg,
                                            int     Timeout        = 5*1000, 
                                            bool    KeepAlive      = true, 
                                            string  UserAgent      = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_13_1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.84 Safari/537.36",
                                            string  ContentType    = "application/x-www-form-urlencoded; charset=UTF-8",
                                            int     Retry          = 0,
                                            CookieContainer Cookie = null,
                                            ProxyInfo Proxy        = null)
        {
            object oValue = GetOrPost(sUrl, out Errmsg, null, false, Timeout, KeepAlive, UserAgent, ContentType, Retry, Cookie, null, Proxy: Proxy);
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
                                    out string Errmsg,
                                    int     Timeout        = 10*1000, 
                                    bool    KeepAlive      = true, 
                                    string  UserAgent      = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_13_1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.84 Safari/537.36",
                                    string  ContentType    = "application/x-www-form-urlencoded; charset=UTF-8",
                                    int     Retry          = 0,
                                    CookieContainer Cookie = null,
                                    ProxyInfo Proxy = null)
        {
            object oValue = GetOrPost(sUrl, out Errmsg, null, false, Timeout, KeepAlive, UserAgent, ContentType, Retry, Cookie, null, Proxy: Proxy);
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
