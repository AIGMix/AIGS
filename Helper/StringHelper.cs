using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AIGS.Helper
{
    public class StringHelper
    {
        /// <summary>
        /// 将多个数组组合到一起
        /// </summary>
        /// <param name="sArrary">数组集合</param>
        /// <returns></returns>
        public static string[] ArraryAppend(params string[][] sArrary)
        {
            if (sArrary == null)
                return null;

            List<string> aList = new List<string>();
            for(int i = 0; i < sArrary.Length; i++)
            {
                string[] sStr = sArrary[i];
                foreach (string sObj in sStr)
                    aList.Add(sObj);
            }

            return aList.ToArray();
        }

        /// <summary>
        /// 获取两个字符串开头的相同串
        /// </summary>
        /// <param name="sString1">字符串1</param>
        /// <param name="sString2">字符串2</param>
        /// <param name="bAaIsSame">区分大小写</param>
        /// <returns></returns>
        public static string FindPreSameString(string sString1, string sString2, bool bAaIsDifferent = true)
        {
            if (String.IsNullOrWhiteSpace(sString1) || String.IsNullOrWhiteSpace(sString2))
                return "";

            string sRet = null;
            for (int i = 0; i < sString1.Length && i < sString2.Length; i++)
            {
                if (!IsSameChar(sString1[i], sString2[i], bAaIsDifferent))
                    break;

                sRet += sString1[i];
            }

            return sRet;
        }


        /// <summary>
        /// 是否为相同的字符
        /// </summary>
        /// <param name="cChar1">字符1</param>
        /// <param name="cChar2">字符2</param>
        /// <param name="bAaIsDifferent">区分大小写</param>
        /// <returns></returns>
        public static bool IsSameChar(char cChar1, char cChar2, bool bAaIsDifferent = true)
        {
            if (bAaIsDifferent)
                return cChar1 == cChar2 ? true : false;
            else
                return Char.ToLower(cChar1) == Char.ToLower(cChar2) ? true : false;
        }

        /// <summary>
        /// 删除最后一行
        /// </summary>
        /// <param name="sString"></param>
        /// <returns></returns>
        public static string RemovTailLine(string sString)
        {
            if(String.IsNullOrWhiteSpace(sString))
                return "";

            int iIndex      = 0;
            string pRet     = "";
            string[] sPlit  = null;
            
            if((iIndex = sString.LastIndexOf('\n')) <= 0)
                return "";

            sPlit = sString.Split('\n');
            for (int i = 0; sPlit != null && i < sPlit.Count() - 1; i++)
            {
                if(i == 0)
                    pRet += sPlit[i]; 
                else
                    pRet += "\n" + sPlit[i]; 
            }
            return pRet;
        }


        /// <summary>
        /// 获取子串。
        /// </summary>
        /// <param name="sMsg">字符串</param>
        /// <param name="FindStr">寻找的Key,如果有等于号需要加上,如“key=”</param>
        /// <param name="EndChar">寻找的结束符号</param>
        public static string GetSubString(string sMsg, string FindStr, string EndChar)
        {
            string formt = FindStr + "(.*)";
            if (!string.IsNullOrWhiteSpace(EndChar))
                formt = FindStr + "(.*?)" + EndChar;

            Regex reg = new Regex(formt);
            Match aTmp = reg.Match(sMsg);
            if (string.IsNullOrWhiteSpace(aTmp.Value))
            {
                if (!string.IsNullOrWhiteSpace(EndChar))
                    return GetSubString(sMsg, FindStr, null);
                return null;
            }

            string sText = aTmp.Value;
            int iLen = sText.Length - FindStr.Length;
            iLen -= string.IsNullOrWhiteSpace(EndChar) ? 0 : EndChar.Length;
            sText = sText.Substring(FindStr.Length, iLen);

            return sText;
        }




        #region 删除重复字符
        /// <summary>
        /// 删除相邻的重复字符
        /// </summary>
        /// <param name="sString"></param>
        /// <param name="cChar"></param>
        /// <returns></returns>
        public static string RemoveAdjacentRepetitiveChar(string sString, char cChar)
        {
            if (String.IsNullOrWhiteSpace(sString))
                return "";

            char cCmpChr = '0';
            string sRet = "";
            for (int i = 0; i < sString.Length; i++)
            {
                if (i != 0 && cCmpChr == sString[i] && cCmpChr == cChar)
                    continue;

                cCmpChr = sString[0];
                sRet += sString[i];
            }

            return sRet;
        }

        /// <summary>
        /// 删除相邻的重复字符
        /// </summary>
        /// <param name="sString"></param>
        /// <returns></returns>
        public static string RemoveAdjacentRepetitiveChar(string sString)
        {
            if (String.IsNullOrWhiteSpace(sString))
                return "";

            char cCmpChr    = '0';
            string sRet     = "";
            for (int i = 0; i < sString.Length; i++ )
            {
                if(i != 0 && cCmpChr == sString[i])
                    continue;

                cCmpChr = sString[0];
                sRet    += sString[i];
            }

            return sRet;
        }
        #endregion
    }
}
