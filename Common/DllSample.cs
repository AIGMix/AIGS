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
using System.Runtime.InteropServices;

namespace AIGS.Common
{
    /// <summary>
    /// C++的结构体转C#结构体
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct StructSample
    {
        //普通的32位整数
        public UInt32 X;            
        public UInt32 Y;

        //1字节无符号整数数组
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 2, ArraySubType = UnmanagedType.U1)]
        public byte[] Reserve;    
  
        //short型定长数组
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst=4, ArraySubType = UnmanagedType.I2)]
        public short[] AdjCellAccFlag;

        //指针
        public IntPtr pPtr;

        //固定大小的字符串
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string sString;  
    }

    /// <summary>
    /// 回调函数定义
    /// </summary>
    public delegate void CallBackSample(string strFormat, params object[] pars);

    public class DllSample
    {
        /// <summary>
        /// 动态库函数声明
        /// </summary>
        /// <Param name="intHandle">指针</Param>
        /// <Param name="objs">结构体数组</Param>
        /// <Param name="iObjNum">数组大小</Param>
        /// <Param name="value">输出字符串 申请方式:StringBuilder Value = new StringBuilder(200);</Param>
        /// <Param name="valueLength">字符串大小</Param>
        /// <Param name="pCallBack">回调函数</Param>
        [System.Runtime.InteropServices.DllImport("TEST_AIGS.dll", EntryPoint = "Test", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Test(ref IntPtr intHandle,
                                        StructSample[] objs,
                                        int iObjNum,
                                        StringBuilder value, 
                                        int valueLength,
                                        CallBackSample pCallBack);

       
    }
}
