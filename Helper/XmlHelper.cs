using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace AIGS.Helper
{
    public class XmlHelper
    {
        #region 操作
        /// <summary>
        /// 获取节点
        /// </summary>
        /// <param name="aXmlHandle"></param>
        /// <param name="sXPath"></param>
        /// <returns></returns>
        public static XmlNode GetXmlNode(XmlDocument aXmlHandle, string sXPath)
        {
            //返回XPath节点的值
            if (aXmlHandle == null || String.IsNullOrWhiteSpace(sXPath))
                return null;

            XmlNode aNode = aXmlHandle.SelectSingleNode(sXPath);
            return aNode;
        }

        /// <summary>
        /// 添加节点
        /// </summary>
        /// <param name="aXmlHandle"></param>
        /// <param name="sXPath"></param>
        /// <returns></returns>
        public static XmlNode AddXmlNode(XmlDocument aXmlHandle, string sXPath, string sValue = null)
        {
            //返回XPath节点的值
            if (aXmlHandle == null || String.IsNullOrWhiteSpace(sXPath))
                return null;

            //获取节点名称
            sXPath = sXPath.Replace('\\', '/');
            string[] sPlits = sXPath.Split('/');
            for(int i = 0; i < sPlits.Count(); i++)
            {
                if (String.IsNullOrWhiteSpace(sPlits[i]))
                    return null;
            }
            
            //判断根是否合格，根只能有一个
            XmlElement aRoot = aXmlHandle.DocumentElement;
            if (aRoot == null)
            {
                aRoot = aXmlHandle.CreateElement(sPlits[0]);
                aXmlHandle.AppendChild(aRoot);
            }
            else if (aRoot.Name != sPlits[0])
                return null;
               
            //逐渐添加
            XmlNode pRet = aRoot;
            for(int i = 1; i < sPlits.Count(); i++)
            {
                XmlElement aNewNode = aXmlHandle.CreateElement(sPlits[i]);
                pRet.AppendChild(aNewNode);
                pRet = aNewNode;
            }

            //如果有值则赋值
            if (!String.IsNullOrWhiteSpace(sValue))
                pRet.Value = sValue;

            return pRet;
        }

        /// <summary>
        /// 打开
        /// </summary>
        /// <param name="sFilePath"></param>
        /// <returns></returns>
        public static XmlDocument Open(string sFilePath = null)
        {
            try
            {
                //创建一个XML对象
                XmlDocument aXmlHandle = new XmlDocument();
                aXmlHandle.XmlResolver = null;

                if (!String.IsNullOrWhiteSpace(sFilePath))
                    aXmlHandle.Load(sFilePath);
                return aXmlHandle;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="sFilePath"></param>
        /// <param name="xDocument"></param>
        /// <returns></returns>
        public static bool Save(string sFilePath, XmlDocument xDocument)
        {
            if(String.IsNullOrWhiteSpace(sFilePath) || xDocument == null)

            try
            {
                //需要指定编码格式，否则在读取时会抛：根级别上的数据无效。
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Encoding = new UTF8Encoding(false);
                settings.Indent = true;

                //写入文件
                XmlWriter xWrite = XmlWriter.Create(sFilePath, settings);
                xDocument.Save(xWrite);
                xWrite.Flush();
                xWrite.Close();
                return true;
            }
            catch {  }
            return false;
        }
        #endregion

        #region 对外接口
        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="sXPath"></param>
        /// <returns></returns>
        public static string GetValue(string sFilePath, string sXPath)
        {
            XmlDocument aHandle = Open(sFilePath);
            if (aHandle == null)
                return null;

            XmlNode aNode = GetXmlNode(aHandle, sXPath);
            if (aNode == null)
                return null;

            return aNode.InnerText;
        }

        /// <summary>
        /// 获取属性
        /// </summary>
        /// <param name="sXPath"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetAttributes(string sFilePath, string sXPath)
        {
            Dictionary<string, string> pRet = new Dictionary<string, string>();
            XmlDocument aHandle = Open(sFilePath);
            if (aHandle == null)
                return pRet;

            XmlNode aNode = GetXmlNode(aHandle, sXPath);
            if (aNode == null)
                return pRet;

            for (int i = 0; i < aNode.Attributes.Count; i++)
            {
                pRet.Add(aNode.Attributes[i].Name, aNode.Attributes[i].Value);
            }
            return pRet;
        }

        /// <summary>
        /// 转结构体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sFilePath"></param>
        /// <param name="sXPath"></param>
        /// <returns></returns>
        public static T ConverXmlFileToObject<T>(string sFilePath, string sXPath)
        {
            XmlDocument aHandle = Open(sFilePath);
            if (aHandle == null)
                return default(T);
            
            XmlNode aNode = GetXmlNode(aHandle, sXPath);
            if (aNode == null)
                return default(T);

            string[] sPath = sXPath.Replace('\\', '/').Split('/');
            string sJsonString = JsonHelper.ConverXmlNodeToJson(aHandle);
            return JsonHelper.ConverStringToObject<T>(sJsonString, sPath);
        }
        
        /// <summary>
        /// 将对象转为XML字符串
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="aObject"></param>
        /// <returns></returns>
        public static string ConverObjectToXmlString<T>(T aObject)
        {
            MemoryStream aStream = new MemoryStream();
            XmlSerializer aSerializer = new XmlSerializer(typeof(T));
            try
            {
                aSerializer.Serialize(aStream, aObject);
            }
            catch (InvalidOperationException)
            {
                throw;
            }

            aStream.Position = 0;
            StreamReader aReader = new StreamReader(aStream);
            string pRet = aReader.ReadToEnd();

            aReader.Dispose();
            aStream.Dispose();

            return pRet;
        }
        #endregion
    }
}
