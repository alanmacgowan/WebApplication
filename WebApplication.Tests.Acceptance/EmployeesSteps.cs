using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TechTalk.SpecFlow;
using WebApplication.Tests.Acceptance.Base;
using WebApplication.Tests.Acceptance.Pages;
using Unity;

namespace WebApplication.Tests.Acceptance
{
    [Binding]
    public class EmployeesSteps
    {
        private HomePage _homePage;
        private EmployeeListPage _employeeListPage;

        public EmployeesSteps()
        {
            _homePage = UnityContainerFactory.GetContainer().Resolve<HomePage>();
            _employeeListPage = UnityContainerFactory.GetContainer().Resolve<EmployeeListPage>();
        }

        [Given(@"I am on the Home Page")]
        public void GivenIAmOnTheHomePage()
        {
            _homePage.Open();
        }

        [When(@"I click on Employees menu")]
        public void WhenIClickOnEmployeesMenu()
        {
            _homePage.ClickEmployeesMenu();
        }

        [Then(@"Employee List should be displayed")]
        public void ThenEmployeeListShouldBeDisplayed()
        {
            Assert.IsTrue(_employeeListPage.TableDisplayed());
        }
    }
}
