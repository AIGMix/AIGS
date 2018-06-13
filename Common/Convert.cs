using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using System.IO;

namespace AIGS.Common
{
    public class Convert
    {
        #region IntPtr与其他类型的转换

        #region struct<->IntPtr

        /// <summary>
        /// 功能描述：根据指针和需要解析的结构体的数量，解析数据指针里面对应的结构体数据
        /// </summary>
        /// <typeparam name="T">传入结构体类型</typeparam>
        /// <param name="in_pStructure">结构体对应指针</param>
        /// <param name="iNum">指针中包含结构体数据个数</param>
        /// <returns>获取指定个数结构体信息</returns>
        public static T[] ConvertIntPtrToStructInfo<T>(IntPtr in_pStructure, int iNum)
        {
            try
            {
                IntPtr ipOneOfT;
                T[] aRetStructs = new T[iNum];
                if (iNum <= 0)
                {
                    return aRetStructs;
                }
                int iSizeOfStruct = Marshal.SizeOf(typeof(T));
                byte[] byBuffer = new byte[iNum * iSizeOfStruct];
                Marshal.Copy(in_pStructure, byBuffer, 0, byBuffer.Length);
                for (int i = 0; i < iNum; i++)
                {
                    ipOneOfT = System.Runtime.InteropServices.Marshal.AllocHGlobal(iSizeOfStruct);
                    System.Runtime.InteropServices.Marshal.Copy(byBuffer, i * iSizeOfStruct, ipOneOfT, iSizeOfStruct);
                    aRetStructs[i] = (T)Marshal.PtrToStructure(ipOneOfT, typeof(T));
                    System.Runtime.InteropServices.Marshal.FreeHGlobal(ipOneOfT);
                }
                return aRetStructs;
            }
            catch (Exception exc)
            {
                throw exc;
            }
        }
        public static T ConvertIntPtrToStructInfo<T>(IntPtr in_pStructure)
        {
            T[] pRet = ConvertIntPtrToStructInfo<T>(in_pStructure, 1);
            return pRet[0];
        }




        /// <summary>
        ///功能描述:将c#里面的托管的结构体转换为非托管的结构体，以指针的形式传入到动态库中调用
        /// <typeparam name="T">模板类型参数</typeparam>
        /// <param name="aStructures">指定对象数据结构体实例</param>
        /// <param name="iNum">需要转化为非托管的对象个数</param>
        /// <returns>返回非托管结构体的首地址指针</returns>
        public static IntPtr ConvertManageStructToIntPtr<T>(IEnumerable<T> aStructures, int iNum)
        {
            try
            {
                IntPtr intPtr;
                int i = 0;
                int intSize = Marshal.SizeOf(typeof(T));
                intPtr = Marshal.AllocHGlobal(intSize * iNum);
                foreach (T var in aStructures)
                {
                    IntPtr newPtr = new IntPtr(intPtr.ToInt32() + i * intSize);
                    Marshal.StructureToPtr(var, newPtr, false);
                    i++;
                }


                return intPtr;
            }
            catch (Exception exc)
            {
                throw exc;
            }
        }
        public static IntPtr ConvertManageStructToIntPtr<T>(T aStructures)
        {
            T[] info = new T[1];
            info[0] = aStructures;

            IntPtr pTmp = ConvertManageStructToIntPtr<T>(info, 1);
            return pTmp;
        }


        #endregion

        #region int[]->IntPtr
        /// <summary>
        ///功能描述:将数组传入动态库中
        /// </summary>
        /// <param name="udtIntArr">传入数组</param>
        /// <param name="iNum">数组的数量</param>
        /// <returns>返回数组的内容的全局指针</returns>
        public static IntPtr ConverIntArrToIntPtr(int[] udtIntArr, int iNum)
        {
            try
            {
                if (udtIntArr == null || iNum <= 0)
                {
                    return IntPtr.Zero;
                }
                int intSizeOfObject;
                intSizeOfObject = iNum * (Marshal.SizeOf(typeof(int)));
                IntPtr ipInput = Marshal.AllocHGlobal(intSizeOfObject);
                System.Runtime.InteropServices.Marshal.Copy(udtIntArr, 0, ipInput, udtIntArr.Length);
                return ipInput;
            }
            catch (System.Exception exc)
            {
                throw exc;
            }
        }
        #endregion

        #region byte[]->IntPtr
        public static IntPtr ConverIntArrToIntPtr(byte[] udtIntArr, int intNumOfObject)
        {
            try
            {
                if (udtIntArr == null)
                    return IntPtr.Zero;

                int intSizeOfObject;
                intSizeOfObject = intNumOfObject * (Marshal.SizeOf(typeof(byte)));
                IntPtr ipInput = Marshal.AllocHGlobal(intSizeOfObject);
                System.Runtime.InteropServices.Marshal.Copy(udtIntArr, 0, ipInput, udtIntArr.Length);
                return ipInput;
            }
            catch (System.Exception exc)
            {
                throw exc;
            }
        }
        #endregion

        #region string->IntPtr
        /// <summary>
        /// 功能描述：从字符串获取字符串的指针
        /// </summary>
        /// <param name="strText">获取的字符串</param>
        /// <returns>返回字符串对应的指针</returns>
        public static IntPtr GetIntPtrFromString(string strText)
        {
            try
            {
                byte[] bytArr = System.Text.Encoding.Default.GetBytes(strText);
                byte[] bytStrArr = new byte[bytArr.Length + 1];

                for (int i = 0; i < bytArr.Length; i++)
                {
                    bytStrArr[i] = bytArr[i];
                }
                bytStrArr[bytStrArr.Length - 1] = 0;
                IntPtr pStr = System.Runtime.InteropServices.Marshal.AllocHGlobal(bytStrArr.Length);
                System.Runtime.InteropServices.Marshal.Copy(bytStrArr, 0, pStr, bytStrArr.Length);
                return pStr;
            }
            catch (Exception exc)
            {
                throw exc;
            }
        }
        #endregion

        #region IntPtr->Else

        /// <summary>
        /// 指针转字符串
        /// </summary>
        public static string ConverIntPtrToString(IntPtr in_pString)
        {
            return Marshal.PtrToStringAnsi(in_pString);
        }

        /// <summary>
        /// 指针转Bytes
        /// </summary>
        public static Byte[] ConverIntPtrToByte(IntPtr in_pByte, int in_ByteNum)
        {
            Byte[] pRet = new Byte[in_ByteNum];
            Marshal.Copy(in_pByte, pRet, 0, in_ByteNum);
            return pRet;
        }

        #endregion

        #region free IntPtr
        /// <summary>
        /// 功能描述：释放指针内存
        /// </summary>
        /// <param name="intPtr">传入的指针</param>
        public static void FreeGlobalIntPtrMemory(IntPtr intPtr)
        {
            try
            {
                if (intPtr.ToInt32() > 0)
                    System.Runtime.InteropServices.Marshal.FreeHGlobal(intPtr);
            }
            catch (Exception exc)
            {
                throw exc;
            }
        }
        #endregion

        #endregion

        #region enum->Hash<int,string>
        /// <summary>
        /// 枚举转字典
        /// </summary>
        /// <param name="enumType">类型</param>
        /// <returns></returns>
        public static Dictionary<int, string> ConverEnumToDictionary(Type enumType)
        {
            Dictionary<int, string> result = new Dictionary<int, string>();
            foreach (int key in Enum.GetValues(enumType))
            {
                string value = Enum.GetName(enumType, key);
                result.Add(key, value);
            }

            return result;
        }

        /// <summary>
        /// 枚举转字符串
        /// </summary>
        /// <param name="iEnum"></param>
        /// <param name="enumType"></param>
        /// <param name="iDefaultEnum"></param>
        /// <returns></returns>
        public static string ConverEnumToString(int iEnum, Type enumType, int iDefaultEnum = -1)
        {
            Array aList = Enum.GetValues(enumType);
            foreach (int key in aList)
            {
                if(key == iEnum)
                    return Enum.GetName(enumType, key);
            }

            if (iDefaultEnum == -1 || aList.Length <= iDefaultEnum)
                return "";

            return Enum.GetName(enumType, iDefaultEnum);
        }   

        /// <summary>
        /// 字符串转枚举量
        /// </summary>
        /// <param name="sEnum"></param>
        /// <param name="enumType"></param>
        /// <param name="iDefaultEnum"></param>
        /// <param name="iIgnoreUpLower"></param>
        /// <returns></returns>
        public static int ConverStringToEnum(string sEnum, Type enumType, int iDefaultEnum = -1, bool iIgnoreUpLower = true)
        {
            if (String.IsNullOrWhiteSpace(sEnum))
                return iDefaultEnum;

            sEnum = iIgnoreUpLower ? sEnum.ToLower() : sEnum;

            Array aList = Enum.GetValues(enumType);
            foreach (int key in aList)
            {
                string sCmpString = Enum.GetName(enumType, key);
                sCmpString = iIgnoreUpLower ? sCmpString.ToLower() : sCmpString;

                if (sCmpString == sEnum)
                    return key;
            }

            return iDefaultEnum;
        }

        #endregion

        #region string -> Other
        /// <summary>
        /// STRING->INT
        /// </summary>
        /// <param name="sValue">值</param>
        /// <param name="iDefault">默认值</param>
        public static int ConverStringToInt(string sValue, int iDefault = 0)
        {
            int iRet = iDefault;
            int.TryParse(sValue, out iRet);

            return iRet;
        }

        #endregion

        /// <summary>
        /// Byte[] -> BitmapImage
        /// </summary>
        /// <param name="sByteArray"></param>
        /// <returns></returns>
        public static BitmapImage ConverByteArrayToBitmapImage(byte[] sByteArray)
        {
            BitmapImage bmp = null;
            try
            {
                bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.StreamSource = new MemoryStream(sByteArray);
                bmp.EndInit();
            }
            catch
            {
                bmp = null;
            }
            return bmp;
        }
    }
}
