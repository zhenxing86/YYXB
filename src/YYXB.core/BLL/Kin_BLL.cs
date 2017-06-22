using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YYXB.Model;
using System.Data;
using YYXB.IDal;

namespace  YYXB.Core.BLL
{
    public class Kin_BLL :IKin
    {
        private static readonly IKin dal = YYXB.Core.DALFactory.DataAccess.CreateKin();

        public DataSet IGetList(IList<TextAndValue> tv, string proname)
        {
            DataSet ds = dal.IGetList(tv, proname);
            if (tv.Count > 0 && tv[0].Value == "error")
            {
                ds = null;
                log4net.LogManager.GetLogger("验证错误：").Error("javakey:" + tv[1].Value
                    + "__________javakeynew:" + tv[2].Value + "__________netkeynew:" + tv[3].Value);
            }
            return ds;
        }

        public BaseModel<IList<string>> IExecute(IList<TextAndValue> tv, string proname)
        {
            BaseModel<IList<string>> bm = dal.IExecute(tv, proname);
            if (tv.Count > 0 && tv[0].Value == "error")
            {
                IList<string> ls = new List<string>();
                ls.Add(tv[1].Value);
                ls.Add(tv[2].Value);
                ls.Add(tv[3].Value);
                bm.code = -1;
                bm.info = "验证失败";
                bm.result = ls;
                log4net.LogManager.GetLogger("验证错误：").Error("执行存储过程：" + proname + "javakey:" + tv[1].Value
                    + "__________javakeynew:" + tv[2].Value + "__________netkeynew:" + tv[3].Value);
            }
            return bm;
        }

        public object IExecute(Dictionary<string, string> dict, string proname)
        {
            return dal.IExecute(dict, proname);
        }
    }
}
