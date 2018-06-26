using OrmLiteVsFastSerializer.Interfaces;
using ServiceStack.OrmLite;
using ServiceStack.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OrmLiteVsFastSerializer
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

            _dbFactory = new OrmLiteConnectionFactory($"Uid={dbUsername};Password={dbPassword};Server={dbAddress};Port={dbPort};Database={dbDatabase}", MySqlDialect.Provider);
            SetTableMeta();
        }

        public void MyTestMethod<T>(T coreObject) where T : CoreObject
        {
            long id = 0;
            using (var _db = _dbFactory.Open())
            {
                id = _db.Insert<T>(coreObject, selectIdentity: true);

                if (DateTime.Now.Ticks == 0)
                {
                    coreObject.Id = (uint)id;
                    _db.Delete(coreObject);
                }
                if (DateTime.Now.Ticks == 0)
                {
                    _db.DeleteById<Customer>(id);
                }
                if (DateTime.Now.Ticks == 0)
                {
                    coreObject.Id = (uint)id;
                    coreObject.ObjectName = "FUCK A DUCK";
                    _db.Update<Customer>(coreObject);
                }
            }           
        }

        public T Fetch<T>(uint coreObjectId) where T : CoreObject
        {
            return null;
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

        internal List<T> FetchAll<T>()
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
