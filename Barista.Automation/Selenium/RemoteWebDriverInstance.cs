namespace Barista.Automation.Selenium
{
  using Barista.Jurassic.Library;
  using OpenQA.Selenium;
  using OpenQA.Selenium.Remote;

  public class RemoteWebDriverInstance : WebDriverInstance, IJavaScriptExecutor
  {
    public RemoteWebDriverInstance(ObjectInstance prototype, RemoteWebDriver webDriver)
      : base(prototype, webDriver)
    {
      this.RemoteWebDriver = webDriver;
    }

    public RemoteWebDriver RemoteWebDriver
    {
      get;
      set;
    }

    [JSFunction(Name = "executeAsyncScript")]
    public object ExecuteAsyncScript(string script, params object[] args)
    {
      return this.RemoteWebDriver.ExecuteAsyncScript(script, args);
    }

    [JSFunction(Name = "executeScript")]
    public object ExecuteScript(string script, params object[] args)
    {
      return this.RemoteWebDriver.ExecuteScript(script, args);
    }
  }
}
