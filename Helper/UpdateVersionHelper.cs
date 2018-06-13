#region << 版本说明 >>
/****************************************************
 * Creator by  : Yaron (yaronhuang@foxmail.com)
 * Create date : 2018-1-30
 * Modification History :
 * Date           Programmer         Amendment
 * 
*******************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace AIGS.Helper
{
    public class UpdateVersionHelper
    {
        #region 获取网络版本

        static string FILE_UPDATE_VERSION = "UpdateVersion.xml";
        /***************XML文件说明************************
         *功能：1、提供主程序路径（主程序用于获取版本号）
         *     2、提供待更新的文件列表   
         * 
         * 样式：
         *    <UpdateVersion>
         *      <MainFile>AlbumDL.exe</MainFile>
         *      <DependFile>
         *          <Path>AlbumDL.exe</Path>
         *       </DependFile>
         *    </UpdateVersion>
         ************************************************/
        static string ASHX_FILE_NAME = "AutoUpdate.ashx";
        static string PHP_FILE_NAME = "AutoUpdate.php";

        public static bool CreatUpdateVersionFile(string sOutputDir, string sMainFilePath, string[] sDependFilePaths = NULL)
        {
            if (String.IsNullOrWhiteSpace(sOutputDir) || String.IsNullOrWhiteSpace(sMainFilePath))
                return false;

            XElement xElement = new XElement(
                new XElement("UpdateVersion",
                    new XElement("MainFile", sMainFilePath)
                    )
                );

            for(int i = 0; sDependFilePaths != null && i < sDependFilePaths.Count(); i++)
            {
                XmlHelper.AddXmlNode(xElement, "DependFile\\Path");
            }
 
             //需要指定编码格式，否则在读取时会抛：根级别上的数据无效。 第 1 行 位置 1异常
             XmlWriterSettings settings = new XmlWriterSettings();
             settings.Encoding = new UTF8Encoding(false);
             settings.Indent = true;
             XmlWriter xw = XmlWriter.Create(xmlPath,settings);
             xElement.Save(xw);
             //写入文件
             xw.Flush();
             xw.Close();
        }



        public struct FileInfo
        {
            public string Name;
            public string Path;
        }

        public struct AutoUpdate
        {
            public string VersionFile;
            public List<FileInfo> FileList;
        }

        /// <summary>
        /// 新建自动更新的XML文件（测试用）
        /// </summary>
        /// <param name="aFilePaths"></param>
        public static void CreatAutoUpdateFile(List<string> aFilePaths)
        {
            if (aFilePaths == null || aFilePaths.Count == 0)
                return;

            AutoUpdate aRecord = new AutoUpdate();
            aRecord.VersionFile = aFilePaths[0];
            aRecord.FileList = new List<FileInfo>();

            for(int i = 0; aFilePaths != null && i < aFilePaths.Count; i++)
            {
                string sName = Path.GetFileName(aFilePaths[i]);
                string sPath = Path.GetDirectoryName(aFilePaths[i]);

                aRecord.FileList.Add(new FileInfo { Name = sName, Path = sPath });
            }

            string sStr = XmlHelper.ConverObjectToXmlString<AutoUpdate>(aRecord);
            File.WriteAllText(XML_FLE_NAME, sStr);
        }



        /// <summary>
        /// 是否有新版本
        /// </summary>
        /// <param name="sXmlDir"></param>
        /// <returns></returns>
        public static bool HaveNewVersion(string sXmlDir)
        {
            string sSelfVersion = VersionHelper.GetSelfVersion();
            string sOnlineVersion = NetHelper.DownloadString(sXmlDir + '/' + PHP_FILE_NAME);
            if (!String.IsNullOrWhiteSpace(sOnlineVersion))
                sOnlineVersion = VersionHelper.GetCorrectString(sOnlineVersion);

            if (VersionHelper.Compare(sSelfVersion, sOnlineVersion) >= 0)
                return false;
            return true;
        }

        /// <summary>
        /// 获取下载的文件列表
        /// </summary>
        /// <param name="sXmlDir"></param>
        /// <returns></returns>
        static List<FileInfo> GetUpdateFileList(string sXmlDir)
        {
            List<FileInfo> pRet = XmlHelper.ConverXmlFileToObject<List<FileInfo>>(sXmlDir + '/' + XML_FLE_NAME, "AutoUpdate/FileList/FileInfo");
            if (pRet == null)
            {
                pRet = new List<FileInfo>();
                FileInfo aRecord = XmlHelper.ConverXmlFileToObject<FileInfo>(sXmlDir + '/' + XML_FLE_NAME, "AutoUpdate/FileList/FileInfo");
                if (!String.IsNullOrWhiteSpace(aRecord.Name) && !String.IsNullOrWhiteSpace(aRecord.Path))
                    pRet.Add(aRecord);
            }
            return pRet;
        }

        /// <summary>
        /// 下载
        /// </summary>
        /// <param name="sXmlDir"></param>
        /// <returns></returns>
        static bool DownloadFiles(string sXmlDir)
        {
            List<FileInfo> pFileList = GetUpdateFileList(sXmlDir);
            if (pFileList.Count == 0)
                return false;

            foreach (FileInfo Info in pFileList)
            {
                string sFilePath = sXmlDir + '/' + Info.Path + '/' + Info.Name;
                string sToPath = AppDomain.CurrentDomain.BaseDirectory + "\\NewVersion\\" + Info.Name;
                if(NetHelper.DownloadFile(sFilePath, sToPath) != 0)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 更新版本
        /// </summary>
        /// <param name="sArg"></param>
        /// <param name="sXmlDir"></param>
        /// <returns>True则退出程序</returns>
        public static bool UpdateOnlineVersion(string[] sArg, string sXmlDir)
        {
            if (IsUpdateProcess(sArg))
            {
                UpdateExe(sArg);
                return true;
            }

            //查询版本
            if (!HaveNewVersion(sXmlDir))
                return false;

            //下载文件
            if (!DownloadFiles(sXmlDir))
                return false;

            //转载Main参
            string sArguments = "";
            for (int i = 1; i < sArg.Count(); i++)
                sArguments += " " + sArg[i];
            OpenTmpExe(sArguments);

            return true;
        }

        #endregion



        #region 第一步先保存EXE和动态库

        /// <summary>
        /// 保存新版本
        /// </summary>
        /// <param name="bBytes">文件内容</param>
        /// <param name="iBytesLen">内容长度</param>
        /// <param name="sExePath">Application.ExecutablePath</param>
        public static void SaveNewVersion(byte[] bBytes, int iBytesLen, string sExePath)
        {
            string sFileName = Path.GetFileName(sExePath);
            string sFilePath = Path.GetDirectoryName(Path.GetFullPath(sExePath)) + "\\NewVersion\\";

            //创建目录
            Directory.CreateDirectory(sFilePath);
            File.Delete(sFilePath + sFileName);

            //写入文件
            FileStream writer = new FileStream(sFilePath + sFileName, FileMode.CreateNew, FileAccess.Write);
            writer.Write(bBytes, 0, iBytesLen);
            writer.Close();
        }


        #endregion

        #region 第二步打开临时的新版本EXE，然后退出本进程
        /// <summary>
        /// 打开临时的Exe（".\\NewVersion\\"目录下的EXE）
        /// </summary>
        /// <param name="sArg">Main参</param>
        /// <desc>开启临时EXE之后需要关闭自己</desc>> 
        public static void OpenTmpExe(string sArg)
        {
            string sExePath = Application.ExecutablePath;
            string sFileName = Path.GetFileName(sExePath);
            string sFilePath = Path.GetDirectoryName(Path.GetFullPath(sExePath)) + "\\NewVersion\\";

            //启动临时EXE
            Process aProcess = new Process();
            aProcess.StartInfo.WorkingDirectory = sFilePath;
            aProcess.StartInfo.FileName = sFilePath + sFileName;
            aProcess.StartInfo.Arguments = "UPDATE_EXE " + sArg;
            aProcess.StartInfo.UseShellExecute = false;
            aProcess.StartInfo.CreateNoWindow = false;
            aProcess.Start();
        }
        #endregion

        #region 第三步，复制新版本文件到原目录，并启动原目录下的EXE，再退出本进程
        /// <summary>
        /// 是否为更新进程
        /// </summary>
        /// <param name="sArg">Main参</param>
        public static bool IsUpdateProcess(string[] sArg)
        {
            if (sArg != null && sArg.Count() != 0 && sArg[0] == "UPDATE_EXE")
                return true;

            return false;
        }

        /// <summary>
        /// 更新版本
        /// </summary>
        public static void UpdateExe(string[] sArg)
        {
            string sExeName = Path.GetFileName(Application.ExecutablePath);
            string sFilePath = Path.GetDirectoryName(Path.GetFullPath(Application.ExecutablePath));

            //目录去掉"\\NewVersion\\",则为正式EXE的目录
            int iIndex = sFilePath.IndexOf("NewVersion");
            string sRealPath = sFilePath.Substring(0, iIndex);

            //复制文件
            int iCopyRetryNum = 5;
            string[] sFileNames = PathHelper.GetFileNames(sFilePath);
            for (int i = 0; sFileNames != null &&i < sFileNames.Count(); i++)
            {
                string sCopyFileName = sRealPath + "\\" + sFileNames[i];
                if (CopyFile(sFilePath + "\\" + sFileNames[i], sCopyFileName) == false)
                {
                    if (iCopyRetryNum-- < 0)
                        break;
                    Thread.Sleep(2000);
                }
            }

            //转载Main参
            string sArguments = "";
            for (int i = 1; i < sArg.Count(); i++)
                sArguments += " " + sArg[i];

            //启动正式版本
            Process aProcess = new Process();
            aProcess.StartInfo.WorkingDirectory = sRealPath;
            aProcess.StartInfo.FileName = sRealPath + "\\" + sExeName;
            aProcess.StartInfo.Arguments = sArguments;
            aProcess.StartInfo.UseShellExecute = false;
            aProcess.StartInfo.CreateNoWindow = false;
            aProcess.Start();
        }
        #endregion


        /// <summary>
        /// 复制文件
        /// </summary>
        /// <param name="souceFileName">文件来源</param>
        /// <param name="destFileName">目标文件</param>
        private static bool CopyFile(string souceFileName, string destFileName)
        {
            try
            {
                if (File.Exists(destFileName))
                    File.Delete(destFileName);
                File.Copy(souceFileName, destFileName, true);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
