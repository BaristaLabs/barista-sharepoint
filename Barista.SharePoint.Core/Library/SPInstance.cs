namespace Barista.SharePoint.Library
{
  using System;
  using Jurassic;
  using Jurassic.Library;
  using Microsoft.SharePoint;
  using Microsoft.SharePoint.Administration;
  using System.Text;
  using Barista.Library;
  using Microsoft.SharePoint.Utilities;

  /// <summary>
  /// SharePoint namespace. Contains functions to interact with SharePoint.
  /// </summary>
  [Serializable]
  public class SPInstance : ObjectInstance
  {
    private readonly SPContextInstance m_context;
    private readonly SPFarmInstance m_farm;

    public SPInstance(ScriptEngine engine, BaristaContext context, SPFarm farmContext)
      : base(engine)
    {
      m_context = new SPContextInstance(this.Engine, context);
      m_farm = new SPFarmInstance(this.Engine.Object.InstancePrototype, farmContext);

      this.PopulateFields();
      this.PopulateFunctions();
    }

    #region Properties
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
    #endregion

    #region Functions
    [JSDoc("Returns a value that indicates if a file exists at the specified url.")]
    [JSFunction(Name = "exists")]
    public bool Exists(string fileUrl)
    {
      SPFile file;
      if (SPHelper.TryGetSPFile(fileUrl, out file))
      {
        return true;
      }

      return false;
    }

    [JSDoc("Loads the file at the specified url as a byte array.")]
    [JSFunction(Name = "loadFileAsByteArray")]
    public Base64EncodedByteArrayInstance LoadFileAsByteArray(string fileUrl)
    {
      SPFile file;
      if (SPHelper.TryGetSPFile(fileUrl, out file))
      {
        var data = file.OpenBinary(SPOpenBinaryOptions.None);
        return new Base64EncodedByteArrayInstance(this.Engine.Object.InstancePrototype, data);
      }

      throw new JavaScriptException(this.Engine, "Error", "Could not locate the specified file:  " + fileUrl);
    }

    [JSDoc("Loads the file at the specified url as a string.")]
    [JSFunction(Name = "loadFileAsString")]
    public string LoadFileAsString(string fileUrl)
    {
      string path;
      bool isHiveFile;
      string fileContents;
      if (SPHelper.TryGetSPFileAsString(fileUrl, out path, out fileContents, out isHiveFile))
        return fileContents;

      throw new JavaScriptException(this.Engine, "Error", "Could not locate the specified file:  " + fileUrl);
    }

    [JSDoc("Loads the file at the specified url as a JSON Object.")]
    [JSFunction(Name = "loadFileAsJSON")]
    public object LoadFileAsJson(string fileUrl)
    {
      string path;
      bool isHiveFile;
      string fileContents;
      if (SPHelper.TryGetSPFileAsString(fileUrl, out path, out fileContents, out isHiveFile))
      {
        return JSONObject.Parse(this.Engine, fileContents, null);
      }

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
      return EmailHelper.SendEmail(to, cc, bcc, from, subject, messageBody, appendFooter);
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

    [JSDoc("Writes the specified contents to the file located at the specified url")]
    [JSFunction(Name = "write")]
    public SPFileInstance Write(string fileUrl, object contents)
    {
      byte[] data;
      if (contents is Base64EncodedByteArrayInstance)
        data = (contents as Base64EncodedByteArrayInstance).Data;
      else if (contents is StringInstance || contents is string)
        data = Encoding.UTF8.GetBytes((string)contents);
      else if (contents is ObjectInstance)
        data = Encoding.UTF8.GetBytes(JSONObject.Stringify(this.Engine, contents, null, null));
      else
        data = Encoding.UTF8.GetBytes(contents.ToString());

      SPFile result;
      if (SPHelper.TryGetSPFile(fileUrl, out result))
      {
        SPWeb web;
        if (SPHelper.TryGetSPWeb(fileUrl, out web))
        {
          result = web.Files.Add(fileUrl, data);
        }
        else
        {
          throw new JavaScriptException(this.Engine, "Error", "Could not locate the specified web:  " + fileUrl);
        }
      }
      else
      {
        result.SaveBinary(data);
      }

      return new SPFileInstance(this.Engine.Object.InstancePrototype, result);
    }

    [JSDoc("Starts a new monitored scope that can be used to profile script executino.")]
    [JSFunction(Name = "beginMonitoredScope")]
    public SPMonitoredScopeInstance BeginMonitoredScope(string name)
    {
      var monitoredScope = new SPMonitoredScope(name, 11000, new SPExecutionTimeCounter(), new SPCriticalTraceCounter());
      return new SPMonitoredScopeInstance(this.Engine.Object.Prototype, monitoredScope);
    }

    [JSDoc("Ends a previously created monitored scope that can be used to profile script execution.")]
    [JSFunction(Name = "endMonitoredScope")]
    public object EndMonitoredScope(object monitoredScope)
    {
      if (monitoredScope == null || monitoredScope == Undefined.Value || monitoredScope == Null.Value ||
          (monitoredScope is SPMonitoredScopeInstance) == false)
        return Null.Value;

      var scope = monitoredScope as SPMonitoredScopeInstance;
      scope.SPMonitoredScope.Dispose();
      return scope.ElapsedTime;
    }

    #endregion

    #region Nested Classes
    private class HandleEventFiring : SPItemEventReceiver
    {
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
