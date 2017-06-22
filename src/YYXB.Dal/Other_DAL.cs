using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

using YYXB.Dal.DBUtility;
using YYXB.Model;

namespace YYXB.Dal
{
    public class Other_DAL
    {
        //public IList<class_album> Iclass_album_GetList(int tuid)
        //{
        //    //albumid,title,photocount,coverphotodatetime,net,cid,cname
        //    SqlParameter[] spms = {
        //        new SqlParameter("@tuid", tuid)
        //    };
        //    IList<class_album> lsx = new List<class_album>();


        //    IList<class_album_Info> ls = new List<class_album_Info>();

        //    using (SqlDataReader rdr = SqlHelper.ExecuteReader(SqlHelper.ConnectionString, CommandType.StoredProcedure, "ReportApp..class_album_GetList", spms))
        //    {
        //        while (rdr.Read())
        //        {
        //            class_album_Info cat = new class_album_Info();
        //            if (!rdr.IsDBNull(0)) cat.albumid = rdr.GetInt32(0);
        //            if (!rdr.IsDBNull(1)) cat.albumName = rdr.GetString(1);
        //            if (!rdr.IsDBNull(2)) cat.photoCount = rdr.GetInt32(2);
        //            if (!rdr.IsDBNull(3)) cat.Coverphotoupdatetime = rdr.GetDateTime(3);
        //            if (!rdr.IsDBNull(4)) cat.Net = rdr.GetInt32(4);
        //            if (!rdr.IsDBNull(5)) cat.cid = rdr.GetInt32(5);
        //            if (!rdr.IsDBNull(6)) cat.cname = rdr[6].ToString();
        //            if (!rdr.IsDBNull(7)) cat.coverphoto = rdr[7].ToString();
        //            if (!rdr.IsDBNull(8)) cat.yp =rdr.GetInt32(8);

        //            ls.Add(cat);
        //        }
        //    }
        //    ls = ls.OrderByDescending(x => x.Coverphotoupdatetime).ToList();
        //    lsx = (
        //            from x in ls
        //            group x by new { x.cid, x.cname } into g
        //            select new class_album()
        //            {
        //                cid = g.Key.cid,
        //                cname = g.Key.cname,
        //                albumlist = ls.Where(n => n.cid == g.Key.cid && n.albumid>0).ToList().Count == 0 ? new List<album>() :
        //                        (
        //                            from m in ls.Where(n => n.cid == g.Key.cid).ToList()
        //                            select new album()
        //                            {
        //                                albumid = m.albumid,
        //                                cid=m.cid,
        //                                albumName = m.albumName,
        //                                albumCover = m.albumCover,
        //                                photoCount = m.photoCount
        //                            }
        //                        ).OrderByDescending(x=>x.albumid).ToList()
        //            }
        //                ).ToList();

        //    return lsx;
        //}

        public int Iclass_album_Add(String title, String description, Int32 classId, Int32 kId, Int32 userId, String author)
        {
            SqlParameter[] spms = {
                new SqlParameter("@ReturnValue", SqlDbType.Int, 4),
				new SqlParameter("@Title", SqlDbType.NVarChar, 50),
				new SqlParameter("@Description", SqlDbType.Char, 100),
				new SqlParameter("@ClassId", SqlDbType.Int, 4),
				new SqlParameter("@KId", SqlDbType.Int, 4),
                new SqlParameter("@Author",SqlDbType.NVarChar,50),
				new SqlParameter("@UserId", SqlDbType.Int, 4)
            };
            spms[0].Direction = ParameterDirection.ReturnValue;
            spms[1].Value = title;
            spms[2].Value = description;
            spms[3].Value = classId;
            spms[4].Value = kId;
            spms[5].Value = author;
            spms[6].Value = userId;



            SqlHelper.ExecuteNonQuery(SqlHelper.ConnectionString, CommandType.StoredProcedure, "classapp..class_album_ADD", spms);

            string str = spms[0].Value.ToString();

            Int32 r;
            Int32.TryParse(str, out r);

            return r;

        }


    }
}
