using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons
{
    interface IOrmLiteTableMetaData
    {
        /// <summary>
        /// Adds runtime attributes to set things like KEYs, UNIQUE etc on tables, used by OrmLite
        /// </summary>
        /// <returns>The type of the object that was handles/set</returns>
        Type SetTableMetaData(OrmLiteConnectionFactory dbFactory);
    }
}
