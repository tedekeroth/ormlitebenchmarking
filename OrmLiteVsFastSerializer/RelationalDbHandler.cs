using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using tWorks.Alfa.AlfaCommons.Actors;
using tWorks.SQLController;
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
            string fields = string.Join(",", pInfos.Select(x => $"{x.Name}"));
            string values = string.Join(",", pInfos.Select(x => $"@{x.Name}"));
            MySqlCommand cmd = new MySqlCommand($"INSERT INTO customer ({fields}) VALUES ({values})");

            foreach (PropertyInfo pInfo in pInfos)
            {
                cmd.Parameters.AddWithValue(pInfo.Name, pInfo.GetValue(coreObject));
            }
            SQL_Result result = mysqlHandler.Execute(cmd);
        }

        internal List<Customer> FetchAll()
        {
            MySqlCommand cmd = new MySqlCommand($"SELECT * FROM customer");
            SQL_Result result = mysqlHandler.ExecuteQuery(cmd);

            PropertyInfo[] pInfos = typeof(Customer).GetProperties();

            List<Customer> customers = new List<Customer>();
            for (int i = 0; i < result.rows.Count; i++)
            {
                Customer c = new Customer();
                foreach (PropertyInfo pInfo in pInfos)
                {
                    object o = result.Get(i, pInfo.Name);
                    if (o is null || o is DBNull)
                        continue;
                    try
                    {
                        pInfo.SetValue(c, Convert.ChangeType(o, pInfo.PropertyType));
                    }
                    catch(Exception e)
                    {

                    }
                }
                customers.Add(c);
            }

            return customers;
        }
    }
}
