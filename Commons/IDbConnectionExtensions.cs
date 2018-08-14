using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Data
{
    public static class IDbConnectionExtensions
    {
        private static List<string> GetColumnNames(IDbConnection db, string tableName, IOrmLiteDialectProvider provider)
        {
            var columns = new List<string>();
            using (var cmd = db.CreateCommand())
            {
                cmd.CommandText = getCommandText(tableName, provider);
                var tbl = new DataTable();
                tbl.Load(cmd.ExecuteReader());
                for (int i = 0; i < tbl.Columns.Count; i++)
                {
                    columns.Add(tbl.Columns[i].ColumnName);
                }

            }
            return columns;
        }

        private static string getCommandText(string tableName, IOrmLiteDialectProvider provider)
        {
            if (provider == MySqlDialect.Provider)
                return string.Format("select * from {0} limit 1", tableName);
            return string.Format("select * from {0} limit 1", tableName);
        }

        public static void AlterTable<T>(this IDbConnection db, IOrmLiteDialectProvider provider) where T : new()
        {
            
            ModelDefinition md = new ModelDefinition();
            md.ModelType = typeof(T);
            
            var model = ModelDefinition<T>.Definition;
            var namingStrategy = provider.NamingStrategy;
            // just create the table if it doesn't already exist
            var tableName = namingStrategy.GetTableName(model.ModelName);
            if (db.TableExists(tableName) == false)
            {
                db.CreateTable<T>(overwrite: false);
                return;
            }

            // find each of the missing fields
            var columns = GetColumnNames(db, model.ModelName, provider);
            var missing = ModelDefinition<T>.Definition.FieldDefinitions
                                            .Where(field => columns.Contains(namingStrategy.GetColumnName(field.FieldName)) == false)
                                            .ToList();
            if (missing.Count == 0)
                return;

            string alterSql = "";
            string addSql = "";
            // add a new column for each missing field
            foreach (var field in missing)
            {
                var alt = db.GetDialectProvider().ToAddColumnStatement(typeof(T), field); // Should be made more efficient, one query for all changes instead of many
                int index = alt.IndexOf("ADD ");
                alterSql = alt.Substring(0, index);
                addSql += alt.Substring(alt.IndexOf("ADD COLUMN")).Replace(";", "") + ", ";
            }
            if (addSql.Length > 2)
                addSql = addSql.Substring(0, addSql.Length - 2);
            string fullSql = alterSql + addSql;
            db.ExecuteSql(fullSql);
        }
    }
}
