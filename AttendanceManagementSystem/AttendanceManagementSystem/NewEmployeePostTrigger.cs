using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using AttendanceManagementSystem.BaseAttendance;
using AttendanceManagementSystem.BaseEmployee;

namespace AttendanceManagementSystem.DatabaseManagement.Trigger
{
    public class NewEmployeeAdditionTrigger
    {
        // Declaring delegates for events
        public delegate Task EmployeeCreator(Employee employee, string empColLink, string attColLink);
        public delegate Task AttendanceCreator(string id, string attColLink);

        // Declaring events
        public static EmployeeCreator EmployeeCreatorHandler;
        public static AttendanceCreator AttendanceCreatorHandler;

        DbOperations db = DbOperations.GetInstance;

        public static async Task OnAddNewEmployeeAsync(Employee employee, string empColLink, string attColLink)
        {
            await EmployeeCreatorHandler?.Invoke(employee, empColLink, attColLink);
        }

        public static async Task OnNewEmployeeAddedAsync(string empId, string colLink)
        {
            await AttendanceCreatorHandler?.Invoke(empId, colLink);
        }
    }
}
