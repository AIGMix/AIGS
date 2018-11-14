using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Web;
using System.Data;
using System.Configuration;
using System.Collections;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using AIGS.Helper;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace AIGS.Tools
{

    public class ArtistInfo
    {
        public int ID { get; set; }                      //艺人ID
        public string Name { get; set; }                 //艺人名称
        public string Url { get; set; }                  //连接
        public string Picture { get; set; }              //图片
        public string PictureUrl { get; set; }           //图片链接
        public int Popularity { get; set; }              //热度
    }

    public class AlbumInfo
    {
        public int ID { get; set; }                      //专辑ID
        public string Title { get; set; }                //专辑名称
        public string ReleaseDate { get; set; }          //专辑发行日期
        public string CopyRight { get; set; }            //专辑版权所属
        public int Duration { get; set; }                //专辑时长
        public string Cover { get; set; }                //专辑封面编码
        public string ConvrUrl { get; set; }             //专辑封面链接
        public int NumberOfTracks { get; set; }          //专辑曲目数量
        public int NumberOfVideos { get; set; }          //专辑视频数量
        public int NumberOfVolumes { get; set; }         //专辑碟数量
        public string Name { get; set; }                 //歌手名称
    };

    public class TrackInfo
    {
        public int ID { get; set; }                      //曲目ID
        public string Title { get; set; }                //曲目名称
        public int Duration { get; set; }                //曲目时长(单位秒)
        public int TrackNumber { get; set; }             //曲目序号
        public int VolumeNumber { get; set; }            //曲目所属碟
        public string Version { get; set; }              //曲目版本
        public string Copyright { get; set; }            //曲目版权
        public string AlbumTitle { get; set; }           //专辑名称
    };

    public class VideoInfo
    {
        public int ID { get; set; }                      //MVID
        public string Title { get; set; }                //MV标题
        public int Duration { get; set; }                //MV时长
        public int TrackNumber { get; set; }             //MV序号
        public string ReleaseDate { get; set; }          //发行日期
        public string ImageId { get; set; }              //图片
        public string ImageUrl { get; set; }             //图片链接
    };

    public class PlayListInfo
    {
        public string Uuid { get; set; }                 //ID
        public string Title { get; set; }                //标题
        public int NumberOfTracks { get; set; }          //曲目数量
        public int NumberOfVideos { get; set; }          //视频数量
        public string Description { get; set; }          //描述
        public int Duration { get; set; }                //时长
        public string LastUpdated { get; set; }          //最后更新的时间
        public string Created { get; set; }              //创建时间
        public string Type { get; set; }                 //类型
        public string Url { get; set; }                  //链接
        public bool PublicPlaylist { get; set; }         //公共
        public string Image { get; set; }                //图片
        public string ImageUrl { get; set; }             //图片链接
    }




    public class AlbumDL
    {
        public enum ErrCode
        {
            SUCCESS,
            GETALBUMERR,
            GETTRACKERR,
            GETSTREAMERR,
        }

        public enum SoundQuality
        {
            LOW,                            //低品质
            HIGH,                           //高品质
            LOSSLESS,                       //无损
        }

        public enum CountryCode
        {
            US,                             //美
            GB,                             //英
            NZ,                             //新
            HK,                             //港
            TW,                             //台
            AU,                             //澳
            FR,                             //法
            DE,                             //德
        }

        #region 下载

        private static bool RemoteCertificateValidate(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors error)
        {
            //为了通过证书验证，总是返回true
            return true;
        }

        public static string DownloadString(string url)
        {
          
            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                //System.Console.WriteLine("https connections.....");
                //ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                ServicePointManager.ServerCertificateValidationCallback += RemoteCertificateValidate;
                // 这里设置了协议类型。
                //ServicePointManager.SecurityProtocol =SecurityProtocolType.Tls12;// (SecurityProtocolType)3072;// SecurityProtocolType.Tls1.2; 
                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12; //(SecurityProtocolType)768 | (SecurityProtocolType)3072
                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                //ServicePointManager.CheckCertificateRevocationList = true;
                //ServicePointManager.DefaultConnectionLimit = 100;
                //ServicePointManager.Expect100Continue = false;
            }
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            //增加下面两个属性即可  
            request.KeepAlive = false;
            request.ProtocolVersion = HttpVersion.Version10;
           
            request.Method = "GET";    //使用get方式发送数据
            //request.ContentType = "application/x-www-form-urlencoded";
            request.Referer = "";
            request.AllowAutoRedirect = true;
            //request.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.2; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";
            request.Accept = "*/*";
            string result = string.Empty;
            try
            {
                //获取网页响应结果
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                //Thread.Sleep(2000);
                Stream stream = response.GetResponseStream();
                //client.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                using (StreamReader sr = new StreamReader(stream))
                {
                    result = sr.ReadToEnd();
                }

                if (request != null)
                {
                    request.Abort();
                }

                if (response != null)
                {
                    response.Close();
                }

            }
            catch (System.Net.WebException e)
            {
                return result;
            }
            return result;
        }

        private static string ReadStreamFromResponse(WebResponse response)
        {
            using (Stream responseStream = response.GetResponseStream())
            {
                using (var sr = new StreamReader(responseStream))
                {
                    return sr.ReadToEnd();
                }
            }
        }
        //public static string DownloadString(string url)
        //{
        //    ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
        //    var request = WebRequest.Create(url);
        //    request.Method = "GET";
        //    System.Threading.Tasks.Task<WebResponse> task = System.Threading.Tasks.Task.Factory.FromAsync(
        //        request.BeginGetResponse,
        //        asyncResult => request.EndGetResponse(asyncResult),
        //        null);

        //    return task.ContinueWith(t => ReadStreamFromResponse(t.Result)).Result;
        //}
        #endregion

        #region 获取数据
        enum EVENT
        {
            Search,                             //查找

            GetAlbumInfo,                       //专辑基本信息
            GetAlbumTracks,                     //专辑歌曲
            GetPlayList,                        //播放列表
            GetPlayListTracks,                  //播放列表歌曲
            GetTrack,                           //歌曲详情
            GetStreamUrl,                       //歌曲链接

            GetArtistInfo,                      //艺人作品
            GetVideo,                           //艺人MV

            GetArtistCover,                     //艺人封面
            GetAlbumCover,                      //专辑封面
            GetPlayListCover,                   //播放列表封面
            GetVideoCover,                      //MV封面
        }
        enum SearchType
        {
            ARTISTS,                        //查找艺人
            ALBUMS,                         //查找专辑
            TRACKS,                         //查找曲目
            VIDEOS,                         //查找MV
            PLAYLISTS,                      //查找播放列表
        }

        /// <summary>
        /// 功能：装载UrlAlbumBase\SongInfo\SongStream
        /// 注意：1、UrlAlbumBase获取专辑的基本信息
        ///      2、SongInfo可获取曲目信息,包括title(名称)、duration(时常\秒)、artist(歌手)
        ///      3、SongStream可获取曲目下载链接,参数url
        /// </summary>
        static string GetUrl(EVENT eEvent, string sIDOrName, int iLimitNum = 100, string sSessionID = null, int eType = 0, CountryCode eCountryCode = CountryCode.US)
        {
            //??limit  countryCode 
            string sQus = "";
            string sCode = "";
            string sConver = "";
            switch (eEvent)
            {
                case EVENT.GetAlbumInfo:
                    return "https://webapi.tidal.com/v1/share/albums/" + sIDOrName + "?token=hZ9wuySZCmpLLiui";
                case EVENT.GetAlbumTracks:
                    return "https://webapi.tidal.com/v1/share/albums/" + sIDOrName + "/tracks?token=hZ9wuySZCmpLLiui";
                case EVENT.GetPlayList:
                    return "https://webapi.tidal.com/v1/share/playlists/" + sIDOrName + "?token=hZ9wuySZCmpLLiui";
                case EVENT.GetPlayListTracks:
                    return "https://webapi.tidal.com/v1/share/playlists/" + sIDOrName + "/items?token=hZ9wuySZCmpLLiui&limit=" + iLimitNum;
                case EVENT.GetTrack:
                    return "https://webapi.tidal.com/v1/share/tracks/" + sIDOrName + "?token=hZ9wuySZCmpLLiui";
                case EVENT.GetStreamUrl:
                    sQus = AIGS.Common.Convert.ConverEnumToString(eType, typeof(SoundQuality), (int)SoundQuality.LOSSLESS);
                    sCode = AIGS.Common.Convert.ConverEnumToString((int)eCountryCode, typeof(SoundQuality), (int)CountryCode.US);
                    return "https://api.tidal.com/v1/tracks/" + sIDOrName + "/streamurl?sessionId=" + sSessionID + "&soundQuality=" + sQus + "&countryCode=" + sCode;
                case EVENT.GetArtistInfo:
                    return "https://webapi.tidal.com/v1/share/artists/" + sIDOrName + "/albums?token=hZ9wuySZCmpLLiui&filter=ALL&countryCode=" + sCode + "&limit=" + iLimitNum;
                case EVENT.GetVideo:
                    return "http://api.tidalhifi.com/v1/artists/" + sIDOrName + "/videos?countryCode=" + sCode + "&token=hZ9wuySZCmpLLiui";
                case EVENT.Search:
                    sQus = AIGS.Common.Convert.ConverEnumToString(eType, typeof(SearchType));
                    sCode = AIGS.Common.Convert.ConverEnumToString((int)eCountryCode, typeof(CountryCode));
                    return "https://listen.tidal.com/v1/search?query=" + sIDOrName + "&limit=" + iLimitNum + "&offset=0&types=" + sQus + "&countryCode=" + sCode + "&sessionid=" + sSessionID;
                case EVENT.GetArtistCover:
                case EVENT.GetPlayListCover:
                case EVENT.GetVideoCover:
                    sConver = sIDOrName.Replace('-','/');
                    return "http://resources.wimpmusic.com/images/" + sConver + "/1080x720.jpg";
                case EVENT.GetAlbumCover:
                    sConver = sIDOrName.Replace('-','/');
                    return "http://resources.tidal.com/images/" + sConver + "/1280x1280.jpg";
            }
            return null;
        }

        /// <summary>
        /// 获取网络Jason数据
        /// </summary>
        /// <param name="eType"></param>
        /// <param name="iID"></param>
        /// <param name="sSessionID"></param>
        /// <param name="eQuality"></param>
        /// <returns></returns>
        static string GetNetInfo(EVENT eEvent, string sIDOrName, int iLimitNum = 100, string sSessionID = null, int eType = 0, CountryCode eCountryCode = CountryCode.US)
        {
            string sNmae = HttpUtility.UrlEncode(sIDOrName);
            string sUrl = GetUrl(eEvent, sNmae, iLimitNum, sSessionID, eType, eCountryCode);
            //return DownloadString(sUrl);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            return NetHelper.DownloadString(sUrl, 5000);
        }

        /// <summary>
        /// 查找
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sKeyName"></param>
        /// <param name="SessionID"></param>
        /// <param name="eType"></param>
        /// <param name="eCode"></param>
        /// <returns></returns>
        static List<T> GetSearchInfo<T>(string sKeyName, int iLimitNum, string SessionID, SearchType eType, CountryCode eCode)
        {
            string sJsonText = GetNetInfo(EVENT.Search, sKeyName, iLimitNum, SessionID, (int)eType, eCode);
            if (String.IsNullOrWhiteSpace(sJsonText))
                return null;

            string sFindName = AIGS.Common.Convert.ConverEnumToString((int)eType, typeof(SearchType));
            List<T> pRet = JsonHelper.ConverStringToObject<List<T>>(sJsonText, sFindName.ToLower(), "items");
            return pRet;
        }

        #endregion



        #region 对外接口

        /// <summary>
        /// 获取专辑基本信息
        /// </summary>
        /// <param name="iAlbumID"></param>
        /// <param name="Info"></param>
        /// <returns></returns>
        public static int GetAlbumInfo(int iAlbumID, ref AlbumInfo Info)
        {
            string sJsonText = GetNetInfo(EVENT.GetAlbumInfo, iAlbumID.ToString());
            if (String.IsNullOrWhiteSpace(sJsonText))
                return -1;

            Info        = JsonHelper.ConverStringToObject<AlbumInfo>(sJsonText);
            Info.Name   = JsonHelper.GetValue(sJsonText, "artist", "name");

            if (!String.IsNullOrWhiteSpace(Info.Cover))
                Info.ConvrUrl = GetUrl(EVENT.GetAlbumCover, Info.Cover);
            return 0;
        }

        /// <summary>
        /// 获取播放列表
        /// </summary>
        /// <param name="sUuid"></param>
        /// <param name="Info"></param>
        /// <returns></returns>
        public static int GetPlayList(string sUuid, ref PlayListInfo Info)
        {
            string sJsonText = GetNetInfo(EVENT.GetPlayList, sUuid);
            if (String.IsNullOrWhiteSpace(sJsonText))
                return -1;

            Info        = JsonHelper.ConverStringToObject<PlayListInfo>(sJsonText);
            if (!String.IsNullOrWhiteSpace(Info.Image))
                Info.ImageUrl = GetUrl(EVENT.GetPlayListCover, Info.Image);
            return 0;
        }

        /// <summary>
        /// 获取全部歌曲
        /// </summary>
        /// <param name="iAlbumID"></param>
        /// <returns></returns>
        public static List<TrackInfo> GetAlbumTracks(int iAlbumID)
        {
            string sJsonText = GetNetInfo(EVENT.GetAlbumTracks, iAlbumID.ToString());
            if (String.IsNullOrWhiteSpace(sJsonText))
                return null;

            //获取集合
            List<TrackInfo> pRet = JsonHelper.ConverStringToObject<List<TrackInfo>>(sJsonText, "items");

            //检查是否有重名现象，如果有的话，则以Titlt + "(Version)"命名文件
            Dictionary<string, bool> pNameHash = new Dictionary<string, bool>();
            for (int j = 0; j < pRet.Count; j++)
            {
                if (pNameHash.ContainsKey(pRet[j].Title))
                    pNameHash[pRet[j].Title] = true;
                else
                    pNameHash.Add(pRet[j].Title, false);
            }
            for(int i = 0; i < pRet.Count; i++)
            {
                TrackInfo Info = pRet[i];
                if (pNameHash[Info.Title])
                    Info.Title += " (" + Info.Version + ")";

                pRet[i] = Info;
            }

            //排序
            int iIsEnd = 0;
            int iVolumeNumber = 0;
            List<TrackInfo> pTmp = new List<TrackInfo>();
            while(true)
            {
                iIsEnd = 1;
                iVolumeNumber++;
                for (int i = 0; i < pRet.Count; i++)
                {
                    if (pRet[i].VolumeNumber != iVolumeNumber)
                        continue;

                    int iIndex = 0;
                    for (int j = 0; j < pTmp.Count; j++, iIndex++)
                    {
                        if (pTmp[j].VolumeNumber < iVolumeNumber)
                            continue;

                        if (pTmp[j].TrackNumber > pRet[i].TrackNumber)
                            break;
                    }

                    pTmp.Insert(iIndex, pRet[i]);
                    iIsEnd = 0;
                }

                if (iIsEnd == 1)
                    break;
            }

            return pTmp;
        }

        /// <summary>
        /// 获取播放列表的曲目
        /// </summary>
        /// <param name="sUuid"></param>
        /// <returns></returns>
        public static List<TrackInfo> GetPlayListTracks(string sUuid, int iLimitNum)
        {
            string sJsonText = GetNetInfo(EVENT.GetPlayListTracks, sUuid, iLimitNum);
            if (String.IsNullOrWhiteSpace(sJsonText))
                return null;

            //获取集合
            List<TrackInfo> pRet = new List<TrackInfo>();
            List<object> pArrary = JsonHelper.ConverStringToObject<List<object>>(sJsonText, "items");
            if (pArrary == null || pArrary.Count == 0)
                return pRet;

            //检查是否有重名现象，如果有的话，则以Titlt + "(Version)"命名文件
            Dictionary<string, bool> pNameHash = new Dictionary<string, bool>();
            for (int j = 0; j < pArrary.Count; j++)
            {
                TrackInfo aInfo = JsonHelper.ConverStringToObject<TrackInfo>(pArrary[j].ToString(), "item");
                if (pNameHash.ContainsKey(aInfo.Title))
                    aInfo.Title += " (" + aInfo.Version + ")";
                else
                    pNameHash.Add(aInfo.Title, false);

                pRet.Add(aInfo);
            }

            return pRet;
        }


        /// <summary>
        /// 获取歌曲详细信息
        /// </summary>
        /// <param name="iSongID"></param>
        /// <param name="iAlbumID"></param>
        /// <param name="Info"></param>
        /// <returns></returns>
        public static int GetTrackInfo(int iSongID, ref TrackInfo Info)
        {
            string sJsonText = GetNetInfo(EVENT.GetTrack, iSongID.ToString());
            if (String.IsNullOrWhiteSpace(sJsonText))
                return -1;

            Info = JsonHelper.ConverStringToObject<TrackInfo>(sJsonText);
            return 0;
        }

        /// <summary>
        /// 获取下载链接
        /// </summary>
        /// <param name="iSongID"></param>
        /// <param name="sSessionID"></param>
        /// <param name="eQuality"></param>
        /// <param name="Info"></param>
        /// <returns></returns>
        public static string GetStreamUrl(int iSongID, string sSessionID, SoundQuality eQuality, CountryCode eCode)
        {
            string sJsonText = GetNetInfo(EVENT.GetStreamUrl, iSongID.ToString(), -1, sSessionID, (int)eQuality);
            if (String.IsNullOrWhiteSpace(sJsonText))
                return null;

            return JsonHelper.GetValue(sJsonText, "url"); 
        }


        /// <summary>
        /// 将专辑信息转为打印字符串
        /// </summary>
        /// <param name="Info"></param>
        /// <returns></returns>
        public static string ConvertAlbumInfoToString(AlbumInfo Info, List<TrackInfo> aTrackList)
        {
            string AlbumString = "";

            AlbumString += "[ID]          " + Info.ID + "\r\n";
            AlbumString += "[Title]       " + Info.Title + "\r\n";
            AlbumString += "[Artists]     " + Info.Name + "\r\n";
            AlbumString += "[CopyRight]   " + Info.CopyRight + "\r\n";
            AlbumString += "[ReleaseDate] " + Info.ReleaseDate + "\r\n";
            AlbumString += "[SongNum]     " + Info.NumberOfTracks + "\r\n";
            AlbumString += "[Duration]    " + Info.Duration + "\r\n";
            AlbumString += '\n';

            if (aTrackList.Count != Info.NumberOfTracks)
                return AlbumString;

            AlbumString += '\n';
            for (int i = 0; i < Info.NumberOfVolumes; i++)
            {
                AlbumString += "===========Volume ".ToString() + (i + 1).ToString() + "=============\r\n";
                for (int j = 0; j < Info.NumberOfTracks; j++)
                {
                    if (aTrackList[j].VolumeNumber != i + 1)
                        continue;

                    AlbumString += ("[" + aTrackList[j].TrackNumber + "]").PadRight(8) + aTrackList[j].Title + "\r\n";
                }
                AlbumString += '\n';
                AlbumString += '\n';
            }
            return AlbumString;
        }

        /// <summary>
        /// 播放列表信息转为字符串
        /// </summary>
        /// <param name="Info"></param>
        /// <param name="aTrackList"></param>
        /// <returns></returns>
        public static string ConvertPlayListInfoToString(PlayListInfo Info, List<TrackInfo> aTrackList)
        {
            string pRetString = "";

            pRetString += "[uuid]              " + Info.Uuid + "\r\n";
            pRetString += "[Title]             " + Info.Title + "\r\n";
            pRetString += "[Type]              " + Info.Type + "\r\n";
            pRetString += "[SongNum]           " + Info.NumberOfTracks + "\r\n";
            pRetString += "[VideoNum]          " + Info.NumberOfVideos + "\r\n";
            pRetString += "[Duration]          " + Info.Duration + "\r\n";
            pRetString += "[PublicPlaylist]    " + Info.PublicPlaylist + "\r\n";
            pRetString += "[Url]               " + Info.Url + "\r\n";
            pRetString += "[Created]           " + Info.Created + "\r\n";
            pRetString += "[LastUpdated]       " + Info.LastUpdated + "\r\n";
            pRetString += "[Description]       " + Info.Description + "\r\n";
            pRetString += '\n';
            for (int i = 0; i < Info.NumberOfTracks && aTrackList.Count == Info.NumberOfTracks; i++)
            {
                pRetString += ("[" + aTrackList[i].TrackNumber + "]").PadRight(8) + aTrackList[i].Title + "\r\n";
            }
            return pRetString;
        }


        #endregion


        #region 查找
        /// <summary>
        /// 查找艺人
        /// </summary>
        /// <param name="sKeyName"></param>
        /// <param name="SessionID"></param>
        /// <param name="eCode"></param>
        /// <returns></returns>
        public static List<ArtistInfo> SearchArtist(string sKeyName, int iLimitNum, string SessionID, CountryCode eCode)
        {
            List<ArtistInfo> pRet = GetSearchInfo<ArtistInfo>(sKeyName, iLimitNum, SessionID, SearchType.ARTISTS, eCode);
            return pRet;
        }

        /// <summary>
        /// 查找专辑
        /// </summary>
        /// <param name="sKeyName"></param>
        /// <param name="SessionID"></param>
        /// <param name="eCode"></param>
        /// <returns></returns>
        public static List<AlbumInfo> SearchAlbum(string sKeyName, int iLimitNum, string SessionID, CountryCode eCode)
        {
            List<AlbumInfo> pRet = GetSearchInfo<AlbumInfo>(sKeyName, iLimitNum, SessionID, SearchType.ALBUMS, eCode);
            for (int i = 0; pRet != null && i < pRet.Count; i++ )
            {
                AlbumInfo Info = pRet[i];
                if (!String.IsNullOrWhiteSpace(Info.Cover))
                {
                    Info.ConvrUrl = GetUrl(EVENT.GetAlbumCover, Info.Cover);
                    pRet[i] = Info;
                }
            }
            return pRet;
        }

        /// <summary>
        /// 查找曲目
        /// </summary>
        /// <param name="sKeyName"></param>
        /// <param name="SessionID"></param>
        /// <param name="eCode"></param>
        /// <returns></returns>
        public static List<TrackInfo> SearchTrack(string sKeyName, int iLimitNum, string SessionID, CountryCode eCode)
        {
            List<TrackInfo> pRet = GetSearchInfo<TrackInfo>(sKeyName, iLimitNum, SessionID, SearchType.TRACKS, eCode);
            return pRet;
        }

        /// <summary>
        /// 查找MV
        /// </summary>
        /// <param name="sKeyName"></param>
        /// <param name="SessionID"></param>
        /// <param name="eCode"></param>
        /// <returns></returns>
        public static List<VideoInfo> SearchVideo(string sKeyName, int iLimitNum, string SessionID, CountryCode eCode)
        {
            List<VideoInfo> pRet = GetSearchInfo<VideoInfo>(sKeyName, iLimitNum, SessionID, SearchType.VIDEOS, eCode);
            for (int i = 0; pRet != null && i < pRet.Count; i++)
            {
                VideoInfo Info = pRet[i];
                if (!String.IsNullOrWhiteSpace(Info.ImageId))
                {
                    Info.ImageUrl = GetUrl(EVENT.GetVideoCover, Info.ImageId);
                    pRet[i] = Info;
                }
            }
            return pRet;
        }

        /// <summary>
        /// 查找播放列表
        /// </summary>
        /// <param name="sKeyName"></param>
        /// <param name="SessionID"></param>
        /// <param name="eCode"></param>
        /// <returns></returns>
        public static List<PlayListInfo> SearchPlayList(string sKeyName, int iLimitNum, string SessionID, CountryCode eCode)
        {
            List<PlayListInfo> pRet = GetSearchInfo<PlayListInfo>(sKeyName, iLimitNum, SessionID, SearchType.PLAYLISTS, eCode);
            for (int i = 0; pRet != null && i < pRet.Count; i++)
            {
                PlayListInfo Info = pRet[i];
                if (!String.IsNullOrWhiteSpace(Info.Image))
                {
                    Info.ImageUrl = GetUrl(EVENT.GetPlayListCover, Info.Image);
                    pRet[i] = Info;
                }
            }
            return pRet;
        }

        

        #endregion

        #region 导入下载列表文件解析
        static string GetGroupName(string sMsg)
        {
            if (String.IsNullOrWhiteSpace(sMsg))
                return null;

            int iStart = sMsg.IndexOf('[');
            int iEnd = sMsg.IndexOf(']');
            if (iStart < 0 || iEnd < 0 || iEnd <= iStart)
                return null;

            string sRet = sMsg.Substring(iStart + 1, iEnd - iStart - 1);
            return sRet;
        }


        public static List<string> GetDownloadListFormFile(string sFilePath, string sFindGroup)
        {
            string sNowGroup = null;
            List<string> pRet = new List<string>();
            if (String.IsNullOrWhiteSpace(sFindGroup))
                return pRet;
            sFindGroup = sFindGroup.ToLower();

            string[] aLines = File.ReadAllLines(sFilePath);
            foreach(string sMsg in aLines)
            {
                string sName = GetGroupName(sMsg);
                if (!String.IsNullOrWhiteSpace(sName))
                {
                    sNowGroup = sName.ToLower();
                    continue;
                }

                if (sNowGroup != null && sFindGroup == sNowGroup && !String.IsNullOrWhiteSpace(sMsg))
                    pRet.Add(sMsg);
            }

            return pRet;
        }

        #endregion


        #region 动态参数
        public ErrCode aErrCode;
        public bool IsGetAlbumInfoOver;
        public AlbumInfo aAlbumInfo;
        public List<TrackInfo> aTrackInfos;
        void ThreadGetAlbumInfo(object sID)
        {
            aErrCode = ErrCode.SUCCESS;
            aTrackInfos = null;
            IsGetAlbumInfoOver = false;
            aAlbumInfo = new AlbumInfo();
            int iCheck = GetAlbumInfo(Common.Convert.ConverStringToInt(sID.ToString()), ref aAlbumInfo);
            if(iCheck != 0)
            {
                aErrCode = ErrCode.GETALBUMERR;
                IsGetAlbumInfoOver = true;
                return;
            }

            aTrackInfos = GetAlbumTracks(Common.Convert.ConverStringToInt(sID.ToString()));
            if (aTrackInfos != null)
            {
                for (int i = 0; i < aTrackInfos.Count; i++)
                {
                    TrackInfo aInfo = aTrackInfos[i];
                    aInfo.AlbumTitle = aAlbumInfo.Title;
                    aTrackInfos[i] = aInfo;
                }
            }
            else
                aErrCode = ErrCode.GETTRACKERR;

            IsGetAlbumInfoOver = true;
        }

        public Thread StartGetAlbumInfoThread(string sUrl)
        {
            Thread aThreadHandle = ThreadHelper.Start(ThreadGetAlbumInfo, sUrl);
            return aThreadHandle;
        }

        #endregion


    }

}
