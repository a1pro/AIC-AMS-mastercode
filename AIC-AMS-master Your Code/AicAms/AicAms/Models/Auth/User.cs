using System;
using Realms;

namespace AicAms.Models.Auth
{
    public class User : RealmObject
    {
        public string EmpId { get; set; }

        public string Login { get; set; }

        public string Token { get; set; }

        public string FullName { get; set; }

        public bool IsManager { get; set; }

        public bool IsGregorianLocale { get; set; }
    }
}
