using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;
using System.Data.SqlClient;
using System.Configuration;

using YYXB.Model;
using YYXB.Dal.DBUtility;
using YYXB.IDal;


namespace YYXB.Dal
{
    public class Kin_DAL:IKin 
    {
        public DataSet IGetList(IList<TextAndValue> tv, string proname)
        {
            DataSet ds = new DataSet();
            if ((tv.Count == 0 ? "right": tv[0].Value) != "error")
            {
                IList<SqlParameter> spms = new List<SqlParameter>();

                foreach (TextAndValue t in tv)
                {
                    spms.Add(new SqlParameter(t.Text, t.Value));
                }
                ds = SqlHelper.ExecuteDataSet(SqlHelper.ConnectionString, CommandType.StoredProcedure, proname, spms.ToArray());
            }

            return ds;
        }

        public BaseModel<IList<string>> IExecute(IList<TextAndValue> tv, string proname)
        {

            BaseModel<IList<string>> bm = new BaseModel<IList<string>>();

            if (tv[0].Value != "error")
            {
                IList<SqlParameter> spms = new List<SqlParameter>();

                foreach (TextAndValue t in tv)
                {
                    spms.Add(new SqlParameter(t.Text, t.Value));
                }

                try
                {
                    using (SqlDataReader rdr = SqlHelper.ExecuteReader(SqlHelper.ConnectionString, CommandType.StoredProcedure, proname, spms.ToArray()))
                    {
                        while (rdr.Read())
                        {
                            if (!rdr.IsDBNull(0)) bm.code = rdr.GetInt32(0);
                            if (!rdr.IsDBNull(1)) bm.info = rdr.GetString(1);
                            bm.result = new List<string>();
                        }
                    }
                }
                catch (Exception ex)
                {
                    log4net.LogManager.GetLogger("执行存储过程出错：").Error(ex);
                    bm.code = -1;
                    bm.info = "数据请求失败";
                    bm.result = new List<string>();
                }
            }

            return bm;
        }

        public object IExecute(Dictionary<string, string> dict, string proname)
        {
            IList<SqlParameter> spms = new List<SqlParameter>();
            int index = 0;
            int retIndex = -1;
            foreach (KeyValuePair<string, string> item in dict)
            {
                spms.Add(new SqlParameter(item.Key, item.Value));
                if (item.Key.ToLower().IndexOf("@returnvalue") > -1)
                {
                    spms[index].Direction = ParameterDirection.ReturnValue;
                    retIndex = index;
                }
                else
                {
                    index++;
                }
            }
            object str = SqlHelper.ConnectionString;
            if (!dict.ContainsKey("@content") || dict["@content"] != "@@")
            {
                SqlHelper.ExecuteNonQuery(SqlHelper.ConnectionString, CommandType.StoredProcedure, proname, spms.ToArray());
                if (retIndex > -1)
                {
                    str = spms[retIndex].Value;
                }
            }
            return str;
        }
       
    }
}
