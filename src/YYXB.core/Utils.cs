using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Web.Security;

namespace YYXB.Core
{
    public class Utils
    {
        public static string MakeIdentifyCode(string UserName, string Power)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(UserName);
            builder.Append(ConfigurationManager.AppSettings["IdentifyCode"]);
            builder.Append(Power);
            builder.Append(HDVal());
            return Md5hash_String(builder.ToString(), "SHA1");
        }

        public static string HDVal()
        {
            int lpVolumeSerialNumber = 0;
            int lpMaximumComponentLength = 0;
            int lpFileSystemFlags = 0;
            string lpVolumeNameBuffer = null;
            string lpFileSystemNameBuffer = null;
            int num4 = GetVolumeInformation(@"c:\", lpVolumeNameBuffer, 0x100, ref lpVolumeSerialNumber, lpMaximumComponentLength, lpFileSystemFlags, lpFileSystemNameBuffer, 0x100);
            return lpVolumeSerialNumber.ToString();
        }

        [DllImport("kernel32.dll")]
        private static extern int GetVolumeInformation(string lpRootPathName, string lpVolumeNameBuffer, int nVolumeNameSize, ref int lpVolumeSerialNumber, int lpMaximumComponentLength, int lpFileSystemFlags, string lpFileSystemNameBuffer, int nFileSystemNameSize);

        public static string Md5hash_String(string InputString, string format)
        {
            if (format == "SHA1")
            {
                InputString = FormsAuthentication.HashPasswordForStoringInConfigFile(InputString, "SHA1");
                return InputString;
            }
            if (format == "MD5")
            {
                InputString = FormsAuthentication.HashPasswordForStoringInConfigFile(InputString, "MD5");
            }
            return InputString;
        }

    }
}
