using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AicAms.Helpers;
using Xamarin.Forms;

namespace AicAms.Models.Summary
{
    public class ShiftSummary
    {
        public byte ShiftId { get; set; }

        public DateTime Date { get; set; }

        public int WorkDuration { get; set; }

        public int ShiftDuration { get; set; }

        public int BeginEarly { get; set; }

        public int BeginLate { get; set; }

        public int OutEarly { get; set; }

        public int OutLate { get; set; }

        public string Status { get; set; }

        public DateTime PunchIn { get; set; }

        public DateTime PunchOut { get; set; }

        //calculated fields:
        public string ShiftDurationComment { get; set; }

        public string WorkDurationComment { get; set; }

        public string PunchInComment { get; set; }

        public string PunchOutComment { get; set; }

        public string BeginEarlyComment { get; set; }

        public string BeginLateComment { get; set; }

        public string OutEarlyComment { get; set; }

        public string OutLateComment { get; set; }

        public string DurationComment { get; set; }

        public string DateComment { get; set; }

        public string DayComment { get; set; }

        //for grid:

        public Color BackgroundColor { get; set; } = Color.Transparent;

        public void Calculate(bool isGregorian = true, bool forGrid = false)
        {
            var sdc = TimeSpan.FromSeconds(ShiftDuration);
            ShiftDurationComment = $"{(int)sdc.TotalHours:00}:{sdc.Minutes:00}:{sdc.Seconds:00}";
            var wdc = TimeSpan.FromSeconds(WorkDuration);
            WorkDurationComment = $"{(int)wdc.TotalHours:00}:{wdc.Minutes:00}:{wdc.Seconds:00}";

            if (forGrid)
            {
                PunchInComment = PunchIn == DateTime.MinValue ? "00:00" : $"{PunchIn.Hour:00}:{PunchIn.Minute:00}";
                PunchOutComment = PunchOut == DateTime.MinValue ? "00:00" : $"{PunchOut.Hour:00}:{PunchOut.Minute:00}";
            }
            else
            {
                PunchInComment = PunchIn == DateTime.MinValue ? "00:00:00" : $"{PunchIn.Hour:00}:{PunchIn.Minute:00}:{PunchIn.Second:00}";
                PunchOutComment = PunchOut == DateTime.MinValue ? "00:00:00" : $"{PunchOut.Hour:00}:{PunchOut.Minute:00}:{PunchOut.Second:00}";
            }

            var bec = TimeSpan.FromSeconds(BeginEarly);
            BeginEarlyComment = $"{(int)bec.TotalHours:00}:{bec.Minutes:00}:{bec.Seconds:00}";
            var blc = TimeSpan.FromSeconds(BeginEarly);
            BeginLateComment = $"{(int)blc.TotalHours:00}:{blc.Minutes:00}:{blc.Seconds:00}";

            var oec = TimeSpan.FromSeconds(OutEarly);
            OutEarlyComment = $"{(int)oec.TotalHours:00}:{oec.Minutes:00}:{oec.Seconds:00}";
            var olc = TimeSpan.FromSeconds(OutLate);
            OutLateComment = $"{(int)olc.TotalHours:00}:{olc.Minutes:00}:{olc.Seconds:00}";

            var dur = PunchOut - PunchIn;
            DurationComment = PunchIn == DateTime.MinValue || PunchOut == DateTime.MinValue ? "00:00" : $"{dur.Hours:00}:{dur.Minutes:00}";
            if (isGregorian)
                DateComment = $"{Date.Day:00}.{Date.Month:00}.{Date.Year}";
            else
            {
                var hijri = DateLocaleConvert.ConvertGregorianToHijri(Date);
                DateComment = $"{hijri.Day:00}.{hijri.Month:00}.{hijri.Year}";
            }

            string[] names = isGregorian ? new CultureInfo("en-US").DateTimeFormat.AbbreviatedDayNames : new CultureInfo("ar-SA").DateTimeFormat.AbbreviatedDayNames;
            DayComment = names[DateLocaleConvert.GetDayOfWeek(Date)];
        }
    }
}
