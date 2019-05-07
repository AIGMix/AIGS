using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace AIGS.Common
{
    public static class Extensions
    {
        #region String
        /// <summary>
        /// 是否空白
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsBlank(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        /// <summary>
        /// 是否不为空白
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsNotBlank(this string str)
        {
            return !string.IsNullOrWhiteSpace(str);
        }

        /// <summary>
        /// 裁剪子串直到sub
        /// </summary>
        /// <param name="str"></param>
        /// <param name="sub"></param>
        /// <param name="comparison"></param>
        /// <returns></returns>
        public static string SubstringUntil(this string str, string sub,
            StringComparison comparison = StringComparison.Ordinal)
        {
            var index = str.IndexOf(sub, comparison);
            return index < 0 ? str : str.Substring(0, index);
        }

        /// <summary>
        /// 裁剪子串从sub之后开始
        /// </summary>
        /// <param name="str"></param>
        /// <param name="sub"></param>
        /// <param name="comparison"></param>
        /// <returns></returns>
        public static string SubstringAfter(this string str, string sub,
            StringComparison comparison = StringComparison.Ordinal)
        {
            var index = str.IndexOf(sub, comparison);
            return index < 0 ? string.Empty : str.Substring(index + sub.Length, str.Length - index - sub.Length);
        }

        /// <summary>
        /// 去除数字
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string StripNonDigit(this string str)
        {
            return Regex.Replace(str, "\\D", "");
        }

        /// <summary>
        /// String->Double
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static double ParseDouble(this string str, double defaultValue = default(double))
        {
            const NumberStyles styles = NumberStyles.Float | NumberStyles.AllowThousands;
            var format = NumberFormatInfo.InvariantInfo;
            double result = defaultValue;
            double.TryParse(str, styles, format, out result);
            return result;
        }

        /// <summary>
        /// String->Int
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int ParseInt(this string str, int defaultValue = default(int))
        {
            const NumberStyles styles = NumberStyles.AllowThousands;
            var format = NumberFormatInfo.InvariantInfo;
            int result = defaultValue;
            int.TryParse(str, styles, format, out result);
            return result;
        }

        /// <summary>
        /// String->Long
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static long ParseLong(this string str, long defaultValue = default(long))
        {
            const NumberStyles styles = NumberStyles.AllowThousands;
            var format = NumberFormatInfo.InvariantInfo;
            long result = defaultValue;
            long.TryParse(str, styles, format, out result);
            return result;
        }

        public static DateTimeOffset ParseDateTimeOffset(this string str)
        {
            return DateTimeOffset.Parse(str, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AssumeUniversal);
        }

        public static DateTimeOffset ParseDateTimeOffset(this string str, string format)
        {
            return DateTimeOffset.ParseExact(str, format, DateTimeFormatInfo.InvariantInfo,
                DateTimeStyles.AssumeUniversal);
        }

        /// <summary>
        /// 字符翻转
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
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

        public static IEnumerable<TSource> Distinct<TSource, TKey>(this IEnumerable<TSource> enumerable,
            Func<TSource, TKey> selector)
        {
            var existing = new HashSet<TKey>();

            foreach (var element in enumerable)
            {
                if (existing.Add(selector(element)))
                    yield return element;
            }
        }
    }
}
