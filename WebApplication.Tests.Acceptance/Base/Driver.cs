using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication.Tests.Acceptance.Base
{
    public static class Driver
    {
        private static WebDriverWait _browserWait;

        private static IWebDriver _browser;

        public static IWebDriver Browser
        {
            get
            {
                if (_browser == null)
                {
                    throw new NullReferenceException("The WebDriver browser instance was not initialized. You should first call the method Start.");
                }
                return _browser;
            }
            private set
            {
                _browser = value;
            }
        }

        public static WebDriverWait BrowserWait
        {
            get
            {
                if (_browserWait == null || _browser == null)
                {
                    throw new NullReferenceException("The WebDriver browser wait instance was not initialized. You should first call the method Start.");
                }
                return _browserWait;
            }
            private set
            {
                _browserWait = value;
            }
        }

        public static void StartBrowser(BrowserTypes browserType = BrowserTypes.Firefox, bool headless = false, int defaultTimeOut = 30)
        {
            switch (browserType)
            {
                case BrowserTypes.Firefox:
                    if (headless)
                    {
                        var firefoxOptions = new FirefoxOptions();
                        firefoxOptions.AddArgument("--headless");
                        Browser = new FirefoxDriver(firefoxOptions);
                    }
                    else
                    {
                        Browser = new FirefoxDriver();
                    }
                    break;
                case BrowserTypes.InternetExplorer:
                    Browser = new InternetExplorerDriver();
                    break;
                case BrowserTypes.Chrome:
                    if (headless)
                    {
                        var chromeOptions = new ChromeOptions();
                        chromeOptions.AddArguments("--headless");
                        chromeOptions.AddArguments("window-size=1200x600");
                        Browser = new ChromeDriver(chromeOptions);
                    }
                    else
                    {
                        Browser = new ChromeDriver();
                    }
                    break;
                default:
                    break;
            }
            BrowserWait = new WebDriverWait(Browser, TimeSpan.FromSeconds(defaultTimeOut));
        }

        public static void StopBrowser()
        {
            Browser.Quit();
            Browser = null;
            BrowserWait = null;
        }
    }

}
