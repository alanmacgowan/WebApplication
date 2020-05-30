using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebApplication.Models;

namespace WebApplication.Tests
{
    [TestClass]
    public class EmployeesControllerTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            //Arrange
            var employee = new Employee();

            //Act
            employee.Id = 1;
            employee.FirstName = "Juan";
            employee.LastName = "Perez";

            //Assert
            Assert.IsTrue(employee.FirstName == "Juan");

        }

        [TestMethod]
        public void TestMethod2()
        {
            //Arrange
            var employee = new Employee();

            //Act
            employee.Id = 1;
            employee.FirstName = "Juan";
            employee.LastName = "Perez";

            //Assert
            Assert.IsTrue(employee.LastName == "Perez");

        }

    }
}
