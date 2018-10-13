using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AIGS.Helper;
using System.IO;
using System.Web;
using System.Collections.Specialized;
using System.Collections;

namespace AIGS.Tool
{
    public class YTBTool
    {
        #region 宏定义

        static string m_DebugUrl = "https://www.youtube.com/watch?v=jKyDCrDo580";
        static string m_PreGetInfo = "http://www.youtube.com/get_video_info?video_id=";
        static string[] m_PreVideo = { "https://www.youtube.com/watch?v=", "http://www.youtube.com/watch?v=" };
        
        #endregion

        #region 枚举与结构体

        public enum ErrCode
        {
            Success,
            UrlErr,
            GetVideoInfoErr,
        }

        public enum Extension
        {
            eWebm,
            eMp4,
            eM4a,
            e3gpp
        }

        public enum Type
        {
            VideoAudio,
            VideoOnly,
            AudioOnly,
        }

        public struct DownloadUrlInfo
        {
            public int iTag;                                   //iTag值(Format Code)
            public string Url;                                 //下载链接
            public string Quality;                             //品质
            public string Codecs;                              //编码器 
            public int Size;                                   //大小（Byte）
            public int BitRate;                                //比特率
            public int Fps;                                    //帧数
            public string Resolution;                          //分辨率
            public Type Type;                                  //类型
            public Extension Extension;                        //扩展名
        }

        public struct VideoInfo
        {
            public string Url;                                 //原视频链接
            public string ID;                                  //原视频ID
            public string Title;                               //标题
            public string Duration;                            //时长（秒）
            public string PhotoUrl;                            //图片Url
            public List<DownloadUrlInfo> DownloadUrls;         //下载Url
            public ErrCode ErrCode;                            //错误码
        }

        #endregion

        #region 基本工具
        /// <summary>
        /// 获取ID
        /// </summary>
        /// <param name="sUrl"></param>
        /// <returns></returns>
        static string GetVideoID(string sUrl)
        {
            if (String.IsNullOrWhiteSpace(sUrl))
                return null;

            for (int i = 0; i < m_PreVideo.Count(); i++)
            {
                if (sUrl.IndexOf(m_PreVideo[i]) != 0)
                    continue;

                string[] sArray = sUrl.Split('=');
                if (sArray.Length != 2)
                    return null;

                return sArray[1];
            }

            return null;
        }

        static string GetNetWorkData(string sID)
        {
            string sUrl     = m_PreGetInfo + sID;
            string sString  = NetHelper.DownloadString(sUrl, 5000);
            string sMsg = sString;// HttpUtility.UrlDecode(sString, Encoding.UTF8);

            byte[] data;
            string[] sArray = sMsg.Split('&');
            FileStream fs = new FileStream("E:\\ak2.txt", FileMode.Create);
            for (int i = 0; i < sArray.Count(); i++)
            {
                data = System.Text.Encoding.Default.GetBytes(sArray[i]);
                fs.Write(data, 0, data.Length);

                data = System.Text.Encoding.Default.GetBytes("\n");
                fs.Write(data, 0, data.Length);
            }
            fs.Flush();
            fs.Close();

            sMsg =HttpUtility.UrlDecode(sString, Encoding.UTF8);
            sArray = sMsg.Split('&');
            fs = new FileStream("E:\\ak3.txt", FileMode.Create);
            for (int i = 0; i < sArray.Count(); i++)
            {
                data = System.Text.Encoding.Default.GetBytes(sArray[i]);
                fs.Write(data, 0, data.Length);

                data = System.Text.Encoding.Default.GetBytes("\n");
                fs.Write(data, 0, data.Length);
            }
            fs.Flush();
            fs.Close();

            sMsg = HttpUtility.UrlDecode(sMsg, Encoding.UTF8);
            sMsg = HttpUtility.UrlDecode(sMsg, Encoding.UTF8);
            sArray = sMsg.Split('&');
            fs = new FileStream("E:\\ak4.txt", FileMode.Create);
            for (int i = 0; i < sArray.Count(); i++)
            {
                data = System.Text.Encoding.Default.GetBytes(sArray[i]);
                fs.Write(data, 0, data.Length);

                data = System.Text.Encoding.Default.GetBytes("\n");
                fs.Write(data, 0, data.Length);
            }
            fs.Flush();
            fs.Close();


            fs = new FileStream("E:\\ak.txt", FileMode.Create);
            data = System.Text.Encoding.Default.GetBytes(sMsg);
            fs.Write(data, 0, data.Length);
            //清空缓冲区、关闭流
            fs.Flush();
            fs.Close();


            return sMsg;
        }

        /// <summary>
        /// 获取标题
        /// </summary>
        /// <param name="sString"></param>
        /// <returns></returns>
        static string GetStringPara(string sString, string sKey)
        {
            string sRet;
            List<string> sFindString = new List<string>();

            if (sKey == "Title")
                sFindString.Add("title=");
            if (sKey == "PhotoUrl")
                sFindString.Add("thumbnail_url=");
            if (sKey == "Duration")
                sFindString.Add("length_seconds=");

            if (sKey == "FmtList")
                sFindString.Add("fmt_list=");
            if (sKey == "UrlEncodedFmtStreamMap")
                sFindString.Add("url_encoded_fmt_stream_map=");
            if (sKey == "AdaptiveFmts")
                sFindString.Add("adaptive_fmts=");

            if (sKey == "Url")
                sFindString.Add("url=");
            if (sKey == "S")
                sFindString.Add("s=");
            if (sKey == "Itag")
                sFindString.Add("itag=");
            if (sKey == "Type")
                sFindString.Add("type=");
            if (sKey == "Quality")
            {
                sFindString.Add("quality=");
                sFindString.Add("quality_label=");
            }

            if (sKey == "Dur")
                sFindString.Add("dur=");
            if (sKey == "Clen")
                sFindString.Add("clen=");
            if (sKey == "BitRate")
                sFindString.Add("bitrate=");
            if (sKey == "Fps")
                sFindString.Add("fps=");
            if (sKey == "Size")
                sFindString.Add("size=");
            if (sKey == "Sig")
                sFindString.Add("sig=");
            if (sKey == "Ratebypass")
                sFindString.Add("ratebypass=");
            

            string[] sArray = sString.Split('&');
            for (int i = 0; i < sFindString.Count; i++)
            {
                for (int j = 0; j < sArray.Count(); j++)
                {
                    if(0 == sArray[j].IndexOf(sFindString[i]))
                    {
                        sRet = sArray[j].Substring(sFindString[i].Length);
                        sRet = HttpUtility.UrlDecode(sRet, Encoding.UTF8);
                        return sRet;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 获取iTag对应分辨率的哈希表
        /// </summary>
        /// <param name="sString"></param>
        /// <returns></returns>
        static Hashtable GetFmtList(string sString)
        {
            //18/640x240,36/320x120,17/176x144
            Hashtable FmtListHash = new Hashtable();
            string sFmtString     = GetStringPara(sString, "FmtList");

            string[] sArray       = sFmtString.Split(',');
            foreach (string sBuf in sArray)
            {
                string[] sFmts = sBuf.Split('/');
                FmtListHash.Add(sFmts[0], sFmts[1]);
            }

            return FmtListHash;
        }

        /// <summary>
        /// 获取扩展类型
        /// </summary>
        /// <param name="sTypeString"></param>
        /// <returns></returns>
        static Extension GetExtension(string sTypeString)
        {
            //video/mp4; codecs="avc1.42001E, mp4a.40.2"
            Extension eRet   = Extension.eWebm;
            string[] sArray  = sTypeString.Split(';');
            string[] sArray2 = sArray[0].Split('/');

            if (sArray2[1] == "mp4")
            {
                eRet = Extension.eMp4;
                if(sArray2[0] == "audio")
                    eRet = Extension.eM4a;
            }
            if (sArray2[1] == "3gpp")
                eRet = Extension.e3gpp;
            if (sArray2[1] == "webm")
                eRet = Extension.eWebm;
            return eRet;
        }

        /// <summary>
        /// 获取编码
        /// </summary>
        /// <param name="sTypeString"></param>
        /// <returns></returns>
        static string GetCodecs(string sTypeString)
        {
            //type=video/mp4; codecs="avc1.42001E, mp4a.40.2"
            string[] sArray = sTypeString.Split(';');
            string[] sArray2 = sArray[1].Split('=');

            return sArray2[1];
        }

        /// <summary>
        /// 获取类型
        /// </summary>
        /// <param name="sTypeString"></param>
        /// <returns></returns>
        static Type GetType(string sTypeString)
        {
            string[] sArray = sTypeString.Split(';');
            string[] sArray2 = sArray[0].Split('/');

            if (sArray2[0] == "audio")
                return Type.AudioOnly;
            else
            {
                string sCodecs = GetCodecs(sTypeString);
                if(sCodecs.IndexOf(',') >= 0)
                    return Type.VideoAudio;
            }

            return Type.VideoOnly;
        }

        /// <summary>
        /// 获取真实的Url
        /// </summary>
        /// <param name="sUrl"></param>
        /// <param name="sStreamMapObj"></param>
        /// <returns></returns>
        static string GetRelUrl(string sUrl, string sStreamMapObj)
        {
            string sEncryptedSig = null;
            string sSignature    = GetStringPara(sStreamMapObj, "Sig");
            if(String.IsNullOrWhiteSpace(sSignature))
            {
                sEncryptedSig = GetStringPara(sStreamMapObj, "S");
            }
            sUrl = HttpUtility.UrlDecode(sUrl, Encoding.UTF8);
            sUrl += "&signature=" + sSignature;
            if (GetStringPara(sUrl, "Ratebypass") == null)
                sUrl += "&ratebypass=yes";

            return sUrl;
        }

        /// <summary>
        /// 获取下载链接信息
        /// </summary>
        /// <param name="sString"></param>
        /// <returns></returns>
        static List<DownloadUrlInfo> GetDownloadUrlInfo(string sString)
        {
            List<DownloadUrlInfo> aRet = new List<DownloadUrlInfo>();

            //获取iTag对应分辨率的哈希表
            Hashtable FmtListHash = GetFmtList(sString);
            if (FmtListHash.Count <= 0)
                return aRet;

            //解析链接
            string sStreamMap = GetStringPara(sString, "UrlEncodedFmtStreamMap") + ',' + GetStringPara(sString, "AdaptiveFmts");
            string[] sArray     = sStreamMap.Split(',');
            foreach (string sBuf in sArray)
            {
                string iTag  = GetStringPara(sBuf, "Itag");
                string sUrl  = GetStringPara(sBuf, "Url");
                if (String.IsNullOrWhiteSpace(iTag) || String.IsNullOrWhiteSpace(sUrl))
                    continue;

                DownloadUrlInfo aInfo = new DownloadUrlInfo();
                string sType = GetStringPara(sBuf, "Type");
                string sResolution = null;

                if (FmtListHash.ContainsKey(iTag))
                    sResolution = FmtListHash[iTag].ToString();
                else
                    sResolution = GetStringPara(sBuf, "Size");

                aInfo.Url             = GetRelUrl(sUrl, sBuf);
                aInfo.iTag            = Common.Convert.ConverStringToInt(iTag);
                aInfo.Quality         = GetStringPara(sBuf, "Quality");
                aInfo.Resolution      = sResolution;
                aInfo.Type            = GetType(sType);
                aInfo.Extension       = GetExtension(sType);
                aInfo.Codecs          = GetCodecs(sType);
                aInfo.Fps             = Common.Convert.ConverStringToInt(GetStringPara(sBuf, "Fps"));
                aInfo.BitRate         = Common.Convert.ConverStringToInt(GetStringPara(sBuf, "BitRate"));

                string sTmp           = aInfo.Url.Substring(aInfo.Url.IndexOf('?') + 1);
                aInfo.Size            = Common.Convert.ConverStringToInt(GetStringPara(sTmp, "Clen"));
                aRet.Add(aInfo);
            }
            return aRet;
        }

        #endregion




        /// <summary>
        /// 获取链接信息
        /// </summary>
        /// <param name="sUrl"></param>
        /// <returns></returns>
        public static VideoInfo Parse(string sUrl)
        {
            sUrl = m_DebugUrl;

            ErrCode eErrCode                   = ErrCode.Success;
            VideoInfo aInfo                    = new VideoInfo();
            string sID                         = null;
            string sTitle                      = null;
            string sPhotoUrl                   = null;
            string sDuration                   = null;
            List<DownloadUrlInfo> pDownloadUrl = null;

            //获取ID
            sID = GetVideoID(sUrl);
            if(String.IsNullOrWhiteSpace(sID))
            {
                eErrCode = ErrCode.UrlErr;
                goto RETURN;
            }

            //从网上下载信息
            string sString = NetHelper.DownloadString(m_PreGetInfo + sID, 1000);
            if (String.IsNullOrWhiteSpace(sString))
            {
                eErrCode = ErrCode.GetVideoInfoErr;
                goto RETURN;
            }

            //获取标题\图片Url\时长\下载链接
            sTitle       = GetStringPara(sString, "Title");
            sPhotoUrl    = GetStringPara(sString, "PhotoUrl");
            sDuration    = GetStringPara(sString, "Duration");
            pDownloadUrl = GetDownloadUrlInfo(sString);
            //GetNetWorkData(sID);

        RETURN:
            aInfo.ID           = sID;
            aInfo.Title        = sTitle;
            aInfo.ErrCode      = eErrCode;
            aInfo.Url          = sUrl;
            aInfo.Duration     = sDuration;
            aInfo.PhotoUrl     = sPhotoUrl;
            aInfo.DownloadUrls = pDownloadUrl;
            return aInfo;
        }
    }
}
