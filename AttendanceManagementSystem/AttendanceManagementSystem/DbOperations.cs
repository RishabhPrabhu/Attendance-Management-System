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

namespace AttendanceManagementSystem.DatabaseManagement
{
    public class DbOperations : IDbOperations
    {
        private static DbOperations instance = new DbOperations();
        private DbOperations() { }
        public static DbOperations GetInstance
        {
            get
            {
                return instance;
            }
        }
        private string _EndPointUrl { get; set; }
        public string EndPointUrl
        {
            get
            {
                return _EndPointUrl;
            }
            set
            {
                _EndPointUrl = value;
            }
        }
        private string _PrimaryKey { get; set; }
        public string PrimaryKey
        {
            get
            {
                return _PrimaryKey;
            }
            set
            {
                _PrimaryKey = value;
            }
        }

        private static DocumentClient currentClient;
        private static Database currentDatabase;
        private static string logCollectionLink;

        /// <summary>
        /// Get the database client connection using the EndpointUrl and the Authorization Key.
        /// </summary>
        /// <param name="endPointUrl">The end point Url of the database account</param>
        /// <param name="authKey">The primary key of the database account</param>
        /// <returns>The Document client with the connection to the database</returns>
        public DocumentClient GetClientConnection(string endPointUrl, string authKey)
        {
            EndPointUrl = endPointUrl;
            PrimaryKey = authKey;

            try
            {
                currentClient = new DocumentClient(new Uri(EndPointUrl), PrimaryKey);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return currentClient;
        }

        /// <summary>
        /// Get the database by name, or create a new one if one with the given name doesn't exist.
        /// </summary>
        /// <param name="dbId">The id of the database to be searched or created</param>
        /// <returns>The databse with the given id.</returns>
        public async Task<Database> GetOrCreateDatabaseAsync(string dbId)
        {
            try
            {
                IEnumerable<Database> query = from db in currentClient.CreateDatabaseQuery()
                                              where db.Id == dbId
                                              select db;
                currentDatabase = query.FirstOrDefault();
                if (currentDatabase == null)
                {
                    try
                    {
                        currentDatabase = await currentClient.CreateDatabaseIfNotExistsAsync(new Database { Id = dbId });
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return currentDatabase;
        }

        /// <summary>
        /// Get the Collection by name, or create a new one if one with the given name doesn't exist.
        /// </summary>
        /// <param name="collectionId">The id of the collection to be searched or created.</param>
        /// <returns>The collection with the given id.</returns>
        public async Task<DocumentCollection> GetOrCreateCollectionAsync(string collectionId)
        {
            DocumentCollection documentCollection = null;
            try
            {
                IEnumerable<DocumentCollection> query = from collection in currentClient.CreateDocumentCollectionQuery(currentDatabase.SelfLink)
                                                        where collection.Id == collectionId
                                                        select collection;
                documentCollection = query.FirstOrDefault();
                if (documentCollection == null)
                {
                    try
                    {
                        documentCollection = await currentClient.CreateDocumentCollectionIfNotExistsAsync(currentDatabase.SelfLink, new DocumentCollection { Id = collectionId });
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return documentCollection;
        }

        /// <summary>
        /// Get or create a document with a particular ID
        /// </summary>
        /// <typeparam name="T">Class of the document</typeparam>
        /// <param name="documentObject">Object that is to be created in the document</param>
        /// <param name="collectionLink">The link of the collection which has to contain the document.</param>
        /// <returns></returns>
        public async Task<Document> GetOrCreateDocumentAsync<T>(T documentObject, string collectionLink)
            where T : IBaseEmployee
        {
            Document doc = GetDocumentById(documentObject.id, collectionLink);
            if (doc == null)
            {
                try
                {
                    doc = await currentClient.CreateDocumentAsync(collectionLink, documentObject);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            return doc;
        }

        /// <summary>
        /// Function to get the document by id
        /// </summary>
        /// <param name="id">id of the document to be fetched</param>
        /// <param name="collectionLink">link of the collection which contains the document</param>
        /// <returns>if the document is fetched, it returns the detched document, else null</returns>
        public Document GetDocumentById(string id, string collectionLink)
        {
            Document doc = null;
            try
            {
                doc = currentClient.CreateDocumentQuery<Document>(collectionLink)
                            .Where(d => d.Id == id)
                            .AsEnumerable()
                            .FirstOrDefault();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return doc;
        }

        /// <summary>
        /// Function to fetch all documents in a collection
        /// </summary>
        /// <param name="collectionLink">The collection link from which document is to be fetched</param>
        /// <returns>List od Documents that are, if any, fetched</returns>
        public List<Document> GetDocumentsInACollection(string collectionLink)
        {
            List<Document> docList = null;
            try
            {
                docList = currentClient.CreateDocumentQuery<Document>(collectionLink).AsEnumerable().ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return docList;
        }

        /// <summary>
        /// Inserts the received employee details in the database
        /// </summary>
        /// <param name="employee">The object that contsins the details of the new employee</param>
        /// <param name="employeeColelctionLink">The link of the employee collection in database</param>
        /// <returns></returns>
        public async Task<bool> InsertNewEmployeeAsync(Employee employee, string employeeColelctionLink)
        {
            // Creating a document for this employee
            Document doc = await GetOrCreateDocumentAsync<Employee>(employee, employeeColelctionLink);
            if (doc is null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Creates a Log document in the Log collection, just once
        /// </summary>
        /// <param name="colLink">The link of the Log collection</param>
        /// <returns></returns>
        public async Task CreateLogDocumentAsync(string colLink)
        {
            logCollectionLink = colLink;
            Log log = new Log()
            {
                id = Properties.Resources.LogCollectionName,
                Messages = null
            };
            await GetOrCreateDocumentAsync<Log>(log, colLink);
        }

        /// <summary>
        /// Write Log in the database
        /// </summary>
        /// <param name="newMessage">The log to be written</param>
        /// <param name="colLink">The link of the Log collection</param>
        /// <returns></returns>
        public async Task<bool> WriteLogAsync(Message newMessage)
        {
            Document doc = GetDocumentById(Properties.Resources.LogCollectionName, logCollectionLink);
            Log log = JsonConvert.DeserializeObject<Log>(doc.ToString());
            if (log.Messages is null)
            {
                log.Messages = new List<Message>();
            }
            List<Message> messageList = log.Messages;
            // Add the new message at top of the list
            messageList.Insert(0, newMessage);
            // Replace the existing document with the updated one.
            try
            {
                await currentClient.ReplaceDocumentAsync(doc.SelfLink, log);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        /// <summary>
        /// Function to insert a new attendance document for the given employee id, if not exists.
        /// </summary>
        /// <param name="id">The employee id</param>
        /// <param name="collectionLink">The link of the attendance collection</param>
        /// <returns></returns>
        public async Task<bool> InsertNewAttendanceAsync(string id, string collectionLink)
        {
            // Create the attendance object
            EmployeeAttendance employeeAttendance = new EmployeeAttendance
            {
                Date = DateTime.Now.ToShortDateString(),
            };
            Attendance attendance = new Attendance
            {
                id = id,
                EmpAttendance = new List<EmployeeAttendance>
                    {
                        employeeAttendance
                    }
            };

            // Create the new document
            Document doc = await GetOrCreateDocumentAsync<Attendance>(attendance, collectionLink);
            if (doc is null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Checkout the employee for the current day
        /// </summary>
        /// <param name="id">The employee id</param>
        /// <param name="collectionLink">The link of the collection</param>
        /// <returns></returns>
        public async Task<bool> CheckOutAsync(string id, string collectionLink)
        {
            bool checkOutSuccess = false;
            Document document = GetDocumentById(id, collectionLink);
            // Fetch object from the document.
            Attendance attendance = JsonConvert.DeserializeObject<Attendance>(document.ToString());
            List<EmployeeAttendance> list = attendance.EmpAttendance;

            // Check if employee has checked in or not today, or if the employee has already checked out
            bool notCheckedIn = true;
            bool alreadyCheckedOut = false;
            foreach (var item in list)
            {
                if (item.Date == DateTime.Now.ToShortDateString())
                {
                    if (item.CheckInTime == null && item.CheckOutTime == null)
                    {
                        // Employee has not checked in today
                        notCheckedIn = true;
                        break;
                    }
                    else if (item.CheckInTime != null && item.CheckOutTime == null)
                    {
                        // Employee has checked in today but not checked out
                        notCheckedIn = false;
                        break;
                    }
                    else if (item.CheckInTime != null && item.CheckOutTime != null)
                    {
                        // Employee has already checked in and checked out for today
                        alreadyCheckedOut = true;
                        break;
                    }
                }
            }
            if (alreadyCheckedOut)
            {
                Console.WriteLine(Properties.Resources.EmployeeAlreadyCheckedOut, id);
            }
            else if (notCheckedIn)
            {
                Console.WriteLine(Properties.Resources.EmployeeNotCheckedIn, id);
            }
            else
            {
                foreach (var item in list)
                {
                    if (item.Date == DateTime.Now.ToShortDateString() && item.CheckInTime != null && item.CheckOutTime == null)
                    {
                        item.CheckOutTime = DateTime.Now.ToShortTimeString();
                        break;
                    }
                }
                // Replace the existing document with the undated one.
                try
                {
                    await currentClient.ReplaceDocumentAsync(document.SelfLink, attendance);
                    checkOutSuccess = true;
                    Console.WriteLine(Properties.Resources.CheckOutSuccess);
                    // Write to Log
                    await CreateandWriteLogAsync(Properties.Resources.LogEmployeeCheckedOut + attendance.id);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            return checkOutSuccess;
        }

        /// <summary>
        /// Check In function for the employee
        /// </summary>
        /// <param name="id">The employee id</param>
        /// <param name="collectionLink">The link of the attendance collection</param>
        /// <returns></returns>
        public async Task<bool> CheckInAsync(string id, string collectionLink)
        {
            bool checkInSuccess = false;
            // Fetch object from the document.
            Document document = GetDocumentById(id, collectionLink);
            Attendance attendance = JsonConvert.DeserializeObject<Attendance>(document.ToString());
            List<EmployeeAttendance> list = attendance.EmpAttendance;

            // Check if already checked in
            bool alreadyCheckedIn = false;
            foreach (var item in list)
            {
                if (item.Date == DateTime.Now.ToShortDateString() && item.CheckInTime != null)
                {
                    alreadyCheckedIn = true;
                    break;
                }
            }

            // If already checked in, display message, else check in
            if (alreadyCheckedIn)
            {
                Console.WriteLine(Properties.Resources.EmployeeAlreadyCheckedIn, attendance.id);
            }
            else
            {
                foreach (var item in list)
                {
                    if (item.Date == DateTime.Now.ToShortDateString() && item.CheckInTime == null)
                    {
                        item.CheckInTime = DateTime.Now.ToShortTimeString();
                        break;
                    }
                }
                // Replace the existing document with the updated one.
                try
                {
                    await currentClient.ReplaceDocumentAsync(document.SelfLink, attendance);
                    checkInSuccess = true;
                    Console.WriteLine(Properties.Resources.CheckInSuccess);
                    // Write to Log
                    await CreateandWriteLogAsync(Properties.Resources.LogEmployeeCheckedIn + attendance.id);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            return checkInSuccess;
        }

        /// <summary>
        /// Function to mark employee as present or absent for the day
        /// </summary>
        /// <param name="collectionLink">The link of the attendance collection</param>
        public async Task<bool> MarkStatusForTodayAsync(string collectionLink)
        {
            bool statusMarked = false;
            List<Document> docList = GetDocumentsInACollection(collectionLink);
            foreach (var doc in docList)
            {
                if (doc != null)
                {
                    // Fetch object from the document.
                    Attendance attendance = JsonConvert.DeserializeObject<Attendance>(doc.ToString());
                    List<EmployeeAttendance> attendanceList = attendance.EmpAttendance;

                    // Mark Sttatus
                    foreach (var currentDayAttendance in attendanceList)
                    {
                        if (currentDayAttendance.Date == DateTime.Now.ToShortDateString() &&
                            currentDayAttendance.CheckInTime != null &&
                            currentDayAttendance.Status != Properties.Resources.Present)
                        {
                            currentDayAttendance.Status = Properties.Resources.Present;
                        }
                        else if (currentDayAttendance.Date == DateTime.Now.ToShortDateString() &&
                            currentDayAttendance.CheckInTime == null &&
                            currentDayAttendance.Status != Properties.Resources.Absent)
                        {
                            currentDayAttendance.Status = Properties.Resources.Absent;
                        }
                    }
                    // Replace the existing document with the updated one.
                    try
                    {
                        await currentClient.ReplaceDocumentAsync(doc.SelfLink, attendance);
                        statusMarked = true;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        statusMarked = false;
                    }
                }
            }
            return statusMarked;
        }

        /// <summary>
        /// Subscribes and triggers the logging event
        /// </summary>
        /// <param name="logMessage">The message to be logged</param>
        /// <returns></returns>
        public async Task WriteLogEventAsync(Message logMessage)
        {
            LogTrigger.Logger -= new LogTrigger.LogHandler(WriteLogAsync);
            LogTrigger.Logger += new LogTrigger.LogHandler(WriteLogAsync);
            await LogTrigger.Log(logMessage);
        }

        /// <summary>
        /// Converts the log message into a database compliant object, and then writes the log
        /// </summary>
        /// <param name="message">The log message to be written</param>
        /// <returns></returns>
        public async Task CreateandWriteLogAsync(string message)
        {
            Message logMessage = new Message()
            {
                Date = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString(),
                LogMessage = message
            };
            await WriteLogEventAsync(logMessage);
        }

        /// <summary>
        /// Inserts the current day fields in the attendance collection for marking.
        /// </summary>
        /// <param name="colLink">The link to attendance collection</param>
        /// <returns></returns>
        public async Task<bool> AddTodayAttendanceFieldsAsync(string colLink)
        {
            bool fieldsAdded = false;
            List<Document> docList = GetDocumentsInACollection(colLink);
            foreach (var doc in docList)
            {
                if (doc != null)
                {
                    // Fetch object from the document.
                    Attendance attendance = JsonConvert.DeserializeObject<Attendance>(doc.ToString());
                    List<EmployeeAttendance> attendanceList = attendance.EmpAttendance;
                    bool fieldsExist = false;
                    // Mark Sttatus
                    foreach (var currentDayAttendance in attendanceList)
                    {
                        if (currentDayAttendance.Date == DateTime.Now.ToShortDateString())
                        {
                            fieldsExist = true;
                        }
                    }
                    if (fieldsExist == false)
                    {
                        // Create the attendance object
                        EmployeeAttendance employeeAttendance = new EmployeeAttendance
                        {
                            Date = DateTime.Now.ToShortDateString(),
                        };
                        attendanceList.Insert(0, employeeAttendance);
                    }
                    // Replace the existing document with the updated one.
                    try
                    {
                        await currentClient.ReplaceDocumentAsync(doc.SelfLink, attendance);
                        fieldsAdded = true;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        fieldsAdded = false;
                    }
                }
            }
            return fieldsAdded;
        }

        /// <summary>
        /// Validates the new employee before inserting into the database.
        /// </summary>
        /// <param name="newEmployee">The object of new employee</param>
        /// <param name="empColLink">The link to the employee collection</param>
        /// <returns>If the newEmployee is valid or not</returns>
        public bool ValidateNewEmployee(Employee newEmployee, string empColLink)
        {
            bool isNewEmployeeValid = true;
            // Check for a valid integer employee id
            try
            {
                int.Parse(newEmployee.id);
                if (newEmployee.ManagerOfThisEmployee != null) { int.Parse(newEmployee.ManagerOfThisEmployee.id); }
            }
            catch (Exception e)
            {
                isNewEmployeeValid = false;
            }
            // Check validity by comparing with each employee
            List<Document> docList = GetDocumentsInACollection(empColLink);
            foreach (var doc in docList)
            {
                if (isNewEmployeeValid == false) { break; }
                if (doc != null)
                {
                    // Fetch object from the document.
                    Employee oldEmployee = JsonConvert.DeserializeObject<Employee>(doc.ToString());
                    // Check if the employee id already exists
                    isNewEmployeeValid = oldEmployee.id == newEmployee.id ? false : isNewEmployeeValid;
                    // Check if the employee Email already exists
                    isNewEmployeeValid = oldEmployee.EmailId == newEmployee.EmailId ? false : isNewEmployeeValid;
                }
            }
            return isNewEmployeeValid;
        }
    }
}
