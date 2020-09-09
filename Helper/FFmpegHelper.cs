using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIGS.Common;
using System.IO;
using System.Diagnostics;
namespace AIGS.Helper
{
    public class FFmpegHelper
    {
        /// <summary>
        /// 合并文件
        /// </summary>
        /// <param name="pFiles">源文件列表</param>
        /// <param name="sToFilePath">目标文件地址</param>
        /// <returns>True/False</returns>
        public static bool MergerByFiles(string[] pFiles, string sToFilePath, string sFFmpegPath=null)
        {
            try
            {
                if (sToFilePath.IsBlank() || pFiles == null)
                    return false;

                string sFileName = PathHelper.ReplaceLimitChar(Path.GetFileNameWithoutExtension(sToFilePath), "-");

                //创建目录
                string sTmpPath = Path.GetDirectoryName(sToFilePath);
                var di = new DirectoryInfo(sTmpPath);
                if (!di.Exists)
                    di.Create();

                //创建临时文件
                string sTmpFile = sTmpPath + "\\" + sFileName + "TMP.txt";
                foreach (string item in pFiles)
                {
                    FileHelper.Write("file \'" + item + "\'\n", false, sTmpPath + "\\" + sFileName + "TMP.txt");
                }
                
                //删除旧文件
                if (File.Exists(sToFilePath))
                    File.Delete(sToFilePath);

                if (sFFmpegPath.IsNotBlank())
                {
                    if (!File.Exists(sFFmpegPath))
                        return false;
                }
                else
                    sFFmpegPath = "ffmpeg.exe";

                //启动合并转换进程
                string cmd = "-f concat -safe 0 -i \"" + sTmpFile + "\" -c copy \"" + sToFilePath + "\"";
                int exitCode = CmdHelper.StartExe(sFFmpegPath, cmd, IsShowWindow: false, IsWatiForExit: true);

                //删除临时文件
                File.Delete(sTmpFile);
                return exitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        public static bool Convert(string sSrcFile, string sDescFile)
        {
            sSrcFile  = Path.GetFullPath(sSrcFile);
            sDescFile = Path.GetFullPath(sDescFile);
            if (!File.Exists(sSrcFile))
                return false;
            if (File.Exists(sDescFile))
                File.Delete(sDescFile);

            string sFFmpegPath = "ffmpeg.exe";
            string sCmd = "-i \"" + sSrcFile + "\" -c copy \"" + sDescFile + "\"";
            int exitCode = CmdHelper.StartExe(sFFmpegPath, sCmd, IsShowWindow: false, IsWatiForExit: true);
            return exitCode == 0;
        }

        public static bool IsExist()
        {
            string sCom = "ffmpeg -V&exit";
            Process p                          = new Process();  //设置要启动的应用程序
            p.StartInfo.FileName               = "cmd.exe";      //是否使用操作系统shell启动
            p.StartInfo.UseShellExecute        = false;          //接受来自调用程序的输入信息
            p.StartInfo.RedirectStandardInput  = true;           //输出信息
            p.StartInfo.RedirectStandardOutput = true;           //输出错误
            p.StartInfo.RedirectStandardError  = true;           //不显示程序窗口
            p.StartInfo.CreateNoWindow         = true;           //启动程序
            p.Start();
            p.StandardInput.WriteLine(sCom);
            p.StandardInput.AutoFlush = true;   

            StreamReader reader = p.StandardOutput;
            StreamReader error  = p.StandardError;
            string sOutput      = reader.ReadToEnd() + error.ReadToEnd();
            p.WaitForExit();
            p.Close();

            if (sOutput.IsBlank())
                return false;
            sOutput = sOutput.Substring(sOutput.IndexOf(sCom) + sCom.Length).ToLower();
            if (sOutput.IndexOf("version") > 0 && sOutput.IndexOf("copyright") > 0)
                return true;
            return false;
        }
    }
}
