using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AicAms.Helpers;
using AicAms.Resources;
using Xamarin.Forms;

namespace AicAms.Models.Reports
{
    public class ReportRequest : ObservableObject
    {
        public int Id { get; set; }

        public string EmpId { get; set; }

        public string EmpLogin { get; set; }

        public string RetId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public DateTime RequestedDate { get; set; }

        public string Reason { get; set; }

        public int Status { get; set; }

        public RequestType RequestType { get; set; }

        // bindable rows 

        private bool _selected;

        public bool IsSelected
        {
            get { return _selected; }
            set
            {
                SetProperty(ref _selected, value); 
                OnPropertyChanged(nameof(CheckImg));
            }
        }

        public string CheckImg => IsSelected ? "Checked.png" : "Unchecked.png";

        private Color _bgColor;

        public Color BackgroundColor
        {
            get { return _bgColor; }
            set { SetProperty(ref _bgColor, value); }
        }

        //calculated rows

        public string DateComment { get; set; }

        public string StatusComment { get; set; }

        public string DateFromComment { get; set; }

        public string DateToComment { get; set; }

        public string TimeFromComment { get; set; }

        public string TimeToComment { get; set; }

        public string RetStr { get; set; }

        public void Calculate(bool isGregorian = true)
        {
            if (isGregorian)
            {
                DateComment = $"{RequestedDate.Day:00}.{RequestedDate.Month:00}.{RequestedDate.Year} ";
                DateFromComment = $"{StartDate.Day:00}.{StartDate.Month:00}.{StartDate.Year:00}";
                DateToComment = $"{EndDate.Day:00}.{EndDate.Month:00}.{EndDate.Year:00}";
            }
            else
            {
                var hijri = DateLocaleConvert.ConvertGregorianToHijri(RequestedDate);
                DateComment = $"{hijri.Day:00}.{hijri.Month:00}.{hijri.Year} ";
                hijri = DateLocaleConvert.ConvertGregorianToHijri(StartDate);
                DateFromComment = $"{hijri.Day:00}.{hijri.Month:00}.{hijri.Year:00}";
                hijri = DateLocaleConvert.ConvertGregorianToHijri(EndDate);
                DateToComment = $"{hijri.Day:00}.{hijri.Month:00}.{hijri.Year:00}";
            }

            TimeFromComment = StartTime.ToString("hh:mm tt", CultureInfo.InvariantCulture);
            TimeToComment = EndTime.ToString("hh:mm tt", CultureInfo.InvariantCulture);

            StatusComment = Status == 0
                ? "UP"
                : Status == 1
                    ? "AC"
                    : "RJ";

            RetStr = RetId == "VAC"
                ? Resource.VacationStr
                : RetId == "EXC"
                    ? Resource.ExcuseStr
                    : RetId;
        }
    }
}
