using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AicAms.Models.Auth;
using AicAms.Models.BaseResult;
using AicAms.Models.Department;
using AicAms.Models.Employee;
using AicAms.Models.Summary;

namespace AicAms.ApiFactory
{
    public class DashboardFactory : BaseFactory
    {
        public async Task<OperationResult<Department[]>> GetDepartments(string token, CancellationToken cancellation)
        {
            return await Request<Department[]>("Dashboard/departments", HttpMethod.Get, cancellation, new Dictionary<string, string>
            {
                {"token", token}
            });
        }

        public async Task<OperationResult<Employee[]>> GetEmployeesForDepartment(string token, int depId, CancellationToken cancellation)
        {
            return await Request<Employee[]>("Dashboard/EmployeesForDepartment", HttpMethod.Get, cancellation, new Dictionary<string, string>
            {
                {"token", token},
                {"depId", depId.ToString()}
            });
        }

        public async Task<OperationResult<CommonSummary>> DaySummary(string token, long ticks, CancellationToken cancellation)
        {
            return await Request<CommonSummary>("Dashboard/DaySummary", HttpMethod.Get, cancellation, new Dictionary<string, string>
            {
                {"token", token},
                {"ticks", ticks.ToString()}
            });
        }

        public async Task<OperationResult<CommonSummary>> DaySummaryForEmployee(string token, string empId, long ticks, CancellationToken cancellation)
        {
            return await Request<CommonSummary>("Dashboard/DaySummaryForEmp", HttpMethod.Get, cancellation, new Dictionary<string, string>
            {
                {"token", token},
                {"ticks", ticks.ToString()},
                {"empId", empId}
            });
        }

        public async Task<OperationResult<CommonSummary>> MonthSummary(string token, long ticksStart, long ticksEnd, CancellationToken cancellation)
        {
            return await Request<CommonSummary>("Dashboard/MonthSummary", HttpMethod.Get, cancellation, new Dictionary<string, string>
            {
                {"token", token},
                {"ticksStart", ticksStart.ToString()},
                {"ticksEnd", ticksEnd.ToString()}
            });
        }

        public async Task<OperationResult<CommonSummary>> MonthSummaryForEmployee(string token, string empId, long ticksStart, long ticksEnd, CancellationToken cancellation)
        {
            return await Request<CommonSummary>("Dashboard/MonthSummaryForEmp", HttpMethod.Get, cancellation, new Dictionary<string, string>
            {
                {"token", token},
                {"ticksStart", ticksStart.ToString()},
                {"ticksEnd", ticksEnd.ToString()},
                {"empId", empId}
            });
        }

        public async Task<OperationResult<ShiftSummary>> ShiftSummary(string token, byte shiftId, long ticks, CancellationToken cancellation)
        {
            return await Request<ShiftSummary>("Dashboard/ShiftSummary", HttpMethod.Get, cancellation, new Dictionary<string, string>
            {
                {"token", token},
                {"ticks", ticks.ToString()},
                {"shiftId", shiftId.ToString()}
            });
        }

        public async Task<OperationResult<ShiftSummary>> ShiftSummaryForEmployee(string token, string empId, byte shiftId, long ticks, CancellationToken cancellation)
        {
            return await Request<ShiftSummary>("Dashboard/ShiftSummaryForEmp", HttpMethod.Get, cancellation, new Dictionary<string, string>
            {
                {"token", token},
                {"ticks", ticks.ToString()},
                {"empId", empId},
                {"shiftId", shiftId.ToString()}
            });
        }

        public async Task<OperationResult<ShiftSummary[]>> ShiftSummaries(string token, long ticksStart, long ticksEnd, CancellationToken cancellation)
        {
            return await Request<ShiftSummary[]>("Dashboard/ShiftSummaries", HttpMethod.Get, cancellation, new Dictionary<string, string>
            {
                {"token", token},
                {"ticksStart", ticksStart.ToString()},
                {"ticksEnd", ticksEnd.ToString()},
            });
        }

        public async Task<OperationResult<ShiftSummary[]>> ShiftSummariesForEmployee(string token, string empId, long ticksStart, long ticksEnd, CancellationToken cancellation)
        {
            return await Request<ShiftSummary[]>("Dashboard/ShiftSummariesForEmp", HttpMethod.Get, cancellation, new Dictionary<string, string>
            {
                {"token", token},
                {"ticksStart", ticksStart.ToString()},
                {"ticksEnd", ticksEnd.ToString()},
                {"empId", empId},
            });
        }

        public async Task<OperationResult<AttendanceList[]>> AttendanceListViews(string token, long ticksStart, long ticksEnd, CancellationToken cancellation)
        {
            return await Request<AttendanceList[]>("Dashboard/AttendanceListViews", HttpMethod.Get, cancellation, new Dictionary<string, string>
            {
                {"token", token},
                {"ticksStart", ticksStart.ToString()},
                {"ticksEnd", ticksEnd.ToString()},
            });
        }

        public async Task<OperationResult<AttendanceList[]>> AttendanceListViewsForEmp(string token, string empId, long ticksStart, long ticksEnd, CancellationToken cancellation)
        {
            return await Request<AttendanceList[]>("Dashboard/AttendanceListViewsForEmp", HttpMethod.Get, cancellation, new Dictionary<string, string>
            {
                {"token", token},
                {"ticksStart", ticksStart.ToString()},
                {"ticksEnd", ticksEnd.ToString()},
                {"empId", empId},
            });
        }
    }
}
