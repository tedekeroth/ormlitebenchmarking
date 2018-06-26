using OrmLiteVsFastSerializer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.OrmLite;
using tWorks.Alfa.AlfaCommons.Actors;
using ServiceStack.DataAnnotations;
using ServiceStack;
using System.Data;

namespace OrmLiteVsFastSerializer.MetaDataImplementations
{
    internal class MetaDataCustomer : IOrmLiteTableMetaData
    {
        public Type SetTableMetaData(OrmLiteConnectionFactory dbFactory)
        {
            Type type = typeof(Customer);
            type.GetProperty(nameof(Customer.Id)).AddAttributes(new PrimaryKeyAttribute(), new AutoIncrementAttribute());
                
            type.GetProperty(nameof(Customer.PopulationRegistryNumber)).AddAttributes(new IndexAttribute { Unique = false });
            type.GetProperty(nameof(Customer.Firstname)).AddAttributes(new IndexAttribute { Unique = false });
            type.GetProperty(nameof(Customer._MiddleName)).AddAttributes(new IndexAttribute { Unique = false });
            type.GetProperty(nameof(Customer.Lastname)).AddAttributes(new IndexAttribute { Unique = false });
            type.GetProperty(nameof(Customer.Username)).AddAttributes(new IndexAttribute { Unique = false });
            type.GetProperty(nameof(Customer.Century)).AddAttributes(new IndexAttribute { Unique = false });

            using (var _db = dbFactory.Open())
            {
                // AlterTable will create if not exist, otherwise add columns that was added to the PCO
                _db.AlterTable<Customer>(MySqlDialect.Provider);
            }
            
            return type;
        }
    }
}
