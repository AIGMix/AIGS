using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AIGS.Helper
{
    public class JsonHelper
    {
        public static string Errmsg;

        /// <summary>
        /// 字符串转结构体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sStr">字符串</param>
        /// <param name="sKeyName">关键字</param>
        /// <returns></returns>
        public static T ConverStringToObject<T>(string sStr, params string[] sKeyName)
        {
            if (String.IsNullOrWhiteSpace(sStr))
                return default(T);

            foreach(string sName in sKeyName)
            {
                JObject jo = JObject.Parse(sStr);
                if(jo[sName] == null)
                    return default(T);

                sStr = jo[sName].ToString();
            }

            try
            {
                T pRet = JsonConvert.DeserializeObject<T>(sStr);
                return pRet;
            }
            catch(Exception e)
            {
                Errmsg = e.Message;
                return default(T);
            }
        }

        /// <summary>
        /// 结构体转字符串
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="aStruct"></param>
        /// <returns></returns>
        public static string ConverObjectToString<T>(T aStruct)
        {
            try
            {
                string sRet = JsonConvert.SerializeObject(aStruct);
                return sRet;
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// 将XMLNode转为Json字符串
        /// </summary>
        /// <param name="aNode"></param>
        /// <returns></returns>
        public static string ConverXmlNodeToJson(XmlNode aNode)
        {
            try
            {
                string sRet = Newtonsoft.Json.JsonConvert.SerializeXmlNode(aNode);
                return sRet;
            }
            catch
            {
                return "";
            }
        }


        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="sStr"></param>
        /// <param name="sKeyName"></param>
        /// <returns></returns>
        public static string GetValue(string sStr, params string[] sKeyName)
        {
            if (String.IsNullOrWhiteSpace(sStr))
                return null;

            try
            {
                foreach (string sName in sKeyName)
                {
                    JObject jo = JObject.Parse(sStr);
                    if (jo[sName] == null)
                        return null;

                    sStr = jo[sName].ToString();
                }

                return sStr;
            }
            catch
            {
                return null;
            }
        }

    }
}
