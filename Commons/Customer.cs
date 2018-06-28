using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons
{
    [Serializable]
    public class Customer : Actor
    {
        public enum AnbaroStatuses
        {
            EjTillfrågad,
            Tillåtet,
            Nekas
        }

        public enum CustomProperties
        {
            CustomerJourneyComment, DeceasedDate, COAddress, Categories, _Sex, _PersonIdFormat, _MiddleName, _CustomerData, QuickAddressPostAddress, OtherPostAddress, SendConfirmationSMSOnBooking, LoginCredentialsSnailMail
        }

        public enum Sexes
        {
            Unknown,
            Male,
            Female
        }

        public enum PersonIdFormats
        {
            Swedish,
            Norwegian,
            Estonian
        }

        #region Fields (8) 

        private uint _century;
        private List<uint> _legitimations;
        private List<uint> _quickAddresses;
        private bool _updateFromRegAutomatically;

        #endregion Fields 

        #region Constructors (3) 

        public Customer()
        {
        }

        public Customer(uint id, string username, int customerNumber, string firstname, string lastname, List<uint> legitimations) : base(id, username)
        {
            Firstname = firstname;
            Lastname = lastname;
        }

        public Customer(string populationRegistryNumber, string firstname, string lastname) : base()
        {
            populationRegistryNumber = populationRegistryNumber.Replace("-", "");
            this.PopulationRegistryNumber = populationRegistryNumber;
            this.Firstname = firstname;
            this.Lastname = lastname;
        }

        #endregion Constructors 

        #region Properties (8) 

        public uint Century
        {
            get { return _century; }
            set { _century = value; }
        }

        /// <summary>
        /// Sätts via FOB-uppdateringen.
        /// </summary>
        public bool Classified { get; set; }

        /// <summary>
        /// Anger om kunden är en gruppkund. Om så är fallet ska kundens specifika namn anges separat vid bokning
        /// </summary>
        public bool GroupCustomer { get; set; }

        public string CustomerNumber { get; set; }

        public string HomeAddress { get; set; }

        public uint HomeAddressObj { get; set; }

        public string County { get; set; }

        public string Muni { get; set; }

        public uint _MuniId { get; set; }

        public string ZipCode { get; set; }

        public string HomeCity { get; set; }

        public string PopulationRegistryNumber { get; set; }
        
        public bool UpdateFromRegAutomatically
        {
            get { return _updateFromRegAutomatically; }
            set { _updateFromRegAutomatically = value; }
        }

        /// <summary>
        /// 2015-02-23: Doesnt seen to be populated. This data exists in MiscInfo? Use _GetCustomerCards(...)
        /// </summary>
        public List<uint> CustomerCards { get; set; }

        /// <summary>
        /// Anger om kundens folkbokföringsadress har blivit bekräftad.
        /// </summary>
        public bool VerifiedFOBAdress { get; set; }

        /// <summary>
        /// Lista på de kunder som denna kund har access till. Ska byggas ut med Read/write specificering samt ev också andra saker
        /// </summary>
        public List<uint> _AvailableCustomers { get; set; }

        public AnbaroStatuses _AnbaroStatus { get; set; }

        /// <summary>
        /// När man reggar sig på webbsidan/appen måste man fylla i uppgifter. Denna visar som så skett.
        /// </summary>
        public DateTime _FirstLoginUpdateTimestamp { get; set; }

        public DateTime DeceasedDate
        {
            get
            {
                object o = _GetCustomProperty(CustomProperties.DeceasedDate.ToString());
                if (o != null && o is DateTime)
                {
                    return (DateTime)o;
                }
                return DateTime.MinValue;
            }
            set
            {
                _SetCustomProperty(CustomProperties.DeceasedDate.ToString(), value);
            }
        }

        public string COAdress
        {
            get
            {
                object o = _GetCustomProperty(CustomProperties.COAddress.ToString());
                if (o != null && o is string)
                {
                    return (string)o;
                }
                return "";
            }
            set
            {
                if (value == null || value.Length == 0)
                    _RemoveCustomProperty(CustomProperties.COAddress.ToString());
                else
                    _SetCustomProperty(CustomProperties.COAddress.ToString(), value);
            }
        }
        

        public List<string> Categories
        {
            get
            {
                object o = _GetCustomProperty(CustomProperties.Categories.ToString());
                if (o != null && o is List<string>)
                {
                    return (List<string>)o;
                }
                return null;
            }
            set
            {
                _SetCustomProperty(CustomProperties.Categories.ToString(), value);
            }
        }

        public Sexes _Sex
        {
            get
            {
                return Sexes.Male;
            }
            set
            {
                _SetCustomProperty(CustomProperties._Sex.ToString(), value);
            }
        }

        public PersonIdFormats _PersonIdFormat
        {
            get
            {
                object o = _GetCustomProperty(CustomProperties._PersonIdFormat.ToString());
                if (o == null)
                {
                    return PersonIdFormats.Swedish;
                }
                return (PersonIdFormats)o;
            }
            set
            {
                _SetCustomProperty(CustomProperties._PersonIdFormat.ToString(), value);
            }
        }

        public string _MiddleName
        {
            get
            {
                object o = _GetCustomProperty(CustomProperties._MiddleName.ToString());
                if (o == null)
                {
                    return "";
                }
                return (string)o;
            }
            set
            {
                _SetCustomProperty(CustomProperties._MiddleName.ToString(), value);
            }
        }

        public uint _DistrictId { get; set; }


        /// <summary>
        /// Says if the customer is a test customer or not. Can be used to filter out test customers from regular customers in some cases.
        /// </summary>
        public bool _IsTestCustomer { get; set; }
        public CustomerSettings _CustomerSettings { get; set; }

        #endregion Properties 
        
        public override string ToString()
        {
            return Firstname + " " + Lastname;
        }
        
        public Sexes _GetSexFromPopulationRegistryId()
        {
           return Sexes.Male;
        }

    }
}
