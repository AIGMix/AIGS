﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AIGS.Helper;
using System.IO;
using System.Web;
using System.Collections.Specialized;
using System.Collections;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace AIGS.Tool
{
    internal static class Decipherer
    {
        public static string DecipherWithOperations(string cipher, string operations)
        {
            return operations.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).Aggregate(cipher, ApplyOperation);
        }

        public static string DecipherWithVersion(string cipherVersion)
        {
            string jsUrl = string.Format("http://s.ytimg.com/yts/jsbin/player-{0}.js", cipherVersion);
            string js = NetHelper.DownloadString(jsUrl);
            if (String.IsNullOrWhiteSpace(js))
                return null;

            //Find "C" in this: var A = B.sig||C (B.s)
            string functNamePattern = @"(\w+)\s*=\s*function\(\s*(\w+)\s*\)\s*{\s*\2\s*=\s*\2\.split\(\""\""\)\s*;(.+)return\s*\2\.join\(\""\""\)\s*}\s*;";
            var funcName = Regex.Match(js, functNamePattern).Groups[1].Value;
            if (funcName.Contains("$"))
            {
                funcName = "\\" + funcName; //Due To Dollar Sign Introduction, Need To Escape
            }

            string funcPattern = @"(?!h\.)" + @funcName + @"=function\(\w+\)\{.*?\}"; //Escape funcName string
            var funcBody = Regex.Match(js, funcPattern, RegexOptions.Singleline).Value; //Entire sig function
            var lines = funcBody.Split(';'); //Each line in sig function

            string idReverse = "", idSlice = "", idCharSwap = ""; //Hold name for each cipher method
            string functionIdentifier = "";
            string operations = "";
            foreach (var line in lines.Skip(1).Take(lines.Length - 2)) //Matches the funcBody with each cipher method. Only runs till all three are defined.
            {
                if (!string.IsNullOrEmpty(idReverse) && !string.IsNullOrEmpty(idSlice) &&
                    !string.IsNullOrEmpty(idCharSwap))
                {
                    break; //Break loop if all three cipher methods are defined
                }

                functionIdentifier = GetFunctionFromLine(line);
                string reReverse = string.Format(@"{0}:\bfunction\b\(\w+\)", functionIdentifier); //Regex for reverse (one parameter)
                string reSlice = string.Format(@"{0}:\bfunction\b\([a],b\).(\breturn\b)?.?\w+\.", functionIdentifier); //Regex for slice (return or not)
                string reSwap = string.Format(@"{0}:\bfunction\b\(\w+\,\w\).\bvar\b.\bc=a\b", functionIdentifier); //Regex for the char swap.

                if (Regex.Match(js, reReverse).Success)
                {
                    idReverse = functionIdentifier; //If def matched the regex for reverse then the current function is a defined as the reverse
                }

                if (Regex.Match(js, reSlice).Success)
                {
                    idSlice = functionIdentifier; //If def matched the regex for slice then the current function is defined as the slice.
                }

                if (Regex.Match(js, reSwap).Success)
                {
                    idCharSwap = functionIdentifier; //If def matched the regex for charSwap then the current function is defined as swap.
                }
            }

            foreach (var line in lines.Skip(1).Take(lines.Length - 2))
            {
                Match m;
                functionIdentifier = GetFunctionFromLine(line);

                if ((m = Regex.Match(line, @"\(\w+,(?<index>\d+)\)")).Success && functionIdentifier == idCharSwap)
                {
                    operations += "w" + m.Groups["index"].Value + " "; //operation is a swap (w)
                }

                if ((m = Regex.Match(line, @"\(\w+,(?<index>\d+)\)")).Success && functionIdentifier == idSlice)
                {
                    operations += "s" + m.Groups["index"].Value + " "; //operation is a slice
                }

                if (functionIdentifier == idReverse) //No regex required for reverse (reverse method has no parameters)
                {
                    operations += "r "; //operation is a reverse
                }
            }

            return operations.Trim();
        }

        private static string ApplyOperation(string cipher, string op)
        {
            switch (op[0])
            {
                case 'r':
                    return new string(cipher.ToCharArray().Reverse().ToArray());

                case 'w':
                    {
                        int index = GetOpIndex(op);
                        return SwapFirstChar(cipher, index);
                    }

                case 's':
                    {
                        int index = GetOpIndex(op);
                        return cipher.Substring(index);
                    }

                default:
                    throw new NotImplementedException("Couldn't find cipher operation.");
            }
        }

        private static string GetFunctionFromLine(string currentLine)
        {
            Regex matchFunctionReg = new Regex(@"\w+\.(?<functionID>\w+)\("); //lc.ac(b,c) want the ac part.
            Match rgMatch = matchFunctionReg.Match(currentLine);
            string matchedFunction = rgMatch.Groups["functionID"].Value;
            return matchedFunction; //return 'ac'
        }

        private static int GetOpIndex(string op)
        {
            string parsed = new Regex(@".(\d+)").Match(op).Result("$1");
            int index = Int32.Parse(parsed);

            return index;
        }

        private static string SwapFirstChar(string cipher, int index)
        {
            var builder = new StringBuilder(cipher);
            builder[0] = cipher[index];
            builder[index] = cipher[0];

            return builder.ToString();
        }
    }



    public class YTBTool
    {
        #region 宏定义

        static string m_DebugUrl = "https://www.youtube.com/watch?v=jKyDCrDo580";
        static string m_PreGetInfo = "http://www.youtube.com/get_video_info?video_id=";
        static string[] m_PreVideo = { "https://www.youtube.com/watch?v=", "http://www.youtube.com/watch?v=" };
        
        static string H_FLAG_TITLE           = "title";
        static string H_FLAG_PHOTOURL        = "thumbnail_url";
        static string H_FLAG_DURATION        = "length_seconds";
        static string H_FLAG_FMTLIST         = "fmt_list";

        static string H_FLAG_STREAM_MAP      = "url_encoded_fmt_stream_map";
        static string H_FLAG_ADAPTIVE_FMT    = "adaptive_fmts";

        static string H_FLAG_DOWNLOAD_URL    = "url";
        static string H_FLAG_SIGNATURATE_S   = "s";
        static string H_FLAG_SIGNATURATE_SIG = "sig";
        static string H_FLAG_ITAG            = "itag";
        static string H_FLAG_TYPE            = "type";
        static string H_FLAG_QUALITY         = "quality";
        static string H_FLAG_QUALITY2        = "quality_label";

        static string H_FLAG_FILESIZE        = "clen";
        static string H_FLAG_BITERATE        = "bitrate";
        static string H_FLAG_FPS             = "fps";
        static string H_FLAG_RESOLUTION      = "size";
        static string H_FLAG_RATE_BY_PASEE   = "ratebypass";

        #endregion

        #region 枚举与结构体

        public enum ErrCode
        {
            Success,
            UrlErr,             
            GetVideoInfoErr,    
            GetVideJasonErr,
            GetOperationErr,
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

        /// <summary>
        /// 获取标题
        /// </summary>
        /// <param name="sString"></param>
        /// <returns></returns>
        static string GetStringPara(string sString, string sKey)
        {
            string sRet;
            string sFindKey = sKey + '=';
            string[] sArray = sString.Split('&');
            for (int j = 0; j < sArray.Count(); j++)
            {
                if (0 == sArray[j].IndexOf(sFindKey))
                {
                    sRet = sArray[j].Substring(sFindKey.Length);
                    sRet = HttpUtility.UrlDecode(sRet, Encoding.UTF8);
                    return sRet;
                }
            }

            return null;
        }

        /// <summary>
        /// 获取iTag对应分辨率的哈希表
        /// </summary>
        /// <param name="sString"></param>
        /// <returns></returns>
        static Hashtable GetFmtList(string sFmtString)
        {
            //18/640x240,36/320x120,17/176x144
            Hashtable FmtListHash = new Hashtable();
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
        static string GetRelUrl(string sStreamMapObj, string sOperationString)
        {
            string sEncryptedSig = null;
            string sSignature    = GetStringPara(sStreamMapObj, H_FLAG_SIGNATURATE_SIG);
            if(String.IsNullOrWhiteSpace(sSignature))
            {
                sEncryptedSig = GetStringPara(sStreamMapObj, H_FLAG_SIGNATURATE_S);
                sSignature = Decipherer.DecipherWithOperations(sEncryptedSig, sOperationString);
            }

            string sUrl = GetStringPara(sStreamMapObj, H_FLAG_DOWNLOAD_URL);

            sUrl = HttpUtility.UrlDecode(sUrl, Encoding.UTF8);
            sUrl += "&signature=" + sSignature;
            if (GetStringPara(sUrl, H_FLAG_RATE_BY_PASEE) == null)
                sUrl += "&ratebypass=yes";

            return sUrl;
        }

        /// <summary>
        /// 获取下载链接信息
        /// </summary>
        /// <param name="sString"></param>
        /// <returns></returns>
        static List<DownloadUrlInfo> GetDownloadUrlInfo(string sStreamMap, string sFmtString, string sOperationString)
        {
            List<DownloadUrlInfo> aRet = new List<DownloadUrlInfo>();
            Hashtable FmtListHash      = GetFmtList(sFmtString);

            //解析链接
            string[] sArray = sStreamMap.Split(',');
            foreach (string sBuf in sArray)
            {
                string iTag  = GetStringPara(sBuf, H_FLAG_ITAG);
                if (String.IsNullOrWhiteSpace(iTag))
                    continue;

                //获取类型
                string sType = GetStringPara(sBuf, H_FLAG_TYPE);

                //获取分辨率
                string sResolution = null;
                if (FmtListHash.ContainsKey(iTag))
                    sResolution = FmtListHash[iTag].ToString();
                else
                    sResolution = GetStringPara(sBuf, H_FLAG_RESOLUTION);

                //获取质量
                string sQuality = GetStringPara(sBuf, H_FLAG_QUALITY);
                if (sQuality == null)
                    sQuality = GetStringPara(sBuf, H_FLAG_QUALITY2);

                //获取链接
                string sUrl = GetRelUrl(sBuf, sOperationString);

                //赋值
                DownloadUrlInfo aInfo = new DownloadUrlInfo();
                aInfo.Url             = sUrl;
                aInfo.iTag            = Common.Convert.ConverStringToInt(iTag);
                aInfo.Quality         = sQuality;
                aInfo.Resolution      = sResolution;
                aInfo.Type            = GetType(sType);
                aInfo.Extension       = GetExtension(sType);
                aInfo.Codecs          = GetCodecs(sType);
                aInfo.Fps             = Common.Convert.ConverStringToInt(GetStringPara(sBuf, H_FLAG_FPS));
                aInfo.BitRate         = Common.Convert.ConverStringToInt(GetStringPara(sBuf, H_FLAG_BITERATE));

                string sTmp           = aInfo.Url.Substring(aInfo.Url.IndexOf('?') + 1);
                aInfo.Size            = Common.Convert.ConverStringToInt(GetStringPara(sTmp, H_FLAG_FILESIZE));
                aRet.Add(aInfo);
            }
            return aRet;
        }

        /// <summary>
        /// 获取Jason数据
        /// </summary>
        /// <param name="sUrl"></param>
        /// <returns></returns>
        private static JObject GetVideoJson(string sUrl)
        {
            string sPageSource = NetHelper.DownloadString(sUrl, 5000);
            if (sPageSource == null)
                return null;

            if (sPageSource.Contains("<div id=\"watch-player-unavailable\">"))
                return null;

            var sRegex = new Regex(@"ytplayer\.config\s*=\s*(\{.+?\});", RegexOptions.Multiline);
            string sJson = sRegex.Match(sPageSource).Result("$1");
            return JObject.Parse(sJson);
        }

        /// <summary>
        /// 获取PlayerVersion（用于解析加密Signature）
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        private static string GetPlayerVersion(JObject json)
        {
            if (json == null)
                return null;

            var sRegex = new Regex(@"player-(.+?).js");
            string sJson = json["assets"]["js"].ToString();
            return sRegex.Match(sJson).Result("$1");
        }

        #endregion

        /// <summary>
        /// 获取链接信息
        /// </summary>
        /// <param name="sUrl"></param>
        /// <returns></returns>
        public static VideoInfo Parse(string sUrl)
        {
            ErrCode eErrCode                   = ErrCode.Success;
            VideoInfo aInfo                    = new VideoInfo();
            string sID                         = null;
            string sTitle                      = null;
            string sPhotoUrl                   = null;
            string sDuration                   = null;
            string sPlayerVersion              = null;
            string sFmtString                  = null;
            string sStreamMapString            = null;
            string sOperationString            = null;
            List<DownloadUrlInfo> pDownloadUrl = null;

            //获取ID
            sID = GetVideoID(sUrl);
            if(String.IsNullOrWhiteSpace(sID))
            {
                eErrCode = ErrCode.UrlErr;
                goto RETURN;
            }

            //获取页面Jason数据
            JObject aJason = GetVideoJson(sUrl);
            if(aJason == null)
            {
                eErrCode = ErrCode.GetVideJasonErr;
                goto RETURN;
            }

            //获取数据
            sPlayerVersion   = GetPlayerVersion(aJason);
            sTitle           = aJason["args"][H_FLAG_TITLE].ToString();
            sPhotoUrl        = aJason["args"][H_FLAG_PHOTOURL].ToString();
            sDuration        = aJason["args"][H_FLAG_DURATION].ToString();
            sFmtString       = aJason["args"][H_FLAG_FMTLIST].ToString();
            sStreamMapString = aJason["args"][H_FLAG_STREAM_MAP].ToString() + ',' + aJason["args"][H_FLAG_ADAPTIVE_FMT].ToString();

            //通过sPlayerVersion获取解析串
            sOperationString = Decipherer.DecipherWithVersion(sPlayerVersion);
            if(sOperationString == null)
            {
                eErrCode = ErrCode.GetOperationErr;
                goto RETURN;
            }

            //获取下载信息
            pDownloadUrl     = GetDownloadUrlInfo(sStreamMapString, sFmtString, sOperationString);

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


