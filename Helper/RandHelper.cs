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
using System.Linq;
using System.Text;

namespace AIGS.Helper
{
    public class RandHelper
    {

        /// <summary>
        /// 生成随机字符串（INT）
        /// </summary>
        /// <param name="iLength">字符串长度</param>
        /// <returns></returns>
        public static string GetIntRandom(int iLength, int iBig = -1, int iSmall = -1)
        {
            string sRet = "";
            if (iSmall > 0 && iBig > 0 && iBig <= iSmall)
                return sRet;

            if (iBig < 0)
            {
                iSmall = 0;
                iBig = 10;
            }
            
            System.Random aRecord = new Random();
            for (int i = 0; i < iLength; i++)
            {
                sRet += aRecord.Next(iSmall, iBig).ToString();
            }
            return sRet;
        }

        /// <summary>
        /// 生成随机字符串（STRING）
        /// </summary>
        /// <param name="iLength">字符串长度</param>
        /// <returns></returns>
        public static string GetStringRandom(int iLength)
        {
            string sRet = "";
            char[] Pattern = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };
            System.Random random = new Random(~unchecked((int)DateTime.Now.Ticks));
            for (int i = 0; i < iLength; i++)
            {
                int rnd = random.Next(0, Pattern.Length);
                sRet += Pattern[rnd];
            }
            return sRet;
        }

        /// <summary>
        /// 生成随机字符串（INT+STRING）
        /// </summary>
        /// <param name="iLength">字符串长度</param>
        /// <returns></returns>
        public static string GetIntStringRandom(int iLength)
        {
            string sRet = "";
            char[] Pattern = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };
            System.Random random = new Random(~unchecked((int)DateTime.Now.Ticks));
            for (int i = 0; i < iLength; i++)
            {
                int rnd = random.Next(0, Pattern.Length);
                sRet += Pattern[rnd];
            }
            return sRet;
        }


    }
}
