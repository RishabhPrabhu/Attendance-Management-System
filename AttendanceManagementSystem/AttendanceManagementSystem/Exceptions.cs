using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttendanceManagementSystem.Exceptions
{
    public class AMSExceptions : Exception
    {
        public AMSExceptions(string message)
            : base(message) { }

        public AMSExceptions(string message, params object[] args)
            : base(string.Format(message, args)) { }

    }
}
