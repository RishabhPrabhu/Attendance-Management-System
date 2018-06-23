using System;
using System.Collections.Generic;
using System.Text;

namespace AttendanceManagementSystem.BaseLog
{
    public interface IMessage
    {
        string Date { get; set; }
        string LogMessage { get; set; }
    }
}
