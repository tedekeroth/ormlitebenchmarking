using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using tWorks.Alfa.AlfaCommons.Actors;
using tWorks.Core.CoreCommons;
using tWorks.SQLController;
using tWorks.SQLController.MySQL;

namespace OrmLiteVsFastSerializer
{
    public class DbHandler
    {
        MySQLHandler mysqlHandler;
        protected Dictionary<Type, tWorks.Core.CoreCommons.CoreObjectTabelledSettings> _type2TableSettings;
        protected Dictionary<Type, string> _type2Tables;
        private string databaseName = "ormlite";

        public event EventHandler<string> OnLogEvent;

        public DbHandler()
        {
            
        }

        public void Start()
        {
            mysqlHandler = new MySQLHandler("127.0.0.1", "root", "root", databaseName);
            mysqlHandler.Connect();
            CreateType2Tables();
            CheckCoreObjectTabelledTables();
        }

        private SQL_Result ExecuteNonQueryCmdExtended(MySqlCommand cmd)
        {
            SQL_Result sR = mysqlHandler.Execute(cmd);
            int failCounter = 0;
            return sR;
        }

        public string AddCoreObjectTabelledToDatabase(uint id, Type type, tWorks.Core.CoreCommons.CoreObjectTabelled coreObj, byte[] data)
        {
            CoreObjectTabelledSettings cOT = _type2TableSettings[coreObj.GetType()];
            string tableName = cOT.TableName;

            List<tWorks.Core.CoreCommons.TableColumnItem> values = tWorks.Core.CoreCommons.CoreObjectTabelledFunctions.GetTableValues((tWorks.Core.CoreCommons.CoreObject)coreObj, _type2TableSettings[coreObj.GetType()]);

            coreObj.AddAdditionalColumns(values);
            MySql.Data.MySqlClient.MySqlCommand cmd = new MySql.Data.MySqlClient.MySqlCommand();
            cmd.Parameters.AddWithValue("?id", id);
            cmd.Parameters.AddWithValue("?data", data);
            cmd.Parameters.AddWithValue("?objtype", type.FullName);
            cmd.CommandTimeout = 100;
            foreach (tWorks.Core.CoreCommons.TableColumnItem item in values)
                cmd.Parameters.AddWithValue("?" + item.GetTableColumnName(), item.Value);
            StringBuilder sB = new StringBuilder("INSERT INTO " + tableName + " (id, objectType, data");
            foreach (tWorks.Core.CoreCommons.TableColumnItem item in values)
                sB.Append("," + item.GetTableColumnName());
            sB.Append(") VALUES (?id, ?objtype, ?data");
            foreach (tWorks.Core.CoreCommons.TableColumnItem item in values)
                sB.Append(", ?" + item.GetTableColumnName());

            sB.Append(")");
            cmd.CommandText = sB.ToString();

            //lock (lockObject)
            //{
            SQL_Result result = ExecuteNonQueryCmdExtended(cmd);

            if (result.NbrOfRowsAffected == 1)
                return "OK";
            else
                return result.SQLError;
            //}
        }

        internal uint GetAutoIncrementValue<T>()
        {
            string tn = _type2TableSettings[typeof(T)].TableName;
            SQL_Result result = mysqlHandler.ExecuteQuery($"SELECT id FROM {tn} ORDER BY id DESC LIMIT 1");
            return result.rows.Count > 0 ? result.GetUInt(0, "id") : 1;
        }

        public void CreateType2Tables()
        {
            _type2Tables = new Dictionary<Type, string>();
            _type2TableSettings = new Dictionary<Type, tWorks.Core.CoreCommons.CoreObjectTabelledSettings>();
            Assembly[] asms = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in asms)
            {
                Type[] typeArray = assembly.GetTypes();
                foreach (Type type in typeArray)
                {
                    if (type.IsSubclassOf(typeof(tWorks.Core.CoreCommons.CoreObject)))
                    {
                        #region Kontrollera att det finns tabeller för alla coreObjectTabelled-objekt och att de har rätt fält
                        if (typeof(tWorks.Core.CoreCommons.CoreObjectTabelled).IsAssignableFrom(type))
                        {
                            tWorks.Core.CoreCommons.CoreObjectTabelled cOT = (tWorks.Core.CoreCommons.CoreObjectTabelled)Activator.CreateInstance(type);
                            tWorks.Core.CoreCommons.CoreObjectTabelledSettings tS = cOT.GetTableSettings();
                            string tableName = "obj_" + type.Name.ToLower();
                            if (tS.SameTableAsParent)
                            {
                                tableName = "obj_" + type.BaseType.Name.ToLower();
                                tWorks.Core.CoreCommons.CoreObjectTabelled cOT2 = (tWorks.Core.CoreCommons.CoreObjectTabelled)Activator.CreateInstance(type.BaseType);
                                tS = cOT2.GetTableSettings();
                                tS.SQLLoadString = "1=0";
                            }
                            tS.TableName = tableName;
                            _type2Tables.Add(type, tableName);

                            if (!this._type2TableSettings.ContainsKey(type))
                            {
                                this._type2TableSettings.Add(type, tS);
                            }
                        }
                        #endregion
                    }
                }
            }
        }

        public void CheckCoreObjectTabelledTables()
        {
            if (_type2Tables == null)
            {
                return;
            }

            foreach (KeyValuePair<Type, string> kvp1 in _type2Tables)
            {
                string tableName = kvp1.Value;
                Type type = kvp1.Key;

                if (type == typeof(Customer))
                    CreateCoreObjectTabelled(type, tableName);
            }
        }

        public void CreateCoreObjectTabelled(Type type, string tableName)
        {
            tWorks.Core.CoreCommons.CoreObject tmpObj = (tWorks.Core.CoreCommons.CoreObject)Activator.CreateInstance(type, new object[] { });
            foreach (PropertyInfo pI in type.GetProperties())
            {
                Type t = pI.PropertyType;
                if (t.IsValueType || t == typeof(String))
                    continue;
                tWorks.Core.CoreCommons.Attributes.PropertyAttribute pA = (tWorks.Core.CoreCommons.Attributes.PropertyAttribute)pI.GetCustomAttribute(typeof(tWorks.Core.CoreCommons.Attributes.PropertyAttribute));
                if (pA != null && pA.IsCustomProperty)
                    continue;
                Object o = null;
                try
                {
                    o = Activator.CreateInstance(t, new object[] { });
                }
                catch (Exception e) { }
                if (o != null)
                    pI.SetValue(tmpObj, o);
            }
            CreateCoreObjectTabelled(tmpObj, tableName);
        }

        public void CreateCoreObjectTabelled(tWorks.Core.CoreCommons.CoreObject obj, string tableName)
        {
            bool found = false;
            SQL_Result res = mysqlHandler.ExecuteQuery("SHOW TABLES");
            for (int i = 0; i < res.rows.Count; i++)
            {
                if (res.rows[i].values[0].ToString().Equals(tableName, StringComparison.CurrentCultureIgnoreCase))
                    found = true;
            }
            if (!found)
                mysqlHandler.ExecuteQuery("CREATE TABLE " + tableName + " (`id` INT(10) UNSIGNED NOT NULL, `objectType` VARCHAR(255) NOT NULL, `data` LONGBLOB NOT NULL,PRIMARY KEY (`Id`)) COLLATE='latin1_swedish_ci' ENGINE=InnoDB;");

            //res = SRef.dbHandler.ProcessQuery("SHOW COLUMNS FROM "+tableName, null);                            
            tWorks.Core.CoreCommons.CoreObjectTabelledSettings tableSettings2 = _type2TableSettings[obj.GetType()];
            Dictionary<string, Type> cols = new Dictionary<string, Type>();
            #region Hitta alla kolumner som ska finnas

            List<tWorks.Core.CoreCommons.TableColumnItem> items = tWorks.Core.CoreCommons.CoreObjectTabelledFunctions.GetTableValues(obj, _type2TableSettings[obj.GetType()]);
            foreach (tWorks.Core.CoreCommons.TableColumnItem item in items)
                cols.Add(item.GetTableColumnName(), item.PropertyType);
            if (tableSettings2.AdditionalColumns != null)
            {
                foreach (KeyValuePair<string, Type> kvp in tableSettings2.AdditionalColumns)
                {
                    cols.Add(kvp.Key.ToLower(), kvp.Value);
                }
            }
            #endregion


            HashSet<string> indexes = new HashSet<string>();
            if (tableSettings2.Indexes != null)
            {
                foreach (string s in tableSettings2.Indexes)
                    indexes.Add(s.ToLower());
            }
            HashSet<string> existingColumns = new HashSet<string>();

            res = mysqlHandler.ExecuteQuery("SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + tableName + "' AND table_schema = '" + databaseName + "'");
            //res = SRef.dbHandler.ProcessQuery("SELECT Field FROM (SHOW COLUMNS FROM " + tableName.ToLower() + ")", null);
            foreach (SQL_Row row in res.rows)
                existingColumns.Add(((string)row.values[0]).ToLower());

            foreach (KeyValuePair<string, Type> kvp in cols)
            {
                if (!existingColumns.Contains(kvp.Key))
                {
                    string t = "VARCHAR(255) NOT NULL DEFAULT ''";
                    if (kvp.Value == typeof(StringBuilder)) // 2014-08-25: hack so that we can specify "TEXT" instead of just VARCHAR
                    {
                        t = "TEXT NULL ";
                    }
                    else if (kvp.Value == typeof(byte))
                        t = "TINYINT";

                    else if (kvp.Value == typeof(ushort) || kvp.Value == typeof(short))
                    {
                        t = "SMALLINT";
                        if (kvp.Value == typeof(ushort))
                            t += " UNSIGNED";
                    }
                    else if (kvp.Value == typeof(uint) || kvp.Value == typeof(int))
                    {
                        t = "INT";
                        if (kvp.Value == typeof(uint))
                            t += " UNSIGNED";
                    }
                    else if (kvp.Value == typeof(ulong) || kvp.Value == typeof(long))
                    {
                        t = "BIGINT";
                        if (kvp.Value == typeof(ulong))
                            t += " UNSIGNED";
                    }
                    else if (kvp.Value == typeof(float))
                    {
                        t = "FLOAT";
                    }
                    else if (kvp.Value == typeof(double) || kvp.Value == typeof(decimal))
                    { //verkar jobbigt att översätta .NET decimal till mysql decimal
                        t = "DOUBLE";
                    }
                    else if (kvp.Value == typeof(DateTime))
                    {
                        t = "DATETIME";
                    }
                    else if (kvp.Value == typeof(bool))
                    {
                        t = "TINYINT";
                    }

                    string query = "ALTER TABLE " + tableName + " ADD COLUMN " + kvp.Key + " " + t;

                    mysqlHandler.ExecuteQuery(query);

                    if (indexes.Contains(kvp.Key))
                    {
                        query = "ALTER TABLE " + tableName + " ADD INDEX `" + kvp.Key + "` (`" + kvp.Key + "`)";
                        mysqlHandler.ExecuteQuery(query);
                    }
                }
            }
        }

        public List<T> ReadObjectsTabelled<T>()
        {
            Type type = typeof(T);
            List<T> returnList = new List<T>();
            //lock (lockObject)
            //{
            DateTime start = DateTime.Now;
            //om ingen where-sats angivits ska alla objekt av typen läsas in precis som med vanliga coreObjects

            {
                string tableName = _type2Tables[type];
                string query = "SELECT id,data FROM " + tableName; //+ " WHERE objectType='" + type.FullName + "'
                SQL_Result r = mysqlHandler.ExecuteQuery(query);
                for (int i = 0; i < r.rows.Count; i++)
                {
                    T o;
                    try
                    {
                        byte[] data = r.GetBytes(i, "data");
                        o = (T)Activator.CreateInstance(type, data);
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Unable to create object of type '" + type + "' and id " + r.rows[i].values[0] + ": " + e.ToString());
                    }
                    if (o != null)
                    {
                        returnList.Add(o);
                    }
                }
            }
           
            TimeSpan ts = DateTime.Now.Subtract(start);
            string name = type.Name;
            int maxLength = 25;
            if (name.Length > maxLength)
                name = name.Substring(0, maxLength);
            else
                name = name.PadRight(maxLength, ' ');

            return returnList;
        }

    }
}
