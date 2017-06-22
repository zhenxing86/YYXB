using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YYXB.Model
{
    public class WhereModel
    {
        public int Fetchnum { get; set; }
        public int NextId { set; get; }
        public int Page { set; get; }

        public int Size { set; get; }

        public string Type { set; get; }

        public string SeacherText { set; get; }

        public int UserId { set; get; }

        public int ThemeId { set; get; }

        public string Title { set; get; }

        public string Name { set; get; }


        public int cuid { set; get; }

        public int roleid { set; get; }

        #region bookstore
        public string tip { get; set; }
        public string status { get; set; }
        public string userid { get; set; }
        public string kid { get; set; }
        #endregion



       
    }
}
