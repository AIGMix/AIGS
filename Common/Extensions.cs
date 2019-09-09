using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Windows.Media.Imaging;
using AIGS.Helper;
namespace AIGS.Common
{
    public static class Extensions
    {
        #region String
        /// <summary>
        /// 是否空白 | 是否不为空白
        /// </summary>
        public static bool IsBlank(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }
        public static bool IsNotBlank(this string str)
        {
            return !string.IsNullOrWhiteSpace(str);
        }
        public static bool IsChinese(this string str)
        {
            return StringHelper.IsChinese(str);
        }
        public static bool IsEnglish(this string str)
        {
            return StringHelper.IsEnglisth(str);
        }
        public static string ToPassword(this string str, bool equalLen = false)
        {
            if (str.IsBlank())
                return "";
            if (!equalLen)
                return "●●●●●●";
            string sret = "";
            for (int i = 0; i < str.Length; i++)
                sret += "●";
            return sret;
        }

        /// <summary>
        /// 去除数字
        /// </summary>
        public static string StripNonDigit(this string str)
        {
            return Regex.Replace(str, "\\D", "");
        }
        /// <summary>
        /// String->Double | Int | Long
        /// </summary>
        public static double ParseDouble(this string str, double defaultValue = default(double))
        {
            if (str.IsBlank())
                return defaultValue;
            const NumberStyles styles = NumberStyles.Float | NumberStyles.AllowThousands;
            var format = NumberFormatInfo.InvariantInfo;
            double result = defaultValue;
            double.TryParse(str, styles, format, out result);
            return result;
        }
        public static int ParseInt(this string str, int defaultValue = default(int))
        {
            if (str.IsBlank())
                return defaultValue;
            const NumberStyles styles = NumberStyles.AllowThousands;
            var format = NumberFormatInfo.InvariantInfo;
            int result = defaultValue;
            int.TryParse(str, styles, format, out result);
            //return Convert.ConverStringToInt(str, defaultValue);
            return result;
        }
        public static long ParseLong(this string str, long defaultValue = default(long))
        {
            if (str.IsBlank())
                return defaultValue;
            const NumberStyles styles = NumberStyles.AllowThousands;
            var format = NumberFormatInfo.InvariantInfo;
            long result = defaultValue;
            long.TryParse(str, styles, format, out result);
            return result;
        }
        /// <summary>
        /// 字符翻转
        /// </summary>
        public static string Reverse(this string str)
        {
            var sb = new StringBuilder(str.Length);
            for (var i = str.Length - 1; i >= 0; i--)
                sb.Append(str[i]);
            return sb.ToString();
        }

        public static string UrlEncode(this string url)
        {
            return HttpUtility.UrlEncode(url, Encoding.UTF8);
        }

        public static string UrlDecode(this string url)
        {
            return HttpUtility.UrlDecode(url, Encoding.UTF8);
        }

        public static string HtmlEncode(this string url)
        {
            return WebUtility.HtmlEncode(url);
        }

        public static string HtmlDecode(this string url)
        {
            return WebUtility.HtmlDecode(url);
        }

        public static string JoinToString<T>(this IEnumerable<T> enumerable, string separator)
        {
            return string.Join(separator, enumerable);
        }

        public static string[] Split(this string input, params string[] separators)
        {
            return input.Split(separators, StringSplitOptions.RemoveEmptyEntries);
        }

        #endregion

        public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> enumerable)
        {
            return enumerable ?? Enumerable.Empty<T>();
        }

        public static IEnumerable<TSource> Distinct<TSource, TKey>(this IEnumerable<TSource> enumerable, Func<TSource, TKey> selector)
        {
            var existing = new HashSet<TKey>();

            foreach (var element in enumerable)
            {
                if (existing.Add(selector(element)))
                    yield return element;
            }
        }

        #region BitmapImage
        public static BitmapImage ToBitmapImage(this byte[] sByteArray)
        {
            return Convert.ConverByteArrayToBitmapImage(sByteArray);
        }
        public static BitmapImage ToBitmapImage(this Bitmap bitmap)
        {
            return Convert.ConverBitmapToBitmapImage(bitmap);
        }
        #endregion
    }
}
