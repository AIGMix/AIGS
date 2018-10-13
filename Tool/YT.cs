using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Web;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Text.RegularExpressions;
using System.Collections.Specialized;

using System.IO.Compression;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
namespace YTDownload_CMD
{
    public class YTUrlInfo
    {
        public int iTag;            //iTag值
        public string URL;          //下载链接
        public string eQuality;     //品质
        public string Type;         //类型
        public string Resolution;   //分辨率
        public string Extension;    //扩展名
    }
    public class YTVideoInfo
    {
        public string Url;                  //原视频链接
        public string ID;                   //原视频ID
        public string Title;                //标题
        public List<string> PhotoUrl;       //图片Url
        public List<YTUrlInfo> UrlList;     //Url信息集合
        public YT.ErrCode eECode;           //错误码

        public bool Flag;                   //备用标志
    }

    public class YT
    {
        //https://www.youtube.com/watch?v=M3exHtTtDOw

        #region 宏定义
        private const string URLPRE1 = "https://www.youtube.com/watch?v=";
        private const string URLPRE2 = "http://www.youtube.com/watch?v=";
        private const string URLINFO = "http://www.youtube.com/get_video_info?video_id=";
        #endregion

        #region 枚举量
        public enum ErrCode
        {
            Success,
            Err_GetVideoID,
            Err_GetVideoInfo,
            Err_GetBaseInfo,
            Err_GetDownloadUrl,
        }

        public enum Extension
        {
            eWebm,
            eMp4,
            e3gpp,
        }
        #endregion

        #region 解析Url接口返回的信息
        /// <summary>
        /// 通过URL获取相应的视频ID。
        /// </summary>
        /// <param name="Url">Url</param>
        /// <param name="ID">视频ID</param>
        private static int GetVideoID(string Url,ref string ID)
        {
            if (string.IsNullOrWhiteSpace(Url))
                return -1;

            if(Url.IndexOf(URLPRE1) == 0)
            {
                string[] sArray = Url.Split('=');
                if (sArray.Length != 2)
                    return -1;
                ID = sArray[1];
                return 0;
            }
            else if (Url.IndexOf(URLPRE2) == 0)
            {
                string[] sArray = Url.Split('=');
                if (sArray.Length != 2)
                    return -1;
                ID = sArray[1];
                return 0;
            }

            return -1;
        }

        /// <summary>
        /// 获取视频基本信息。
        /// </summary>
        private static int GetVideoBaseInfo(string sMsg, ref YTVideoInfo Info)
        {
            string sText;
            //Title
            sText = GetParaValue(sMsg, "title=", "&");
            if (string.IsNullOrWhiteSpace(sText))
                return -1;
            sText = HttpUtility.UrlDecode(sText, Encoding.UTF8);
            Info.Title = sText;

            //ID
            sText = GetParaValue(sMsg, "video_id=", "&");
            if (string.IsNullOrWhiteSpace(sText))
                return -1;
            Info.ID = sText;

            //PhotoUrl
            sText = GetParaValue(sMsg, "thumbnail_url=", "&");
            if (string.IsNullOrWhiteSpace(sText))
                Info.PhotoUrl.Add(null);
            else{
                Info.PhotoUrl.Add(HttpUtility.UrlDecode(sText, Encoding.UTF8));
            }

            sText = GetParaValue(sMsg, "iurlmq=", "&");
            if (string.IsNullOrWhiteSpace(sText))
                Info.PhotoUrl.Add(null);
            else{
                Info.PhotoUrl.Add(HttpUtility.UrlDecode(sText, Encoding.UTF8));
            }

            sText = GetParaValue(sMsg, "iurl=", "&");
            if (string.IsNullOrWhiteSpace(sText))
                Info.PhotoUrl.Add(null);
            else{
                Info.PhotoUrl.Add(HttpUtility.UrlDecode(sText, Encoding.UTF8));
            }
            return 0;
        }

        /// <summary>
        /// 获取视频下载链接信息。
        /// </summary>
        private static int GetVideoUrlInfo(string sMsg, ref YTVideoInfo Info)
        {
            String[] lArgs = sMsg.Split('&');
            if (lArgs.Length == 0)
                return -1;

            //获取视频链接的品质
            Hashtable FmtListHasp = new Hashtable();
            for(int i = 0; i < lArgs.Length; i++)
            {
                string sText = GetParaValue(sMsg, "fmt_list=", "&");
                if (string.IsNullOrWhiteSpace(sText))
                    continue;
                sText = HttpUtility.UrlDecode(sText, Encoding.UTF8);
                
                string[] sList = sText.Split(',');
                foreach(string sBuf in sList)
                {
                    string[] sFmts = sBuf.Split('/');
                    FmtListHasp.Add(sFmts[0],sFmts[1]);
                }
                break;
            }
            if(FmtListHasp.Count == 0)
                return -1;


            //视频链接的流
            string FmStreamMap = null;
            for(int i = 0; i < lArgs.Length; i++)
            {
                FmStreamMap = GetParaValue(sMsg, "url_encoded_fmt_stream_map=", null);
                if (string.IsNullOrWhiteSpace(FmStreamMap))
                    continue;
                FmStreamMap = HttpUtility.UrlDecode(FmStreamMap, Encoding.UTF8);
                break;
            }
            if (FmStreamMap == null)
                return -1;

            //获取每一个视频链接
            string sTmp;
            String[] lUrlInfos = FmStreamMap.Split(',');
            for(int i = 0; i < lUrlInfos.Length; i++)
            {
                string sUrl = null, iTag = null, quality = null, type = null;
                lArgs = lUrlInfos[i].Split('&');
                for(int j = 0; j < lArgs.Length; j++)
                {
                    if (iTag == null)
                        if ((sTmp = GetParaValue(lArgs[j], "itag=", null)) != null)
                            iTag = HttpUtility.UrlDecode(sTmp, Encoding.UTF8);
                    if (quality == null)
                        if ((sTmp = GetParaValue(lArgs[j], "quality=", null)) != null)
                            quality = HttpUtility.UrlDecode(sTmp, Encoding.UTF8);
                    if (type == null)
                        if ((sTmp = GetParaValue(lArgs[j], "type=", null)) != null)
                            type = HttpUtility.UrlDecode(sTmp, Encoding.UTF8);
                    if (sUrl == null)
                        if ((sTmp = GetParaValue(lArgs[j], "url=", null)) != null)
                        {
                            sUrl = HttpUtility.UrlDecode(sTmp, Encoding.UTF8);
                            sUrl = HttpUtility.UrlDecode(sUrl, Encoding.UTF8);
                        }
                }
                if (sUrl == null || iTag == null || quality == null || type == null)
                    continue;
                YTUrlInfo aNewUrl = new YTUrlInfo();
                aNewUrl.eQuality = quality;
                aNewUrl.URL = sUrl;
                aNewUrl.Type = type;
                int.TryParse(iTag,out aNewUrl.iTag);
                aNewUrl.Extension = GetExtensionByiTag(aNewUrl.iTag);
                aNewUrl.Resolution = (string)FmtListHasp[iTag];

                if (Info.UrlList == null)
                    Info.UrlList = new List<YTUrlInfo>();
                Info.UrlList.Add(aNewUrl);
            }
            return 0;
        }

        /// <summary>
        /// 获取视频下载链接的品质。
        /// </summary>
        private static List<int> GetFmtList(string sMsg)
        {
            String[] lArgs = sMsg.Split('&');
            if (lArgs.Length == 0)
                return null;

            //获取视频链接的品质
            List<int> FmtListList = new List<int>();
            for (int i = 0; i < lArgs.Length; i++)
            {
                string sText = GetParaValue(sMsg, "fmt_list=", null);
                if (string.IsNullOrWhiteSpace(sText))
                    continue;
                sText = HttpUtility.UrlDecode(sText, Encoding.UTF8);

                string[] sList = sText.Split(',');
                foreach (string sBuf in sList)
                {
                    string[] sFmts = sBuf.Split('/');
                    FmtListList.Add(int.Parse(sFmts[0]));
                }
                break;
            }

            return FmtListList;
        }

        #endregion

        #region 对外接口
        /// <summary>
        /// 获取视频信息。
        /// </summary>
        public static YTVideoInfo GetVideoInfo(string Url)
        {
            YTVideoInfo Info = InitYTVideoInfo();
            Info.Url = Url;

            //获取ID
            string ID = null;
            if (GetVideoID(Url, ref ID) != 0)
            {
                Info.eECode = ErrCode.Err_GetVideoID;
                return Info;
            }
            
            //获取VideoInfo
            string sURL_Info = URLINFO + ID;
            string sMsg = DownloadString(sURL_Info);
            if (string.IsNullOrWhiteSpace(sMsg))
            {
                Info.eECode = ErrCode.Err_GetVideoInfo;
                return Info;
            }

            //获取基础信息
            if (GetVideoBaseInfo(sMsg, ref Info) != 0)
            {
                Info.eECode = ErrCode.Err_GetBaseInfo;
                return Info;
            }

            //获取下载链接
            if (GetVideoUrlInfo(sMsg, ref Info) != 0)
            {
                Info.eECode = ErrCode.Err_GetDownloadUrl;
                return Info;
            }

            return Info;
        }

        /// <summary>
        /// 获取下一个品质的链接序号
        /// </summary>
        public static int GetNextQualityIndex(int iTag, Extension eType, YTVideoInfo Info)
        {
            List<int> SupportItag = new List<int>();
            SupportItag.Add(13);//3GPP (MPEG-4 encoded) Low quality
            SupportItag.Add(17);//3GPP (MPEG-4 encoded) Small quality
            SupportItag.Add(36);//3GPP (MPEG-4 encoded) Small quality
            SupportItag.Add(18);//MP4  (H.264 encoded) Medium quality
            SupportItag.Add(43);//WEBM  (H.264 encoded) Medium quality
            SupportItag.Add(22);//MP4  (H.264 encoded) High quality
            SupportItag.Add(37);//MP4  (H.264 encoded) High quality

            int iIndex = -1;
            for (int i = 0; i < SupportItag.Count; i++ )
            {
                if (SupportItag[i] != iTag)
                    continue;
                iIndex = i;
                break;
            }
            iIndex = iIndex == -1?SupportItag.Count - 1:iIndex;

            int iRet = 0;
            for (int i = iIndex; i >= 0; i-- )
            {
                for(int j =0; j < Info.UrlList.Count; j++)
                {
                    if (Info.UrlList[j].iTag != SupportItag[i])
                        continue;
                    if (Info.UrlList[j].Extension != GetExtensionByEnum(eType))
                        continue;
                    return j;
                }
            }
            return iRet;
        }

        /// <summary>
        /// 获取最佳品质的链接序号
        /// </summary>
        public static int GetMaxQualityIndex(Extension eType, YTVideoInfo Info)
        {
            return GetNextQualityIndex(-1, eType, Info);
        }

        /// <summary>
        /// 将信息写入到文件中
        /// </summary>
        public static bool SaveYTVideoInfoToFile(YTVideoInfo Info, string sDir)
        {
            if (string.IsNullOrWhiteSpace(sDir))
                return false;
            if (Info.eECode != ErrCode.Success)
                return false;

            if (!Directory.Exists(sDir))
                Directory.CreateDirectory(sDir);

            int iIndex = 0;
            string[] Lines = new string[100];
            Lines[iIndex++] = PadRightEx("[URL]", 20) + Info.Url;
            Lines[iIndex++] = PadRightEx("[ID]", 20) + Info.ID;
            Lines[iIndex++] = PadRightEx("[Title]", 20) + Info.Title;
            Lines[iIndex++] = "\n";
            Lines[iIndex++] = PadRightEx("========PhotoUrl========", 20);
            for (int i = 0, iNum = 0; i < Info.PhotoUrl.Count; i++ )
            {
                if (string.IsNullOrWhiteSpace(Info.PhotoUrl[i]))
                    continue;

                Lines[iIndex++] = PadRightEx("[" + iNum + "]", 20) + Info.PhotoUrl[i];
                iNum++;
            }
            Lines[iIndex++] = "\n";
            Lines[iIndex++] = PadRightEx("========UrlList========", 20);
            for (int i = 0; i < Info.UrlList.Count; i++)
            {
                Lines[iIndex++] = PadRightEx(i + "[iTag]", 20) + Info.UrlList[i].iTag;
                Lines[iIndex++] = PadRightEx(i + "[URL]", 20) + Info.UrlList[i].URL;
                Lines[iIndex++] = PadRightEx(i + "[eQuality]", 20) + Info.UrlList[i].eQuality;
                Lines[iIndex++] = PadRightEx(i + "[Type]", 20) + Info.UrlList[i].Type;
                Lines[iIndex++] = PadRightEx(i + "[Resolution]", 20) + Info.UrlList[i].Resolution;
                Lines[iIndex++] = "\n";
            }

            string sFileName = sDir + "\\" + Info.Title + ".txt";
            File.WriteAllLines(sFileName, Lines);
            return true;
        }

        /// <summary>
        /// 查询链接是否正确
        /// </summary>
        public static bool IsUrlCorrect(string sUrl)
        {
            sUrl = sUrl.Trim();
            if(sUrl.IndexOf(URLPRE1)!=0)
                if(sUrl.IndexOf(URLPRE2)!=0)
                    return false;

            return true;
        }

        #endregion

        #region 关于下载与解析
        /// <summary>
        /// 功能：从Url获取信息
        /// </summary>
        public static string DownloadString(string Url)
        {
            string RetMsg;
            WebClient client = new WebClient();
            try
            {
                WebRequest myre = WebRequest.Create(Url);
            }
            catch
            {
                //WriteLog(exp);
                client.Dispose();
                return null;
            }
            try
            {
                RetMsg = client.DownloadString(Url);
            }
            catch
            {
                //WriteLog(exp);
                client.Dispose();
                return null;
            }
            client.Dispose();
            return RetMsg;
        }

        /// <summary>
        /// 功能：下载文件
        /// </summary>
        public static int DownloadFile(string Url, string FilePath)
        {
            if (string.IsNullOrWhiteSpace(Url) || string.IsNullOrWhiteSpace(FilePath))
                return -1;
            
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
            WebClient client = new WebClient();
            try
            {
                WebRequest myre = WebRequest.Create(Url);
            }
            catch{return -1;}
            try
            {
                client.DownloadFile(Url, FilePath);
            }
            catch { return -1; }
            return 0;
        }
        
        /// <summary>
        /// 分析url链接，返回参数集合
        /// </summary>
        /// <param name="url">url链接</param>
        /// <param name="baseUrl"></param>
        /// <returns></returns>
        private static System.Collections.Specialized.NameValueCollection ParseUrl(string url, out string baseUrl)
        {
            baseUrl = "";
            if (string.IsNullOrEmpty(url))
                return null;
            System.Collections.Specialized.NameValueCollection nvc = new System.Collections.Specialized.NameValueCollection();
            try
            {
                int questionMarkIndex = url.IndexOf('?');

                if (questionMarkIndex == -1)
                    baseUrl = url;
                else
                    baseUrl = url.Substring(0, questionMarkIndex);
                if (questionMarkIndex == url.Length - 1)
                    return null;
                string ps = url.Substring(questionMarkIndex + 1);

                // 开始分析参数对   
                System.Text.RegularExpressions.Regex re = new System.Text.RegularExpressions.Regex(@"(^|&)?(\w+)=([^&]+)(&|$)?", System.Text.RegularExpressions.RegexOptions.Compiled);
                System.Text.RegularExpressions.MatchCollection mc = re.Matches(ps);

                foreach (System.Text.RegularExpressions.Match m in mc)
                {
                    nvc.Add(m.Result("$2").ToLower(), m.Result("$3"));
                }
            }
            catch { }
            return nvc;
        }
        #endregion

        #region 工具

        private static string GetExtensionByiTag(int iTag)
        {
            switch(iTag)
            {
                case 13:
                case 17:
                case 36: return ".3gpp";
                case 18:
                case 22:
                case 37: return ".mp4";
                case 43: return ".webm";
                default: return ".mp4";
            }
        }
        public static string GetExtensionByEnum(Extension eType)
        {
            switch(eType)
            {
                case Extension.eMp4: return ".mp4";
                case Extension.eWebm: return ".webm";
                case Extension.e3gpp: return ".3gpp";
                default: return ".mp4";
            }
        }


        /// <summary>
        /// 初始化YTVideoInfo
        /// </summary>
        private static YTVideoInfo InitYTVideoInfo()
        {
            YTVideoInfo Info = new YTVideoInfo();
            Info.UrlList = new List<YTUrlInfo>();
            Info.PhotoUrl = new List<string>();
            Info.eECode = ErrCode.Success;

            return Info;
        }

        /// <summary>
        /// 使用正则表达式获取参数值(可以看成一条获取字符串子串的函数)。
        /// </summary>
        /// <param name="sMsg">字符串</param>
        /// <param name="FindStr">寻找的Key,如果有等于号需要加上,如“key=”</param>
        /// <param name="EndChar">寻找的结束符号</param>
        private static string GetParaValue(string sMsg, string FindStr, string EndChar)
        {
            string formt = FindStr + "(.*)";
            if (!string.IsNullOrWhiteSpace(EndChar))
                formt = FindStr + "(.*?)" + EndChar;

            Regex reg = new Regex(formt);
            Match aTmp = reg.Match(sMsg);
            if (string.IsNullOrWhiteSpace(aTmp.Value))
                return null;

            string sText = aTmp.Value;
            int iLen = sText.Length - FindStr.Length;
            iLen -= string.IsNullOrWhiteSpace(EndChar) ? 0 : EndChar.Length;
            sText = sText.Substring(FindStr.Length, iLen);

            return sText;
        }

        /// <summary>
        /// 向文本文件中写入内容
        /// </summary>
        /// <param name="filePath">文件的绝对路径</param>
        /// <param name="text">写入的内容</param>
        /// <param name="encoding">编码</param>
        private static void WriteText(string filePath, string text, Encoding encoding)
        {
            //向文件写入内容
            File.WriteAllText(filePath, text, encoding);
        }

        private static string PadRightEx(string str, int totalByteCount)
        {
            Encoding coding = Encoding.GetEncoding("gb2312");
            int dcount = 0;
            foreach (char ch in str.ToCharArray())
            {
                if (coding.GetByteCount(ch.ToString()) == 2)
                    dcount++;
            }
            string w = str.PadRight(totalByteCount - dcount, ' ');
            return w;
        }
        #endregion
    }








    /// <summary>
    /// Http连接操作帮助类 
    /// </summary>
    public class HttpHelper
    {
        #region 模拟GET
        /// <summary>
        /// GET请求
        /// </summary>
        /// <param name="Url">The URL.</param>
        /// <param name="postDataStr">The post data string.</param>
        /// <returns>System.String.</returns>
        public static string HttpGet(string Url, string postDataStr)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url + (postDataStr == "" ? "" : "?") + postDataStr);
            request.Method = "GET";
            request.ContentType = "text/html;charset=UTF-8";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();

            return retString;
        }
        #endregion

        #region 模拟POST
        /// <summary>
        /// POST请求
        /// </summary>
        /// <param name="posturl">The posturl.</param>
        /// <param name="postData">The post data.</param>
        /// <returns>System.String.</returns>
        public static string HttpPost(string posturl, string postData)
        {
            Stream outstream = null;
            Stream instream = null;
            StreamReader sr = null;
            HttpWebResponse response = null;
            HttpWebRequest request = null;
            Encoding encoding = System.Text.Encoding.GetEncoding("utf-8");
            byte[] data = encoding.GetBytes(postData);
            // 准备请求...
            try
            {
                // 设置参数
                request = WebRequest.Create(posturl) as HttpWebRequest;
                CookieContainer cookieContainer = new CookieContainer();
                request.CookieContainer = cookieContainer;
                request.AllowAutoRedirect = true;
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = data.Length;
                outstream = request.GetRequestStream();
                outstream.Write(data, 0, data.Length);
                outstream.Close();
                //发送请求并获取相应回应数据
                response = request.GetResponse() as HttpWebResponse;
                //直到request.GetResponse()程序才开始向目标网页发送Post请求
                instream = response.GetResponseStream();
                sr = new StreamReader(instream, encoding);
                //返回结果网页（html）代码
                string content = sr.ReadToEnd();
                string err = string.Empty;
                return content;
            }
            catch (Exception ex)
            {
                string err = ex.Message;
                return string.Empty;
            }
        }

        /// <summary>
        /// 模拟httpPost提交表单
        /// </summary>
        /// <param name="url">POS请求的网址</param>
        /// <param name="data">表单里的参数和值</param>
        /// <param name="encoder">页面编码</param>
        /// <returns></returns>
        public static string CreateAutoSubmitForm(string url, Dictionary<string, string> data, Encoding encoder)
        {
            StringBuilder html = new StringBuilder();
            html.AppendLine("<html>");
            html.AppendLine("<head>");
            html.AppendFormat("<meta http-equiv=\"Content-Type\" content=\"text/html; charset={0}\" />", encoder.BodyName);
            html.AppendLine("</head>");
            html.AppendLine("<body onload=\"OnLoadSubmit();\">");
            html.AppendFormat("<form id=\"pay_form\" action=\"{0}\" method=\"post\">", url);
            foreach (KeyValuePair<string, string> kvp in data)
            {
                html.AppendFormat("<input type=\"hidden\" name=\"{0}\" id=\"{0}\" value=\"{1}\" />", kvp.Key, kvp.Value);
            }
            html.AppendLine("</form>");
            html.AppendLine("<script type=\"text/javascript\">");
            html.AppendLine("<!--");
            html.AppendLine("function OnLoadSubmit()");
            html.AppendLine("{");
            html.AppendLine("document.getElementById(\"pay_form\").submit();");
            html.AppendLine("}");
            html.AppendLine("//-->");
            html.AppendLine("</script>");
            html.AppendLine("</body>");
            html.AppendLine("</html>");
            return html.ToString();
        }
        #endregion


        #region 预定义方法或者变更
        //默认的编码
        private Encoding encoding = Encoding.Default;
        //HttpWebRequest对象用来发起请求
        private HttpWebRequest request = null;
        //获取影响流的数据对象
        private HttpWebResponse response = null;
        /// <summary>
        /// 根据相传入的数据，得到相应页面数据
        /// </summary>
        /// <param name="strPostdata">传入的数据Post方式,get方式传NUll或者空字符串都可以</param>
        /// <returns>string类型的响应数据</returns>
        private HttpResult GetHttpRequestData(HttpItem objhttpitem)
        {
            //返回参数
            HttpResult result = new HttpResult();
            try
            {
                #region 得到请求的response
                using (response = (HttpWebResponse)request.GetResponse())
                {
                    result.Header = response.Headers;
                    if (response.Cookies != null)
                    {
                        result.CookieCollection = response.Cookies;
                    }
                    if (response.Headers["set-cookie"] != null)
                    {
                        result.Cookie = response.Headers["set-cookie"];
                    }

                    MemoryStream _stream = new MemoryStream();
                    //GZIIP处理
                    if (response.ContentEncoding != null && response.ContentEncoding.Equals("gzip", StringComparison.InvariantCultureIgnoreCase))
                    {
                        //开始读取流并设置编码方式
                        //new GZipStream(response.GetResponseStream(), CompressionMode.Decompress).CopyTo(_stream, 10240);
                        //.net4.0以下写法
                        _stream = GetMemoryStream(new GZipStream(response.GetResponseStream(), CompressionMode.Decompress));
                    }
                    else
                    {
                        //开始读取流并设置编码方式
                        //response.GetResponseStream().CopyTo(_stream, 10240);
                        //.net4.0以下写法
                        _stream = GetMemoryStream(response.GetResponseStream());
                    }
                    //获取Byte
                    byte[] RawResponse = _stream.ToArray();
                    //是否返回Byte类型数据
                    if (objhttpitem.ResultType == ResultType.Byte)
                    {
                        result.ResultByte = RawResponse;
                    }
                    //从这里开始我们要无视编码了
                    if (encoding == null)
                    {
                        string temp = Encoding.Default.GetString(RawResponse, 0, RawResponse.Length);
                        //<meta(.*?)charset([\s]?)=[^>](.*?)>
                        Match meta = Regex.Match(temp, "<meta([^<]*)charset=([^<]*)[\"']", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                        string charter = (meta.Groups.Count > 2) ? meta.Groups[2].Value : string.Empty;
                        charter = charter.Replace("\"", string.Empty).Replace("'", string.Empty).Replace(";", string.Empty);
                        if (charter.Length > 0)
                        {
                            charter = charter.ToLower().Replace("iso-8859-1", "gbk");
                            encoding = Encoding.GetEncoding(charter);
                        }
                        else
                        {
                            if (response.CharacterSet.ToLower().Trim() == "iso-8859-1")
                            {
                                encoding = Encoding.GetEncoding("gbk");
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(response.CharacterSet.Trim()))
                                {
                                    encoding = Encoding.UTF8;
                                }
                                else
                                {
                                    encoding = Encoding.GetEncoding(response.CharacterSet);
                                }
                            }
                        }
                    }
                    //得到返回的HTML
                    result.Html = encoding.GetString(RawResponse);
                    //最后释放流
                    _stream.Close();
                }
                #endregion
            }
            catch (WebException ex)
            {
                //这里是在发生异常时返回的错误信息
                result.Html = "String Error";
                response = (HttpWebResponse)ex.Response;
            }
            if (objhttpitem.IsToLower)
            {
                result.Html = result.Html.ToLower();
            }
            return result;
        }

        /// <summary>
        /// 4.0以下.net版本取数据使用
        /// </summary>
        /// <param name="streamResponse">流</param>
        private static MemoryStream GetMemoryStream(Stream streamResponse)
        {
            MemoryStream _stream = new MemoryStream();
            int Length = 256;
            Byte[] buffer = new Byte[Length];
            int bytesRead = streamResponse.Read(buffer, 0, Length);
            // write the required bytes  
            while (bytesRead > 0)
            {
                _stream.Write(buffer, 0, bytesRead);
                bytesRead = streamResponse.Read(buffer, 0, Length);
            }
            return _stream;
        }

        /// <summary>
        /// 为请求准备参数
        /// </summary>
        ///<param name="objhttpItem">参数列表</param>
        /// <param name="_Encoding">读取数据时的编码方式</param>
        private void SetRequest(HttpItem objhttpItem)
        {
            // 验证证书
            SetCer(objhttpItem);
            // 设置代理
            SetProxy(objhttpItem);
            //请求方式Get或者Post
            request.Method = objhttpItem.Method;
            request.Timeout = objhttpItem.Timeout;
            request.ReadWriteTimeout = objhttpItem.ReadWriteTimeout;
            //Accept
            request.Accept = objhttpItem.Accept;
            //ContentType返回类型
            request.ContentType = objhttpItem.ContentType;
            //UserAgent客户端的访问类型，包括浏览器版本和操作系统信息
            request.UserAgent = objhttpItem.UserAgent;
            // 编码
            SetEncoding(objhttpItem);
            //设置Cookie
            SetCookie(objhttpItem);
            //来源地址
            request.Referer = objhttpItem.Referer;
            //是否执行跳转功能
            request.AllowAutoRedirect = objhttpItem.Allowautoredirect;
            //设置Post数据
            SetPostData(objhttpItem);
            //设置最大连接
            if (objhttpItem.Connectionlimit > 0)
            {
                request.ServicePoint.ConnectionLimit = objhttpItem.Connectionlimit;
            }
        }
        /// <summary>
        /// 设置证书
        /// </summary>
        /// <param name="objhttpItem"></param>
        private void SetCer(HttpItem objhttpItem)
        {
            if (!string.IsNullOrEmpty(objhttpItem.CerPath))
            {
                //这一句一定要写在创建连接的前面。使用回调的方法进行证书验证。
                ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(CheckValidationResult);
                //初始化对像，并设置请求的URL地址
                request = (HttpWebRequest)WebRequest.Create(GetUrl(objhttpItem.URL));
                //创建证书文件
                X509Certificate objx509 = new X509Certificate(objhttpItem.CerPath);
                //添加到请求里
                request.ClientCertificates.Add(objx509);
            }
            else
            {
                //初始化对像，并设置请求的URL地址
                request = (HttpWebRequest)WebRequest.Create(GetUrl(objhttpItem.URL));
            }
        }
        /// <summary>
        /// 设置编码
        /// </summary>
        /// <param name="objhttpItem">Http参数</param>
        private void SetEncoding(HttpItem objhttpItem)
        {
            if (string.IsNullOrEmpty(objhttpItem.Encoding) || objhttpItem.Encoding.ToLower().Trim() == "null")
            {
                //读取数据时的编码方式
                encoding = null;
            }
            else
            {
                //读取数据时的编码方式
                encoding = System.Text.Encoding.GetEncoding(objhttpItem.Encoding);
            }
        }
        /// <summary>
        /// 设置Cookie
        /// </summary>
        /// <param name="objhttpItem">Http参数</param>
        private void SetCookie(HttpItem objhttpItem)
        {
            if (!string.IsNullOrEmpty(objhttpItem.Cookie))
            {
                //Cookie
                request.Headers[HttpRequestHeader.Cookie] = objhttpItem.Cookie;
            }
            //设置Cookie
            if (objhttpItem.CookieCollection != null)
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(objhttpItem.CookieCollection);
            }
        }
        /// <summary>
        /// 设置Post数据
        /// </summary>
        /// <param name="objhttpItem">Http参数</param>
        private void SetPostData(HttpItem objhttpItem)
        {
            //验证在得到结果时是否有传入数据
            if (request.Method.Trim().ToLower().Contains("post"))
            {
                //写入Byte类型
                if (objhttpItem.PostDataType == PostDataType.Byte)
                {
                    //验证在得到结果时是否有传入数据
                    if (objhttpItem.PostdataByte != null && objhttpItem.PostdataByte.Length > 0)
                    {
                        request.ContentLength = objhttpItem.PostdataByte.Length;
                        request.GetRequestStream().Write(objhttpItem.PostdataByte, 0, objhttpItem.PostdataByte.Length);
                    }
                }//写入文件
                else if (objhttpItem.PostDataType == PostDataType.FilePath)
                {
                    StreamReader r = new StreamReader(objhttpItem.Postdata, encoding);
                    byte[] buffer = Encoding.Default.GetBytes(r.ReadToEnd());
                    r.Close();
                    request.ContentLength = buffer.Length;
                    request.GetRequestStream().Write(buffer, 0, buffer.Length);
                }
                else
                {
                    //验证在得到结果时是否有传入数据
                    if (!string.IsNullOrEmpty(objhttpItem.Postdata))
                    {
                        byte[] buffer = Encoding.Default.GetBytes(objhttpItem.Postdata);
                        request.ContentLength = buffer.Length;
                        request.GetRequestStream().Write(buffer, 0, buffer.Length);
                    }
                }
            }
        }
        /// <summary>
        /// 设置代理
        /// </summary>
        /// <param name="objhttpItem">参数对象</param>
        private void SetProxy(HttpItem objhttpItem)
        {
            if (string.IsNullOrEmpty(objhttpItem.ProxyUserName) && string.IsNullOrEmpty(objhttpItem.ProxyPwd) && string.IsNullOrEmpty(objhttpItem.ProxyIp))
            {
                //不需要设置
            }
            else
            {
                //设置代理服务器
                WebProxy myProxy = new WebProxy(objhttpItem.ProxyIp, false);
                //建议连接
                myProxy.Credentials = new NetworkCredential(objhttpItem.ProxyUserName, objhttpItem.ProxyPwd);
                //给当前请求对象
                request.Proxy = myProxy;
                //设置安全凭证
                request.Credentials = CredentialCache.DefaultNetworkCredentials;
            }
        }
        /// <summary>
        /// 回调验证证书问题
        /// </summary>
        /// <param name="sender">流对象</param>
        /// <param name="certificate">证书</param>
        /// <param name="chain">X509Chain</param>
        /// <param name="errors">SslPolicyErrors</param>
        /// <returns>bool</returns>
        public bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            // 总是接受    
            return true;
        }
        #endregion
        #region 普通类型
        /// <summary>    
        /// 传入一个正确或不正确的URl，返回正确的URL
        /// </summary>    
        /// <param name="URL">url</param>   
        /// <returns>
        /// </returns>
        public static string GetUrl(string URL)
        {
            if (!(URL.Contains("http://") || URL.Contains("https://")))
            {
                URL = "http://" + URL;
            }
            return URL;
        }
        ///<summary>
        ///采用https协议访问网络,根据传入的URl地址，得到响应的数据字符串。
        ///</summary>
        ///<param name="objhttpItem">参数列表</param>
        ///<returns>String类型的数据</returns>
        public HttpResult GetHtml(HttpItem objhttpItem)
        {
            //准备参数
            SetRequest(objhttpItem);
            //调用专门读取数据的类
            return GetHttpRequestData(objhttpItem);
        }
        #endregion
    }
    /// <summary>
    /// Http请求参考类 
    /// </summary>
    public class HttpItem
    {
        string _URL;
        /// <summary>
        /// 请求URL必须填写
        /// </summary>
        public string URL
        {
            get { return _URL; }
            set { _URL = value; }
        }
        string _Method = "GET";
        /// <summary>
        /// 请求方式默认为GET方式
        /// </summary>
        public string Method
        {
            get { return _Method; }
            set { _Method = value; }
        }
        int _Timeout = 100000;
        /// <summary>
        /// 默认请求超时时间
        /// </summary>
        public int Timeout
        {
            get { return _Timeout; }
            set { _Timeout = value; }
        }
        int _ReadWriteTimeout = 30000;
        /// <summary>
        /// 默认写入Post数据超时间
        /// </summary>
        public int ReadWriteTimeout
        {
            get { return _ReadWriteTimeout; }
            set { _ReadWriteTimeout = value; }
        }
        string _Accept = "text/html, application/xhtml+xml, */*";
        /// <summary>
        /// 请求标头值 默认为text/html, application/xhtml+xml, */*
        /// </summary>
        public string Accept
        {
            get { return _Accept; }
            set { _Accept = value; }
        }
        string _ContentType = "text/html";
        /// <summary>
        /// 请求返回类型默认 text/html
        /// </summary>
        public string ContentType
        {
            get { return _ContentType; }
            set { _ContentType = value; }
        }
        string _UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)";
        /// <summary>
        /// 客户端访问信息默认Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)
        /// </summary>
        public string UserAgent
        {
            get { return _UserAgent; }
            set { _UserAgent = value; }
        }
        string _Encoding = string.Empty;
        /// <summary>
        /// 返回数据编码默认为NUll,可以自动识别
        /// </summary>
        public string Encoding
        {
            get { return _Encoding; }
            set { _Encoding = value; }
        }
        private PostDataType _PostDataType = PostDataType.String;
        /// <summary>
        /// Post的数据类型
        /// </summary>
        public PostDataType PostDataType
        {
            get { return _PostDataType; }
            set { _PostDataType = value; }
        }
        string _Postdata;
        /// <summary>
        /// Post请求时要发送的字符串Post数据
        /// </summary>
        public string Postdata
        {
            get { return _Postdata; }
            set { _Postdata = value; }
        }
        private byte[] _PostdataByte = null;
        /// <summary>
        /// Post请求时要发送的Byte类型的Post数据
        /// </summary>
        public byte[] PostdataByte
        {
            get { return _PostdataByte; }
            set { _PostdataByte = value; }
        }
        CookieCollection cookiecollection = null;
        /// <summary>
        /// Cookie对象集合
        /// </summary>
        public CookieCollection CookieCollection
        {
            get { return cookiecollection; }
            set { cookiecollection = value; }
        }
        string _Cookie = string.Empty;
        /// <summary>
        /// 请求时的Cookie
        /// </summary>
        public string Cookie
        {
            get { return _Cookie; }
            set { _Cookie = value; }
        }
        string _Referer = string.Empty;
        /// <summary>
        /// 来源地址，上次访问地址
        /// </summary>
        public string Referer
        {
            get { return _Referer; }
            set { _Referer = value; }
        }
        string _CerPath = string.Empty;
        /// <summary>
        /// 证书绝对路径
        /// </summary>
        public string CerPath
        {
            get { return _CerPath; }
            set { _CerPath = value; }
        }
        private Boolean isToLower = true;
        /// <summary>
        /// 是否设置为全文小写
        /// </summary>
        public Boolean IsToLower
        {
            get { return isToLower; }
            set { isToLower = value; }
        }
        private Boolean allowautoredirect = true;
        /// <summary>
        /// 支持跳转页面，查询结果将是跳转后的页面
        /// </summary>
        public Boolean Allowautoredirect
        {
            get { return allowautoredirect; }
            set { allowautoredirect = value; }
        }
        private int connectionlimit = 1024;
        /// <summary>
        /// 最大连接数
        /// </summary>
        public int Connectionlimit
        {
            get { return connectionlimit; }
            set { connectionlimit = value; }
        }
        private string proxyusername = string.Empty;
        /// <summary>
        /// 代理Proxy 服务器用户名
        /// </summary>
        public string ProxyUserName
        {
            get { return proxyusername; }
            set { proxyusername = value; }
        }
        private string proxypwd = string.Empty;
        /// <summary>
        /// 代理 服务器密码
        /// </summary>
        public string ProxyPwd
        {
            get { return proxypwd; }
            set { proxypwd = value; }
        }
        private string proxyip = string.Empty;
        /// <summary>
        /// 代理 服务IP
        /// </summary>
        public string ProxyIp
        {
            get { return proxyip; }
            set { proxyip = value; }
        }
        private ResultType resulttype = ResultType.String;
        /// <summary>
        /// 设置返回类型String和Byte
        /// </summary>
        public ResultType ResultType
        {
            get { return resulttype; }
            set { resulttype = value; }
        }

    }
    /// <summary>
    /// Http返回参数类
    /// </summary>
    public class HttpResult
    {
        string _Cookie = string.Empty;
        /// <summary>
        /// Http请求返回的Cookie
        /// </summary>
        public string Cookie
        {
            get { return _Cookie; }
            set { _Cookie = value; }
        }
        CookieCollection cookiecollection = null;
        /// <summary>
        /// Cookie对象集合
        /// </summary>
        public CookieCollection CookieCollection
        {
            get { return cookiecollection; }
            set { cookiecollection = value; }
        }
        private string html = string.Empty;
        /// <summary>
        /// 返回的String类型数据 只有ResultType.String时才返回数据，其它情况为空
        /// </summary>
        public string Html
        {
            get { return html; }
            set { html = value; }
        }
        private byte[] resultbyte = null;
        /// <summary>
        /// 返回的Byte数组 只有ResultType.Byte时才返回数据，其它情况为空
        /// </summary>
        public byte[] ResultByte
        {

            get { return resultbyte; }
            set { resultbyte = value; }
        }
        private WebHeaderCollection header = new WebHeaderCollection();
        //header对象
        public WebHeaderCollection Header
        {
            get { return header; }
            set { header = value; }
        }

    }

    /// <summary>
    /// 返回类型
    /// </summary>
    public enum ResultType
    {
        String,//表示只返回字符串
        Byte//表示返回字符串和字节流
    }

    /// <summary>
    /// Post的数据格式默认为string
    /// </summary>
    public enum PostDataType
    {
        String,//字符串
        Byte,//字符串和字节流
        FilePath//表示传入的是文件
    }



}
