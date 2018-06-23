using System;

namespace AttendanceManagementSystem.BaseLog
{
    public class Message : IMessage
    { 
        private string _Date;
        public string Date { get { return _Date; } set { _Date = value; } }

        private string _LogMessage;
        public string LogMessage { get { return _LogMessage; } set { _LogMessage = value; } }
    }
}
