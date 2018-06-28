using ServiceStack.OrmLite;
using ServiceStack.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace Commons
{
    public class OrmLiteDbHandler
    {
        private string dbUsername = "root";
        private string dbPassword = "root";
        private string dbAddress = "127.0.0.1";
        private string dbPort = "3306";
        private string dbDatabase = "ormlite";

        private static OrmLiteConnectionFactory _dbFactory;

        public event EventHandler<string> OnLogEvent;

        public OrmLiteDbHandler()
        {
        }

        public void Start()
        {
            InitOrmLite();
        }

        private void InitOrmLite()
        {
            JsConfig.IncludeTypeInfo = true;
            OrmLiteConfig.ThrowOnError = JsConfig.ThrowOnError = true;
            //OrmLiteConfig.BeforeExecFilter = dbCmd => Console.WriteLine(dbCmd.GetDebugString());
            _dbFactory = new OrmLiteConnectionFactory($"Uid={dbUsername};Password={dbPassword};Server={dbAddress};Port={dbPort};Database={dbDatabase};SslMode=None", MySqlDialect.Provider);
            SetTableMeta();
        }

        public void AddRow<T>(T coreObject) where T : CoreObject
        {
            long id = 0;
            using (var _db = _dbFactory.Open())
            {
                id = _db.Insert<T>(coreObject, selectIdentity: true);
            }           
        }
        
        private void SetTableMeta()
        {
            // We get the current assembly through the current class
            var currentAssembly = Assembly.GetExecutingAssembly();

            // we filter the defined classes according to the interfaces they implement
            var stuff = currentAssembly.DefinedTypes.Where(type => type.ImplementedInterfaces.Any(inter => inter == typeof(IOrmLiteTableMetaData))).ToList();
            
            foreach(Type type in stuff)
            {
                IOrmLiteTableMetaData temp = (IOrmLiteTableMetaData)Activator.CreateInstance(type);
                temp.SetTableMetaData(_dbFactory);
            }
            OnLogEvent?.Invoke(this, $"{stuff.Count} table meta data initialized");
        }

        public List<T> FetchAll<T>()
        {
            using (var _db = _dbFactory.Open())
            {
                List<T> list = _db.Select<T>();
                return list;
            }
        }

        internal void Delete(Customer customer)
        {
            using (var _db = _dbFactory.Open())
            {
                int i = _db.Delete(customer);
            }
        }

        internal void Delete(uint id)
        {
            using (var _db = _dbFactory.Open())
            {
                int i = _db.DeleteById<Customer>(id);
            }
        }
    }
}
