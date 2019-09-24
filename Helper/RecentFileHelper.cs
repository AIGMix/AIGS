using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIGS.Helper
{
    public class RecentFileHelper
    {
        #region 宏
        /// <summary>
        /// 配置文件中文件数量的标志
        /// </summary>
        private const string SIGN_FILE_NUM = "RECENT_FILES_NUM";

        /// <summary>
        /// 配置文件中文件路径的前缀标志
        /// </summary>
        private const string SIGN_FILE = "RECENT_FILES";

        /// <summary>
        /// 配置文件中的最近打开的文件的组名
        /// </summary>
        private const string SIGN_GROUP = "RECENT_FILES";

        /// <summary>
        /// 默认的最大数量
        /// </summary>
        private const int SIGN_MAX_PATH = 30;
        #endregion

        /// <summary>
        /// 获取最近打开的文件的全部路径
        /// </summary>
        /// <param name="sConfigPath"></param>
        public static List<string> GetPaths(string sConfigPath = null)
        {
            //获取最近打开的文件的数量
            int iNum = ConfigHelper.GetValue(SIGN_FILE_NUM, 0, SIGN_GROUP, sConfigPath);
            if (iNum <= 0)
                return new List<string>();

            //获取各个文件路径
            List<string> pList = new List<string>();
            for (int i = 0; i < iNum; i++)
            {
                string sPath = ConfigHelper.GetValue(SIGN_FILE + i, null, SIGN_GROUP, sConfigPath);
                if (String.IsNullOrWhiteSpace(sPath))
                    continue;

                if(pList.Find(s => s.Equals(sPath)) == null)
                    pList.Add(sPath);
            }
            return pList;

        }



        /// <summary>
        /// 清空最近打开文件的数量
        /// </summary>
        /// <param name="sConfigPath"></param>
        public static void ClearPaths(string sConfigPath = null)
        {
            ConfigHelper.SetValue(SIGN_FILE_NUM, 0, SIGN_GROUP, sConfigPath);
        }




        /// <summary>
        /// 添加一个最近打开文件的路径
        /// </summary>
        /// <param name="sPath">路径</param>
        /// <param name="iMaxPathNum">路径数量的最大值</param>
        /// <param name="sConfigPath">配置文件路径</param>
        public static void AddPath(string sPath, int iMaxPathNum = SIGN_MAX_PATH, string sConfigPath = null)
        {
            //获取全部路径
            List<string> pList = GetPaths(sConfigPath);

            if (iMaxPathNum <= 0)
                iMaxPathNum = SIGN_MAX_PATH;

            //将新增的路径放到最前面
            pList.Remove(sPath);
            pList.Insert(0, sPath);

            //设置到配置文件中
            int iNum = pList.Count();
            ConfigHelper.SetValue(SIGN_FILE_NUM, iNum, SIGN_GROUP, sConfigPath);
            for (int i = 0; i < iNum && i < iMaxPathNum; i++)
            {
                ConfigHelper.SetValue(SIGN_FILE + i, pList[i], SIGN_GROUP, sConfigPath);
            }
        }
    }
}
