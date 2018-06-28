using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons
{
    [Serializable]
    public class CustomerSettings
    {
        public string Platform { get; set; }

        public bool CustomerConnect_NotifStarted { get; set; }
        public bool CustomerConnect_NotifFinished { get; set; }
        public bool CustomerConnect_NotifCancelled { get; set; }
        public bool CustomerConnect_NotifNoShowed { get; set; }
        public bool CustomerConnect_NotifDelayed { get; set; }

        public bool NotifStarted { get; set; }
        public bool NotifFinished { get; set; }
        public bool NotifCancelled { get; set; }
        public bool NotifNoShowed { get; set; }
        public bool NotifDelayed { get; set; }

        public bool ShareGpsPosition { get; set; }

        public uint ViewingCustomer { get; set; }

    }
}
