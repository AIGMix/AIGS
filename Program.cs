using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AIGS.Helper;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Threading;

namespace AIGS
{
    class Program:Window
    {
        public static void ThreadFuncDownlaod(object data)
        {
            Tidal.StreamUrl url = (Tidal.StreamUrl)data;
            DownloadFileHepler.Start(url.Url, "e:\\1.m4a");
        }

        static void Main(string[] args)
        {
            ThreadPoolManager TEST = new ThreadPoolManager(1);
            //string sErr;
            //Tidal.Tool.LogIn("masterhd1902@qq.com", "bitchjolin", out sErr);
            ////Tidal.Tool.GetPlaylist("e70292cf-0208-4de3-9dad-535b798b8f89", out sErr);
            //Tidal.Album album = Tidal.Tool.GetAlbum("79412401", out sErr);
            //Tidal.StreamUrl url = Tidal.Tool.GetStreamUrl(album.Tracks[0].ID, "LOW", out sErr);
            //ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadFuncDownlaod), url);
            while (true) ;
            //Tidal.Account Account = new Tidal.Account();
            //Account.LogIn("masterhd1902@qq.com", "bitchjolin");
            //Tidal.TidalTool.User = Account;
            //Tidal.Video v = Tidal.TidalTool.GetVideo("45219814");
            //Dictionary<string, string> pResolist = Tidal.TidalTool.GetVideoResolutionList("45219814");

            //List<string> pList = Tidal.TidalTool.GetVideoM3u8FileUrlList(pResolist.ElementAt(2).Value);
            //List<string> pFiles = new List<string>();
            //for (int i = 0; i < pList.Count(); i++)
            //{
            //    string sPath = "e:\\7\\test\\" + i + ".mp4";
            //    bool bCheck  = (bool)DownloadFileHepler.Start(pList[i], sPath, ContentType:null, UserAgent:null);
            //    if (bCheck == false)
            //        i = i;
            //    pFiles.Add(sPath);
            //}
            //FFmpegHelper.MergerByFiles(pFiles.ToArray(), "E:\\7\\TAGET.mp4");

            List<string> pFiles = new List<string>();
            for (int i = 0; i < 42; i++)
            {
                string sPath = "e:\\7\\test\\" + i + ".mp4";
                pFiles.Add(sPath);
            }
            FFmpegHelper.MergerByFiles(pFiles.ToArray(), "E:\\7\\TAGET.mp4");


            
            //Tidal.TidalTool.GetAlbum("100270475");
            //Tidal.TidalTool.GetAlbumTracks("100270475");
            //Tidal.TidalTool.GetStreamUrl("100270477","LOSSLESS");


            //Tool.Test();

            //Album aInfo = new Album();
            //PlayList aList = new PlayList();
            //TidalTool.GetSessionID(null,null);
            //TidalTool.GetPlayList("36ea71a8-445e-41a4-82ab-6628c581535d", ref aList);
            //TidalTool.GetPlayListTracks("36ea71a8-445e-41a4-82ab-6628c581535d", 100);
            //TidalTool.GetAlbumInfo(96146858, ref aInfo);
            //TidalTool.GetAlbumTracks(97697819);

            //AlbumInfo aAlbumInfo = new AlbumInfo();
            //AlbumDL.GetAlbumTracks(97697819);
            //AlbumDL.GetAlbumInfo(96146858, ref aAlbumInfo);
            //YTBTool.Parse(null);



            //DownloadFileHepler.CreateGetHttpResponse("http://127.0.0.1:9001/", 1000, "Lin=ff",null);
            //Helper.ConfigHelper.GetValue("TETS");
            //Helper.ScreenShotHelper aScreen = new Helper.ScreenShotHelper();
            //aScreen.StartCut();


        }
    }
}
