using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;

namespace AIGS.Helper
{
    /*********************************************************************
    //C++结构
    struct AIGS_DATE
    {
        unsigned int Year:23;
        unsigned int Month:4;
        unsigned int Day:5;
    }
    //第一种C#相应结构
    public struct AIGS_DATE
    {
        private uint value0;
        public uint Year
        {
            get{return BitHelper.get(this.value0, 0, 23);}
            set{this.value0 = BitHelper.set(value, this.value0, 0 , 23);}
        }
        public uint Month
        {
            get{return BitHelper.get(this.value0, 23, 4);}
            set{this.value0 = BitHelper.set(value, this.value0, 23 , 4);}
        }
        public uint Day
        {
            get{return BitHelper.get(this.value0, 27, 5);}
            set{this.value0 = BitHelper.set(value, this.value0, 27, 5);}
        }
    }
    //第二种C#相应结构
    public struct AIGS_DATE2
    {
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 2, ArraySubType = UnmanagedType.U4)]
        public uint[] Value;

        [ByteAlignHelper.FieldInfo(23)]
        public uint Year
        {
            get { return ByteAlignHelper.get(this, "Year"); }
            set { Value = ByteAlignHelper.set(value, this, "Year"); }
        }
        [ByteAlignHelper.FieldInfo(4)]
        public uint Month
        {
            get { return ByteAlignHelper.get(this, "Month"); }
            set { Value = ByteAlignHelper.set(value, this, "Month"); }
        }
        [ByteAlignHelper.FieldInfo(5)]
        public uint Day
        {
            get { return ByteAlignHelper.get(this, "Day"); }
            set { Value = ByteAlignHelper.set(value, this, "Day"); }
        }
    }
    *********************************************************************/
    public class ByteAlignHelper
    {

        #region 声明字段的属性
        [global::System.AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
        public sealed class FieldInfoAttribute : Attribute
        {
            /// <summary>
            /// 字段bit数量
            /// </summary>
            int length;
            public FieldInfoAttribute(int length)
            {
                this.length = length;
            }
            public int Length { get { return length; } }
        }
        #endregion


        /// <summary>
        /// 获取值字段
        /// </summary>
        /// <param name="aThis"></param>
        /// <returns></returns>
        static FieldInfo GetValueField(object aThis)
        {
            try
            {
                Type aType = aThis.GetType();
                FieldInfo[] aArrary = aType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                foreach (FieldInfo aField in aArrary)
                {
                    if (aField.Name == "Value")
                    {
                        if (aField.FieldType != typeof(uint[]))
                            return null;

                        return aField;
                    }
                }
            }
            catch
            { }

            return null;
        }

        /// <summary>
        /// 获取相应值
        /// </summary>
        /// <param name="aThis"></param>
        /// <param name="iIndex"></param>
        /// <returns></returns>
        static uint GetValue(object aThis, int iIndex)
        {
            FieldInfo aFieldInfo = GetValueField(aThis);
            if (aFieldInfo == null)
                return 0;

            object aValue = aFieldInfo.GetValue(aThis);
            if (aValue == null)
                return 0;

            uint[] aValueList = (uint[])aValue;
            if (aValueList == null || aValueList.Count() <= iIndex)
                return 0;

            return aValueList[iIndex];
        }

        /// <summary>
        /// 设置相应值
        /// </summary>
        /// <param name="aThis"></param>
        /// <param name="iIndex"></param>
        /// <param name="iValue"></param>
        /// <returns></returns>
        static int SetValue(object aThis, int iIndex, uint iValue)
        {
            FieldInfo aFieldInfo = GetValueField(aThis);
            if (aFieldInfo == null)
                return 0;

            object aValue = aFieldInfo.GetValue(aThis);
            if (aValue == null)
                return 0;

            uint[] aValueList = (uint[])aValue;
            if (aValueList == null || iIndex >= aValueList.Count())
                return -1;

            aValueList[iIndex] = iValue;
            aFieldInfo.SetValue(aThis, aValueList);
            return 0;
        }

        /// <summary>
        /// 获取字段长度
        /// </summary>
        /// <param name="aInfo"></param>
        /// <returns></returns>
        static int GetFieldLength(PropertyInfo aInfo)
        {
            object[] aAttributes = aInfo.GetCustomAttributes(typeof(FieldInfoAttribute), false);
            if (aAttributes != null || aAttributes.Length == 1)
                return ((FieldInfoAttribute)aAttributes[0]).Length;

            return 0;
        }

        /// <summary>
        /// 获取字段参数
        /// </summary>
        /// <param name="aThis"></param>
        /// <param name="sFieldName"></param>
        /// <param name="out_Offset"></param>
        /// <param name="out_Length"></param>
        /// <param name="out_TotalValueIndex"></param>
        /// <returns></returns>
        static int GetFieldPara(object aThis, string sFieldName, ref int out_Offset, ref int out_Length, ref int out_TotalValueIndex)
        {
            try
            {
                int iOffset = 0;
                int iValueIndex = 0;
                Type aType = aThis.GetType();
                PropertyInfo[] aPropertyArrary = aType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                foreach (PropertyInfo aProperty in aPropertyArrary)
                {
                    ///获取字段长度，果超过了4字节则需要重置
                    int iLength = GetFieldLength(aProperty);
                    if (iOffset + iLength > 32)
                    {
                        iValueIndex++;
                        iOffset = 0;
                    }

                    if (aProperty.Name == sFieldName)
                    {
                        out_Offset = iOffset;
                        out_Length = iLength;
                        out_TotalValueIndex = iValueIndex;
                        break;
                    }
                    //bit位递增
                    iOffset += iLength;
                }
                return 0;
            }
            catch
            {
                return -1;
            }
        }

        /// <summary>
        /// 获取字段值
        /// </summary>
        /// <param name="iTotalValue">总值</param>
        /// <param name="aThis">结构句柄</param>
        /// <param name="sTotalValueName">总值的名称</param>
        /// <param name="sFieldName">字段名称</param>
        /// <returns></returns>
        public static uint get(object aThis, string sFieldName)
        {
            int iOffset = -1;
            int iLength = -1;
            int iValueIndex = 0;
            uint iValue = 0;
            int iRet = GetFieldPara(aThis, sFieldName, ref iOffset, ref iLength, ref iValueIndex);
            if (iRet != 0)
                return iValue;

            iValue = GetValue(aThis, iValueIndex);
            return AIGS.Helper.BitHelper.get(iValue, iOffset, iLength);
        }

        /// <summary>
        /// 设置字段值
        /// </summary>
        /// <param name="iFieldValue">字段值</param>
        /// <param name="iTotalValue">总值</param>
        /// <param name="aThis">结构句柄</param>
        /// <param name="sTotalValueName">总值名称</param>
        /// <param name="sFieldName">字段名称</param>
        /// <returns></returns>
        public static int set(uint iFieldValue, object aThis, string sFieldName)
        {
            int iOffset = -1;
            int iLength = -1;
            int iValueIndex = 0;
            uint iValue = 0;
            int iRet = GetFieldPara(aThis, sFieldName, ref iOffset, ref iLength, ref iValueIndex);
            if (iRet != 0)
                return -1;

            iValue = GetValue(aThis, iValueIndex);
            iValue = AIGS.Helper.BitHelper.set(iFieldValue, iValue, iOffset, iLength);
            return SetValue(aThis, iValueIndex, iValue);
        }

    }
}
