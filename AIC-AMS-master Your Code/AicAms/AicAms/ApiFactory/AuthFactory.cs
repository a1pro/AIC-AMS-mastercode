using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AicAms.Models.Auth;
using AicAms.Models.BaseResult;

namespace AicAms.ApiFactory
{
    public class AuthFactory : BaseFactory
    {
        public async Task<OperationResult<User>> SignIn(string login, string password, bool isAndroid, CancellationToken token)
        {
            return await Request<User>("auth/signin", HttpMethod.Get, token, new Dictionary<string, string>
            {
                {"login", login},
                {"password", password},
                {"isAndroid", isAndroid.ToString() }
            });
        }
    }
}
