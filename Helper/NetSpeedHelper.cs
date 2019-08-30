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

        bool   FirstTime;
        double DownloadSpeed;
        int    CurPro;
        DateTime StartTime;
        UpdateProgressNotify pProgress;

        /// <summary>
        /// 更新进度条回调
        /// </summary>
        public delegate bool UpdateProgressNotify(int iProgressValue, double dCurSpeed);

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
        /// 删除临时文件
        /// </summary>
        private void RemoveTmpFile()
        {
            string sPath = SystemHelper.GetExeDirectoryName();
            string[] sArra = PathHelper.GetFileNames(sPath);
            for (int i = 0; i < sArra.Count(); i++)
            {
                try
                {
                    string sName = Path.GetFileName(sArra[i]);
                    if (sName.IsNotBlank() && sName.IndexOf("NetSpeedTmp-") >= 0)
                        File.Delete(sArra[i]);
                }
                catch { }
            }
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
            RemoveTmpFile();
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

            await DownloadFileHepler.StartAsync(TEST_URL, sFilePath, null, UpdateDownloadNotify, CompleteDownloadNotify, null, 3);
            RemoveTmpFile();
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
            CurPro = iValue;

            //更新下载速度
            TimeSpan ts = (DateTime.Now - StartTime);
            DownloadSpeed = lAlreadyDownloadSize / ts.TotalMilliseconds / 1000;

            //更新进度条
            if (pProgress != null)
                return pProgress(iValue, DownloadSpeed);

            
            return true;
        }

        /// <summary>
        /// 下载完成回调
        /// </summary>
        private void CompleteDownloadNotify(long lTotalSize, object data)
        {
            TimeSpan ts = (DateTime.Now - StartTime);
            DownloadSpeed = lTotalSize / ts.TotalMilliseconds / 1000;

            //更新进度条
            if (pProgress != null)
                pProgress(100, DownloadSpeed);
        }

    }
}
