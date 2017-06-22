using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using YYXB.Model;

namespace YYXB.IDal
{
    public interface IKin
    {
        DataSet IGetList(IList<TextAndValue> tv, string proname);
        BaseModel<IList<string>> IExecute(IList<TextAndValue> tv, string proname);
        object IExecute(Dictionary<string, string> dict, string proname);
    }
}
