using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons
{
    [Serializable]
    public class ContactItem
    {

        #region Fields (3) 

        /// <summary>
        ///  Should be removed! Only contains the type of the ContactItem, so we can replace it with the new contactitem subtypes
        /// </summary>
        public Type _contactType;

        private string _data;
        private string _description;

        #endregion Fields 

        #region Constructors (2) 

        public ContactItem()
        {
        }


        public ContactItem(string central)
            : this(null, central, null)
        {
        }

        public ContactItem(string data, string central)
            : this(data, central, null)
        {
        }

        public ContactItem(string data, string central, string description)
            : this()
        {
            this.Data = data;
            this.Central = central;
            this.Description = description;
        }

        #endregion Constructors 

        #region Properties (3) 

       public Type ContactType
        {
            get
            {
                return this.GetType();
            }
        }

        public string Data
        {
            get { return _data; }
            set { _data = value; }
        }

        /// <summary>
        /// Specar namnet på centralen som det ska skickas till.
        /// </summary>
        public string Central { get; set; }

        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        public string MiscData1 { get; set; }
        public string MiscData2 { get; set; }
        public string MiscData3 { get; set; }

        public virtual bool AutoDelivery
        {
            get
            {
                return false;
            }
        }

        #endregion Properties 

        #region Methods (2) 


        // Public Methods (2) 

        public override bool Equals(object obj)
        {
            if (obj != null && obj.GetType() == this.GetType())
            {
                ContactItem ci = (ContactItem)obj;

                bool dataEqual = false;
                bool centralEqual = false;

                if (string.Equals(this.Data, ci.Data))
                    dataEqual = true;

                if (this.Central != null && ci.Central != null)
                {
                    if (string.Equals(this.Central, ci.Central))
                        centralEqual = true;
                }
                else
                    centralEqual = true; // jämför bara central om båda är satta, för vissa typer (email, phone etc) hanteras detta speciellt.

                if (dataEqual && centralEqual)
                    return true;
                else
                    return false;
            }
            else
                return false;
        }

        public override int GetHashCode()
        {
            return (ContactType.ToString() + Data + Description).GetHashCode();
        }

       
        public virtual bool IsPrimary()
        {
            return false;
        }


        #endregion Methods 

       
    }
}
