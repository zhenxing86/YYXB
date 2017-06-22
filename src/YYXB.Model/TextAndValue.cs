using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YYXB.Model
{
    public class TextAndValue
    {
        public TextAndValue()
        {
        }
        public TextAndValue( string txt,string val)
        {
            _txt = txt;
            _val = val;
        }
        private String _txt="";
        public String Text
        {
            get { return _txt; }
            set { _txt = value; }
        }

        private String _val = "";
        public String Value
        {
            get { return _val; }
            set { _val = value; }
        }

    }

}
