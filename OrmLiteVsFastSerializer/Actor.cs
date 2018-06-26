using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrmLiteVsFastSerializer
{
    [Serializable]
    public class Actor : CoreObject
    {
        #region Fields (6) 

        // 2009-08-13: Not needed anymore. private ActorCategories _actorCategory;
        private List<ContactItem> _contactInfo;
        private ContactItem _currentContactItem;
        private ContactItem _defaultContactItem;
        private string _firstname;
        private string _lastname;
        private string _username;

        #endregion Fields 

        #region Constructors (2) 

      
        public Actor()
        {
            this.ContactInfo = new List<ContactItem>();
            this.ContactDetails = new ContactDetails(new List<ContactItem>(), null, null);
        }

        #endregion Constructors 

        #region Properties (9) 

        public uint _ActorRole { get; set; }

        /// <summary>
        /// Used for deciding how an actor may connect to the system, such as Website, OperatorClient etc. The Type is supposed to be a ContactType
        /// </summary>
        public List<Type> _AllowedLoginChannels { get; set; }

        /// <summary>
        /// Contains a list of all available contact items, the default and the current contact item.
        /// </summary>
        // 2016-01-10:  Att lägga till attribut strular rejält med jefla CoreObjectTabelled.
        //              Olika saker utgår från att fälten i ContactDetails finns i tabellen, vilket det inte gör, och yada yada.
        //    [Attributes.Property("kontaktinformation")]
        //  
        public ContactDetails ContactDetails { get; set; }

        /// <summary>
        /// OBSOLETE! Use ContactDetails instead.
        /// </summary>
        [Obsolete]
        public List<ContactItem> ContactInfo
        {
            get
            {
                if (ContactDetails != null)
                    return ContactDetails.ContactItemList;
                else
                    return null;
            }
            set
            {
                if (ContactDetails != null)
                    ContactDetails.ContactItemList = value;
            }
        }

        /// <summary>
        /// OBSOLETE! Use ContactDetails instead.
        /// </summary>
        /// <value>The current contact item.</value>
        [Obsolete]
        public ContactItem CurrentContactItem
        {
            get
            {
                if (ContactDetails != null)
                    return ContactDetails.CurrentContactItem;
                else
                    return null;
            }
            set
            {
                if (ContactDetails != null)
                    ContactDetails.CurrentContactItem = value;
            }
        }

        /// <summary>
        /// OBSOLETE! Use ContactDetails instead.
        /// </summary>
        [Obsolete]
        public ContactItem DefaultContactItem
        {
            get
            {
                if (ContactDetails != null)
                    return ContactDetails.DefaultContactItem;
                else
                    return null;
            }
            set
            {
                if (ContactDetails != null)
                    ContactDetails.DefaultContactItem = value;
            }
        }

        /// <summary>
        /// The first name of the actor/user.
        /// </summary>
        public string Firstname
        {
            get { return _firstname; }
            set { _firstname = value; }
        }

        /// <summary>
        /// Det valda språket för denna användare
        /// </summary>
       public uint Language { get; set; }

        /// <summary>
        /// The last name of the actor/user.
        /// </summary>
        public string Lastname
        {
            get { return _lastname; }
            set { _lastname = value; }
        }

        /// <summary>
        /// The username of the actor, such as "8102151435" or "EHakansson". Must be unique among all actors
        /// </summary>
        public string Username
        {

            get { return _username; }
            set { _username = value; }
        }

        #endregion Properties 

        #region Methods (8) 

        // Public Methods (8) 


       
        public void ClearContactItems(Type contactItemType)
        {
            this.ContactDetails.ContactItemList = this.ContactDetails.ContactItemList.Where(ci => ci.GetType() != contactItemType).ToList();
        }

       

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            return this.GetHashCode() == obj.GetHashCode();
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
        

        #endregion Methods 

        public Actor(uint id, string username)
        {
            this.Id = id;
            this.Username = username;
            this.ContactDetails = new ContactDetails(new List<ContactItem>(), null, null);
        }

        public List<ContactItem> _GetContactItems(Type contactType)
        {
            return _GetContactItems(new Type[] { contactType });
        }

        /// <summary>
        /// Gets all contectitems that are of type <see cref="{T}"/>
        /// </summary>
        /// <typeparam name="T">The type</typeparam>
        /// <returns></returns>
        public List<T> _GetContactItems<T>()
        {
            List<T> list = ContactDetails.ContactItemList.OfType<T>().ToList();
            return list;
        }
        /// <summary>
        /// Searches the contact item list for the specified contact type and returns a list of found ContactItems.
        /// </summary>
        /// <param name="contactType">the type to search for</param>
        /// <returns>A list of ContactItems that are of the specified type.</returns>
        public List<ContactItem> _GetContactItems(Type[] contactTypes) //ContactItem.ContactTypes contactType)
        {
            List<ContactItem> list = this.ContactDetails.ContactItemList.Where(ci => contactTypes.Contains(ci.GetType())).ToList();
            return list;
        }
        
        /// <summary>
        /// Returns the default contact item data as a string if there is any, otherwise returns an empty string.
        /// </summary>
        public string GetDefaultContactItemData()
        {
            return ContactDetails?.DefaultContactItem?.Data?.ToString() ?? "";
        }
    }
}
