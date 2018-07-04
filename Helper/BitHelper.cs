using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace AIGS.Helper
{
    public class BitHelper
    {
        #region 按bit位设置值

        /// <summary>
        /// 获取相应bit位的值
        /// </summary>
        /// <param name="iTotalValue">总值</param>
        /// <param name="iOffset">偏移</param>
        /// <param name="iLength">bit位数量</param>
        /// <returns></returns>
        public static uint get(uint iTotalValue, int iOffset, int iLength)
        {
            Contract.Requires(iOffset + iLength <= 32);
            uint uTemp = 0;
            for (int i = 0; i < iLength; i++)
            {
                uTemp |= (uint)(1 << i);
            }
            return (uint)((iTotalValue >> iOffset) & uTemp);
        }

        /// <summary>
        /// 设置相应bit位值
        /// </summary>
        /// <param name="iFieldValue">设置的值</param>
        /// <param name="iTotalValue">总值</param>
        /// <param name="iOffset">偏移</param>
        /// <param name="iLength">bit位数量</param>
        /// <returns></returns>
        public static uint set(uint iFieldValue, uint iTotalValue, int iOffset, int iLength)
        {
            Contract.Requires(iOffset + iLength <= 32);

            //获取设置的值到相应字段后的真实值
            uint uTemp = 0;
            for (int i = 0; i < iLength; i++)
            {
                uTemp |= (uint)(1 << i);
            }
            iFieldValue = (uint)(iFieldValue & uTemp) << iOffset;

            //将原始值的相应字段先进行清空
            uTemp = ~(uTemp << iOffset);
            iTotalValue = iTotalValue & uTemp;

            //或
            iTotalValue |= iFieldValue;
            return iTotalValue;
        }

        #endregion
    }



}
