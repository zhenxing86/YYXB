using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using YYXB.Core;

namespace YYXB.Controllers
{
    public class UtilsController: Controller
    {
        public string UploadFile()
        {
            string retUrl = "0";
            Response.ContentType = "application/json";
            try
            {
                retUrl = Upload();
            }
            catch (Exception ex)
            {
                retUrl = ex.Message;
            }
            return retUrl;
        }

        public string UploadFileEx()
        {
            string retUrl = "0";
            Response.ContentType = "application/json";
            try
            {
                retUrl = UploadEx();
            }
            catch (Exception ex)
            {
                retUrl = ex.Message;
            }
            return retUrl;
        }

        private string Upload()
        {
            //var file = Request.Files["filedata"];
            //if (file == null)
            //    return "文件异常";
            ////定义允许上传的文件扩展名
            //Hashtable extTable = new Hashtable();
            //extTable.Add("image", "gif,jpg,jpeg,png,bmp");
            ////extTable.Add("file", "doc,xls,ppt,docx,xlsx,pptx,pdf,txt,mp3");
            ////extTable.Add("media", "mp3");

            //string filename = System.Guid.NewGuid().ToString();
            //string fileExt = Path.GetExtension(file.FileName).ToLower();

            //if (String.IsNullOrEmpty(fileExt) || Array.IndexOf(((String)extTable["image"]).Split(','), fileExt.Substring(1).ToLower()) == -1)
            //{
            //    return "上传文件扩展名是不允许的扩展名。只允许" + ((String)extTable["image"]) + "格式。";
            //}

            string returnrul = "";
            string uploadPath = "/YYXBImages/" + DateTime.Now.ToString("yyyyMMdd") + "/";

            string filepath = Server.MapPath(uploadPath);
            if (!Directory.Exists(filepath))
            {
                Directory.CreateDirectory(filepath);
            }
            //string width = Request["width"] ?? "324";
            //string height = Request["height"] ?? "242";
            //int swidth = 324;
            //int sheight = 242;
            //if (!int.TryParse(width, out swidth))
            //{
            //    swidth = 324;
            //} 
            //if (!int.TryParse(height, out sheight))
            //{
            //    sheight = 242;
            //}
            //string retainOriginal = Request["retainOriginal "] ?? "0";
            HttpFileCollectionBase fs = Request.Files;
            for (int i = 0; i < fs.Count; i++)
            {
                HttpPostedFileBase file = fs[i];
                string filename = System.Guid.NewGuid().ToString();
                string fileExt = Path.GetExtension(file.FileName).ToLower();
                string fullFileName = Path.Combine(filepath, string.Format("{0}{1}", filename, fileExt));
                string smallFileName = Path.Combine(filepath, string.Format("{0}_small{1}", filename, fileExt));

                Stream st = file.InputStream;
                BinaryReader br = new BinaryReader(st);
                byte[] imgdata = br.ReadBytes(Int32.Parse(st.Length.ToString()));
                FileStream bignewFile = new FileStream(fullFileName, FileMode.Create);
                bignewFile.Write(imgdata, 0, imgdata.Length);
                bignewFile.Close();
                br.Close();
                st.Close();

                //PTImage.CutForCustom(fullFileName, smallFileName, swidth, sheight, 95);
                //if (retainOriginal == "0")
                //{
                //    System.IO.File.Delete(fullFileName);
                //}
                //returnrul += "," + string.Format("http://{0}/{1}/{2}{3}", Request.Url.Authority, uploadPath, filename + "_small", fileExt);

                returnrul += "," + string.Format("http://{0}/{1}/{2}{3}", Request.Url.Authority, uploadPath, filename, fileExt);
            }

            if (returnrul.Length > 0)
                returnrul = returnrul.Substring(1);

            return returnrul;
        }

        private string UploadEx()
        {
            string returnrul = "";
            string uploadPath = "/YYXBFiles/" + DateTime.Now.ToString("yyyyMMdd") + "/";

            string filepath = Server.MapPath(uploadPath);
            if (!Directory.Exists(filepath))
            {
                Directory.CreateDirectory(filepath);
            }
            HttpFileCollectionBase fs = Request.Files;
            for (int i = 0; i < fs.Count; i++)
            {
                HttpPostedFileBase file = fs[i];
                string filename = System.Guid.NewGuid().ToString();
                string fileExt = Path.GetExtension(file.FileName).ToLower();
                string fullFileName = Path.Combine(filepath, string.Format("{0}{1}", filename, fileExt));

                Stream st = file.InputStream;
                BinaryReader br = new BinaryReader(st);
                byte[] imgdata = br.ReadBytes(Int32.Parse(st.Length.ToString()));
                FileStream bignewFile = new FileStream(fullFileName, FileMode.Create);
                bignewFile.Write(imgdata, 0, imgdata.Length);
                bignewFile.Close();
                br.Close();
                st.Close();

                returnrul += "," + string.Format("http://{0}/{1}/{2}{3}", Request.Url.Authority, uploadPath, filename, fileExt);
            }

            if (returnrul.Length > 0)
                returnrul = returnrul.Substring(1);

            return returnrul;
        }

    }
}
