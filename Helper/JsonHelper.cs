using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using AIGS.Common;
using Newtonsoft.Json.Serialization;

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
            if (sStr.IsBlank())
                return default(T);
            try
            {
                string sjson = sStr;
                foreach (string sName in sKeyName)
                {
                    JObject jo = JObject.Parse(sjson);
                    if(jo[sName] == null)
                        return default(T);

                    sjson = jo[sName].ToString();
                }
            
                T pRet = JsonConvert.DeserializeObject<T>(sjson);
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
        /// <param name="bOnlyHaveMemberAttr">只转换参数前加了 [JsonProperty("名称")] 的参数 </param>
        /// <returns></returns>
        public static string ConverObjectToString<T>(T aStruct, bool bOnlyHaveMemberAttr = false)
        {
            try
            {
                if (aStruct == null)
                    return null;
                if(bOnlyHaveMemberAttr == false)
                    return JsonConvert.SerializeObject(aStruct);
                else
                {
                    return JsonConvert.SerializeObject(aStruct, Newtonsoft.Json.Formatting.Indented, new JsonSerializerSettings { ContractResolver = new DynamicContractResolver(typeof(T))});
                }
            }
            catch
            {
                return "";
            }
        }

        class DynamicContractResolver : DefaultContractResolver
        {
            private readonly Type _type;
            public DynamicContractResolver(Type type)
            {
                _type = type;
            }
            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                IList<JsonProperty> properties = base.CreateProperties(type, memberSerialization);
                IList<JsonProperty> propertiesReturn = new List<JsonProperty>();
                foreach (JsonProperty item in properties)
                {
                    string PropertyNameTemp = item.PropertyName.ToLower().Trim();
                    if (type == _type && !item.HasMemberAttribute)
                        continue;
                    propertiesReturn.Add(item);
                }
                return propertiesReturn;
            }
        }



        /// <summary>
        /// 将字典类型序列化为json字符串
        /// </summary>
        /// <typeparam name="TKey">字典key</typeparam>
        /// <typeparam name="TValue">字典value</typeparam>
        /// <param name="dict">要序列化的字典数据</param>
        /// <returns>json字符串</returns>
        public static string SerializeDictionaryToJsonString<TKey, TValue>(Dictionary<TKey, TValue> dict)
        {
            if (dict.Count == 0)
                return "";

            string jsonStr = JsonConvert.SerializeObject(dict);
            return jsonStr;
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
                string sjson = sStr;
                foreach (string sName in sKeyName)
                {
                    JObject jo = JObject.Parse(sjson);
                    if (jo[sName] == null)
                        return null;

                    sjson = jo[sName].ToString();
                }
                return sjson;
            }
            catch
            {
                return null;
            }
        }

    }
}
