using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using OrmLiteTests;
using ServiceStack;
using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.Converters;
using ServiceStack.Text;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Collections;
using Newtonsoft.Json;
using System.Linq.Expressions;

namespace Commons
{
    public class OrmLiteDbHandler
    {
        private List<string> _dbAccounts;

        private string dbUsername = "root";
        private string dbPassword = "root";
        private string dbAddress = "127.0.0.1";
        private string dbPort = "3306";
        private string dbDatabase = "ormlite";
        private Random randomizer;

        private static OrmLiteConnectionFactory _dbFactory;

        public event EventHandler<string> OnLogEvent;

        public OrmLiteDbHandler()
        {
            _dbAccounts = new List<string>() { "test1", "test2" };
            randomizer = new Random(DateTime.Now.Millisecond);
        }

        public void Start()
        {
            InitOrmLite();
        }

        private T GetTest<T>(Dictionary<string, object> d, string key)
        {
            object o = d[key];
            if (o is T)
                return (T)o;

            object o2 = Convert.ChangeType(o, typeof(T));
            return (T)o2;
        }

        private void InitOrmLite()
        {
            //Dictionary<string, object> test = new Dictionary<string, object>();
            //test.Add("test1", "true");
            //GetTest<bool>(test, "test1");

            //test["test1"] = new JsvStringSerializer().SerializeToString<List<uint>>(new List<uint> { 1, 2, 3 });
            //GetTest<List<uint>>(test, "test1");

            //test["test1"] = new List<uint> { 1, 2, 3 };
            //GetTest<bool>(test, "test1");
            
            // JsConfig.IncludeTypeInfo = true;
            OrmLiteConfig.ThrowOnError = JsConfig.ThrowOnError = true;
            // OrmLiteConfig.BeforeExecFilter = dbCmd => Console.WriteLine(dbCmd.GetDebugString());
            
            // Tracer.Instance = new ConsoleTracer();

            MySqlDialect.Provider.RegisterConverter<string>(new MyStringConverter());
            MySqlDialect.Provider.RegisterConverter<DateTime>(new SpecialMySqlDateTimeConverter());
            MySqlDialect.Provider.RegisterConverter<TimeSpan>(new MyTimeSpanConverter());
            MySqlDialect.Provider.StringSerializer = new MyJsonSerializer();

            _dbFactory = new OrmLiteConnectionFactory($"Uid={dbUsername};Password={dbPassword};Server={dbAddress};Port={dbPort};Database=ormlite;SslMode=None;Allow User Variables=True", MySqlDialect.Provider);

            StringConverter converter = OrmLiteConfig.DialectProvider.GetStringConverter();
            Console.WriteLine(converter.GetColumnDefinition(255));
            Console.WriteLine(converter.GetColumnDefinition(1000));
            Console.WriteLine(converter.GetColumnDefinition(70000));

            SetTableMeta2();

            int c = 0;
            int temp = 0;
            List<Type> coreObjectTypes = GetCoreObjectTypes();
            List<string> tablesCreated = new List<string>();

            foreach(Type type in coreObjectTypes)
            {
                //if (type.IsSubclassOf(typeof(CoreObjectTemporary)))
                //{
                //    temp++;
                //    continue;
                //}   

                using (IDbConnection db = _dbFactory.Open())
                {
                    try
                    {
                        string tableName = type.GetModelMetadata().ModelName;
                        bool exists = db.TableExists(tableName);
                        if (exists)
                        {
                            Console.WriteLine($"EXISTS: {tableName}");
                        }
                        
                        {
                            db.CreateTable(false, type);
                            tablesCreated.Add(tableName.ToLower());
                            c++;
                        }
                        
                        
                    }
                    catch(MySqlException mex)
                    {
                        Console.WriteLine(mex);
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine(e);
                    }
                    
                }
            }
            Console.WriteLine($"C={c}, temp={temp}");
            tablesCreated.Sort();
            Console.WriteLine(string.Join(Environment.NewLine, tablesCreated));
        }

        private void RunAssert(CoreObject co)
        {
            if (co._CustomProperties["MyBoolProp"] is bool)
                Console.WriteLine("    MyBoolProp is bool :)");
            else
                Console.WriteLine("    MyBoolProp is NOT bool"); 

            if (co._CustomProperties["MyDateTime"] is DateTime)
                Console.WriteLine("    MyDateTime is dateTime :)");
            else
                Console.WriteLine("    MyDateTime is NOT datetime");

            if (co._CustomProperties["Settings"] is CustomerSettings)
            {
                Console.WriteLine("    Settings is CustomerSettings :)");
            }
            else
                Console.WriteLine("    Settings is NOT CustomerSettings");

            if (co._CustomProperties["MyList"] is IList && co._CustomProperties["MyList"].GetType().IsGenericType)
            {
                Console.WriteLine("    MyList is generic List :)");
            }
            else
                Console.WriteLine("    MyList is NOT generic List");

            if (co is Customer)
            {
                Console.WriteLine("    is Customer :)");
                Customer cust = (Customer)co;
                
                if (cust.ContactDetails?.ContactItemList?[0] is AlfaOnline)
                    Console.WriteLine("    ContactItem is AlfaOnline :)");
                else
                    Console.WriteLine("    ContactItem is NOT AlfaOnline");
            }
            else
                Console.WriteLine("    is NOT customer");
        }

        private void RunInsertAndRead()
        {
            Customer c = new Customer() { Username = "TED ÅÄÖ", DeletedTime = DateTime.MaxValue, MyTimeSpan = new TimeSpan(1, 30, 0) };
            c.ContactDetails = new ContactDetails();
            c.ContactDetails.ContactItemList = new List<ContactItem>();
            c.ContactDetails.ContactItemList.Add(new AlfaOnline("a", "b", "C"));
            c._SetCustomProperty("MyBoolProp", true);
            c._SetCustomProperty("MyDateTime", DateTime.Now);
            c._SetCustomProperty("Settings", new CustomerSettings() { CustomerConnect_NotifCancelled = true, NotifFinished = true });
            c._SetCustomProperty("MyList", new List<uint> { 1,2,3,4 });

            CoreObject co = c;
            long id;

            using (IDbConnection db = _dbFactory.Open())
            {
                var typedApi = db.CreateTypedApi(co.GetType());
                id = typedApi.Insert(co, selectIdentity: true);

                Console.WriteLine($"  Insert - Untyped: {id}");

                string tableName = co.GetType().GetModelMetadata().ModelName;
                List<Dictionary<string, object>> results = db.Select<Dictionary<string, object>>($"SELECT * FROM {tableName} where id={id}");
                List<CoreObject> coreObjects = results.Map(x => (CoreObject)x.FromObjectDictionary(co.GetType()));
                Console.WriteLine($"  Read - Untyped + FromObjectDict: {id}");
                RunAssert(coreObjects[0]);

                Console.WriteLine($"  Read - Typed: {id}");
                coreObjects = db.Select<CoreObject>($"SELECT * FROM {tableName} where id={id}");
                Customer cDes = db.SingleById<Customer>(id);
                RunAssert(cDes);
                Console.WriteLine();

                id = db.Insert<Customer>(c, selectIdentity: true);
                Console.WriteLine($"  Insert - Typed: {id}");

                Console.WriteLine($"  Read - Untyped + FromObjectDict: {id}");
                results = db.Select<Dictionary<string, object>>($"SELECT * FROM {tableName} where id={id}");
                coreObjects = results.Map(x => (CoreObject)x.FromObjectDictionary(co.GetType()));
                RunAssert(coreObjects[0]);

                Console.WriteLine($"  Read - Typed: {id}");
                cDes = db.SingleById<Customer>(id);
                RunAssert(cDes);
            };
        }

        public void TestStringSerializers()
        {
            MySqlDialect.Provider.StringSerializer = new JsvStringSerializer();
            Console.WriteLine("** JsvStringSerializer **");
            RunInsertAndRead();
            Console.WriteLine();
            Console.WriteLine("** JsonStringSerializer **");
            MySqlDialect.Provider.StringSerializer = new JsonStringSerializer();
            RunInsertAndRead();
            Console.WriteLine();
            Console.WriteLine("** MyJsonSerializer  **");
            MySqlDialect.Provider.StringSerializer = new MyJsonSerializer();
            RunInsertAndRead();
        }

        public void Test2(string json = null, Type t = null)
        {
            object latDictionary2 = JsonConvert.DeserializeObject(json, t, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All });
        }

        public void Test3()
        {
            using (IDbConnection db = _dbFactory.Open())
            {
                List<Customer> list = db.Select<Customer>("");
            }

                using (IDbConnection db = _dbFactory.Open())
            {
                Driver d = new Driver() { Firstname = "ted" };
                long id = db.Insert<Driver>(d, selectIdentity: true);
                string tableName = d.GetType().GetModelMetadata().ModelName;
                Dictionary<string, object> bastard = new Dictionary<string, object>();
                bastard.Add("Kolumn17", 123);
                bastard.Add("Kolumn18", 8888L);

                IDbCommand cmd = db.CreateCommand();
                List<string> sets = new List<string>();
                foreach (KeyValuePair<string, object> kvp in bastard)
                {
                    sets.Add($"{kvp.Key}=@{kvp.Key}");
                    cmd.AddParam($"@{kvp.Key}", kvp.Value);
                }
                string sql = $"UPDATE {tableName} SET {string.Join(", ", sets)} WHERE id={id}";
                cmd.CommandText = sql;
                int nbr = cmd.ExecNonQuery();

                InsertTest<Customer>(c => c.Username == "Ted");
            }

        }

        private void InsertTest<T>(Expression<Func<T, bool>> lambda)
        {
            using (IDbConnection db = _dbFactory.Open())
            {
                db.Select<T>(lambda);
            }
        }

        private List<Type> GetCoreObjectTypes()
        {
            //List<Type> typesToIgnore = new List<Type>()
            //{
            //    typeof(CoreObjectTabelledMetaData),
            //    typeof(tWorks.Alfa.AlfaCommons.AlfaFault)
            //};

            List<Type> coreObjectTypes = new List<Type>();
            
            Assembly[] asms = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in asms)
            {
                try
                {
                    Type[] typeArray = assembly.GetTypes();
                    foreach (Type type in typeArray)
                    {
                        if (type.IsSubclassOf(typeof(CoreObject)) && type.BaseType != null && type.IsAbstract == false) // && !typesToIgnore.Contains(type))
                        //if (type == typeof(Customer) || type == typeof(SubCoreServer))
                        {
                            coreObjectTypes.Add(type);
                        }
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            return coreObjectTypes;
        }

        public long AddRow<T>(T coreObject) where T : CoreObject
        {
            var useType = coreObject.GetType();

            //string json = Newtonsoft.Json.JsonConvert.SerializeObject(coreObject);
            //CoreObject coNew = Newtonsoft.Json.JsonConvert.DeserializeObject<CoreObject>(json);
            //CoreObject coNew2 = (CoreObject)Newtonsoft.Json.JsonConvert.DeserializeObject(json, useType);

            long id = 0;
            using (var db = _dbFactory.Open())
            {
                var typedApi = db.CreateTypedApi(useType);
                id = typedApi.Insert(coreObject, selectIdentity: true);
            }
            return id;
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

        private void SetTableMeta2()
        {
            // We get the current assembly through the current class
            var currentAssembly = Assembly.GetExecutingAssembly();

            
                // we filter the defined classes according to the interfaces they implement
            var stuff = currentAssembly.DefinedTypes.Where(type => type.IsSubclassOf(typeof(MetaDataBaseClass))).ToList();

            foreach (Type type in stuff)
            {
                IOrmLiteTableMetaData temp = (IOrmLiteTableMetaData)Activator.CreateInstance(type);
                temp.SetTableMetaData(_dbFactory);
            }

            stuff = currentAssembly.DefinedTypes.Where(type => type.IsSubclassOf(typeof(CoreObject))).ToList();
            foreach (Type type in stuff)
            {
                type.AddAttributes(new AliasAttribute("TEST_" + type.Name));
            }

            OnLogEvent?.Invoke(this, $"{stuff.Count} table meta data initialized");
        }

        public List<CoreObject> FetchAll(Type coreObjectType)
        {
            using (var _db = _dbFactory.Open())
            {
                string tableName = typeof(Customer).GetModelMetadata().ModelName;
                List<Dictionary<string, object>> results = _db.Select<Dictionary<string, object>>($"SELECT * FROM {tableName}");
                List<CoreObject> customers = results.Map(x => (CoreObject)x.FromObjectDictionary(coreObjectType));
                return customers;
            }
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

        public int Delete<T>(T coreObject) where T : CoreObject
        {
            using (var db = _dbFactory.Open())
            {
                int id = db.Delete(coreObject);
                return id;
            }
        }

        public T Get<T>(uint id)
        {
            using (var db = _dbFactory.Open())
            {
                return db.SingleById<T>(id);
            }
        }

        public List<Dictionary<string, object>> CustomSelect(string sql)
        {
            using (var db = _dbFactory.Open())
            {
                string tableName = typeof(Customer).GetModelMetadata().ModelName;
                string query = $"SELECT id FROM {tableName} WHERE id ='{1123123123}'";

                uint id = db.Scalar<uint>(query);
                

                //List<Dictionary<string, object>> list = db.Select<Dictionary<string, object>>(sql);
                //return list;
            }
            return null;
        }

        /// <summary>
        /// The optional parameter should be on form of anonymous object: <code>new { myParamater }</code> Must be populated with variables using the same name as in the sql statement. 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters">should be on form of anonymous object</param>
        public void ExecuteSql(string sql, object p = null)
        {
            using (var db = _dbFactory.Open())
            {
                db.ExecuteSql(sql, p);
            }
        }

        public int ExecuteNonSelectSqlCommand(MySqlCommand cmd)
        {
            if (cmd.CommandText.Trim().StartsWith("SELECT", StringComparison.CurrentCultureIgnoreCase))
            {
                throw new ArgumentException("SELECT statements not allowed");
            }

            using (var db = _dbFactory.Open())
            {
                cmd.Connection = (MySqlConnection)db.ToDbConnection();
                return cmd.ExecuteNonQuery();
            }
        }

        public bool CheckConnection(string connectionName = null)
        {
            if (connectionName == null)
                connectionName = GetRandomDbName();
            
            int v = 0;
            try
            {
                using (var db = _dbFactory.Open(connectionName))
                {
                    //db.ExecuteSql($"INSERT INTO test (firstname) values ({DateTime.Now.Millisecond})");
                    //return true;
                    v = db.Single<int>("SELECT 1");
                    
                    if (v == 1)
                    {
                        OnLogEvent?.Invoke(this, $"{connectionName} connected");
                    }
                    else
                    {
                        OnLogEvent?.Invoke(this, $"{connectionName} NOT connected");
                    }

                }
            }
            catch(MySqlException e)
            {
                OnLogEvent?.Invoke(this, $"{nameof(MySqlException)}: {e.Message}");
                return false;
            }
            return v == 1;
        }
        
        private string GetRandomDbName()
        {
            int dbIndex = randomizer.Next(0, _dbAccounts.Count);
            string dbName = _dbAccounts[dbIndex];
            return dbName;
        }

        public uint GetHighestCoreObjectId()
        {
            List<Type> types = GetCoreObjectTypes();
            using (var db = _dbFactory.Open())
            {
                uint max = uint.MinValue;
                foreach (Type t in types)
                {
                    db.DropAndCreateTables(t);
                    string tableName = t.GetModelMetadata().ModelName;
                    uint m = db.Scalar<uint>($"SELECT max(Id) as id FROM {tableName}");
                    if (m > max)
                        max = m;
                }
                
                return max;
            }
        }
    }
}
