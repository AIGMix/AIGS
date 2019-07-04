using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIGS.Common;
namespace AIGS.Helper
{
    public class NetSpeedHelper
    {
        string TEST_URL = @"http://speedtest-sfo2.digitalocean.com/10mb.test";

        bool FirstTime;
        double DownloadSpeed;
        int CurPro;
        DateTime StartTime;
        UpdateProgressNotify pProgress;

        /// <summary>
        /// 更新进度条回调
        /// </summary>
        public delegate bool UpdateProgressNotify(int dValue);

        /// <summary>
        /// 获取临时文件名
        /// </summary>
        private string GetTmpFilePath()
        {
            string sPath = SystemHelper.GetExeDirectoryName();
            for (int i = 0; i < 3; i++)
            {
                string sName = "NetSpeedTmp-" + RandHelper.GetIntRandom(5, 9, 0);
                string sRet = sPath + "\\" + sName;
                if (File.Exists(sRet))
                    continue;
                return sRet;
            }
            return null;
        }

        /// <summary>
        /// 开始前的准备
        /// </summary>
        private void Prepare(UpdateProgressNotify pFunc)
        {
            CurPro = 0;
            FirstTime = true;
            pProgress = pFunc;
            DownloadSpeed = 0;
        }

        /// <summary>
        /// 启动
        /// </summary>
        public async Task<double> Start(UpdateProgressNotify pFunc = null)
        {
            Prepare(pFunc);
            string sFilePath = GetTmpFilePath();
            if (sFilePath.IsBlank())
                return -1;

            object oObj = await DownloadFileHepler.StartAsync(TEST_URL, sFilePath, null, UpdateDownloadNotify, CompleteDownloadNotify, null, 3);
            if (File.Exists(sFilePath))
                File.Delete(sFilePath);
            return DownloadSpeed;
        }

        /// <summary>
        /// 下载更新回调
        /// </summary>
        private bool UpdateDownloadNotify(long lTotalSize, long lAlreadyDownloadSize, long lIncreSize, object data)
        {
            if(FirstTime)
            {
                FirstTime = false;
                StartTime = DateTime.Now;
            }

            int iValue = (int)(lAlreadyDownloadSize * 100 / lTotalSize);
            if(iValue == CurPro)
                return true;

            //更新进度条
            if (pProgress != null)
                pProgress(iValue);

            //更新下载速度
            if(iValue - CurPro > 3)
            {
                TimeSpan ts = (DateTime.Now - StartTime);
                DownloadSpeed = lAlreadyDownloadSize / ts.TotalMilliseconds / 1000;
            }

            CurPro = iValue;
            return true;
        }

        /// <summary>
        /// 下载完成回调
        /// </summary>
        private void CompleteDownloadNotify(long lTotalSize, object data)
        {
            TimeSpan ts = (DateTime.Now - StartTime);
            DownloadSpeed = lTotalSize / ts.TotalMilliseconds / 1000;
        }

    }
}
