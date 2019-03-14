using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AIGS.Helper;
using System.Reflection;
using System.Text.RegularExpressions;
namespace AIGS
{
    class Program
    {
        static void Main(string[] args)
        {
            
            //Tidal.Account Account = new Tidal.Account();
            //Account.LogIn("masterhd1901@qq.com", "bitchjolin");
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
