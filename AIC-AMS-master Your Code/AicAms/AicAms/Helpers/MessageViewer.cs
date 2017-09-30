using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AicAms.Resources;
using AicAms.Services;

namespace AicAms.Helpers
{
    public static class MessageViewer
    {
        public static async Task MessageAsync(string title, string msg, string cancel)
        {
            await NavigationService.CurrentPage.DisplayAlert(title, msg, cancel);
        }

        public static async Task SuccessAsync(string msg)
        {
            await NavigationService.CurrentPage.DisplayAlert(Resource.SuccessText, msg, Resource.OkText);
        }

        public static async Task ErrorAsync(string msg)
        {
            await NavigationService.CurrentPage.DisplayAlert(Resource.ErrorText, msg, Resource.OkText);
        }

        public static void Message(string title, string msg, string cancel)
        {
            NavigationService.CurrentPage.DisplayAlert(title, msg, cancel);
        }

        public static void Success(string msg)
        {
            NavigationService.CurrentPage.DisplayAlert(Resource.SuccessText, msg, Resource.OkText);
        }

        public static void Error(string msg)
        {
            NavigationService.CurrentPage.DisplayAlert(Resource.ErrorText, msg, Resource.OkText);
        }

        public static async Task<bool> Alert(string msg)
        {
            return await NavigationService.CurrentPage.DisplayAlert(Resource.AlertActionTitle, msg, Resource.OkText, Resource.CancelToolbar);
        }
    }
}
