using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AIGS.Common
{
    public class Property:ViewMoudleBase
    {
        private object key;
        private object value;
        private object desc;
        public object Key { get { return key; } set { this.key = value;OnPropertyChanged(); } }
        public object Value { get { return value; } set { this.value = value; OnPropertyChanged(); } }
        public object Desc { get { return desc; } set { this.desc = value; OnPropertyChanged(); } }

        public Property(object sKey = null, object sValue = null, object sDesc = null )
        {
            Key   = sKey;
            Value = sValue;
            Desc  = sDesc;
        }

        /// <summary>
        /// 设置类中的参数的值
        /// </summary>
        /// <param name="aThis"></param>
        /// <param name="sParaName"></param>
        /// <param name="oValue"></param>
        /// <returns></returns>
        public static bool SetValue(object aThis,  string sParaName,  object oValue)
        {
            try
            {
                Type aType = aThis.GetType();
                //读取属性，带有get/set的参数
                PropertyInfo[] aPropertyArrary = aType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                foreach (PropertyInfo aProperty in aPropertyArrary)
                {
                    if(aProperty.Name == sParaName)
                    {
                        aProperty.SetValue(aThis, oValue);
                        return true;
                    }
                }
                //读取字段
                FieldInfo[] aArrary = aType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                foreach (FieldInfo aField in aArrary)
                {
                    if (aField.Name == sParaName)
                    {
                        aField.SetValue(aThis, oValue);
                        return true;
                    }
                }
            }
            catch { }

            return false;
        }

        /// <summary>
        /// 获取类中参数的值
        /// </summary>
        /// <param name="aThis"></param>
        /// <param name="sParaName"></param>
        /// <returns></returns>
        public static object GetValue(object aThis, string sParaName)
        {
            try
            {
                Type aType = aThis.GetType();
                //读取属性，带有get/set的参数
                PropertyInfo[] aPropertyArrary = aType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                foreach (PropertyInfo aProperty in aPropertyArrary)
                {
                    if (aProperty.Name == sParaName)
                    {
                        return aProperty.GetValue(aThis);
                    }
                }
                //读取字段
                FieldInfo[] aArrary = aType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                foreach (FieldInfo aField in aArrary)
                {
                    if (aField.Name == sParaName)
                    {
                        return aField.GetValue(aThis);
                    }
                }
            }
            catch { }

            return null;
        }

    }
}
