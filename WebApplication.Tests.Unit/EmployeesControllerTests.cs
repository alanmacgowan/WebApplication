using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Web.Mvc;
using WebApplication.Controllers;
using WebApplication.Models;
using WebApplication.Services.Abstract;

namespace WebApplication.Tests.Unit
{
    [TestClass]
    public class EmployeesControllerTests
    {
        private Mock<IEmployeeService> _employeeService;
        private EmployeesController _employeesController;

        public EmployeesControllerTests()
        {
            _employeeService = new Mock<IEmployeeService>();
            _employeesController = new EmployeesController(_employeeService.Object);
        }

        [TestMethod]
        public void Details_Test()
        {
            //Arrange
            var employee = new Employee() { Id = 1, FirstName = "Juan", LastName = "Perez" };
            _employeeService.Setup(x => x.GetById(It.IsAny<int>())).Returns(employee);

            //Act
            ViewResult result = _employeesController.Details(1) as ViewResult;

            //Assert
            Assert.AreEqual((result.Model as Employee).FirstName, employee.FirstName);
        }


    }
}
