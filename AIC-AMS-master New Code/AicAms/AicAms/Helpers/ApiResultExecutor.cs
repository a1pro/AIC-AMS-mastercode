using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AicAms.Models;
using AicAms.Models.Auth;
using AicAms.Models.BaseResult;
using AicAms.Resources;
using AicAms.Services;
using AicAms.ViewModels;
using AicAms.ViewModels.Auth;
using AicAms.Views;
using LoginPage = AicAms.Views.Auth.LoginPage;

namespace AicAms.Helpers
{
    public static class ApiResultExecutor
    {
        public static async Task Execute<T>(OperationResult<T> result)
        {
            switch (result.ResultCode)
            {
                case ResultCode.UnknownError:
                    await MessageViewer.ErrorAsync(Resource.CheckInternetMsg);
                    break;

                case ResultCode.AuthError:
                    await MessageViewer.ErrorAsync(Resource.AuthFailMsg);
                    break;

                case ResultCode.NoAccess:
                    await MessageViewer.ErrorAsync(Resource.NoAccessMsg);
                    break;

                case ResultCode.AuthTokenError:
                    if (!(NavigationService.CurrentPage is LoginPage))
                    {
                        App.Realm.Write(() =>
                        {
                            App.Realm.RemoveAll<User>();
                        });
                        NavigationService.SetDetailPage(new LoginViewModel(), SelectedMenuOptions.Dashboard);
                    }
                    break;

                default:
                    return;
            }
        }
    }
}
