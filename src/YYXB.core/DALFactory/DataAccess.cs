using System;
using System.Reflection;
using System.Configuration;
using com.zgyey.Cache;
using YYXB.IDal;

namespace YYXB.Core.DALFactory
{
    /// <summary>
    /// 抽象工厂模式创建DAL。
    /// web.config 需要加入配置：(利用工厂模式+反射机制+缓存机制,实现动态创建不同的数据层对象接口)  
    /// <appSettings>  
    /// <add key="DAL" value="com.zgygy.AndroidService.sqlserverdal" /> (这里的命名空间根据实际情况更改为自己项目的命名空间)
    /// </appSettings> 
    /// </summary>
    public sealed class DataAccess
    {
        private static readonly string AssemblyPath = ConfigurationManager.AppSettings["DAL"];
        private static ZGYEYCache DataCache = ZGYEYCache.GetCacheService();
        /// <summary>
        /// 创建对象或从缓存获取
        /// </summary>
        public static object CreateObject(string AssemblyPath, string ClassNamespace)
        {
            object objType = DataCache.RetrieveObject(ClassNamespace);//从缓存读取
            if (objType == null)
            {
                try
                {
                    objType = Assembly.Load(AssemblyPath).CreateInstance(ClassNamespace);//反射创建
                    DataCache.AddObject(ClassNamespace, objType);// 写入缓存
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return objType;
        }


        public static IKin CreateKin()
        {
            string ClassNamespace = AssemblyPath + ".Kin_DAL";
            object objType = CreateObject(AssemblyPath, ClassNamespace);
            return (IKin)objType;
        }


  
    }
}