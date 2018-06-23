using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using AttendanceManagementSystem.BaseEmployee;

namespace AttendanceManagementSystem.BaseAttendance
{
    public class Attendance : IBaseEmployee
    {
        private string _id;
        public string id { get { return _id; } set { _id = value; } }

        [JsonProperty(PropertyName = "attendance")]
        public List<EmployeeAttendance> EmpAttendance { get; set; }
    }
}
