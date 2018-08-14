using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelocityDb;

namespace Commons
{
    [Serializable]
    public class CoreObject : OptimizedPersistable, IComparable
    {
        #region Fields (3) 

        public enum ObjectChanges { Added, Updated, Deleted }
        private bool _deleted;
        private uint _id;

        #endregion Fields 

        #region Constructors (3) 

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreObject"/> class.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="displayName">The displayName of the object. In the future should reflect the chosen language.</param>
        public CoreObject(uint id)
        {
            this.Id = id;
            //   this.DisplayName = displayName;
        }

      
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreObject"/> CoreObject, without any Id.
        /// </summary>
        public CoreObject()
        {
        }

        #endregion Constructors 

        #region Properties (6) 
        
        public bool Deleted
        {
            get
            {
                return DeletedTime != DateTime.MinValue;
            }
            set
            {
                if (value)
                    DeletedTime = DateTime.Now;
                else
                    DeletedTime = DateTime.MinValue;
            }
        }

        private DateTime _deletedTime = DateTime.MinValue;
         public DateTime DeletedTime
        {
            get
            {
                return _deletedTime;
            }
            set
            {
                // 2015-08-18: Vi vill int att ett redan satt DateTime för när obj togs bort skrivs över. Är det redan deleted, dvs är objektet redan borttaget och en tid är satt för det, ska vi inte uppd
                if (value == DateTime.MinValue) // om vi ska nollställa det, nollställ =)
                    _deletedTime = value;
                else if (_deletedTime == DateTime.MinValue) // men om det ska sättas ett värde, sätt det bara om värdet just nu är "nollat"
                    _deletedTime = value;
            }
        }

        /// <summary>
        /// The CoreObject-id, is unique among all CoreObjects.
        /// </summary>
        /// <value>The id</value>
       
      
        public string ObjectName { get; set; }


        /// <summary>
        /// 2013-04-07: Any custom properties that are to be considered "special cases" can be placed here. 
        /// </summary>
        private Dictionary<string, object> customProperties;
        public Dictionary<string, object> _CustomProperties
        {
            get
            {
                return customProperties;
            }
            set
            {
                customProperties = value;
            }
        }


        #endregion Properties 

        #region Methods      

      
        public override bool Equals(object obj)
        {
            if (obj == null || !obj.GetType().IsSubclassOf(typeof(CoreObject)))
                return false;

            CoreObject co = (CoreObject)obj;
            if (this.Id == co.Id) // borde kolla type också? Obj kan ju ersättas, samma id men annan typ?
                return true;
            else
                return false;
        }
            

        //Sätter värdet om det inte är null, annars tar bort det från customdata
        public void _SetCustomProperty(string key, object value)
        {
            if (_CustomProperties == null)
                customProperties = new Dictionary<string, object>();
            lock (customProperties)
            {
                if (_CustomProperties.ContainsKey(key))
                    _CustomProperties.Remove(key);
                if (value != null)
                    _CustomProperties.Add(key, value);
            }
        }

        public void _SetCustomProperty<T>(string key, T value)
        {
            if (_CustomProperties == null)
                customProperties = new Dictionary<string, object>();
            lock (customProperties)
            {
                if (_CustomProperties.ContainsKey(key))
                    _CustomProperties.Remove(key);
                if (value != null)
                    _CustomProperties.Add(key, value);
            }
        }

        public object _GetCustomProperty(string key)
        {
            if (_CustomProperties != null)
            {
                lock (customProperties)
                {
                    if (_CustomProperties.ContainsKey(key))
                        return _CustomProperties[key];
                }
            }
            return null;
        }

        public T _GetCustomProperty<T>(string key)
        {
            if (_CustomProperties != null)
            {
                lock (customProperties)
                {
                    if (_CustomProperties.ContainsKey(key))
                        return (T)_CustomProperties[key];
                }
            }
            return default(T);
        }

        public void _RemoveCustomProperty(string key)
        {
            if (_CustomProperties == null)
                return;
            lock (customProperties)
            {
                if (_CustomProperties.ContainsKey(key))
                {
                    _CustomProperties.Remove(key);

                    if (_CustomProperties.Count == 0)
                        _CustomProperties = null;
                }
            }
        }

        #endregion Methods 

       

        #region IComparable Members

        public int CompareTo(object obj)
        {
            return this.ToString().CompareTo(obj.ToString());
        }

        #endregion
    }
}
