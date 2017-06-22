using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Text.RegularExpressions;

namespace YYXB.Dal.DBUtility
{
    /// <summary>
    /// The SqlHelper class is intended to encapsulate high performance, 
    /// scalable best practices for common uses of SqlClient.
    /// </summary>
    public abstract class SqlHelper
    {
        #region 转为中文
        public static string GBToUnicode(string text)
        {
            if (text != null)
            {
                byte[] bytes = System.Text.Encoding.Unicode.GetBytes(text);
                string lowCode = "", temp = "";
                for (int i = 0; i < bytes.Length; i++)
                {
                    if (i % 2 == 0)
                    {
                        temp = System.Convert.ToString(bytes[i], 16);//取出元素4编码内容（两位16进制）
                        if (temp.Length < 2) temp = "0" + temp;
                    }
                    else
                    {
                        string mytemp = Convert.ToString(bytes[i], 16);
                        if (mytemp.Length < 2)
                        {
                            mytemp = "0" + mytemp;
                        }
                        lowCode = lowCode + @"\u" + mytemp + temp;//取出元素4编码内容（两位16进制）
                    }
                }
                return lowCode;
            }
            else
            {
                return text;
            }
        }
        public static string UnicodeToGB(string text)
        {
            MatchCollection mc = Regex.Matches(text, "([\\w]+)|(\\\\u([\\w]{4}))");
            if (mc != null && mc.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (Match m2 in mc)
                {
                    string v = m2.Value;
                    string word = v.Substring(2);
                    byte[] codes = new byte[2];
                    try
                    {

                        int code = Convert.ToInt32(word.Substring(0, 2), 16);
                        int code2 = Convert.ToInt32(word.Substring(2), 16);
                        codes[0] = (byte)code2;
                        codes[1] = (byte)code;
                        sb.Append(Encoding.Unicode.GetString(codes));
                    }
                    catch { sb.Append(v); }

                }
                return sb.ToString();
            }
            else
            {
                return text;
            }
        }
        #endregion

        //Database connection strings
        public static readonly string ConnectionString = ConfigurationManager.ConnectionStrings["SQLConnString"].ConnectionString;
       
        private static Hashtable parmCache = Hashtable.Synchronized(new Hashtable());

     
        /// <summary>
        /// Execute a SqlCommand (that returns no resultset) against the database specified in the connection string 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">a valid connection string for a SqlConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>an int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(string connectionString, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {

            SqlCommand cmd = new SqlCommand();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                int val = 0;
                try
                {
                    PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
                    val = cmd.ExecuteNonQuery();
                    cmd.Parameters.Clear();


                }
                catch (Exception ex)
                {
                    string parmstr = WriteErrorLog(cmdText, commandParameters, ex);
                    throw new Exception("ExecuteNonQuery:$" + cmdText + "$" + parmstr + "$" + ex.Message);
                }
                return val;
            }
        }

        /// <summary>
        /// Execute a SqlCommand (that returns no resultset) against an existing database connection 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="conn">an existing database connection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>an int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(SqlConnection connection, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {

            SqlCommand cmd = new SqlCommand();
            int val = 0;
            try
            {
                PrepareCommand(cmd, connection, null, cmdType, cmdText, commandParameters);
                val = cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();

                //WriteLog(cmdText, commandParameters);
            }
            catch (Exception ex)
            {
                string parmstr = WriteErrorLog(cmdText, commandParameters,ex);
                throw new Exception("ExecuteNonQuery:$" + cmdText + "$" + parmstr + "$" + ex.Message);
            }
            return val;
        }

        private static string WriteErrorLog(string cmdText, SqlParameter[] commandParameters,Exception ex)
        {
            string parmstr = "";
            StringBuilder sb = new StringBuilder();
            if (commandParameters.Length > 1)
            {
                for (int i = 0; i < commandParameters.Length; i++)
                {
                    if (commandParameters[i] != null)
                        sb.AppendFormat("&{0}={1}", commandParameters[i].ParameterName, commandParameters[i].Value);//parmstr = parmstr + commandParameters[i].Value + "-";
                }
            }
            if (sb.Length > 0)
            {
                parmstr = sb.ToString().Substring(1);
            }
            log4net.LogManager.GetLogger("SqlHelper").Error("ExecuteNonQuery:" + cmdText + ";" + parmstr, ex);
            return parmstr;
        }

        public static DataSet ExecuteDataSet(String connection, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {

            SqlCommand cmd = new SqlCommand();
            SqlConnection conn = new SqlConnection(connection);
            DataSet ds = new DataSet();
            try
            {
                PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(ds);
                cmd.Parameters.Clear();
                conn.Close();

                //WriteLog(cmdText, commandParameters);
            }
            catch (Exception ex)
            {
                string parmstr = WriteErrorLog(cmdText, commandParameters, ex);
                conn.Close();
                throw new Exception("ExecuteNonQuery:$" + cmdText + "$" + parmstr + "$" + ex.Message);
            }
            return ds;
        }

        /// <summary>
        /// Execute a SqlCommand (that returns no resultset) using an existing SQL Transaction 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="trans">an existing sql transaction</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>an int representing the number of rows affected by the command</returns>
        public static int ExecuteNonQuery(SqlTransaction trans, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            int val = 0;
            try
            {
                SqlCommand cmd = new SqlCommand();
                PrepareCommand(cmd, trans.Connection, trans, cmdType, cmdText, commandParameters);
                val = cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();

                //WriteLog(cmdText, commandParameters);
            }
            catch (Exception ex)
            {
                string parmstr = WriteErrorLog(cmdText, commandParameters, ex);
                throw new Exception("ExecuteNonQuery:$" + cmdText + "$" + parmstr + "$" + ex.Message);
            }
            return val;
        }

        /// <summary>
        /// Execute a SqlCommand that returns a resultset against the database specified in the connection string 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  SqlDataReader r = ExecuteReader(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">a valid connection string for a SqlConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>A SqlDataReader containing the results</returns>
        public static SqlDataReader ExecuteReader(string connectionString, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();
            SqlConnection conn = new SqlConnection(connectionString);

            // we use a try/catch here because if the method throws an exception we want to 
            // close the connection throw code, because no datareader will exist, hence the 
            // commandBehaviour.CloseConnection will not work
            try
            {
                PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
                SqlDataReader rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                cmd.Parameters.Clear();

                //WriteLog(cmdText, commandParameters);
                return rdr;
            }
            catch (Exception ex)
            {
                string parmstr = WriteErrorLog(cmdText, commandParameters, ex);
                conn.Close();

                throw new Exception("ExecuteNonQuery:$" + cmdText + "$" + parmstr + "$" + ex.Message);
            }
        }

        /// <summary>
        /// Execute a SqlCommand that returns the first column of the first record against the database specified in the connection string 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  Object obj = ExecuteScalar(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connectionString">a valid connection string for a SqlConnection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>An object that should be converted to the expected type using Convert.To{Type}</returns>
        public static object ExecuteScalar(string connectionString, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {
            SqlCommand cmd = new SqlCommand();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                object val = null;
                try
                {
                    PrepareCommand(cmd, connection, null, cmdType, cmdText, commandParameters);
                    val = cmd.ExecuteScalar();
                    cmd.Parameters.Clear();

                    //WriteLog(cmdText, commandParameters);
                }
                catch (Exception ex)
                {
                    string parmstr = WriteErrorLog(cmdText, commandParameters, ex);
                }
                return val;
            }
        }

        /// <summary>
        /// Execute a SqlCommand that returns the first column of the first record against an existing database connection 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  Object obj = ExecuteScalar(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="conn">an existing database connection</param>
        /// <param name="commandType">the CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">the stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">an array of SqlParamters used to execute the command</param>
        /// <returns>An object that should be converted to the expected type using Convert.To{Type}</returns>
        public static object ExecuteScalar(SqlConnection connection, CommandType cmdType, string cmdText, params SqlParameter[] commandParameters)
        {

            SqlCommand cmd = new SqlCommand();
            object val = null;
            try
            {
                PrepareCommand(cmd, connection, null, cmdType, cmdText, commandParameters);
                val = cmd.ExecuteScalar();
                cmd.Parameters.Clear();

                //WriteLog(cmdText, commandParameters);
            }
            catch (Exception ex)
            {
                string parmstr = WriteErrorLog(cmdText, commandParameters, ex);
            }
            return val;
        }

        /// <summary>
        /// add parameter array to the cache
        /// </summary>
        /// <param name="cacheKey">Key to the parameter cache</param>
        /// <param name="cmdParms">an array of SqlParamters to be cached</param>
        public static void CacheParameters(string cacheKey, params SqlParameter[] commandParameters)
        {
            parmCache[cacheKey] = commandParameters;
        }

        /// <summary>
        /// Retrieve cached parameters
        /// </summary>
        /// <param name="cacheKey">key used to lookup parameters</param>
        /// <returns>Cached SqlParamters array</returns>
        public static SqlParameter[] GetCachedParameters(string cacheKey)
        {
            SqlParameter[] cachedParms = (SqlParameter[])parmCache[cacheKey];

            if (cachedParms == null)
                return null;

            SqlParameter[] clonedParms = new SqlParameter[cachedParms.Length];

            for (int i = 0, j = cachedParms.Length; i < j; i++)
                clonedParms[i] = (SqlParameter)((ICloneable)cachedParms[i]).Clone();

            return clonedParms;
        }

        /// <summary>
        /// Prepare a command for execution
        /// </summary>
        /// <param name="cmd">SqlCommand object</param>
        /// <param name="conn">SqlConnection object</param>
        /// <param name="trans">SqlTransaction object</param>
        /// <param name="cmdType">Cmd type e.g. stored procedure or text</param>
        /// <param name="cmdText">Command text, e.g. Select * from Products</param>
        /// <param name="cmdParms">SqlParameters to use in the command</param>
        private static void PrepareCommand(SqlCommand cmd, SqlConnection conn, SqlTransaction trans, CommandType cmdType, string cmdText, SqlParameter[] cmdParms)
        {

            if (conn.State != ConnectionState.Open)
                conn.Open();

            cmd.Connection = conn;
            cmd.CommandText = cmdText;

            if (trans != null)
                cmd.Transaction = trans;

            cmd.CommandType = cmdType;

            if (cmdParms != null)
            {
                foreach (SqlParameter parm in cmdParms)
                    cmd.Parameters.Add(parm);

            }

        }
    }
}
