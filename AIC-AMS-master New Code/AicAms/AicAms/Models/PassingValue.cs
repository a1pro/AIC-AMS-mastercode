using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AicAms.Models
{
   public class PassingValue
    {
        public static string langauage = null;
        public string Lang
        {
            get
            {
                return langauage;
            }
            set
            {
                langauage = value;
            }
        }

        public static int _notificationid = 1;
        public int notificationid
        {
            get
            {
                return _notificationid;
            }
            set
            {
                _notificationid = value;
            }
        }
    }
}
