using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;

namespace AIGS.Helper
{
    public class LogHelper
    {
        #region 内参
        /// <summary>
        /// 文件读写锁
        /// </summary>
        private static readonly object m_WirteLock = new object();
        #endregion

        #region 外部接口
        /// <summary>
        /// 写入开始标志
        /// </summary>
        /// <param name="sProjectName">项目名</param>
        /// <param name="sLogPath">日志地址</param>
        public static void Start(string sProjectName, string sLogPath = null)
        {
            string sBuff = "\r\n[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] " + "***Start Project***:  " + sProjectName + "\r\n";
            //写入
            WriteToLogFile(sBuff, sLogPath);
        }

        /// <summary>
        /// 写入结束标志
        /// </summary>
        /// <param name="sEndMsg">结束信息</param>
        /// <param name="sLogPath">日志地址</param>
        public static void End(string sEndMsg, string sLogPath = null)
        {

            string sBuff = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] " + "***End Project***" ;
            if (!String.IsNullOrWhiteSpace(sEndMsg))
                sBuff += ":  " + sEndMsg;

            //写入
            WriteToLogFile(sBuff, sLogPath);
        }


        /// <summary>
        /// 写入日志
        /// </summary>
        /// <param name="sContent">记录内容</param>
        /// <param name="sTitle">标题</param>
        /// <param name="sDesc">秒速</param>
        /// <param name="sLogPath">日志地址</param>
        /// 
        /// <desc> 日志样式如下
        /// 【2017-02-28 12:05:05】  TiTle
        ///                         This is a Log line.
        /// [Desc]：  Desc
        /// </desc>
        public static void Write(string sContent, string sTitle = null, string sDesc = null, string sLogPath = null)
        {
            //组织记录-加时间
            string sBuff = "\r\n[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "]  ";
            //组织记录-加标题
            if (!String.IsNullOrWhiteSpace(sTitle))
                sBuff += sTitle + "\r\n";

            //组织记录-加记录
            sBuff += "                      " + sContent + "\r\n";
            //组织记录-加秒速
            if (!String.IsNullOrWhiteSpace(sDesc))
                sBuff += "[Desc]".PadRight(8) + sDesc + "\r\n";

            //写入
            WriteToLogFile(sBuff, sLogPath);
        }

        /// <summary>
        /// 写入主题和记录内容
        /// </summary>
        /// <param name="sContent">记录内容</param>
        /// <param name="sTitle">标题</param>
        /// <param name="sLogPath">日志地址</param>
        public static void WritePre(string sContent, string sTitle = null, string sLogPath = null)
        {
            Write(sContent, sTitle, null, sLogPath);
        }

        /// <summary>
        /// 写入描述
        /// </summary>
        /// <param name="sDesc">秒速</param>
        /// <param name="sLogPath">日志地址</param>
        public static void WriteDesc(string sDesc, string sLogPath = null)
        {
            WriteToLogFile("[Desc]".PadRight(8) + sDesc + "\r\n", sLogPath);
        }

        /// <summary>
        /// 将系统异常写入日志
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="sLogPath"></param>
        public static void WriteException(Exception ex, string sLogPath = null)
        {
            string sDesc = "";
            sDesc += "当前时间：" + DateTime.Now.ToString() + '\n';
            sDesc += "异常信息：" + ex.Message + '\n';
            sDesc += "异常对象：" + ex.Source + '\n';
            sDesc += "调用堆栈：\n" + ex.StackTrace.Trim() + '\n';
            sDesc += "触发方法：" + ex.TargetSite + '\n';

            WriteDesc(sDesc, sLogPath);
        }

        #endregion

        #region 工具

        /// <summary>
        /// 往日志文件中写入信息
        /// </summary>
        /// <param name="sContent">内容</param>
        /// <param name="sLogPath">日志地址</param>
        private static void WriteToLogFile(string sContent, string sLogPath = null)
        {
            //设置日志名
            if (String.IsNullOrWhiteSpace(sLogPath))
                sLogPath = GetDefaultPathName();

            //创建目录
            var di = new DirectoryInfo(Path.GetDirectoryName(sLogPath));
            if (!di.Exists)
                di.Create();

            lock (m_WirteLock)
            {
                try
                {
                    StreamWriter FD = new StreamWriter(sLogPath, true);
                    FD.Write(sContent);
                    FD.Flush();
                    FD.Close();
                }
                catch
                { }
            }
        }

        /// <summary>
        /// 获取默认的日志路径文件名
        /// </summary>
        /// <returns></returns>
        private static string GetDefaultPathName()
        {
            int iIndex;
            string sPath = AIGS.Helper.SystemHelper.GetExeDirectoryName();
            string sExeName = AIGS.Helper.SystemHelper.GetExeName();

            //去除.exe
            iIndex = sExeName.ToLower().IndexOf(".exe");
            if (iIndex > 0)
                sExeName = sExeName.Substring(0, iIndex);

            //去除调试状态下的.vshost
            iIndex = sExeName.ToLower().IndexOf(".vshost");
            if (iIndex > 0)
                sExeName = sExeName.Substring(0, iIndex);

            string sRet = sPath + "\\" + sExeName + ".log";
            return sRet;
        }

        #endregion
    }
}
