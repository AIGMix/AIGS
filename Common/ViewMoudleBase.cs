using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;

namespace AIGS.Common
{
    public class ViewMoudleBase : INotifyPropertyChanged
    {
        //public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;
        protected static void OnPropertyChangedStatic(EventHandler<PropertyChangedEventArgs> StaticPropertyChanged, [CallerMemberName] string propertyName = null)
        {
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #region 数据校验
        /// <summary>
        /// 数据校验的回调接口
        /// </summary>
        /// <param name="sPropertyName"></param>
        /// <sample>
        /// string CallBackDataCheck(string sName)
        /// {
        ///     if(sName == "m_Count")
        ///     {
        ///         if(m_Count < 0)
        ///             return "Count need bigger than 0";
        ///     }
        ///     return string.Empty;
        /// }
        /// </sample>
        /// <returns></returns>
        public delegate string DataCheckFunc(string sPropertyName);
        public DataCheckFunc CallBackDataCheck;

        /// <summary>
        /// 数据校验实现
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public string this[string columnName]
        {
            get
            {
                if (CallBackDataCheck != null)
                {
                    string result = CallBackDataCheck(columnName);
                    return result;
                }

                return string.Empty;
            }
        }
        #endregion
    }
}
