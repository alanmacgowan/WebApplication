using OpenQA.Selenium;
using TechTalk.SpecFlow;
using Unity;
using Unity.Lifetime;
using WebApplication.Tests.Acceptance.Base;
using WebApplication.Tests.Acceptance.Pages;
using WebApplication.Tests.Acceptance.Utils;

namespace WebApplication.Tests.Acceptance
{
    [Binding]
    public sealed class Hooks
    {
        [BeforeTestRun(Order = 1)]
        public static void RegisterPages()
        {
            Driver.StartBrowser(BrowserTypes.Chrome, false);
            UnityContainerFactory.GetContainer().RegisterType<EmployeeListPage>(new ContainerControlledLifetimeManager());
        }

        [BeforeTestRun(Order = 2)]
        public static void RegisterDriver()
        {
            UnityContainerFactory.GetContainer().RegisterInstance<IWebDriver>(Driver.Browser);
        }

        [BeforeTestRun(Order = 3)]
        public static void PrepareDataBase()
        {
            DBUtils.ExecutePreTestsScripts();
        }

        [AfterTestRun(Order = 1)]
        public static void CloseBrowser()
        {
            Driver.StopBrowser();
        }


        [AfterTestRun(Order = 2)]
        public static void CleanDataBase()
        {
            DBUtils.ExecutePostTestsScripts();
        }

        [BeforeFeature]
        public static void BeforeFeature()
        {
        }

        [AfterFeature]
        public static void AfterFeature()
        {
        }

        [BeforeStep]
        public static void BeforeStep()
        {
        }

        [AfterStep]
        public static void AfterStep()
        {
        }
    }

}
