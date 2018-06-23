using System;
using System.Collections.Generic;
using System.Text;


namespace AttendanceManagementSystem.BaseAttendance
{
    public interface IEmployeeAttendance
    {
        string Date { get; set; }
        string Status { get; set; }
        string CheckInTime { get; set; }
        string CheckOutTime { get; set; }
    }
}
