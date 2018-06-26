using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using tWorks.Core.CoreCommons;
using tWorks.SQLController.MySQL;

namespace OrmLiteVsFastSerializer
{
    internal class RelationalDbHandler
    {
        MySQLHandler mysqlHandler;
        private string databaseName = "ormlite";
        public event EventHandler<string> OnLogEvent;

        public RelationalDbHandler()
        {

        }

        public void Start()
        {
            mysqlHandler = new MySQLHandler("127.0.0.1", "root", "root", databaseName);
            mysqlHandler.Connect();
        }

        public void Save<T>(T coreObject)
        {
            PropertyInfo[] pInfos = coreObject.GetType().GetProperties();
            string valueString = string.Join(",", pInfos.Select(x => x.Name));
            string values = "";
            foreach(PropertyInfo pInfo in pInfos)
            {
                //pInfo.PropertyType.IsPrimitive
            }

            MySqlCommand cmd = new MySqlCommand($"INSERT INTO customers ({valueString}) VALUES ({values})");
            //mysqlHandler.Execute()
        }
    }
}
