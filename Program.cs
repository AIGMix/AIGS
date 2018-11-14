using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AIGS.Helper;
using System.Reflection;
using AIGS.Tools;
namespace AIGS
{
    class Program
    {

        
        static void Main(string[] args)
        {
            Album aInfo = new Album();
            TidalTool.GetAlbumInfo(96146858, ref aInfo);
            TidalTool.GetAlbumTracks(97697819);

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
