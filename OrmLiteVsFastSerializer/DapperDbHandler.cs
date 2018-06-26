using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrmLiteVsFastSerializer
{
    internal class DapperDbHandler
    {
        private string dbUsername = "root";
        private string dbPassword = "root";
        private string dbAddress = "127.0.0.1";
        private string dbPort = "3306";
        private string dbDatabase = "ormlite";

        SqlConnection conn;

        public DapperDbHandler()
        {
            conn = new SqlConnection($"Uid={dbUsername};Password={dbPassword};Server={dbAddress};Database={dbDatabase}");
            conn.Open();
        }

        public void Insert(Customer customer)
        {
            conn.Insert(customer);
        }
    }
}
