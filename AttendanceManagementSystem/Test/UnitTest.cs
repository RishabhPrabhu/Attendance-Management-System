using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AttendanceManagementSystem;
using Newtonsoft.Json;
using System.Collections.Generic;
using AttendanceManagementSystem.BaseEmployee;
using AttendanceManagementSystem.BaseAttendance;
using AttendanceManagementSystem.BaseLog;
using AttendanceManagementSystem.DatabaseManagement;
using AttendanceManagementSystem.Exceptions;
using Moq;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace AttendanceManagementSystem.Test
{
    [TestClass]
    public class UnitTest
    {
        Mock<IDbOperations> mockDb = new Mock<IDbOperations>();
        string empColLink, attColLink, logColLink;
        private Employee GetValidEmployee()
        {
            Employee employee = new Employee();
            employee.id = "9999";
            employee.EmployeeName = "Test Employee " + employee.id;
            employee.EmailId = employee.EmployeeName + "@testcompany.com";
            employee.ManagerOfThisEmployee = new Manager();
            employee.ManagerOfThisEmployee.id = "10000";
            employee.ManagerOfThisEmployee.EmployeeName = "Test Manager " + employee.ManagerOfThisEmployee.id;
            employee.ManagerOfThisEmployee.EmailId = employee.ManagerOfThisEmployee.EmployeeName + "@testcompany.com";
            return employee;
        }

        private Employee GetInvalidEmployee()
        {
            Employee employee = new Employee();
            employee.id = "one";
            employee.EmployeeName = "Test Employee " + employee.id;
            employee.EmailId = employee.EmployeeName + "@testcompany.com";
            employee.ManagerOfThisEmployee = new Manager();
            employee.ManagerOfThisEmployee.id = "10000";
            employee.ManagerOfThisEmployee.EmployeeName = "Test Manager " + employee.ManagerOfThisEmployee.id;
            employee.ManagerOfThisEmployee.EmailId = employee.ManagerOfThisEmployee.EmployeeName + "@testcompany.com";
            return employee;
        }

        [TestMethod]
        public void IsNewEmployeeValidTest_pass()
        {
            Employee validEmployee = GetValidEmployee();

            mockDb.Setup(x => x.ValidateNewEmployee(validEmployee, empColLink)).Returns(true);

            Assert.IsTrue(mockDb.Object.ValidateNewEmployee(validEmployee, empColLink));
        }

        [TestMethod]
        public void IsNewEmployeeValidTest_fail()
        {
            Employee invalidEmployee = GetInvalidEmployee();

            mockDb.Setup(x => x.ValidateNewEmployee(invalidEmployee, empColLink)).Returns(false);

            Assert.IsFalse(mockDb.Object.ValidateNewEmployee(invalidEmployee, empColLink));
        }

        [TestMethod]
        public void AddNewEmployeeTest_pass()
        {
            Employee employee = GetValidEmployee();

            mockDb.Setup(x => x.InsertNewEmployeeAsync(employee, empColLink)).ReturnsAsync(true);

            Assert.IsTrue(mockDb.Object.InsertNewEmployeeAsync(employee, empColLink).Result);
        }

        [TestMethod]
        public void AddNewEmployeeTest_fail()
        {
            Employee employee = GetInvalidEmployee();

            mockDb.Setup(x => x.InsertNewEmployeeAsync(employee, empColLink)).ReturnsAsync(false);

            Assert.IsFalse(mockDb.Object.InsertNewEmployeeAsync(employee, empColLink).Result);
        }

        [TestMethod]
        public void AddNewAttendanceTest_pass()
        {
            Employee employee = GetValidEmployee();

            mockDb.SetupSequence(x => x.InsertNewAttendanceAsync(employee.id, empColLink))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            Assert.IsTrue(mockDb.Object.InsertNewAttendanceAsync(employee.id, empColLink).Result);
        }

        [TestMethod]
        public void AddNewAttendanceTest_fail()
        {
            Employee employee = GetInvalidEmployee();

            Assert.IsFalse(mockDb.Object.InsertNewAttendanceAsync(employee.id, empColLink).Result);
        }

        [TestMethod]
        public void EmployeeCheckInTest_pass()
        {
            Employee employee = GetValidEmployee();

            mockDb.SetupSequence(x => x.CheckInAsync(employee.id, attColLink))
                .ReturnsAsync(true)
                .ThrowsAsync(new AMSExceptions(Properties.Resources.EmployeeAlreadyCheckedIn, employee.id));

            Assert.IsTrue(mockDb.Object.CheckInAsync(employee.id, attColLink).Result);
        }

        [TestMethod]
        public void EmployeeCheckInTest_fail()
        {
            Employee employee = GetValidEmployee();
            try
            {
                mockDb.Object.CheckInAsync(employee.id, attColLink);
            }
            catch (AMSExceptions ex)
            {
                Assert.Fail();
                Assert.AreEqual(string.Format(Properties.Resources.EmployeeAlreadyCheckedIn, employee.id), ex.Message);
            }
        }

        [TestMethod]
        public void EmployeeCheckOutTest_pass()
        {
            Employee employee = GetValidEmployee();

            mockDb.SetupSequence(x => x.CheckOutAsync(employee.id, attColLink))
                .ReturnsAsync(true)
                .ThrowsAsync(new AMSExceptions(Properties.Resources.EmployeeAlreadyCheckedOut, employee.id));

            Assert.IsTrue(mockDb.Object.CheckOutAsync(employee.id, attColLink).Result);
        }

        [TestMethod]
        public void EmployeeCheckOutTest_fail()
        {
            Employee employee = GetValidEmployee();
            try
            {
                mockDb.Object.CheckOutAsync(employee.id, attColLink);
            }
            catch (AMSExceptions ex)
            {
                Assert.Fail();
                Assert.AreEqual(string.Format(Properties.Resources.EmployeeAlreadyCheckedOut, employee.id), ex.Message);
            }
        }

        [TestMethod]
        public void WriteLogTest_pass()
        {
            Message message = new Message()
            {
                Date = DateTime.Now.ToShortDateString(),
                LogMessage = "Test Log"
            };

            mockDb.Setup(x => x.WriteLogAsync(message)).ReturnsAsync(true);

            Assert.IsTrue(mockDb.Object.WriteLogAsync(message).Result);
        }

        [TestMethod]
        public void WriteLogTest_Fail()
        {
            mockDb.Setup(x => x.WriteLogAsync(new Message())).Throws(new AMSExceptions(Properties.Resources.TryAgain));

            try
            {
                mockDb.Object.WriteLogAsync(new Message());
            }
            catch (AMSExceptions ex)
            {
                Assert.Fail();
                Assert.AreEqual(Properties.Resources.TryAgain, ex.Message);
            }
        }

        [TestMethod]
        public void MarkPresenceStatusTest_pass()
        {
            mockDb.Setup(x => x.MarkStatusForTodayAsync(attColLink)).ReturnsAsync(true);

            Assert.IsTrue(mockDb.Object.MarkStatusForTodayAsync(attColLink).Result);
        }

        [TestMethod]
        public void MarkPresenceStatusTest_Fail()
        {
            mockDb.Setup(x => x.MarkStatusForTodayAsync(attColLink)).ThrowsAsync(new AMSExceptions(Properties.Resources.TryAgain));
            try
            {
                mockDb.Object.MarkStatusForTodayAsync(attColLink);
            }
            catch (AMSExceptions ex)
            {
                Assert.Fail();
                Assert.AreEqual(Properties.Resources.TryAgain, ex.Message);
            }
        }

        [TestMethod]
        public void AddAttendanceFieldsTest_pass()
        {
            mockDb.Setup(x => x.AddTodayAttendanceFieldsAsync(attColLink)).ReturnsAsync(true);

            Assert.IsTrue(mockDb.Object.AddTodayAttendanceFieldsAsync(attColLink).Result);
        }

        [TestMethod]
        public void AddAttendanceFieldsTest_fail()
        {
            mockDb.Setup(x => x.AddTodayAttendanceFieldsAsync(attColLink)).ThrowsAsync(new AMSExceptions(Properties.Resources.TryAgain));
            try
            {
                mockDb.Object.AddTodayAttendanceFieldsAsync(attColLink);
            }
            catch (AMSExceptions ex)
            {
                Assert.Fail();
                Assert.AreEqual(Properties.Resources.TryAgain, ex.Message);
            }
        }
    }
}
