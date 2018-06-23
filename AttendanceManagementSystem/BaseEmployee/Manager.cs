using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace AttendanceManagementSystem.BaseEmployee
{
    public class Manager : IEmployee
    {
        private string _id;
        public string id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }
        private string _EmployeeName;
        public string EmployeeName
        {
            get
            {
                return _EmployeeName;
            }
            set
            {
                _EmployeeName = value;
            }
        }
        private string _EmailId;
        public string EmailId
        {
            get
            {
                return _EmailId;
            }
            set
            {
                _EmailId = value;
            }
        }
    }
}
