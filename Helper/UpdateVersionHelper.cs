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
using System.Xml;
using System.Xml.Linq;

namespace AIGS.Helper
{
    public class UpdateVersionHelper
    {
        #region 获取网络版本

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
        static string FILE_UPDATE_XML = "Update.xml";
        static string FILE_UPDATE_PHP = "Update.php";

        /// <summary>
        /// 创建更新需要的XML文件
        /// </summary>
        /// <param name="sOutputDir">文件输出目录</param>
        /// <param name="sMainFilePath">主程序</param>
        /// <param name="sDependFilePaths">依赖文件集合</param>
        /// <returns></returns>
        public static bool CreatUpdateVersionFile(string sOutputDir, string sMainFilePath, string[] sDependFilePaths = null)
        {
            if (String.IsNullOrWhiteSpace(sOutputDir) || String.IsNullOrWhiteSpace(sMainFilePath))
                return false;

            try
            {
                //删除旧文件
                string sXmlFilePath = sOutputDir + FILE_UPDATE_XML;
                File.Delete(sXmlFilePath);

                //创建新的XML文件
                XmlDocument xDocument = XmlHelper.Open();
                XmlHelper.AddXmlNode(xDocument, "UpdateVersion\\MainFile", sMainFilePath);
                for (int i = 0; sDependFilePaths != null && i < sDependFilePaths.Count(); i++)
                {
                    XmlHelper.AddXmlNode(xDocument, "UpdateVersion\\DependFile\\Path", sDependFilePaths[i]);
                }
                XmlHelper.Save(sOutputDir + FILE_UPDATE_XML, xDocument);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 获取网络版本号
        /// </summary>
        /// <param name="sUpdateXmlPath"></param>
        /// <returns></returns>
        public static string GetOnlineVersion(string sUpdateXmlPath)
        {
            if (String.IsNullOrWhiteSpace(sUpdateXmlPath))
                return null;

            string sOnlineVersion = NetHelper.DownloadString(sUpdateXmlPath + '/' + FILE_UPDATE_PHP);
            return sOnlineVersion;
        }

        /// <summary>
        /// 是否需要更新版本
        /// </summary>
        /// <param name="sUpdateXmlPath"></param>
        /// <returns></returns>
        public static bool NeedUpdate(string sUpdateXmlPath)
        {
            string sSelfVersion     = VersionHelper.GetSelfVersion();
            string sOnlineVersion   = GetOnlineVersion(sUpdateXmlPath);
            if (String.IsNullOrWhiteSpace(sOnlineVersion))
                return false;

            sOnlineVersion = VersionHelper.GetCorrectString(sOnlineVersion);
            if (VersionHelper.Compare(sSelfVersion, sOnlineVersion) >= 0)
                return false;
            return true;
        }

        /// <summary>
        /// 获取下载的文件列表
        /// </summary>
        /// <param name="sUpdateXmlPath"></param>
        /// <returns></returns>
        static string[] GetUpdateFileList(string sUpdateXmlPath)
        {
            if (String.IsNullOrWhiteSpace(sUpdateXmlPath))
                return null;

            XmlNode xNode;
            XmlDocument xDocument;
            List<string> pRet = new List<string>();

            //打开XML文件
            if ((xDocument = XmlHelper.Open(sUpdateXmlPath + '/' + FILE_UPDATE_XML)) == null)
                return null;

            //添加主文件
            xNode = XmlHelper.GetXmlNode(xDocument, "UpdateVersion\\MainFile");
            if (xNode != null && !String.IsNullOrWhiteSpace(xNode.Value))
                pRet.Add(xNode.Value);

            //添加依赖文件
            xNode = XmlHelper.GetXmlNode(xDocument, "UpdateVersion\\DependFile");
            if(xNode != null)
            {
                XmlNodeList xList = xNode.ChildNodes;
                for(int i = 0; xList != null && i < xList.Count; i++)
                {
                    if (xList[i].Name != "Path" || String.IsNullOrWhiteSpace(xList[i].Value))
                        continue;
                    pRet.Add(xList[i].Value);
                }
            }

            if(pRet.Count == 0)
                return null;

            return pRet.ToArray();
        }

        /// <summary>
        /// 更新版本
        /// </summary>
        /// <param name="sArg"></param>
        /// <param name="sUpdateXmlPath"></param>
        /// <returns>True则退出程序</returns>
        public static bool UpdateOnlineVersion(string[] sArg, string sUpdateXmlPath)
        {
            if (IsTempExe(sArg))
            {
                UpdateExe(sArg);
                return true;
            }

            //查询版本
            if (!NeedUpdate(sUpdateXmlPath))
                return false;

            //下载文件
            if (!DownloadFilesToTempDir(sUpdateXmlPath))
                return false;

            //打开临时程序
            OpenTempExe(sArg);
            return true;
        }

        #endregion

        #region 第一步先下载或保存文件到临时目录

        /// <summary>
        /// 下载文件到临时目录
        /// </summary>
        /// <param name="sUpdateXmlPath"></param>
        /// <returns></returns>
        static bool DownloadFilesToTempDir(string sUpdateXmlPath)
        {
            if (String.IsNullOrWhiteSpace(sUpdateXmlPath))
                return false;

            //获取文件集合
            string[] pFileList = GetUpdateFileList(sUpdateXmlPath);
            if (pFileList == null)
                return false;

            //获取基础路径
            string sOnlineBasePath = Path.GetDirectoryName(sUpdateXmlPath);
            string sLocalBasePath = Helper.SystemHelper.GetExeDirectoryName();

            //下载文件
            foreach (string sPath in pFileList)
            {
                string sFromPath = sOnlineBasePath + '\\' + sPath;
                string sToPath = sLocalBasePath + "\\NewVersion\\" + sPath;
                if (NetHelper.DownloadFile(sFromPath, sToPath) != 0)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 保存文件到临时目录
        /// </summary>
        /// <param name="bBytes">文件内容</param>
        /// <param name="iBytesLen">内容长度</param>
        /// <param name="sRelativePath">保存到临时目录的文件相对路径</param>
        public static bool SaveFileToTempDir(byte[] bBytes, int iBytesLen, string sRelativePath)
        {
            if (bBytes == null || iBytesLen < 0 || String.IsNullOrWhiteSpace(sRelativePath))
                return false;

            try
            {
                string sBasePath = Helper.SystemHelper.GetExeDirectoryName() + "\\NewVersion\\";
                string sFilePath = sBasePath + sRelativePath;

                //创建目录
                Directory.CreateDirectory(Path.GetDirectoryName(sFilePath));
                File.Delete(sFilePath);

                //写入文件
                FileStream writer = new FileStream(sFilePath, FileMode.CreateNew, FileAccess.Write);
                writer.Write(bBytes, 0, iBytesLen);
                writer.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 拷贝文件到临时目录
        /// </summary>
        /// <param name="sSrcFilePath">文件来源</param>
        /// <param name="sRelativePath">保存到临时目录的文件相对路径</param>
        /// <returns></returns>
        public static bool CopyFileToTempDir(string sSrcFilePath, string sRelativePath)
        {
            if (String.IsNullOrWhiteSpace(sSrcFilePath) || String.IsNullOrWhiteSpace(sRelativePath))
                return false;

            try
            {
                string sBasePath = Helper.SystemHelper.GetExeDirectoryName() + "\\NewVersion\\";
                string sFilePath = sBasePath + sRelativePath;

                //创建目录
                Directory.CreateDirectory(Path.GetDirectoryName(sFilePath));
                File.Delete(sFilePath);

                //复制文件
                File.Copy(sSrcFilePath, sFilePath, true);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region 第二步打开临时程序，然后退出本进程
        /// <summary>
        /// 打开临时程序
        /// </summary>
        /// <param name="sArg">Main参</param>
        /// <desc>开启临时EXE之后需要关闭自己</desc>> 
        public static void OpenTempExe(string[] sArg)
        {
            string sFileName    = Helper.SystemHelper.GetExeName();
            string sFileDir     = Helper.SystemHelper.GetExeDirectoryName() + "\\NewVersion\\";

            //Main参设置
            string sArguments = "UPDATE_EXE ";
            for (int i = 1; i < sArg.Count(); i++ )
            {
                sArguments += " " + sArg[i];
            }

            //启动临时EXE
            Process aProcess = new Process();
            aProcess.StartInfo.WorkingDirectory = sFileDir;
            aProcess.StartInfo.FileName         = sFileDir + sFileName;
            aProcess.StartInfo.Arguments        = sArguments;
            aProcess.StartInfo.UseShellExecute  = false;
            aProcess.StartInfo.CreateNoWindow   = false;
            aProcess.Start();
        }
        #endregion

        #region 第三步，复制新版本文件到原目录，并启动原目录下的EXE，再退出本进程
        /// <summary>
        /// 是否为临时程序
        /// </summary>
        /// <param name="sArg">Main参</param>
        public static bool IsTempExe(string[] sArg)
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
            string sExeDir  = Helper.SystemHelper.GetExeDirectoryName();
            string sExeName = Helper.SystemHelper.GetExeName();

            //目录去掉"\\NewVersion\\",则为正式EXE的目录
            int iIndex          = sExeDir.LastIndexOf("NewVersion");
            string sRealDir     = sExeDir.Substring(0, iIndex);

            //复制文件
            string[] sFileList  = PathHelper.GetFileNames(sExeDir);
            foreach(string sFileName in sFileList)
            {
                int iCopyRetryNum   = 5;
                string sToPath      = sRealDir + "\\" + sFileName;
                string sFromPath    = sExeDir + "\\" + sFileName;

                while (iCopyRetryNum-- > 0)
                {
                    try
                    {
                        File.Copy(sFromPath, sToPath, true);
                    }
                    catch
                    {
                        Thread.Sleep(1000);
                    }
                }
            }

            //转载Main参
            string sArguments = "";
            for (int i = 1; i < sArg.Count(); i++)
            {
                sArguments += " " + sArg[i];
            }

            //启动正式版本
            Process aProcess = new Process();
            aProcess.StartInfo.WorkingDirectory = sRealDir;
            aProcess.StartInfo.FileName         = sRealDir + "\\" + sExeName;
            aProcess.StartInfo.Arguments        = sArguments;
            aProcess.StartInfo.UseShellExecute  = false;
            aProcess.StartInfo.CreateNoWindow   = false;
            aProcess.Start();
        }
        #endregion
    }
}
