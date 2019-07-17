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
    }
}
