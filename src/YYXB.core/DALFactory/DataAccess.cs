using System;
using System.Reflection;
using System.Configuration;
using com.zgyey.Cache;
using YYXB.IDal;

namespace YYXB.Core.DALFactory
{
    /// <summary>
    /// ���󹤳�ģʽ����DAL��
    /// web.config ��Ҫ�������ã�(���ù���ģʽ+�������+�������,ʵ�ֶ�̬������ͬ�����ݲ����ӿ�)  
    /// <appSettings>  
    /// <add key="DAL" value="com.zgygy.AndroidService.sqlserverdal" /> (����������ռ����ʵ���������Ϊ�Լ���Ŀ�������ռ�)
    /// </appSettings> 
    /// </summary>
    public sealed class DataAccess
    {
        private static readonly string AssemblyPath = ConfigurationManager.AppSettings["DAL"];
        private static ZGYEYCache DataCache = ZGYEYCache.GetCacheService();
        /// <summary>
        /// ���������ӻ����ȡ
        /// </summary>
        public static object CreateObject(string AssemblyPath, string ClassNamespace)
        {
            object objType = DataCache.RetrieveObject(ClassNamespace);//�ӻ����ȡ
            if (objType == null)
            {
                try
                {
                    objType = Assembly.Load(AssemblyPath).CreateInstance(ClassNamespace);//���䴴��
                    DataCache.AddObject(ClassNamespace, objType);// д�뻺��
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