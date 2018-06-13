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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AIGS.Helper
{
    /// <summary>
    /// 版本号
    /// </summary>
    /// 说明： 0.0.0.0  主版本号(Major).次版本号(Minor).修正版本号(Build Number).编译版本号(Revision)
    //
    //    1)、INT和STRING互转
    //      a\int最大值2 1 47 48 3647
    //      b\4位的版本号规定最大的值为 9.99.99.9999
    //      c\3位的版本号规定最大的值为 99.999.9999
    //      e\2位的版本号规定最大的值为 999.9999
    //      d\1位的版本号规定最大的值为 999 999 999
    public class VersionHelper
    {
        #region 工具
        /// <summary>
        /// 获取段的位数（十进制）
        /// </summary>
        /// <param name="iPartNum">段的数量（1\2\3\4）</param>
        /// <param name="iIndex">段序号</param>
        /// <returns></returns>
        private static int GetPartBitNum(int iPartNum, int iIndex)
        {
            if (iIndex >= iPartNum)
                return -1;

            switch(iPartNum)
            {
                case 1:
                    return 9;
                case 2:
                    if (iIndex == 0) return 3;
                    if (iIndex == 1) return 4;
                    break;
                case 3:
                    if (iIndex == 0) return 2;
                    if (iIndex == 1) return 3;
                    if (iIndex == 2) return 4;
                    break;
                case 4:
                    if (iIndex == 0) return 1;
                    if (iIndex == 1) return 2;
                    if (iIndex == 2) return 2;
                    if (iIndex == 3) return 4;
                    break;
            }

            return -1;
        }

        /// <summary>
        /// 位数转整型（十进制。如4位则转成9999）
        /// </summary>
        /// <returns></returns>
        private static int BitNumToInt(int iBitNum)
        {
            if (iBitNum <= 0)
                return -1;

            int iRet = 0;
            for(int i = 0; i< iBitNum; i++)
            {
                iRet += 9 * (int)System.Math.Pow(10, i);
            }

            return iRet;
        }

        #endregion

        #region 查看是否可以转换
        /// <summary>
        /// 查看是否可以转换成String
        /// </summary>
        /// <param name="iVersion">版本号</param>
        /// <param name="iPartNum">段数</param>
        /// <returns></returns>
        public static bool CanConverToString(int iVersion, int iPartNum = 4)
        {
            if (iVersion < 0 || iPartNum < 1 || iPartNum > 4)
                return false;

            //获取最大值
            int iBitSum = 0;
            int iMaxSum = 0;
            for (int i = iPartNum - 1; i >= 0; i--)
            {
                int iCount = GetPartBitNum(iPartNum, i);
                iMaxSum += BitNumToInt(iCount) * (int)System.Math.Pow(10, iBitSum);

                iBitSum += iCount;
            }

            if (iMaxSum >= iVersion)
                return true;

            return false;
        }

        /// <summary>
        /// 查看是否可以转换成Int
        /// </summary>
        /// <param name="sVersion">版本号</param>
        public static bool CanConverToInt(string sVersion)
        {
            int[] iParts = ParseVersion(sVersion);
            if (iParts == null)
                return false;

            int iBitNum, iMaxNum;
            int iPartNum = iParts.Count();
            //查看每一段的值是否超过最大值
            for (int i = 0; i < iPartNum; i++)
            {
                iBitNum = GetPartBitNum(iPartNum, i);
                iMaxNum = BitNumToInt(iBitNum);
                if (iParts[i] > iMaxNum)
                    return false;
            }

            return true;
        }

        #endregion

        /// <summary>
        /// 解析版本号
        /// </summary>
        /// <param name="sVersion">版本号</param>
        public static int[] ParseVersion(string sVersion)
        {
            if(String.IsNullOrWhiteSpace(sVersion))
                return null;

            string[] sParts = sVersion.Split('.');
            if(sParts.Count() > 4 || sParts.Count() <= 0)
                return null;

            int[] iRet = new int[sParts.Count()];
            for(int i = 0; i < sParts.Count(); i++)
            {
                if(!int.TryParse(sParts[i], out iRet[i]))
                    return null;
            }

            return iRet;
        }

        /// <summary>
        /// 版本号补全（往后补0，如1.1->1.1.0.0）
        /// </summary>
        /// <param name="sVersion"></param>
        /// <param name="iPartNum"></param>
        public static string Fill(string sVersion, int iPartNum)
        {
            int[] iParts = ParseVersion(sVersion);
            if (iParts == null)
                return "";

            if (iParts.Count() >= iPartNum)
                return sVersion;

            string sRet = sVersion;
            for(int i = iParts.Count(); i < iPartNum; i++)
            {
                sRet = ".0";
            }

            return sRet;
        }

        /// <summary>
        /// 字符串转整型
        /// </summary>
        /// <param name="sVersion">版本号</param>
        public static int ConverStringToInt(string sVersion)
        {
            if (!CanConverToInt(sVersion))
                return -1;

            int[] iParts = ParseVersion(sVersion);
            int iPartNum = iParts.Count();

            int iBitSum = 0;
            int iRet = 0;
            for (int i = iPartNum - 1; i >= 0; i--)
            {
                int iCount = GetPartBitNum(iPartNum, i);
                iRet += iParts[i] * (int)System.Math.Pow(10, iBitSum);

                iBitSum += iCount;
            }

            return iRet;
        }

        /// <summary>
        /// 整型转字符串
        /// </summary>
        /// <param name="iVersion">版本号</param>
        /// <param name="iPartNum">段数</param>
        /// <returns></returns>
        public static string ConverIntToString(int iVersion, int iPartNum = 4)
        {
            if (!CanConverToString(iVersion, iPartNum))
                return "";

            //获取位数偏移
            int iCount = 0;
            int[] iBitSum = new int[iPartNum];
            for (int i = iPartNum - 1; i >= 0; i--)
            {
                iBitSum[i] = iCount;
                iCount += GetPartBitNum(iPartNum, i);
            }

            //获取每段的数值
            int[] iParts = new int[iPartNum];
            for (int i = 0; i < iPartNum; i++)
            {
                int iNum = iVersion;
                for(int j = 0; j < i; j++)
                    iNum -= iParts[j] * (int)System.Math.Pow(10, iBitSum[j]);

                iParts[i] = iNum / (int)System.Math.Pow(10, iBitSum[i]);
            }

            string sRet = "";
            for(int i = 0; i <iPartNum;i++)
            {
                sRet += i == 0 ? iParts[i].ToString() : '.' + iParts[i].ToString();
            }

            return sRet;
        }

        /// <summary>
        /// 获取文件版本号
        /// </summary>
        /// <param name="sFilePath">文件路径</param>
        public static string GetFileVersion(string sFilePath)
        {
            //查看文件是否存在
            if (!File.Exists(sFilePath))
                return "";

            //版本号
            System.Diagnostics.FileVersionInfo aVersion = System.Diagnostics.FileVersionInfo.GetVersionInfo(sFilePath);
            return aVersion.FileVersion;
        }

        /// <summary>
        /// 获取程序自身版本号
        /// </summary>
        /// <returns></returns>
        public static string GetSelfVersion()
        {
            string sPath = AIGS.Helper.SystemHelper.GetExeDirectoryName();
            string sName = AIGS.Helper.SystemHelper.GetExeName();

            //去除调试状态下的.vshost
            int iIndex = sName.ToLower().IndexOf(".vshost");
            if (iIndex > 0)
                sName = sName.Substring(0, iIndex) + ".exe";

            return GetFileVersion(sPath + '/' + sName);
        }

        /// <summary>
        /// 获取正确的版本号字符串（去除空格换行符等）
        /// </summary>
        /// <param name="sStr"></param>
        /// <returns></returns>
        public static string GetCorrectString(string sStr)
        {
            string sRet = "";
            foreach (char c in sStr)
            {
                if ((Convert.ToInt32(c) >= 48 && Convert.ToInt32(c) <= 57) || c == '.')
                    sRet += c;
            }
            return sRet;
        }

        /// <summary>
        /// 比较两个版本的大小
        /// </summary>
        /// <param name="sVersion1"></param>
        /// <param name="sVersion2"></param>
        /// <returns>1：sVersion1版本高  -1：sVersion2版本高  0：版本一致</returns>
        public static int Compare(string sVersion1, string sVersion2)
        {
            string sV1 = Fill(sVersion1, 4);
            string sV2 = Fill(sVersion2, 4);

            if (String.IsNullOrWhiteSpace(sV1) || String.IsNullOrWhiteSpace(sV2))
                return 0;

            int[] iV1 = ParseVersion(sVersion1);
            int[] iV2 = ParseVersion(sVersion2);

            for(int i = 0; i < 4; i++)
            {
                if (iV1[i] > iV2[i])
                    return 1;
                if (iV1[i] < iV2[i])
                    return -1;
            }

            return 0;
        }

    }
}
