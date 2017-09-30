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

namespace AicAms.ApiFactory
{
    public class ReportsFactory : BaseFactory
    {
        public async Task<OperationResult<RequestType[]>> GetExcuseTypes(CancellationToken cancellation)
        {
            return await Request<RequestType[]>("reports/ExcuseTypes", HttpMethod.Get, cancellation);
        }

        public async Task<OperationResult<RequestType[]>> GetVacationTypes(CancellationToken cancellation)
        {
            return await Request<RequestType[]>("reports/VacationTypes", HttpMethod.Get, cancellation);
        }

        public async Task<OperationResult<bool>> SendExcuse(string token, int excuseId, long dateTicks, long startTimeTicks, long endTimeTicks, string reason, CancellationToken cancellation)
        {
            return await Request<bool>("reports/SendExcuse", HttpMethod.Post, cancellation, new Dictionary<string, string>
            {
                {"token", token},
                {"excuseId", excuseId.ToString()},
                {"dateTicks", dateTicks.ToString()},
                {"startTimeTicks", startTimeTicks.ToString()},
                {"endTimeTicks", endTimeTicks.ToString()},
                {"reason", reason},
            });
        }

        public async Task<OperationResult<bool>> SendVacation(string token, int vacationId, long startDateTicks, long endDateTics, string reason, CancellationToken cancellation)
        {
            return await Request<bool>("reports/SendVacation", HttpMethod.Post, cancellation, new Dictionary<string, string>
            {
                {"token", token},
                {"vacationId", vacationId.ToString()},
                {"startDateTicks", startDateTicks.ToString()},
                {"endDateTics", endDateTics.ToString()},
                {"reason", reason},
            });
        }

        public async Task<OperationResult<ReportRequest[]>> GetMyRequests(string token, int status, int ret, long fromTicks, long toTicks, CancellationToken cancellation)
        {
            return await Request<ReportRequest[]>("reports/MyRequests", HttpMethod.Get, cancellation, new Dictionary<string, string>
            {
                {"token", token},
                {"status", status.ToString()},
                {"ret", ret.ToString()},
                {"fromTicks", fromTicks.ToString()},
                {"toTicks", toTicks.ToString()},
            });
        }

        public async Task<OperationResult<ReportRequest[]>> RequestsForEmp(string token, string empId, long fromTicks, long toTicks, CancellationToken cancellation)
        {
            return await Request<ReportRequest[]>("reports/RequestsForEmp", HttpMethod.Get, cancellation, new Dictionary<string, string>
            {
                {"token", token},
                {"empId", empId},
                {"fromTicks", fromTicks.ToString()},
                {"toTicks", toTicks.ToString()}
            });
        }

        public async Task<OperationResult<bool>> RequestDecision(string token, string isVacTypes, string erqIds, string mrgId, bool approve, CancellationToken cancellation)
        {
            return await Request<bool>("reports/RequestDecision", HttpMethod.Get, cancellation, new Dictionary<string, string>
            {
                {"token", token},
                {"isVacTypes", isVacTypes},
                {"erqIds", erqIds},
                {"mrgId", mrgId},
                {"approve", approve.ToString()},
            });
        }
    }
}
