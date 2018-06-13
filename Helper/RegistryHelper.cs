using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AIGS.Helper
{
    /// <summary>
    /// 注册表操作
    /// </summary>
    public class RegistryHelper
    {

        public enum ROOT
        {
            CLASSES_ROOT,
            CURRENT_CONFIG,
            CURRENT_USER,
            DYN_DATA,
            LOCAL_MACHINE,
            USERS, 
        }

        #region 参数操作
        /// <summary>
        /// 获取全部参数
        /// </summary>
        /// <param name="sKeyPath">参数配置地址（相对路径，如“TMT\\Global”）</param>
        public static string[] GetParaNames(string sKeyPath, string sSubKey = "software\\Microsoft", ROOT aRoot = ROOT.CURRENT_USER)
        {
            RegistryKey aSoftwareKey = GetKeyByPath(sKeyPath, aRoot, sSubKey);
            if (aSoftwareKey == null)
                return null;
            
            return aSoftwareKey.GetValueNames();
        }

        /// <summary>
        /// 删除全部参数
        /// </summary>
        /// <param name="sKeyPath">参数配置地址（相对路径，如“TMT\\Global”）</param>
        public static void ClearParas(string sKeyPath, string sSubKey = "software\\Microsoft", ROOT aRoot = ROOT.CURRENT_USER)
        {
            RegistryKey aSoftwareKey = GetKeyByPath(sKeyPath, aRoot, sSubKey);
            if (aSoftwareKey == null)
                return;

            string[] pList = GetParaNames(sKeyPath);
            foreach(string sPara in pList)
            {
                aSoftwareKey.DeleteValue(sPara);
            }

            aSoftwareKey.Close();
        }

        /// <summary>
        /// 是否存在参数
        /// </summary>
        /// <param name="sKeyPath">参数配置地址（相对路径，如“TMT\\Global”）</param>
        /// <param name="sParaName">参数名</param>
        public static bool ExistPara(string sKeyPath, string sParaName, string sSubKey = "software\\Microsoft", ROOT aRoot = ROOT.CURRENT_USER)
        {
            RegistryKey aSoftwareKey = GetKeyByPath(sKeyPath, aRoot, sSubKey);
            if (aSoftwareKey == null)
                return false;

            string[] pList = GetParaNames(sKeyPath);
            foreach (string sPara in pList)
            {
                if (sParaName == sPara)
                    return true;
            }
            return false;
        }

        #endregion

        #region 获取参数值

        /// <summary>
        /// 获取参数值(STRING)
        /// </summary>
        /// <param name="sKeyPath">参数配置地址（相对路径，如“TMT\\Global”）</param>
        /// <param name="sParaName">参数名</param>
        public static string GetValue(string sKeyPath, string sParaName, string sSubKey = "software\\Microsoft", ROOT aRoot = ROOT.CURRENT_USER)
        {
            RegistryKey aSoftwareKey = GetKeyByPath(sKeyPath, aRoot, sSubKey);
            if (aSoftwareKey == null)
                return "";

            return aSoftwareKey.GetValue(sParaName, "").ToString();
        }

        /// <summary>
        /// 获取参数值(BOOL)
        /// </summary>
        /// <param name="sKeyPath">参数配置地址（相对路径，如“TMT\\Global”）</param>
        /// <param name="sParaName">参数名</param>
        public static bool GetValue(string sKeyPath, string sParaName, bool bDefault = false, string sSubKey = "software\\Microsoft", ROOT aRoot = ROOT.CURRENT_USER)
        {
            string sValue = GetValue(sKeyPath, sParaName, sSubKey, aRoot);
            if (!String.IsNullOrWhiteSpace(sValue))
            {
                bool bRet;
                if (bool.TryParse(sValue, out bRet))
                    return bRet;
            }
            return bDefault;
        }


        /// <summary>
        /// 获取参数值(INT)
        /// </summary>
        /// <param name="sKeyPath">参数配置地址（相对路径，如“TMT\\Global”）</param>
        /// <param name="sParaName">参数名</param>
        public static int GetValue(string sKeyPath, string sParaName, int iDefault = 0, string sSubKey = "software\\Microsoft", ROOT aRoot = ROOT.CURRENT_USER)
        {
            string sValue = GetValue(sKeyPath, sParaName, sSubKey, aRoot);
            if (!String.IsNullOrWhiteSpace(sValue))
            {
                int iRet;
                if (int.TryParse(sValue, out iRet))
                    return iRet;
            }
            return iDefault;
        }


        /// <summary>
        /// 获取参数值(FLOAT)
        /// </summary>
        /// <param name="sKeyPath">参数配置地址（相对路径，如“TMT\\Global”）</param>
        /// <param name="sParaName">参数名</param>
        public static float GetValue(string sKeyPath, string sParaName, float fDefault = 0, string sSubKey = "software\\Microsoft", ROOT aRoot = ROOT.CURRENT_USER)
        {
            string sValue = GetValue(sKeyPath, sParaName, sSubKey, aRoot);
            if (!String.IsNullOrWhiteSpace(sValue))
            {
                float fRet;
                if (float.TryParse(sValue, out fRet))
                    return fRet;
            }
            return fDefault;
        }

        #endregion

        #region 设置参数值
        /// <summary>
        /// 设置参数值
        /// </summary>
        /// <param name="sKeyPath">参数配置地址（相对路径，如“TMT\\Global”）</param>
        /// <param name="sParaName">参数名</param>
        /// <param name="oValue">值</param>
        /// <returns></returns>
        public static bool SetValue(string sKeyPath, string sParaName, object oValue)
        {
            if (String.IsNullOrWhiteSpace(sParaName))
                return false;

            RegistryKey aSoftwareKey = CreatKey(sKeyPath);
            if(aSoftwareKey != null)
            {
                aSoftwareKey.SetValue(sParaName, oValue);
            }
            aSoftwareKey.Close();
            return false;
        }

        #endregion

        #region Key（键值）操作
        /// <summary>
        /// 新建键值
        /// </summary>
        /// <param name="sKeyPath">参数配置地址（相对路径，如“TMT\\Global”）</param>
        /// <returns></returns>
        private static RegistryKey CreatKey(string sKeyPath, string sSubKey = "software\\Microsoft", ROOT aRoot = ROOT.CURRENT_USER)
        {
            if (String.IsNullOrWhiteSpace(sKeyPath))
                return null;

            RegistryKey aSoftwareKey = GetKeyByPath(sKeyPath, aRoot, sSubKey);
            if (aSoftwareKey == null)
            {
                RegistryKey aKey = GetRootKey(aRoot, sSubKey);
                return aKey.CreateSubKey(sKeyPath);
            }

            return aSoftwareKey;
        }

        #endregion

        #region 工具
        /// <summary>
        /// 获取句柄
        /// </summary>
        /// <param name="sKeyPath">参数配置地址（相对路径，如“TMT\\Global”）</param>
        private static RegistryKey GetKeyByPath(string sKeyPath, ROOT aRoot, string sSubKey)
        {
            if (String.IsNullOrWhiteSpace(sKeyPath))
                return null;

            sSubKey = sSubKey.Replace('/', '\\');
            sKeyPath = sKeyPath.Replace('/', '\\');

            RegistryKey aRootKey = GetRootKey(aRoot, sSubKey);
            RegistryKey aSoftwareKey = aRootKey.OpenSubKey(sKeyPath);
            return aSoftwareKey;
        }

        /// <summary>
        /// 获取根句柄
        /// </summary>
        /// <returns></returns>
        private static RegistryKey GetRootKey(ROOT aRoot, string sSubKey)
        {
            RegistryKey aRootKey = Registry.CurrentUser;
            switch(aRoot)
            {
                case ROOT.CLASSES_ROOT:     aRootKey = Registry.ClassesRoot;        break;
                case ROOT.CURRENT_CONFIG:   aRootKey = Registry.CurrentConfig;      break;
                case ROOT.CURRENT_USER:     aRootKey = Registry.CurrentUser;        break;
                case ROOT.DYN_DATA:         aRootKey = Registry.PerformanceData;    break;
                case ROOT.LOCAL_MACHINE:    aRootKey = Registry.LocalMachine;       break;
                case ROOT.USERS:            aRootKey = Registry.Users;              break;
            }

            if (String.IsNullOrWhiteSpace(sSubKey))
                return aRootKey;

            sSubKey = sSubKey.Replace('/', '\\');
            return aRootKey.OpenSubKey(sSubKey);
        }
        #endregion
    }
}
