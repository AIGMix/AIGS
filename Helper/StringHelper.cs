using System;
using System.Collections.Generic;
using System.Linq;

namespace AIGS.Helper
{
    public class StringHelper
    {
        /// <summary>
        /// 是否为纯中文
        /// </summary>
        /// <param name="sStr"></param>
        /// <returns></returns>
        public static bool IsChinese(string sStr)
        {
            //在 ASCII码表中，英文的范围是0-127，而汉字则是大于127
            string text = sStr;
            for (int i = 0; i < text.Length; i++)
            {
                if ((int)text[i] <= 127)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 是否为纯英文
        /// </summary>
        /// <param name="sStr"></param>
        /// <returns></returns>
        public static bool IsEnglisth(string sStr)
        {
            string text = sStr;
            for (int i = 0; i < text.Length; i++)
            {
                if ((int)text[i] > 127) //由于英文的范围只有在 0-127，所以大于127的为汉子
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 是否为数字
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsNumeric(string str)
        {
            char[] ch = new char[str.Length];
            ch = str.ToCharArray();
            for (int i = 0; i < ch.Count(); i++)
            {
                if (ch[i] < 48 || ch[i] > 57)
                    return false;
            }
            return true;
        }

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
            if (string.IsNullOrEmpty(sMsg))
                return null;
            if (string.IsNullOrEmpty(FindStr) && string.IsNullOrEmpty(EndChar))
                return sMsg;

            if(string.IsNullOrEmpty(FindStr))
            {
                int iEndIdx = sMsg.IndexOf(EndChar);
                if (iEndIdx < 0)
                    return null;

                return sMsg.Substring(0, iEndIdx);
            }
            else if (string.IsNullOrEmpty(EndChar))
            {
                int iFindIdx = sMsg.IndexOf(FindStr);
                if (iFindIdx < 0)
                    return null;

                return sMsg.Substring(iFindIdx + FindStr.Length);
            }
            else
            {
                int iFindIdx = sMsg.IndexOf(FindStr);
                if (iFindIdx < 0)
                    return null;

                int iEndIdx = sMsg.IndexOf(EndChar, iFindIdx + FindStr.Length);
                if (iFindIdx < 0)
                    return null;

                if (iEndIdx < 0)
                    return sMsg.Substring(iFindIdx + FindStr.Length);
                return sMsg.Substring(iFindIdx + FindStr.Length, iEndIdx - iFindIdx - FindStr.Length);
            }
        }

        /// <summary>
        /// 计算子串数量
        /// </summary>
        /// <param name="sStr"></param>
        /// <param name="sSubStr"></param>
        /// <returns></returns>
        public static int CountSubString(string sStr, string sSubStr)
        {
            int iCount  = 0;
            string sTmp = sStr;
            while(true)
            {
                int iIndex = sTmp.IndexOf(sSubStr);
                if (iIndex < 0)
                    break;

                iCount++;
                sTmp = sTmp.Substring(iIndex + sSubStr.Length);
                if (string.IsNullOrEmpty(sTmp))
                    break;
            }

            return iCount;
        }

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
                sRet += sString[i];
            }

            return sRet;
        }
    }
}
