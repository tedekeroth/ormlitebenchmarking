using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons
{
    [Serializable]
    public class ContactDetails
    {
        public List<ContactItem> ContactItemList
        {
            get; set;
        }
        public ContactItem CurrentContactItem
        {
            get; set;
        }
        public ContactItem DefaultContactItem
        {
            get; set;
        }
        public bool IgnorePrimaryWaitBuffer
        {
            get; set;
        }

        public ContactDetails(List<ContactItem> contactItemList, ContactItem currentContactItem, ContactItem defaultContactItem)
        {
            ContactItemList = contactItemList;
            CurrentContactItem = currentContactItem;
            DefaultContactItem = defaultContactItem;
        }

        public ContactDetails()
        {
        }
    }
}
