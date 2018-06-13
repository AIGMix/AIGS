using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace AIGS.Helper
{
    public class MssqlHelper
    {
        /// <summary>
        /// 获取数据库连接字符串
        /// </summary>
        /// <param name="sDBName">库名</param>
        /// <param name="sUser">用户名</param>
        /// <param name="sPwd">密码</param>
        /// <param name="sIP">服务器IP</param>
        /// <returns></returns>
        public static string GetConnectionString(string sDBName, string sUser, string sPwd, string sIP = null)
        {
            if (!String.IsNullOrWhiteSpace(sIP) && !AIGS.Helper.NetHelper.IsCorrectIP(sIP))
                return "";

            string ConnectionString = "";
            ConnectionString += String.IsNullOrWhiteSpace(sIP) ? "Data Source=(local);" : "Data Source=" + sIP + ";";
            ConnectionString += "Initial Catalog=" + sDBName + ";";
            ConnectionString += "User ID=" + sUser + ";";
            ConnectionString += "Password=" + sPwd + ";";

            return ConnectionString;
        }

        #region SQL文本命令

        /// <summary>
        /// 查询SQL 文本命令
        /// </summary>
        /// <param name="sConnectionString">连接字符串</param>
        /// <param name="sSQL">SQL文本命令</param>
        /// <returns></returns>
        public static DataTable Query(string sConnectionString, string sSQL)
        {
            return (DataTable)Query(sConnectionString, sSQL, typeof(DataTable));
        }

        /// <summary>
        /// 查询SQL 文本命令
        /// </summary>
        /// <param name="sConnectionString">连接字符串</param>
        /// <param name="sSQL">SQL文本命令</param>
        /// <returns></returns>
        public static DataSet Querys(string sConnectionString, string sSQL)
        {
            return (DataSet)Query(sConnectionString, sSQL, typeof(DataSet));
        }

        /// <summary>
        /// 查询SQL语句
        /// </summary>
        /// <param name="sSQL">SQL语句</param>
        /// <param name="eType">返回的类型(DataTable/DataSet)</param>
        /// <returns></returns>
        private static object Query(string sConnectionString, string sSQL, Type eType)
        {
            if (string.IsNullOrWhiteSpace(sConnectionString))
                return null;

            object oData = null;
            DataTable aTable = new DataTable();
            DataSet aSet = new DataSet();
            using (SqlConnection aCon = new SqlConnection(sConnectionString))
            {
                try
                {
                    SqlCommand aCommand = PrepaerCommand(aCon, CommandType.Text, sSQL);
                    SqlDataAdapter aAdapter = new SqlDataAdapter(aCommand);
                    if (eType == typeof(DataTable))
                    {
                        aAdapter.Fill(aTable);
                        oData = aTable;
                    }
                    if (eType == typeof(DataSet))
                    {
                        aAdapter.Fill(aSet);
                        oData = aSet;
                    }
                }
                catch (System.Data.SqlClient.SqlException ex)
                {
                    throw new Exception(ex.Message);
                }
                return oData;
            }
        }

        #endregion

        #region 存储过程

        /// <summary>
        /// 获取存储过程
        /// </summary>
        /// <param name="sProcName"></param>
        /// <returns></returns>
        public static DataSet GetProducts(string sConnectionString, string sProcName)
        {
            if (string.IsNullOrWhiteSpace(sConnectionString))
                return null;

            DataSet aSet = new DataSet();
            using (SqlConnection aCon = new SqlConnection(sConnectionString))
            {
                try
                {
                    SqlCommand aCommand = PrepaerCommand(aCon, CommandType.StoredProcedure, sProcName);
                    SqlDataAdapter aAdapter = new SqlDataAdapter(aCommand);
                        aAdapter.Fill(aSet);
                }
                catch (System.Data.SqlClient.SqlException ex)
                {
                    throw new Exception(ex.Message);
                }
                return aSet;
            }
        }
        #endregion

        #region 工具
        

        /// <summary>
        /// 准备命令参数
        /// </summary>
        /// <param name="aCon">连接句柄</param>
        /// <param name="eCmdType">命令类型(存储过程\T-SQL语句\表名)</param>
        /// <param name="sCmdText">命令文本</param>
        /// <returns></returns>
        private static SqlCommand PrepaerCommand(SqlConnection aCon, CommandType eCmdType, string sCmdText)
        {
            SqlCommand pRet = new SqlCommand();

            //判断数据库连接状态
            if (aCon.State != ConnectionState.Open)
                aCon.Open();

            pRet.Connection = aCon;
            pRet.CommandType = eCmdType;
            pRet.CommandText = sCmdText;

            return pRet;
        }

        #endregion
    }
}