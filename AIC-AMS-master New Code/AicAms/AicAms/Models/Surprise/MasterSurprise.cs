using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AicAms.Helpers;

namespace AicAms.Models.Surprise
{
    public class MasterSurprise
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }

        public string UsrName { get; set; }

        //calculated rows

        public string Title { get; set; }

        public void Calculate(bool isGregorian)
        {
            string[] names = isGregorian ? new CultureInfo("en-US").DateTimeFormat.DayNames : new CultureInfo("ar-SA").DateTimeFormat.DayNames;
            var hijri = DateLocaleConvert.ConvertGregorianToHijri(Date);
            Title = string.Format(
                "{0} {1}/{2}/{3} {4}", 
                names[DateLocaleConvert.GetDayOfWeek(Date)],
                isGregorian ? Date.Day : hijri.Day,
                isGregorian ? Date.Month : hijri.Month,
                isGregorian ? Date.Year : hijri.Year,
                Date.ToString("hh:mm tt", CultureInfo.InvariantCulture));
        }
    }
}
