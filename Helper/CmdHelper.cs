using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

namespace AIGS.Helper
{
    public class CmdHelper
    {
        #region 工具

        /// <summary>
        /// 获取CMD显示的字符串
        /// </summary>
        /// <param name="sOldText">旧的字符串</param>
        /// <param name="sText">新增的字符串</param>
        /// <param name="iMaxLineNum">行数的最大值</param>
        /// <param name="iDelLineNum">超过行数最大值后删掉的行数</param>
        /// <desc>模拟CMD窗口的显示，每次EXE输出一行，将其添加到最后，如果行数超过限制，则删除掉一些</desc>
        public static string GetPrintString(string sOldText, string sText, int iMaxLineNum, int iDelLineNum)
        {
            string sRet = sOldText;

            if (!String.IsNullOrEmpty(sText))
            {
                //获取行数，如果超过限制则删除掉一些
                string[] sLines = sOldText.Split('\n');
                if (sLines.Count() >= iMaxLineNum)
                {
                    sRet = "";
                    for(int i = iDelLineNum; i < sLines.Count(); i++)
                        sRet += "\n" + sLines[i];
                }
                //将新的字符串添加到最后
                sRet += "\n" + sText;
            }
            return sRet;
        }

        #endregion


        #region 启动一个EXE

        /// <summary>
        /// 启动一个EXE
        /// </summary>
        /// <param name="aProcess">进程句柄</param>
        /// <param name="sExePath">EXE地址</param>
        /// <param name="sArg">Main参</param>
        /// <param name="sWorkDir">工作目录</param>
        /// <param name="IsShowWindow">是否显示CMD窗口</param>
        /// <returns>错误码</returns>
        public static int StartExe( ref Process aProcess,
                                        string sExePath, 
                                        string sArg, 
                                        string sWorkDir = null, 
                                        bool   IsShowWindow = true,
                                        bool   IsWatiForExit = false)
        {
            aProcess.StartInfo.FileName = sExePath;
            aProcess.StartInfo.Arguments = sArg;
            aProcess.StartInfo.UseShellExecute = false;
            aProcess.StartInfo.CreateNoWindow = !IsShowWindow;
            //工作目录
            if (!String.IsNullOrWhiteSpace(sWorkDir))
                aProcess.StartInfo.WorkingDirectory = sWorkDir;

            try
            {
                aProcess.Start();
                if (IsWatiForExit)
                {
                    aProcess.WaitForExit();
                    return aProcess.ExitCode;
                }
            }
            catch
            {
                return -1;
            }

            return 0;
        }


        /// <summary>
        /// 启动一个EXE
        /// </summary>
        /// <param name="sExePath">EXE地址</param>
        /// <param name="sArg">Main参</param>
        /// <param name="sWorkDir">工作目录</param>
        /// <param name="IsShowWindow">是否显示CMD窗口</param>
        /// <returns>错误码</returns>
        public static int StartExe(string sExePath, 
                                    string sArg, 
                                    string sWorkDir = null, 
                                    bool   IsShowWindow = true,
                                    bool IsWatiForExit = false)
        {
            Process aProcess = new Process();
            return StartExe(ref aProcess, sExePath, sArg, sWorkDir, IsShowWindow, IsWatiForExit);
        }


        /// <summary>
        /// 启动一个EXE
        /// </summary>
        /// <param name="aProcess">进程句柄</param>
        /// <param name="sExePath">EXE地址</param>
        /// <param name="sArg">Main参</param>
        /// <param name="sWorkDir">工作目录</param>
        /// <param name="pFunc">输出重定向的响应</param>
        /// <param name="bWaitExit">是否等待结束</param>
        /// <returns>错误码</returns>
        public static int StartExe2(ref Process aProcess, 
                                    string sExePath, 
                                    string sArg, 
                                    string sWorkDir = null, 
                                    System.Diagnostics.DataReceivedEventHandler pFunc = null,
                                    bool bWaitExit = true)
        {
            if (aProcess == null)
                aProcess = new Process();
            aProcess.StartInfo.FileName = sExePath;
            aProcess.StartInfo.Arguments = sArg;
            aProcess.StartInfo.UseShellExecute = false;
            aProcess.StartInfo.CreateNoWindow = true;
            //工作目录
            if (!String.IsNullOrWhiteSpace(sWorkDir))
                aProcess.StartInfo.WorkingDirectory = sWorkDir;     
            //输出重定向
            if (pFunc != null)
            {
                aProcess.StartInfo.RedirectStandardOutput = true;
                aProcess.OutputDataReceived += pFunc;
            }

            try
            {
                aProcess.Start();
                aProcess.BeginOutputReadLine();
                if (bWaitExit)
                {
                    aProcess.WaitForExit();
                    return aProcess.ExitCode;
                }
                return 0;
            }
            catch
            {
                return -1;
            }
        }

       /// <summary>
        /// 启动一个EXE
        /// </summary>
        /// <param name="sExePath">EXE地址</param>
        /// <param name="sArg">Main参</param>
        /// <param name="sWorkDir">工作目录</param>
        /// <param name="pFunc">输出重定向的响应</param>
        /// <returns>错误码</returns>
        public static int StartExe2(string sExePath, 
                                    string sArg, 
                                    string sWorkDir = null, 
                                    System.Diagnostics.DataReceivedEventHandler pFunc = null)
        {
            Process aProcess = new Process();
            return StartExe2(ref aProcess, sExePath, sArg, sWorkDir, pFunc);
        }

        /// <summary>
        /// 查找进程
        /// </summary>
        /// <param name="sName"></param>
        /// <returns></returns>
        public static Process[] FindProcess(string sName)
        {
            List<Process> pRet = new List<Process>();
            Process[] sList = Process.GetProcesses();
            foreach (var item in sList)
            {
                if (item.ProcessName == sName)
                    pRet.Add(item);
            }
            return pRet.ToArray();
        }

        /// <summary>
        /// 杀死所有进程
        /// </summary>
        /// <param name="sName"></param>
        /// <returns></returns>
        public static void KillProcess(string sName)
        {
            try
            {
                Process[] sList = FindProcess(sName);
                foreach (var item in sList)
                {
                    item.Kill();
                }
            }
            catch { }
        }

        #endregion
    }
}
