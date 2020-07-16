using System;
using System.Data;
using System.Windows.Forms;

namespace AIGS.Helper
{
    public class AboutHelper
    {
        /// <summary>
        /// 添加版本信息
        /// </summary>
        /// <param name="sVersion">版本号</param>
        /// <param name="IsDebug">是否为DEBUG</param>
        /// <param name="sDesc">描述</param>
        /// <param name="sVersionInfos">版本信息集合</param>
        /// <returns>格式为 “0.0.0.3#DEBUG#使能任务、忽略任务。” 的字符串</returns>
        public static string AddVersionInfo(string sVersion, bool IsDebug, string sDesc, string sVersionInfos = null)
        {
            string sRet = "";
            if(!String.IsNullOrWhiteSpace(sVersionInfos))
                sRet = sVersionInfos;

            string sType = IsDebug ? "DEBUG" : "RELEASE";
            sRet += sVersion + '#' + sType + '#' + sDesc + "\r\n";
            return sRet;
        }

        /// <summary>
        /// 将版本信息转成行集合
        /// </summary>
        /// <param name="sVersionInfos">版本信息</param>
        /// <returns></returns>
        public static string[] ConverToLines(string sVersionInfos)
        {
            if(String.IsNullOrWhiteSpace(sVersionInfos))
                return null;

            return sVersionInfos.Split('\n');
        }

        /// <summary>
        /// 将版本信息转成表格形式
        /// </summary>
        /// <param name="sVersionInfos">版本信息</param>
        /// <returns></returns>
        public static DataTable ConverToTable(string sVersionInfos)
        {
            if(String.IsNullOrWhiteSpace(sVersionInfos))
                return null;

            DataTable aTable = new DataTable();
            aTable.Columns.Add("Version");
            aTable.Columns.Add("Type");
            aTable.Columns.Add("Desc");

            string[] sLines = ConverToLines(sVersionInfos);
            foreach(string sInfo in sLines)
            {
                if (String.IsNullOrWhiteSpace(sInfo))
                    continue;
                //0.0.0.2#测试版#任务参数添加“自动重试”功能。
                string[] sTmp = sInfo.Split('#');
                aTable.Rows.Add(sTmp);
            }

            return aTable;
        }

        /// <summary>
        /// 获取自身的版本号
        /// </summary>
        public static string GetSelfVersion()
        {
            string sVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            return sVersion;
        }

        /// <summary>
        /// 获取最后编译的日期时间
        /// </summary>
        /// <param name="aThis">窗口句柄</param>
        /// <returns></returns>
        public static string GetLastRevisionTime(Form aThis)
        {
            return System.IO.File.GetLastWriteTime(aThis.GetType().Assembly.Location).ToString();
        }

    }
}
