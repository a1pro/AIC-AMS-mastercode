using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Resources;
using System.Globalization;
using System.Reflection;
using AicAms.DependencyServices;
using AicAms.Helpers;
using AicAms.Resources;

namespace AicAms.Extensions
{
    [ContentProperty("Text")]
    public class TranslateExtension : IMarkupExtension
    {
        private CultureInfo _ci;

        public TranslateExtension()
        {
            _ci = Settings.Culture;
        }

        public string Text { get; set; }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            if (Text == null)
                return "";
            var translation = Resource.ResourceManager.GetString(Text, _ci) ?? Text;
            return translation;
        }
    }
}
