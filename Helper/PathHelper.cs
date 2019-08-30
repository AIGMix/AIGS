using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AIGS.Helper
{
    public class PathHelper
    {
        /// <summary>
        /// 是否为相对路径
        /// </summary>
        public static bool IsRelativePath(string sPath)
        {
            if (String.IsNullOrWhiteSpace(sPath))
                return true;

            sPath = sPath.Trim();
            string sTmp = Path.GetFullPath(sPath);
            if (sTmp.Length == sPath.Length)
                return false;

            return true;
        }

        /// <summary>
        /// 获取绝对路径
        /// </summary>
        public static string GetFullPath(string sPath, string sBasePath = null)
        {
            if (String.IsNullOrWhiteSpace(sPath))
                return sPath;

            if (!IsRelativePath(sPath))
                return sPath;

            if (sBasePath == null)
                return Path.GetFullPath(sPath);

            return sBasePath + '\\' + sPath;
        }

        /// <summary>
        /// 获取相对路径
        /// </summary>
        /// <param name="sPath"></param>
        /// <param name="sBasePath"></param>
        /// <returns></returns>
        public static string GetRelativePath(string sPath, string sBasePath)
        {
            /************************
             * 例子：1）sPath = e:\\\\1\\输出目录\\ChildPara
             *      2）sBasePath = E:\\1\\项目\\
             *      ---》..\\输出目录\\ChildPara
             * **********************/

            sPath = Path.GetFullPath(sPath);
            sBasePath = Path.GetFullPath(sBasePath) + '\\';

            sPath = sPath.Replace('\\', '/');
            sBasePath = sBasePath.Replace('\\', '/');

            //删除重复的'/'
            sPath = AIGS.Helper.StringHelper.RemoveAdjacentRepetitiveChar(sPath, '/');
            sBasePath = AIGS.Helper.StringHelper.RemoveAdjacentRepetitiveChar(sBasePath, '/');

            //获取相同的路径头，如果没有的话（可能不同盘），则没办法获取相对路径
            string sSamePre = AIGS.Helper.StringHelper.FindPreSameString(sPath, sBasePath, false);
            if (String.IsNullOrWhiteSpace(sSamePre))
                return null;

            /************************
             * 除去相同路径头后剩下
             *      1）sPath = 输出目录\\ChildPara
             *      2）sBasePath = 项目\\
             * 则sBasePath有几个'\\',即退几层目录,之后再加上sPath
             * **********************/
            string sPath2 = sPath.Substring(sSamePre.Length, sPath.Length - sSamePre.Length);
            string sBasePath2 = sBasePath.Substring(sSamePre.Length, sBasePath.Length - sSamePre.Length);

            int iCount = sBasePath2.Split('/').Count() - 1;
            string sRet = "";
            for(int i = 0; i < iCount; i++)
            {
                sRet += "../";
            }

            sRet += sPath2;
            sRet = sRet.Replace('/', '\\');
            return sRet;
        }


        /// <summary>
        /// 替换路径中禁止的符号
        /// </summary>
        /// <param name="sPath">路径</param>
        /// <param name="sReplaceChar">替换后的字符串</param>
        /// <returns></returns>
        public static string ReplaceLimitChar(string sPath, string sReplaceChar)
        {
            if (String.IsNullOrWhiteSpace(sPath))
                return sPath;

            sPath = sPath.Replace(":", sReplaceChar);
            sPath = sPath.Replace("/", sReplaceChar);
            sPath = sPath.Replace("?", sReplaceChar);
            sPath = sPath.Replace("<", sReplaceChar);
            sPath = sPath.Replace(">", sReplaceChar);
            sPath = sPath.Replace("|", sReplaceChar);
            sPath = sPath.Replace("\\", sReplaceChar);
            sPath = sPath.Replace("*", sReplaceChar);
            sPath = sPath.Replace("\"", sReplaceChar);

            return sPath;
        }

        /// <summary>
        /// 获取目录下的文件名
        /// </summary>
        /// <param name="sPath"></param>
        /// <returns></returns>
        public static string[] GetFileNames(string sPath)
        {
            DirectoryInfo aDirectory = new DirectoryInfo(sPath);
            FileInfo[] Infos = aDirectory.GetFiles();
            if (Infos.Count() == 0)
                return null;

            string[] aRet = new string[Infos.Count()];
            for (int i = 0; i < aRet.Count(); i++)
                aRet[i] = Infos[i].FullName;
            
            return aRet;
        }

        /// <summary>
        /// 创建目录
        /// </summary>
        /// <param name="sPath"></param>
        /// <returns></returns>
        public static bool Mkdirs(string sPath)
        {
            if(Directory.Exists(sPath))
                return true;
            
            Directory.CreateDirectory(sPath);
            if (!Directory.Exists(sPath))
                return false;
            return true;
        }
    }
}
