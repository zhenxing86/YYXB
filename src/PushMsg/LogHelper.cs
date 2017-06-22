using System;
using System.Collections.Generic;
using System.Text;

using log4net;

namespace ThreedForm
{
    public class LogHelper
    {
        public static readonly log4net.ILog loginfo = log4net.LogManager.GetLogger("loginfo");

        public static readonly log4net.ILog logerror = log4net.LogManager.GetLogger("logerror");

        public static void WriteLog(string info)
        {

            if (loginfo.IsInfoEnabled)
            {
                loginfo.Info(info);
            }
        }

        public static void WriteLogMax6(string mask, params object[] paras)
        {
            WriteLogMax6(false, mask, paras);
        }

        public static void WriteLogMax6(bool error, string mask, params object[] paras)
        {
            try
            {
                string info;

                if (paras == null || paras.Length == 0)
                    return;

                if (paras.Length == 1)
                    info = string.Format(mask, paras[0]);
                else if (paras.Length == 2)
                    info = string.Format(mask, paras[0], paras[1]);
                else if (paras.Length == 3)
                    info = string.Format(mask, paras[0], paras[1], paras[2]);
                else if (paras.Length == 4)
                    info = string.Format(mask, paras[0], paras[1], paras[2], paras[3]);
                else if (paras.Length == 5)
                    info = string.Format(mask, paras[0], paras[1], paras[2], paras[3], paras[4]);
                else if (paras.Length == 6)
                    info = string.Format(mask, paras[0], paras[1], paras[2], paras[3], paras[4], paras[5]);
                else
                    return;


                if (loginfo.IsInfoEnabled)
                {
                    loginfo.Info(info);
                }
            }
            catch
            {

            }
        }

        public static void WriteLog(string info, Exception se)
        {
            if (logerror.IsErrorEnabled)
            {
                logerror.Error(info, se);
            }
        }
    }
}
