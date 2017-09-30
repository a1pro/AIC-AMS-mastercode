using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AicAms.Models.Auth;
using AicAms.Models.BaseResult;
using AicAms.Models.Reports;
using AicAms.Models.Surprise;

namespace AicAms.ApiFactory
{
    public class SurpriseFactory : BaseFactory
    {
        public async Task<OperationResult> SendExcuse(string token, string[] empIds, CancellationToken cancellation)
        {
            var emps = new StringBuilder();
            foreach (var empId in empIds)
            {
                emps.Append(empId + ",");
            }

            return await Request<bool>("Surprise/PullSurprise", HttpMethod.Get, cancellation, new Dictionary<string, string>
            {
                {"token", token},
                {"emps", emps.ToString().TrimEnd(',')}
            });
        }

        public async Task<OperationResult> ResendPush(string token, int msrId, CancellationToken cancellation)
        {
            return await Request<bool>("Surprise/ResendPush", HttpMethod.Get, cancellation, new Dictionary<string, string>
            {
                {"token", token},
                {"msrId", msrId.ToString()}
            });
        }

        public async Task<OperationResult> SendAttendace(string token, int msrId, double latintude, double longitude, CancellationToken cancellation)
        {
            return await Request<bool>("Surprise/PullAttendace", HttpMethod.Get, cancellation, new Dictionary<string, string>
            {
                {"token", token},
                {"msrId", msrId.ToString()},
                {"latintude", latintude.ToString()},
                {"longitude", longitude.ToString()}
            });
        }

        public async Task<OperationResult<MasterSurprise[]>> GetMasterSurprises(string token, CancellationToken cancellation)
        {
            return await Request<MasterSurprise[]>("Surprise/MasterSurprises", HttpMethod.Get, cancellation, new Dictionary<string, string>
            {
                {"token", token},
            });
        }

        public async Task<OperationResult<DetailSurprise[]>> GetDetailSurprises(string token, int msrId, CancellationToken cancellation)
        {
            return await Request<DetailSurprise[]>("Surprise/DetailSurprises", HttpMethod.Get, cancellation, new Dictionary<string, string>
            {
                {"token", token},
                {"msrId", msrId.ToString()},
            });
        }
    }
}
