using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Linq;
using System.Collections;
using YYXB.Core.BLL;
using YYXB.Model;

namespace YYXB.Core.DataProxy
{
    public class KinProxyData
    {
        private static readonly Kin_BLL bll = new Kin_BLL();

        public static DataSet GetDataSet(IList<TextAndValue> tv, string proname)
        {
            try
            {
                return bll.IGetList(tv, proname);
            }
            catch/*(Exception ex)*/
            {
                return null;
            }
        }

        public static BaseModel<IList<string>> Execute(IList<TextAndValue> tv, string proname)
        {
            return bll.IExecute(tv, proname);
        }

        public static object Execute(Dictionary<string, string> dict, string proname)
        {
            return bll.IExecute(dict, proname);
        }
    }

}
