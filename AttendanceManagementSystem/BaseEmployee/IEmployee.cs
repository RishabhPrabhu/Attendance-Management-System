using System;
using System.Collections.Generic;
using System.Text;

namespace AttendanceManagementSystem.BaseEmployee
{
    public interface IEmployee : IBaseEmployee
    {
        string EmployeeName { get; set; }
        string EmailId { get; set; }
    }
}
