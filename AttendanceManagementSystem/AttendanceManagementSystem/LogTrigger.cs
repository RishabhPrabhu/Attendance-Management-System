using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AttendanceManagementSystem.BaseLog;

namespace AttendanceManagementSystem
{
    class LogTrigger
    {
        public delegate Task LogHandler(Message message);
        public static event LogHandler Logger;

        /// <summary>
        /// Writes the log into the database when invoked.
        /// </summary>
        /// <param name="logMessage"></param>
        /// <returns></returns>
        public static async Task Log(Message logMessage)
        {
            await Logger?.Invoke(logMessage);
        }
    }
}
