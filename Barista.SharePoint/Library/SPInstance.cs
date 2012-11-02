namespace Barista.SharePoint.Library
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using Jurassic;
  using Jurassic.Library;
  using Microsoft.SharePoint;
  using Barista.SharePoint.Services;
  using Microsoft.SharePoint.Administration;

  /// <summary>
  /// SharePoint namespace. Contains functions to interact with SharePoint.
  /// </summary>
  public class SPInstance : ObjectInstance
  {
    private SPContextInstance m_context;
    private SPFarmInstance m_farm;

    public SPInstance(ScriptEngine engine, BaristaContext context, SPFarm farmContext)
      : base(engine)
    {
      m_context = new SPContextInstance(this.Engine, context);
      m_farm = new SPFarmInstance(this.Engine.Object.InstancePrototype, farmContext);

      this.PopulateFields();
      this.PopulateFunctions();
    }

    [JSDoc("Gets the current context of the request. Equivalent to SPContext.Current in the server object model.")]
    [JSProperty(Name = "currentContext")]
    public SPContextInstance CurrentContext
    {
      get { return m_context; }
    }

    [JSDoc("Gets the local farm instance. Equivalent to SPFarm.Local in the server object model.")]
    [JSProperty(Name = "farm")]
    public SPFarmInstance Farm
    {
      get { return m_farm; }
    }

    [JSDoc("Loads the file at the specified url as a string.")]
    [JSFunction(Name = "loadFileAsString")]
    public string LoadFileAsString(string fileUrl)
    {
      bool isHiveFile;
      string fileContents;
      if (SPHelper.TryGetSPFileAsString(fileUrl, out fileContents, out isHiveFile))
        return fileContents;

      throw new JavaScriptException(this.Engine, "Error", "Could not locate the specified file:  " + fileUrl);
    }

    [JSDoc("Gets the current user. Equivalent to SPContext.Current.Web.CurrentUser")]
    [JSFunction(Name = "getCurrentUser")]
    public SPUserInstance GetCurrentSPUser()
    {
      return new SPUserInstance(this.Engine.Object.InstancePrototype, BaristaContext.Current.Web.CurrentUser);
    }

    [JSDoc("Sends an email.")]
    [JSFunction(Name = "sendEmail")]
    public bool SendEmail(string to, string cc, string bcc, string from, string subject, string messageBody, bool appendFooter)
    {
      EmailService service = new EmailService();
      return service.SendEmail(to, cc, bcc, from, subject, messageBody, appendFooter);
    }

    [JSDoc("Indicates whether event firing is enabled (true) or disabled (false)")]
    [JSFunction(Name = "toggleListEvents")]
    public void ToggleListEvents(bool enabled)
    {
      HandleEventFiring eventFiring = new HandleEventFiring();

      if (enabled)
        eventFiring.CustomEnableEventFiring();
      else
        eventFiring.CustomDisableEventFiring();
    }

    #region Nested Classes
    private class HandleEventFiring : SPItemEventReceiver
    {
      public HandleEventFiring()
      {
      }

      public void CustomDisableEventFiring()
      {
        this.EventFiringEnabled = false;
      }

      public void CustomEnableEventFiring()
      {
        this.EventFiringEnabled = true;
      }
    }
    #endregion
  }
}
