using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.OrmLite;
using ServiceStack.DataAnnotations;
using ServiceStack;
using System.Data;
using Commons;

namespace OrmLiteTests
{
    internal class MetaDataCustomer : MetaDataBaseClass
    {
        protected override Type GetCoreObjectType()
        {
            return typeof(Customer);
        }

        protected override void SetSpecificTableMetaData(OrmLiteConnectionFactory dbFactory)
        {
            Type type = GetCoreObjectType();
            type.AddAttributes(new AliasAttribute("APA_" + type.Name));
            type.GetProperty(nameof(Customer.PopulationRegistryNumber)).AddAttributes(new IndexAttribute { Unique = false });
            type.GetProperty(nameof(Customer.Firstname)).AddAttributes(new IndexAttribute { Unique = false });
            type.GetProperty(nameof(Customer._MiddleName)).AddAttributes(new IndexAttribute { Unique = false });
            type.GetProperty(nameof(Customer.Lastname)).AddAttributes(new IndexAttribute { Unique = false });
            type.GetProperty(nameof(Customer.Century)).AddAttributes(new IndexAttribute { Unique = false });
        }
    }
}
