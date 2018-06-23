using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace AttendanceManagementSystem.BaseLog
{
    public class Log : BaseEmployee.IBaseEmployee
    {
        private string _id;
        public string id { get { return _id; } set { _id = value; } }

        [JsonProperty(PropertyName = "logs")]
        public List<Message> Messages { get; set; }
    }
}
