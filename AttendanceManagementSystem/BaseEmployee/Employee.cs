using System;
using Newtonsoft.Json;

namespace AttendanceManagementSystem.BaseEmployee
{
    /// <summary>
    /// The structure of the details of an employee in organisation
    /// </summary>
    public class Employee : IEmployee
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
        
        [JsonProperty(PropertyName = "manager")]
        public Manager ManagerOfThisEmployee { get; set; }

    }
}
