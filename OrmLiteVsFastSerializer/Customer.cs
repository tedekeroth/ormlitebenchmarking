using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrmLiteVsFastSerializer
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
                //object o = _GetCustomProperty(CustomProperties._Sex.ToString());
                //if (o == null)
                //{
                //    return _GetSexFromPopulationRegistryId();
                //}
                //return (Sexes)o;
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

        private ushort _sentGDPRInformLetter_DaysAfter20180101;
        /// <summary>
        /// Indicates when a letter was sent to the customer regarding the information on how personal data is stored and used in the system. The value is the number of days after 2018-01-01, used instead of datetime to save space (2 bytes vs 16). 0 = no letter sent.
        /// </summary>        
        public DateTime _SentGDPRInformLetter_DaysAfter20180101
        {
            get
            {
                if (_sentGDPRInformLetter_DaysAfter20180101 == 0)
                    return DateTime.MinValue;
                return new DateTime(2018, 01, 01).AddDays(_sentGDPRInformLetter_DaysAfter20180101);
            }
            set
            {
                if (value == DateTime.MinValue)
                    _sentGDPRInformLetter_DaysAfter20180101 = 0;
                else
                    _sentGDPRInformLetter_DaysAfter20180101 = (ushort)value.Subtract(new DateTime(2018, 01, 01)).TotalDays;
            }
        }

        /// <summary>
        /// Says if the customer is a test customer or not. Can be used to filter out test customers from regular customers in some cases.
        /// </summary>
        public bool _IsTestCustomer { get; set; }

        #endregion Properties 

        #region Methods (2) 
        
        public int _GetAge()
        {
            return _GetAge(DateTime.Now);
        }

        public int _GetAge(DateTime now)
        {
            int age = 50;
            try
            {

                if (_PersonIdFormat == PersonIdFormats.Swedish)
                {
                    try
                    {
                        /* 2014-07-22: Use method instead
                        string temp = Century + PopulationRegistryNumber.Substring(0, 6);
                        int year = int.Parse(temp.Substring(0, 4));
                        int month = int.Parse(temp.Substring(4, 2));
                        int day = int.Parse(temp.Substring(6, 2));
                        if (month < 01)
                            month = 01;
                        if (month > 12)
                            month = 12;
                        if (day < 01)
                            day = 01;
                        if (day > 31)
                            day = 31;
                        DateTime birthDate = new DateTime(year, month, day); 
                         * */

                        DateTime birthDate = _GetBirthDate();

                        age = now.Year - birthDate.Year;
                        if (now.Month < birthDate.Month || (now.Month == birthDate.Month && now.Day < birthDate.Day)) age--;
                    }
                    catch (Exception e)
                    {
                        // 2018-03-22 TE : Fel att logga från AlfaCommons?  SRef.LogException("Exception in Customer.GetAge (swe) on id "+this.Id+" pnbr "+this.PopulationRegistryNumber.ToString(), e);
                        return 50; //standardvärde, bättre än inget alls
                    }
                }
                else if (_PersonIdFormat == PersonIdFormats.Norwegian)
                {
                    DateTime birthDate = _GetBirthDate();
                    age = now.Year - birthDate.Year;
                    if (now.Month < birthDate.Month || (now.Month == birthDate.Month && now.Day < birthDate.Day)) age--;
                }
                if (age < 0)
                    age = 0;
            }
            catch (Exception e)
            {
                // 2018-03-22 TE : Fel att logga från AlfaCommons?  SRef.LogException("Exception in Customer.GetAge on id "+this.Id+" pnbr "+this.PopulationRegistryNumber.ToString(), e);
            }
            return age;
        }

        public DateTime _GetBirthDate()
        {
            if (_PersonIdFormat == PersonIdFormats.Swedish)
            {
                uint cent = Century;
                if (cent == 0)
                    cent = 19;
                string temp = cent + PopulationRegistryNumber.Substring(0, 6);
                try
                {
                    int year, month, day;

                    if (temp.Length < 8)
                        return DateTime.MinValue;

                    if (int.TryParse(temp.Substring(0, 4), out year) &&
                        int.TryParse(temp.Substring(4, 2), out month) &&
                        int.TryParse(temp.Substring(6, 2), out day))
                    {
                        if (year > 0 && month > 0 && day > 0 && month <= 12 && day <= 31)
                            return new DateTime(year, month, day);
                    }

                    return DateTime.MinValue;
                }
                catch (Exception e)
                {
                    //Malformed pnbr
                }
            }
            else if (_PersonIdFormat == PersonIdFormats.Norwegian)
            {
                string year = Century + PopulationRegistryNumber.Substring(4, 2);
                try
                {
                    DateTime birthDate = new DateTime(int.Parse(year), int.Parse(PopulationRegistryNumber.Substring(2, 2)), int.Parse(PopulationRegistryNumber.Substring(0, 2)));
                    return birthDate;
                }
                catch (Exception e)
                {
                    //malformed pnbr
                }
            }
            return DateTime.MinValue; // STd
        }

        /// <summary>
        /// 2017-04-04 TE: Adds or updates an existing ContactItem. Keys are contactType and data
        /// </summary>
        /// <param name="data"></param>
        /// <param name="description"></param>
        /// <param name="contactType"></param>
        public void AddOrUpdateContactItem(string data, string description, Type contactType)
        {
            if (string.IsNullOrEmpty(data))
                return;

            List<ContactItem> cis = this._GetContactItems(contactType);

            List<ContactItem> found = cis.FindAll(x => x.Data.Equals(data));
            foreach (ContactItem ci in found)  // remove contactitems with smae email
                this.ContactDetails.ContactItemList.Remove(ci);

            this.ContactDetails.ContactItemList.Add((ContactItem)Activator.CreateInstance(contactType, data, null, description));
        }

        // Public Methods (2) 
        
        public override string ToString()
        {
            return Firstname + " " + Lastname;
        }

        public string ToString(bool includePopregnbr)
        {
            if (includePopregnbr && this.PopulationRegistryNumber != null && this.PopulationRegistryNumber.Length > 0)
            {
                return Firstname + " " + Lastname + " (" + this.PopulationRegistryNumber + ")";
            }
            else
                return Firstname + " " + Lastname;
        }

        #endregion Methods 
        
        public Sexes _GetSexFromPopulationRegistryId()
        {
            if (_PersonIdFormat == PersonIdFormats.Swedish)
            {
                try
                {
                    int nbr = int.Parse(this.PopulationRegistryNumber[this.PopulationRegistryNumber.Length - 2].ToString());
                    if (nbr % 2 == 0)
                        return Sexes.Female;
                    else
                        return Sexes.Male;
                }
                catch (Exception e)
                {
                    // Weird pop nbr?
                    return Sexes.Unknown;
                }
            }
            else if (_PersonIdFormat == PersonIdFormats.Norwegian)
            {
                int nbr = int.Parse(this.PopulationRegistryNumber[8].ToString());
                if (nbr % 2 == 0)
                    return Sexes.Female;
                else
                    return Sexes.Male;
            }
            return Sexes.Unknown;
        }

    }
}
