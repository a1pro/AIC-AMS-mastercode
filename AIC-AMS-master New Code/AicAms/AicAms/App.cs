using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AicAms.ApiFactory;
using AicAms.DependencyServices;
using AicAms.Helpers;
using AicAms.Models;
using AicAms.Models.Auth;
using AicAms.Models.Views;
using AicAms.Resources;
using AicAms.Services;
using AicAms.ViewModels.Auth;
using AicAms.ViewModels.PushActions;
using AicAms.Views;
using AicAms.Views.Auth;
using AicAms.Views.PushActions;
using Realms;
using Xamarin.Forms;

namespace AicAms
{
    public class App : Application
    {
        public static Realm Realm { get; } = Realm.GetInstance();

        public static RootPage MS { get; set; }

        public App()
        {
            XamEffects.Effects.Init();
            LocaleHelper.Init();
         
            NavigationService.InitPage(new LoginViewModel());
        }

        public static void RequestSurpirse(int rid)
        {
            NavigationService.NavigateModal(new SurpriseAccomplishViewModel(rid));
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
