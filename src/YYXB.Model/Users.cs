using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YYXB.Model
{
    public class Users
    {
        private int _index;

        public int Index
        {
            get { return _index; }
            set { _index = value; }
        }

        public int pcount { get; set; }

        #region Model
        private int _ID;
        private String _account;
        private String _password;
        private int _usertype;
        private String _name;
        private DateTime _regdatetime;
        private int _deletetag;

        public Users()
        {
        }

        /// <summary>
        /// ID
        /// </summary>
        public int ID
        {
            set { _ID = value; }
            get { return _ID; }
        }

        /// <summary>
        /// Account
        /// </summary>
        public String Account
        {
            set { _account = value; }
            get { return _account; }
        }

        /// <summary>
        /// password
        /// </summary>
        public String Password
        {
            set { _password = value; }
            get { return _password; }
        }

        /// <summary>
        /// usertype
        /// </summary>
        public int UserType
        {
            set { _usertype = value; }
            get { return _usertype; }
        }

        

        /// <summary>
        /// name
        /// </summary>
        public String Name
        {
            set { _name = value; }
            get { return _name; }
        }


        /// <summary>
        /// regdatetime
        /// </summary>
        public DateTime RegDatetime
        {
            set { _regdatetime = value; }
            get { return _regdatetime; }
        }

        /// <summary>
        /// deletetag
        /// </summary>
        public int DeleteTag
        {
            set { _deletetag = value; }
            get { return _deletetag; }
        }

        #endregion Model
    }
}
