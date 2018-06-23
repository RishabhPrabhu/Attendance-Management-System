using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
using AttendanceManagementSystem.Demo;

namespace AttendanceManagementSystem.Main
{
    class MainApp
    {
        static void Main(string[] args)
        {
            DemoRun demo = new DemoRun();
            try
            {
                demo.RunAsync().Wait();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

    }
}
