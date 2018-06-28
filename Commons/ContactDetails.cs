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
        private List<ContactItem> _contactItemList = new List<ContactItem>();
        private ContactItem _currentContactItem;
        private ContactItem _defaultContactItem;

        /// <summary>
        /// A list of possible ways to contact the actor, such as by phone or mail.
        /// </summary>
        public List<ContactItem> ContactItemList
        {
            get { return _contactItemList; }
            set { _contactItemList = value; }
        }

        /// <summary>
        /// The currently selected method of contacting the actor
        /// </summary>
        /// <value>The current contact item.</value>
        public ContactItem CurrentContactItem
        {
            get { return _currentContactItem; }
            set { _currentContactItem = value; }
        }

        /// <summary>
        /// The default approach to contacting the actor, such as TL-SAM for a vehicle.
        /// </summary>
       public ContactItem DefaultContactItem
        {
            get { return _defaultContactItem; }
            set { _defaultContactItem = value; }
        }

        /// <summary>
        /// 2012-05-18: Ifall de sekundära kontaktsätten skall användas direkt. Det innebär att systemet direkt kommer skicka ut informationen
        /// på de sekundära kontaktsätten och inte avvakta leverans via de primära.
        /// </summary>
        private bool _ignorePrimaryWaitBuffer = false;
        public bool IgnorePrimaryWaitBuffer
        {
            get
            {
                /* 2014-05-28
                bool hasPrimary = false;
                foreach (ContactItem ci in _contactItemList)
                {
                    ContactType ct = (ContactType)Activator.CreateInstance(ci.ContactType, null);
                    if (ct._IsPrimary)
                    {
                        hasPrimary = true;
                        break;
                    }
                }
                if (!hasPrimary)
                    return true;*/
                return _ignorePrimaryWaitBuffer;
            }
            set
            {
                _ignorePrimaryWaitBuffer = value;
            }
        }

        public ContactDetails(List<ContactItem> contactItemList, ContactItem currentContactItem, ContactItem defaultContactItem)
        {
            _contactItemList = contactItemList;
            _currentContactItem = currentContactItem;
            _defaultContactItem = defaultContactItem;
        }

        /// <summary>        
        /// </summary>
        public ContactDetails()
        {
        }

        public List<ContactItem> _GetSecondaryContactItems()
        {
            List<ContactItem> items = new List<ContactItem>();

            List<int> removes = new List<int>();

            if (this.ContactItemList != null)
            {
                ContactItem[] list = this.ContactItemList.ToArray();
                int counter = 0;
                foreach (ContactItem ci in list)
                {
                    if (ci == null)
                    {
                        removes.Add(counter);
                        continue;
                    }

                    if (!ci.IsPrimary())
                        items.Add(ci);
                    counter++;
                }

                foreach (int id in removes)
                    this.ContactItemList.RemoveAt(id);
            }
            return items;
        }

        public List<ContactItem> _GetPrimaryContactItems()
        {
            List<ContactItem> items = new List<ContactItem>();

            if (this.ContactItemList != null)
            {
                ContactItem[] list = this.ContactItemList.ToArray();
                foreach (ContactItem ci in list)
                {
                    if (ci.IsPrimary())
                        items.Add(ci);
                }
            }
            return items;
        }
        
    }
}
