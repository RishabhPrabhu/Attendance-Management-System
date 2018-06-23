using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace AttendanceManagementSystem.BaseAttendance
{
    public class EmployeeAttendance : IEmployeeAttendance
    {
        private string _Date;
        public string Date { get { return _Date; } set { _Date = value; } }

        private string _Status;
        public string Status { get { return _Status; } set { _Status = value; } }

        private string _CheckInTime;
        public string CheckInTime { get { return _CheckInTime; } set { _CheckInTime = value; } }

        private string _CheckOutTime;
        public string CheckOutTime { get { return _CheckOutTime; } set { _CheckOutTime = value; } }

    }
}
