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
using System.Configuration;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace AIGS.Helper
{
    public class ConfigHelper
    {
        #region 宏
        /// <summary>
        /// 读取的值最大长度
        /// </summary>
        private const int MAX_VALUE_LEN = 1024;

        #endregion

        #region 系统接口引用
        /// <summary>
        /// 往配置文件写入
        /// </summary>
        /// <param name="section">所在域</param>
        /// <param name="key">关键字</param>
        /// <param name="val">值</param>
        /// <param name="filePath">文件地址</param>
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        /// <summary>
        /// 从配置文件中读出
        /// </summary>
        /// <param name="section">所在域</param>
        /// <param name="key">关键字</param>
        /// <param name="def">默认值（查不到则以默认值返回）</param>
        /// <param name="retVal">值</param>
        /// <param name="size">值长度</param>
        /// <param name="filePath">文件地址</param>
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        #endregion

        #region 配置文件基本Key值读写

        /// <summary>
        /// 查询配置文件的值(STRING)
        /// </summary>
        /// <param name="sKey">关键字</param>
        /// <param name="sGroup">组</param>
        /// <param name="sConfigPath">配置文件地址</param>
        /// <returns></returns>
        public static string GetValue(string sKey, string sGroup = null, string sConfigPath = null)
        {
            if (String.IsNullOrWhiteSpace(sKey))
                return "";

            //设置配置名
            if (String.IsNullOrWhiteSpace(sConfigPath))
                sConfigPath = GetDefaultPathName();

            if (String.IsNullOrWhiteSpace(sGroup))
                sGroup = "";

            //读取相应值
            StringBuilder sValue = new StringBuilder(MAX_VALUE_LEN);
            GetPrivateProfileString(sGroup, sKey, "", sValue, MAX_VALUE_LEN, sConfigPath);

            return sValue.ToString();
        }

        /// <summary>
        /// 查询配置文件的值(STRING)
        /// </summary>
        /// <param name="sKey">关键字</param>
        /// <param name="sGroup">组</param>
        /// <param name="sConfigPath">配置文件地址</param>
        /// <returns></returns>
        public static string GetValue(string sKey, string sDefault, string sGroup = null, string sConfigPath = null)
        {
            if (String.IsNullOrWhiteSpace(sKey))
                return sDefault;

            //设置配置名
            if (String.IsNullOrWhiteSpace(sConfigPath))
                sConfigPath = GetDefaultPathName();

            if (String.IsNullOrWhiteSpace(sGroup))
                sGroup = "";

            //读取相应值
            StringBuilder sValue = new StringBuilder(MAX_VALUE_LEN);
            GetPrivateProfileString(sGroup, sKey, sDefault, sValue, MAX_VALUE_LEN, sConfigPath);

            return sValue.ToString();
        }



        /// <summary>
        /// 查询配置文件的值(INT)
        /// </summary>
        /// <param name="sKey">关键字</param>
        /// <param name="iDefault">默认值</param>
        /// <param name="sGroup">组</param>
        /// <param name="sConfigPath">配置文件地址</param>
        /// <returns></returns>
        public static int GetValue(string sKey, int iDefault, string sGroup = null, string sConfigPath = null)
        {
            int iRet;
            string sValue = GetValue(sKey, sGroup, sConfigPath);
            if (!String.IsNullOrWhiteSpace(sValue))
            {
                if (int.TryParse(sValue, out iRet))
                    return iRet;
            }

            return iDefault;
        }

        /// <summary>
        /// 查询配置文件的值(FLOAT)
        /// </summary>
        /// <param name="sKey">关键字</param>
        /// <param name="iDefault">默认值</param>
        /// <param name="sGroup">组</param>
        /// <param name="sConfigPath">配置文件地址</param>
        /// <returns></returns>
        public static float GetValue(string sKey, float fDefault, string sGroup = null, string sConfigPath = null)
        {
            float fRet;
            string sValue = GetValue(sKey, sGroup, sConfigPath);
            if (!String.IsNullOrWhiteSpace(sValue))
            {
                if (float.TryParse(sValue, out fRet))
                    return fRet;
            }

            return fDefault;
        }

        /// <summary>
        /// 设置配置文件的值(STRING)
        /// </summary>
        /// <param name="sKey">关键字</param>
        /// <param name="sValue">值</param>
        /// <param name="sGroup">组</param>
        /// <param name="sConfigPath">配置文件路径</param>
        public static void SetValue(string sKey, string sValue, string sGroup = null, string sConfigPath = null)
        {
            if (String.IsNullOrWhiteSpace(sKey))
                return;

            //设置配置名
            if (String.IsNullOrWhiteSpace(sConfigPath))
                sConfigPath = GetDefaultPathName();

            if (String.IsNullOrWhiteSpace(sGroup))
                sGroup = "";

            if (String.IsNullOrWhiteSpace(sValue))
                sValue = "";

            //写入值
            WritePrivateProfileString(sGroup, sKey, sValue, sConfigPath);
        }

        /// <summary>
        /// 设置配置文件的值(INT)
        /// </summary>
        /// <param name="sKey">关键字</param>
        /// <param name="sValue">值</param>
        /// <param name="sGroup">组</param>
        /// <param name="sConfigPath">配置文件路径</param>
        public static void SetValue(string sKey, int iValue, string sGroup = null, string sConfigPath = null)
        {
            SetValue(sKey, iValue.ToString(), sGroup, sConfigPath);
        }

        /// <summary>
        /// 设置配置文件的值(FLOAT)
        /// </summary>
        /// <param name="sKey">关键字</param>
        /// <param name="sValue">值</param>
        /// <param name="sGroup">组</param>
        /// <param name="sConfigPath">配置文件路径</param>
        public static void SetValue(string sKey, float fValue, string sConfigPath = null)
        {
            SetValue(sKey, fValue.ToString(), sConfigPath);
        }

        #endregion

        #region 工具
        /// <summary>
        /// 获取配置文件的默认路径文件名
        /// </summary>
        /// <returns></returns>
        private static string GetDefaultPathName()
        {
            int iIndex;
            string sPath = AIGS.Helper.SystemHelper.GetExeDirectoryName();
            string sExeName = AIGS.Helper.SystemHelper.GetExeNameWithoutExtension();

            //去除调试状态下的.vshost
            iIndex = sExeName.ToLower().IndexOf(".vshost");
            if (iIndex > 0)
                sExeName = sExeName.Substring(0, iIndex);

            string sRet = sPath + "\\" + sExeName + ".ini";
            return sRet;
        }


        #endregion
    }
}
