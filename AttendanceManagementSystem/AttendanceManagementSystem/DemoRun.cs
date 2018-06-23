using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
using AttendanceManagementSystem.BaseAttendance;
using AttendanceManagementSystem.BaseEmployee;
using AttendanceManagementSystem.BaseLog;
using AttendanceManagementSystem.DatabaseManagement;
using AttendanceManagementSystem.DatabaseManagement.Trigger;

namespace AttendanceManagementSystem.Demo
{
    class DemoRun
    {
        DbOperations db = DbOperations.GetInstance;
        DocumentCollection employeeCollection, attendanceCollection, logCollection;

        /// <summary>
        /// The main function that will be called fom outside to execute the project
        /// </summary>
        /// <returns></returns>
        public async Task RunAsync()
        {
            // Create the databses
            await RunMeFirstAsync();
            // Create Today's field  for attendance
            await db.AddTodayAttendanceFieldsAsync(attendanceCollection.SelfLink);
            // Main menu
            await MainMenuAsync(attendanceCollection.SelfLink, employeeCollection.SelfLink, logCollection.SelfLink);
        }

        /// <summary>
        /// Created the connection to the database and does the pre-verifications and pre-validations work.
        /// </summary>
        /// <returns></returns>
        public async Task RunMeFirstAsync()
        {
            // Get the database connection parameters
            string EndPointUrl = Properties.Resources.ConnectionEndPointUrl;
            string PrimaryKey = Properties.Resources.ConnectionPrimaryKey;

            // Getting client connection
            DocumentClient client = db.GetClientConnection(EndPointUrl, PrimaryKey);
            // Creating database
            Database database = await db.GetOrCreateDatabaseAsync(Properties.Resources.DatabaseName);
            // Creating Employee Collection
            employeeCollection = await db.GetOrCreateCollectionAsync(Properties.Resources.EmployeeCollectionName);
            // Creating Attendance Collection
            attendanceCollection = await db.GetOrCreateCollectionAsync(Properties.Resources.AttendanceCollectionName);
            // Creating Log Collection
            logCollection = await db.GetOrCreateCollectionAsync(Properties.Resources.LogCollectionName);
            // Creating Log document
            await db.CreateLogDocumentAsync(logCollection.SelfLink);
        }

        /// <summary>
        /// The Main Menu that the Employee will see.
        /// </summary>
        public async Task MainMenuAsync(string attendanceCollectionLink, string employeeColelctionLink, string logCollectionLink)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine(Properties.Resources.MainMenu + Properties.Resources.QuestionMark);
                Console.WriteLine(Properties.Resources.OptionAddEmployee);
                Console.WriteLine(Properties.Resources.OptionMarkAttendance);
                Console.WriteLine(Properties.Resources.OptionExit);
                while (true)
                {
                    try
                    {
                        Console.WriteLine(Properties.Resources.AskChoice);
                        char ch = Console.ReadLine()[0];
                        switch (ch)
                        {
                            case '1':
                                await GetEmployeesAsync(attendanceCollectionLink, employeeColelctionLink);
                                break;
                            case '2':
                                await MarkAttendanceAsync(attendanceCollectionLink, employeeColelctionLink);
                                break;
                            case '3':
                                // Work for marking status at the end
                                await db.MarkStatusForTodayAsync(attendanceCollectionLink);
                                Environment.Exit(0);
                                break;
                            default:
                                throw new Exception();
                        }
                        Console.WriteLine(Properties.Resources.PressKey);
                        Console.ReadKey();
                        break;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine();
                        Console.WriteLine(Properties.Resources.InvalidInput);
                        Console.WriteLine();
                        continue;
                    }
                }
            }
        }

        /// <summary>
        /// Function to mark the attendance of the employee
        /// </summary>
        /// <param name="attendanceCollectionLink">The link of the attendance collection in database</param>
        /// <param name="employeeColelctionLink">The link of the employee collection in database</param>
        /// <returns></returns>
        public async Task MarkAttendanceAsync(string attendanceCollectionLink, string employeeColelctionLink)
        {
            Console.Clear();
                Console.Write(Properties.Resources.AskIdForAttendance);
                string id = Console.ReadLine();
            Document employeeByThisId = null;
            try
            {
                // Get Employee with this id
                employeeByThisId = db.GetDocumentById(id, employeeColelctionLink);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            if (employeeByThisId is null)
            {
                Console.WriteLine(Properties.Resources.EmployeeNotFound);
                await GetEmployeesAsync(attendanceCollectionLink, employeeColelctionLink);
            }
            else
            {
                await AttendanceMenuAsync(id, attendanceCollectionLink);
            }
        }

        /// <summary>
        /// Function to display the options of attendance, namely check in and check out
        /// </summary>
        /// <param name="id">The employee id of whose attendance is to be marked</param>
        /// <param name="collectionLink">The link of the attendance collection in the database</param>
        /// <returns></returns>
        public async Task AttendanceMenuAsync(string id, string collectionLink)
        {
            Console.Clear();
            Console.WriteLine(Properties.Resources.MainMenu);
            Console.WriteLine(Properties.Resources.OptionCheckIn);
            Console.WriteLine(Properties.Resources.OptionCheckOut);
            while (true)
            {
                try
                {
                    Console.Write(Properties.Resources.AskChoice);
                    char ch = Console.ReadLine()[0];
                    switch (ch)
                    {
                        case '1':
                            //checkin
                            await db.CheckInAsync(id, collectionLink);
                            break;
                        case '2':
                            //checkout
                            await db.CheckOutAsync(id, collectionLink);
                            break;
                        default:
                            throw new Exception(Properties.Resources.InvalidInput);
                    }
                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine();
                    Console.WriteLine(e.Message);
                    Console.WriteLine();
                    continue;
                }
            }
        }

        /// <summary>
        /// Function to get the details of a new employee
        /// </summary>
        /// <param name="collectionLink">Link of the employee collection in the database</param>
        /// <returns></returns>
        public async Task GetEmployeesAsync(string attendanceCollectionLink, string employeeColelctionLink)
        {
            char ch = 'y';
            while (ch == 'y' || ch == 'Y')
            {
                try
                {
                    Console.WriteLine();
                    Console.Write(Properties.Resources.AskToAddEmployee);
                    ch = Console.ReadLine()[0];
                    switch (ch)
                    {
                        case 'Y':
                        case 'y':
                            await GetNewEmployeeAsync(attendanceCollectionLink, employeeColelctionLink);
                            break;
                        case 'N':
                        case 'n':
                            break;
                        default:
                            throw new Exception(Properties.Resources.InvalidInput);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine();
                    Console.WriteLine(e.Message);
                    Console.WriteLine();
                    ch = 'y';
                    continue;
                }
            }
        }

        /// <summary>
        /// Function to add new employee to the database, and creating its attendance document
        /// </summary>
        /// <param name="attendanceCollectionLink"></param>
        /// <param name="employeeColelctionLink"></param>
        /// <returns></returns>
        public async Task GetNewEmployeeAsync(string attendanceCollectionLink, string employeeColelctionLink)
        {
            // Getting the Employee details
            Employee employee = GetEmployeeDetails();
            if (employee != null)
            {
                // Calling OnNewEmployeeAdd event
                NewEmployeeAdditionTrigger.EmployeeCreatorHandler -= new NewEmployeeAdditionTrigger.EmployeeCreator(CreateNewEmployeeAsync);
                NewEmployeeAdditionTrigger.EmployeeCreatorHandler += new NewEmployeeAdditionTrigger.EmployeeCreator(CreateNewEmployeeAsync);
                await NewEmployeeAdditionTrigger.OnAddNewEmployeeAsync(employee, employeeColelctionLink, attendanceCollectionLink);
                // Write to Log
                await db.CreateandWriteLogAsync(Properties.Resources.LogNewEmployeeAdded + employee.id);
            }
        }

        /// <summary>
        /// Inserts the new employee to the database and triggers post insertion events.
        /// </summary>
        /// <param name="employee">The new employee object</param>
        /// <param name="employeeColelctionLink">The link to employee collection</param>
        /// <param name="attendanceCollectionLink">The link to attendance collection</param>
        /// <returns></returns>
        public async Task CreateNewEmployeeAsync(Employee employee, string employeeColelctionLink, string attendanceCollectionLink)
        {
            bool result = await db.InsertNewEmployeeAsync(employee, employeeColelctionLink);
            if (result is true)
            {
                // call OnNewEmployeeAdded event
                NewEmployeeAdditionTrigger.AttendanceCreatorHandler -= new NewEmployeeAdditionTrigger.AttendanceCreator(CreateNewAttendanceDocForEmployeeAsync);
                NewEmployeeAdditionTrigger.AttendanceCreatorHandler += new NewEmployeeAdditionTrigger.AttendanceCreator(CreateNewAttendanceDocForEmployeeAsync);
                await NewEmployeeAdditionTrigger.OnNewEmployeeAddedAsync(employee.id, attendanceCollectionLink);
                // Write to Log
                await db.CreateandWriteLogAsync(Properties.Resources.LogNewAttendanceCreated + employee.id);
            }
            else
            {
                Console.WriteLine(Properties.Resources.TryAgain);
            }
        }

        /// <summary>
        /// Inserts new attendance document for a new employee into the database.
        /// </summary>
        /// <param name="id">The id of the new employee</param>
        /// <param name="colLink">The link to the attendance collection</param>
        /// <returns></returns>
        public async Task CreateNewAttendanceDocForEmployeeAsync(string id, string colLink)
        {
            // Creating an attendance document for this employee
            bool result= await db.InsertNewAttendanceAsync(id, colLink);
            if(result is false)
            {
                Console.WriteLine(Properties.Resources.TryAgain);
            }
        }

        /// <summary>
        /// Input the Employee details.
        /// </summary>
        /// <returns> The employee object with the entered details. </returns>
        public Employee GetEmployeeDetails()
        {
            bool isNewEmployeeValid = true;
            Employee emp = new Employee();
            Console.Clear();
            Console.WriteLine(Properties.Resources.AskEmployeeDetails);
            Console.Write(Properties.Resources.AskName);
            emp.EmployeeName = Console.ReadLine();
            Console.Write(Properties.Resources.AskID);
            emp.id = Console.ReadLine();
            Console.Write(Properties.Resources.AskEmail);
            emp.EmailId = Console.ReadLine();

            while (true)
            {
                try
                {
                    Console.Write(Properties.Resources.HasAManager, emp.EmployeeName);
                    char ch = Console.ReadLine()[0];
                    switch (ch)
                    {
                        case 'Y':
                        case 'y':
                            emp.ManagerOfThisEmployee = GetManagerDetails();
                            break;
                        case 'N':
                        case 'n':
                            emp.ManagerOfThisEmployee = null;
                            break;
                        default:
                            throw new Exception(Properties.Resources.InvalidInput);
                    }
                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine();
                    Console.WriteLine(e.Message);
                    Console.WriteLine();
                    continue;
                }
            }
            // Check the validity of the new employee
            isNewEmployeeValid = db.ValidateNewEmployee(emp, employeeCollection.SelfLink);
            if (isNewEmployeeValid == false)
            {
                // Since the new employee is not valid, display the error message and ask to enter new details.
                Console.WriteLine(Properties.Resources.Error + Properties.Resources.EmployeeInvalid);
                Console.WriteLine(Properties.Resources.PressKey);
                Console.ReadKey();
                emp = null;
            }
            return emp;
        }

        /// <summary>
        /// Input the Manager's details.
        /// </summary>
        /// <returns> The manager object with the entered details. </returns>
        public Manager GetManagerDetails()
        {
            Manager mgr = new Manager();
            Console.WriteLine(Properties.Resources.AskManagerDetails);
            Console.Write(Properties.Resources.AskName);
            mgr.EmployeeName = Console.ReadLine();
            Console.Write(Properties.Resources.AskID);
            mgr.id = Console.ReadLine();
            Console.Write(Properties.Resources.AskEmail);
            mgr.EmailId = Console.ReadLine();
            return mgr;
        }       
    }
}