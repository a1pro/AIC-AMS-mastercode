using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AicAms.Helpers;
using Xamarin.Forms;

namespace AicAms.Models.Employee
{
    public class Employee : ObservableObject
    {
        public string Id { get; set; }

        public string Name { get; set; }
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
    }
}
