namespace Barista.Automation.Selenium
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using OpenQA.Selenium;
  using System;

  [Serializable]
  public class AlertConstructor : ClrFunction
  {
    public AlertConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "Alert", new AlertInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public AlertInstance Construct()
    {
      return new AlertInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class AlertInstance : ObjectInstance
  {
    private readonly IAlert m_alert;

    public AlertInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public AlertInstance(ObjectInstance prototype, IAlert alert)
      : this(prototype)
    {
      if (alert == null)
        throw new ArgumentNullException("alert");

      m_alert = alert;
    }

    public IAlert Alert
    {
      get { return m_alert; }
    }

    [JSFunction(Name = "accept")]
    public void Accept()
    {
      m_alert.Accept();
    }

    [JSFunction(Name = "dismiss")]
    public void Dismiss()
    {
      m_alert.Dismiss();
    }

    [JSFunction(Name = "sendKeys")]
    public void SendKeys(string keysToSend)
    {
      m_alert.SendKeys(keysToSend);
    }

    [JSProperty(Name = "text")]
    public string Text
    {
      get { return m_alert.Text; }
    }
  }
}
