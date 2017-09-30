using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AicAms.Resources;

namespace AicAms.Models.Summary
{
    public class CommonSummary
    {
        public int Id { get; set; }

        public int WorkDuration { get; set; }

        public int ShiftDuration { get; set; }

        public int BeginLate { get; set; }

        public int GapDurationWithoutExcuse { get; set; }

        //only for month summary:

        public int DaysWork { get; set; }

        public int DaysAbsentWithoutVac { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }


        //only for day summary:

        public DateTime Date { get; set; }

        public string Status { get; set; }

        //calculated fields:

        public string TotalWorkingHours { get; set; }

        public double TotalWorkingHoursPercent { get; set; }

        public int TotalAbsentDays { get; set; }

        public double TotalAbsentDaysPercent { get; set; }

        public string TotalLateHours { get; set; }

        public double TotalLateHoursPercent { get; set; }

        public string TotalWorkingHoursComment { get; set; }

        public string TotalAbsentDaysComment { get; set; }

        public string TotalLateHoursComment { get; set; }

        public void Calculate()
        {
            TotalWorkingHoursPercent = ShiftDuration == 0 ? 0 : WorkDuration / (double)ShiftDuration * 100;

            TotalWorkingHoursPercent = TotalWorkingHoursPercent > 100 ? 100 : TotalWorkingHoursPercent;

            TotalAbsentDaysPercent = DaysWork == 0 ? 0 : DaysAbsentWithoutVac / (double)DaysWork * 100;

            TotalAbsentDaysPercent = TotalAbsentDaysPercent > 100 ? 100 : TotalAbsentDaysPercent;

            TotalLateHoursPercent = WorkDuration == 0 ? 0 : BeginLate / (double)WorkDuration * 100;

            TotalLateHoursPercent = TotalLateHoursPercent > 100 ? 100 : TotalLateHoursPercent;

            var dur = TimeSpan.FromSeconds(WorkDuration);
            var sdur = TimeSpan.FromSeconds(ShiftDuration);
            var bl = TimeSpan.FromSeconds(BeginLate);

            TotalWorkingHoursComment = $"{(int)dur.TotalHours:00}:{dur.Minutes:00} / {(int)sdur.TotalHours:00}:{sdur.Minutes:00}";
            TotalWorkingHours = $"{(int) dur.TotalHours:00}:{dur.Minutes:00}";
            TotalAbsentDaysComment = $"{DaysAbsentWithoutVac} / {DaysWork}";
            TotalLateHoursComment = $"{(int)bl.TotalHours:00}:{bl.Minutes:00} / {(int)dur.TotalHours:00}:{dur.Minutes:00}";
            TotalLateHours = $"{(int) bl.TotalHours:00}:{bl.Minutes:00}";
        }
    }
}
