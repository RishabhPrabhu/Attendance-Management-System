using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using AttendanceManagementSystem.BaseEmployee;
using AttendanceManagementSystem.BaseLog;

namespace AttendanceManagementSystem.DatabaseManagement
{
    public interface IDbOperations
    {
        string EndPointUrl { get; set; }
        string PrimaryKey { get; set; }

        DocumentClient GetClientConnection(string endPointUrl, string primaryKey);
        Task<Database> GetOrCreateDatabaseAsync(string dbId);
        Task<DocumentCollection> GetOrCreateCollectionAsync(string collectionId);
        Task<Document> GetOrCreateDocumentAsync<T>(T documentObject, string collectionLink) where T : IBaseEmployee;
        Document GetDocumentById(string id, string collectionLink);
        Task<bool> CheckInAsync(string id, string collectionLink);
        Task<bool> CheckOutAsync(string id, string collectionLink);
        Task<bool> AddTodayAttendanceFieldsAsync(string colLink);
        Task<bool> MarkStatusForTodayAsync(string collectionLink);
        Task<bool> WriteLogAsync(Message newMessage);
        Task<bool> InsertNewAttendanceAsync(string id, string collectionLink);
        Task<bool> InsertNewEmployeeAsync(Employee employee, string employeeColelctionLink);
        bool ValidateNewEmployee(Employee newEmployee, string empColLink);
    }
}