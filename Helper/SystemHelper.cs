using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Management;
using System.Net;
using System.Windows.Forms; 

namespace AIGS.Helper
{
    public class SystemHelper
    {
        #region 获取当前可执行程序信息
        /// <summary>
        /// 获取当前可执行程序路径文件名
        /// </summary>
        /// <returns></returns>
        public static string GetExePathName()
        {
            return Application.ExecutablePath;
        }

        /// <summary>
        /// 获取当前可执行程序文件名
        /// </summary>
        /// <returns></returns>
        public static string GetExeName()
        {
            string sStr = GetExePathName();
            return Path.GetFileName(sStr);
        }

        /// <summary>
        /// 获取当前可执行程序文件名(不带扩展名)
        /// </summary>
        /// <returns></returns>
        public static string GetExeNameWithoutExtension()
        {
            string sStr = GetExePathName();
            return Path.GetFileNameWithoutExtension(sStr);
        }

        /// <summary>
        /// 获取当前可执行程序路径名
        /// </summary>
        /// <returns></returns>
        public static string GetExeDirectoryName()
        {
            string sStr = GetExePathName();
            return Path.GetDirectoryName(sStr);
        }

        public static string GetWorkDirectory()
        {
            return System.Environment.CurrentDirectory;
        }
        #endregion

        #region 用户相关信息
        /// <summary>
        /// 用户的相关路径
        /// </summary>
        public struct UserFolders
        {
            public string DesktopPath;      // Windows用户桌面路径 
            public string FontsPath;        // Windows用户字体目录路径 
            public string NethoodPath;      // Windows用户网络邻居路径
            public string PersonalPath;     // Windows用户我的文档路径
            public string ProgramsPath;     // Windows用户开始菜单程序路径
            public string RecentPath;       // Windows用户存放用户最近访问文档快捷方式的目录路径  
            public string SendToPath;       // Windows用户发送到目录路径  
            public string StartMenuPath;    // Windows用户开始菜单目录路径  
            public string StartUpPath;      // Windows用户开始菜单启动项目录路径 
            public string FavoritesPath;    // Windows用户收藏夹目录路径  
            public string HistoryPath;      // Windows用户网页历史目录路径  
            public string CookiesPath;      // Windows用户Cookies目录路径 
            public string CachePath;        // Windows用户Cache目录路径  
            public string AppdataPath;      // Windows用户应用程式数据目录路径 
            public string PrinthoodPath;    // Windows用户打印目录路径    
        }

        /// <summary>
        /// 获取用户的各种路径
        /// </summary>
        /// <returns></returns>
        public static UserFolders GetUserFolders()
        {
            UserFolders pRet = new UserFolders();

            string sSubKey  = "software\\Microsoft";
            string sUserFolderPath = "windows/currentversion/explorer/shell folders";

            pRet.DesktopPath    = AIGS.Helper.RegistryHelper.GetValue(sUserFolderPath, "Common Desktop",    sSubKey, AIGS.Helper.RegistryHelper.ROOT.LOCAL_MACHINE);
            pRet.FontsPath      = AIGS.Helper.RegistryHelper.GetValue(sUserFolderPath, "Fonts",             sSubKey, AIGS.Helper.RegistryHelper.ROOT.LOCAL_MACHINE);
            pRet.NethoodPath    = AIGS.Helper.RegistryHelper.GetValue(sUserFolderPath, "Nethood",           sSubKey, AIGS.Helper.RegistryHelper.ROOT.LOCAL_MACHINE);
            pRet.PersonalPath   = AIGS.Helper.RegistryHelper.GetValue(sUserFolderPath, "Personal",          sSubKey, AIGS.Helper.RegistryHelper.ROOT.LOCAL_MACHINE);
            pRet.ProgramsPath   = AIGS.Helper.RegistryHelper.GetValue(sUserFolderPath, "Programs",          sSubKey, AIGS.Helper.RegistryHelper.ROOT.LOCAL_MACHINE);
            pRet.RecentPath     = AIGS.Helper.RegistryHelper.GetValue(sUserFolderPath, "Recent",            sSubKey, AIGS.Helper.RegistryHelper.ROOT.LOCAL_MACHINE);
            pRet.SendToPath     = AIGS.Helper.RegistryHelper.GetValue(sUserFolderPath, "Sendto",            sSubKey, AIGS.Helper.RegistryHelper.ROOT.LOCAL_MACHINE);
            pRet.StartMenuPath  = AIGS.Helper.RegistryHelper.GetValue(sUserFolderPath, "Startmenu",         sSubKey, AIGS.Helper.RegistryHelper.ROOT.LOCAL_MACHINE);
            pRet.StartUpPath    = AIGS.Helper.RegistryHelper.GetValue(sUserFolderPath, "Startup",           sSubKey, AIGS.Helper.RegistryHelper.ROOT.LOCAL_MACHINE);
            pRet.FavoritesPath  = AIGS.Helper.RegistryHelper.GetValue(sUserFolderPath, "Favorites",         sSubKey, AIGS.Helper.RegistryHelper.ROOT.LOCAL_MACHINE);
            pRet.HistoryPath    = AIGS.Helper.RegistryHelper.GetValue(sUserFolderPath, "History",           sSubKey, AIGS.Helper.RegistryHelper.ROOT.LOCAL_MACHINE);
            pRet.CookiesPath    = AIGS.Helper.RegistryHelper.GetValue(sUserFolderPath, "Cookies",           sSubKey, AIGS.Helper.RegistryHelper.ROOT.LOCAL_MACHINE);
            pRet.CachePath      = AIGS.Helper.RegistryHelper.GetValue(sUserFolderPath, "Cache",             sSubKey, AIGS.Helper.RegistryHelper.ROOT.LOCAL_MACHINE);
            pRet.AppdataPath    = AIGS.Helper.RegistryHelper.GetValue(sUserFolderPath, "Appdata",           sSubKey, AIGS.Helper.RegistryHelper.ROOT.LOCAL_MACHINE);
            pRet.PrinthoodPath  = AIGS.Helper.RegistryHelper.GetValue(sUserFolderPath, "Printhood",         sSubKey, AIGS.Helper.RegistryHelper.ROOT.LOCAL_MACHINE);
            return pRet;
        }
        #endregion

        #region 系统环境变量

        /// <summary>
        /// 获取环境变量
        /// </summary>
        /// <param name="sVariableName">变量名</param>
        public static string[] GetEnvironmentVariable(string sVariableName)
        {
            string sText = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine);
            if (String.IsNullOrWhiteSpace(sText))
                return null;

            string[] pRet = sText.Split(';');
            return pRet;
        }

        /// <summary>
        /// 设置环境变量
        /// </summary>
        /// <param name="sVariableName">变量名</param>
        /// <param name="pValueList">值列表</param>
        /// <returns></returns>
        public static bool SetEnvironmentVariable(string sVariableName, string[] pValueList)
        {
            if (pValueList == null)
                return false;

            string sText = "";
            for (int i = 0; i < pValueList.Count(); i++ )
            {
                if (String.IsNullOrWhiteSpace(pValueList[i]))
                    continue;

                sText += pValueList[i] + ';';
            }

            Environment.SetEnvironmentVariable(sVariableName, sText, EnvironmentVariableTarget.Machine);
            return true;
        }

        #endregion

        #region 查询窗口句柄(查询程序是否已经有运行)

        /// <summary>
        /// 根据类名和窗口名来得到窗口句柄
        /// </summary>
        /// <param name="lpClassName"></param>
        /// <param name="lpWindowName"></param>
        /// <returns></returns>
        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("User32.dll", EntryPoint = "FindWindowEx")]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpClassName, string lpWindowName);  



        /// <summary>
        /// 查询Window程序是否存在
        /// </summary>
        /// <param name="sFormText">Window窗口名</param>
        /// <param name="aExceptForm">排除的窗口句柄</param>
        public static bool IsWindowExist(string sFormText, Form aExceptForm = null)
        {
            IntPtr pHandle = FindWindow(null, sFormText);

            //如果没有此窗口则返回false
            if(pHandle == IntPtr.Zero)
                return false;
            //如果有此窗口并且窗口是排除的窗口则返回false(比如查到了自己-this)
            else if (aExceptForm != null)
            {
                if (aExceptForm.Handle == pHandle)
                    return false;
            }

            return true;
        }
        #endregion

        #region 获取本机信息
        /// <summary>  
        /// 操作系统的登录用户名  
        /// </summary>  
        /// <returns>系统的登录用户名</returns>  
        public static string GetUserName()
        {
            try
            {
                string strUserName = string.Empty;
                ManagementClass mc = new ManagementClass("Win32_ComputerSystem");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    strUserName = mo["UserName"].ToString();
                }
                return strUserName;
            }
            catch
            {
                return "unknown";
            }
        }

        /// <summary>  
        /// 获取本机MAC地址  
        /// </summary>  
        /// <returns>本机MAC地址</returns>  
        public static string GetMacAddress()
        {
            try
            {
                string strMac = string.Empty;
                ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    if ((bool)mo["IPEnabled"] == true)
                    {
                        strMac = mo["MacAddress"].ToString();
                        strMac = strMac.Replace(':', '-');
                        break;
                    }
                }

                return strMac;
            }
            catch
            {
                return "unknown";
            }
        }

        /// <summary>  
        /// 获取客户端内网IPv4地址  
        /// </summary>  
        /// <returns>客户端内网IPv4地址</returns>  
        public static string GetClientLocalIPv4Address()
        {
            string strLocalIP = string.Empty;
            try
            {
                IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress ipAddress = ipHost.AddressList[0];
                strLocalIP = ipAddress.ToString();
                return strLocalIP;
            }
            catch
            {
                return "unknown";
            }
        }  


        #endregion

        #region 剪贴板操作

        /// <summary>
        /// 设置剪贴板内容
        /// </summary>
        /// <param name="aData"></param>
        /// <returns></returns>
        public static void SetClipBoardData(object aData)
        {
            Clipboard.SetDataObject(aData, true);
        }

        /// <summary>
        /// 剪贴板是否为空
        /// </summary>
        /// <returns></returns>
        public static bool IsClipBoardEmpty()
        {
            if (Clipboard.GetDataObject().GetFormats().Length == 0)
                return true;
            return false;
        }

        /// <summary>
        /// 获取剪贴板内容
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetClipBoardData<T>()
        {
            if (IsClipBoardEmpty())
                return default(T);

            IDataObject oData = Clipboard.GetDataObject();
            if (oData.GetDataPresent(typeof(T)))
                return (T)oData.GetData(typeof(T));

            return default(T);
        }

        #endregion

        /// <summary>
        /// 设置开机启动项
        /// </summary>
        /// <param name="sName">项名</param>
        /// <param name="bFlag">是否开机启动</param>
        /// <param name="sExePath">执行程序地址</param>
        public static void SetPowerBoot(string sName, bool bFlag, string sExePath = null)
        {
            RegistryKey HKCU = Registry.CurrentUser;
            RegistryKey REG = HKCU.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");

            if (bFlag)
            {
                if (String.IsNullOrWhiteSpace(sExePath))
                    sExePath = GetExePathName();
                else
                    sExePath = Path.GetFullPath(sExePath);

                REG.SetValue(sName, sExePath);
            }
            else
                REG.DeleteValue(sName, false);

            HKCU.Close();
        }

        /// <summary>
        /// 获取系统库的路径
        /// </summary>
        /// <returns></returns>
        public static string GetSystemDllPath()
        {
            if (Environment.Is64BitOperatingSystem)
                return @"C:\Windows\SysWOW64";
            else
                return @"C:\Windows\System32";
        }

        /// <summary>
        /// 判断当前登录用户是否为管理员
        /// </summary>
        public static bool IsRootUser()
        {
            Application.EnableVisualStyles();
            //获得当前登录的Windows用户标示
            System.Security.Principal.WindowsIdentity identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            System.Security.Principal.WindowsPrincipal principal = new System.Security.Principal.WindowsPrincipal(identity);

            return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
        }

    }
}
