using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace AIGS.Common
{
    /****************************************
     使用：
     1、
    <Window.Resources>
        <local:CategoryToSourceConverter x:Key="cts" />
        <local:StatuToNullableBoolConverter x:Key="ctnb" />
    </Window.Resources>

     2、
     <CheckBox IsThreeState="True" IsChecked="{Binding Path= Statu, Converter={StaticResource ctnb}}" />
     
    ****************************************/

    public class EnumToBoolConverter : IValueConverter
    {
        public Type EnumType = null;

        /// <summary>
        /// 根据绑定值 与 Radio按钮设定的值 是否相等，判断是否返回TRUE
        /// </summary>
        /// <param name="value">绑定值</param>
        /// <param name="parameter">Radio按钮设定的值</param>
        /// <returns>BOOL</returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //保存枚举的类型，以便后面可以用
            if (EnumType == null)
                EnumType = value.GetType();

            try
            {
                int iRadioPara = int.Parse(parameter.ToString());
                return iRadioPara == (int)value;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 根据 Radio按钮设定的值 来设置绑定值
        /// </summary>
        /// <param name="value">如果为true则有效</param>
        /// <param name="parameter">Radio按钮设定的值</param>
        /// <returns>枚举量</returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool isChecked = (bool)value;
            if (!isChecked)
                return null;

            try
            {
                return AIGS.Common.Convert.ConverIntToEnum(int.Parse(parameter.ToString()), EnumType);
            }
            catch
            {
                return null;
            }
        }
    }

    /// <summary>
    /// bool反转
    /// </summary>
    public class UnBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if ((bool)value)
                    return false;
                return true;
            }
            catch{return false;}
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if ((bool)value)
                    return false;
                return true;
            }
            catch { return false; }
        }
    }

    /// <summary>
    /// string不为空则返回true
    /// </summary>
    public class StringNotEmptyToBallConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if (value != null && value.ToString().IsNotBlank())
                    return true;
                return false;
            }
            catch { return false; }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if ((bool)value)
                    return null;
                return "true";
            }
            catch { return null; }
        }
    }

    /// <summary>
    /// not empty -> Visibility
    /// </summary>
    public class NotEmptyToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if (value != null)
                    return Visibility.Visible;
                return Visibility.Hidden;
            }
            catch { return Visibility.Hidden; }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
    /// <summary>
    /// empty -> Visibility
    /// </summary>
    public class EmptyToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if (value == null)
                    return Visibility.Visible;
                return Visibility.Hidden;
            }
            catch { return Visibility.Hidden; }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    /// <summary>
    /// empty -> Visibility
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if ((bool)value)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
            catch { return Visibility.Collapsed; }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }


    public class RowHeaderToIndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DataGridRow row = value as DataGridRow;
            return row.GetIndex() + 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

