using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.ObjectModel;
using System.Threading;

namespace Scraper
{
    public class Tests
    {
        readonly string url = "https://www.youtube.com/c/tobiasfateYT/videos";
        int count = 1;
        public IWebDriver driver;

        [SetUp]
        public void Setup()
        {
            driver = new ChromeDriver();
            driver.Manage().Window.Maximize();
        }

        [Test]
        public void ScrapeYoutube()
        {
            driver.Url = url;
            var wait = new WebDriverWait(driver, TimeSpan.FromMilliseconds(10000)); //wait for page to load before running test, maximum wait is 10s
            wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));

            var oldHeight = (long)((IJavaScriptExecutor)driver).ExecuteScript("return document.documentElement.scrollHeight");
            //Scroll down until end of page so that we get all videos in the channel. reference - https://stackoverflow.com/questions/51690101/why-execute-scriptreturn-document-body-scrollheight-with-python-selenium-ret/51702698#51702698
            while (true)
            {
                ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollTo(0, document.documentElement.scrollHeight);");
                //wait for page to load
                Thread.Sleep(2000);
                // Takes new scroll height and compares it with the old height
                var newHeight = (long)((IJavaScriptExecutor)driver).ExecuteScript("return document.documentElement.scrollHeight");
                if (newHeight == oldHeight)
                {
                    // if heights are the same, break loop
                    break;
                }
                oldHeight = newHeight;
            }

            By videoLoc = By.CssSelector("ytd-grid-video-renderer.style-scope.ytd-grid-renderer");
            ReadOnlyCollection<IWebElement> videos = driver.FindElements(videoLoc);
            Console.WriteLine("Total number of videos in " + url + " are " + videos.Count);

            //goes through the video lis and gets the attributes of the videos in the channel
            foreach (IWebElement video in videos)
            {
                string titleResult, viewsResult, uploadDateResult;
                IWebElement videoTitle = video.FindElement(By.CssSelector("#video-title"));
                titleResult = videoTitle.Text;

                IWebElement videoViews = video.FindElement(By.XPath(".//*[@id='metadata-line']/span[1]"));
                viewsResult = videoViews.Text;

                IWebElement uploadDate = video.FindElement(By.XPath(".//*[@id='metadata-line']/span[2]"));
                uploadDateResult = uploadDate.Text;

                Console.WriteLine("Title: " + titleResult);
                Console.WriteLine("Views: " + viewsResult);
                Console.WriteLine("Release date: " + uploadDateResult);
                Console.WriteLine("\n");
                count++;
            }
        }

        [TearDown]
        public void CloseBrowser()
        {
            //exits the browser
            driver.Quit();
        }
    }
}