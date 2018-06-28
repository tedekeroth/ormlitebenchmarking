using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons
{
    [Serializable]
    public class AlfaOnline : ContactItem
    {
        public string MyProperty1 { get; set; }
        public string SomeOtherProperty { get; set; }
        public string MyProperty3 { get; set; }

        public AlfaOnline(string a, string b, string c)
        {
            MyProperty1 = a;
            SomeOtherProperty = b;
            MyProperty3 = c;
        }
    }
}
