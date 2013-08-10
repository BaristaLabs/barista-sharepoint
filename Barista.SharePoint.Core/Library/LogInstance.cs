namespace Barista.SharePoint.Library
{
  using Barista.Library;
  using Barista.SharePoint.Helpers;
  using Jurassic;
  using Jurassic.Library;
  using System;

  [Serializable]
  public class LogInstance : ObjectInstance
  {
    public LogInstance(ScriptEngine engine)
      : base(engine)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    [JSFunction(Name = "getLocalLogEntries")]
    public ArrayInstance GetLocalLogEntries(object guid, [DefaultParameterValue(1)] int daysToLook)
    {
      var result = this.Engine.Array.Construct();
      var correlationId = GuidInstance.ConvertFromJsObjectToGuid(guid);

      foreach (var logEntry in UlsHelper.GetLogsEntriesByCorrelationId(correlationId, daysToLook))
      {
        ArrayInstance.Push(result, new UlsLogEntry(this.Engine, logEntry));
      }
      return result;
    }
  }

  [Serializable]
  public class UlsLogEntry : ObjectInstance
  {
    readonly UlsHelper.UlsLogEntry m_entry;

    public UlsLogEntry(ScriptEngine engine, UlsHelper.UlsLogEntry entry)
      : base(engine)
    {
      m_entry = entry;
      this.PopulateFields();
      this.PopulateFunctions();
    }

    [JSProperty(Name = "Area")]
    public string Area
    {
      get { return m_entry.Area; }
      set { m_entry.Area = value; }
    }

    [JSProperty(Name = "Category")]
    public string Category
    {
      get { return m_entry.Category; }
      set { m_entry.Category = value; }
    }

    [JSProperty(Name = "CorrelationId")]
    public string CorrelationId
    {
      get { return m_entry.CorrelationId; }
      set { m_entry.CorrelationId = value; }
    }

    [JSProperty(Name = "EventID")]
    public string EventId
    {
      get { return m_entry.EventID; }
      set { m_entry.EventID = value; }
    }

    [JSProperty(Name = "Level")]
    public string Level
    {
      get { return m_entry.Level; }
      set { m_entry.Level = value; }
    }

    [JSProperty(Name = "Message")]
    public string Message
    {
      get { return m_entry.Message; }
      set { m_entry.Message = value; }
    }

    [JSProperty(Name = "Process")]
    public string Process
    {
      get { return m_entry.Process; }
      set { m_entry.Process = value; }
    }

    [JSProperty(Name = "ThreadId")]
    public string ThreadId
    {
      get { return m_entry.TID; }
      set { m_entry.TID = value; }
    }

    [JSProperty(Name = "Timestamp")]
    public DateInstance Timestamp
    {
      get { return JurassicHelper.ToDateInstance(this.Engine, m_entry.Timestamp); }
      set { m_entry.Timestamp = DateTime.Parse(value.ToIsoString()); }
    }
  }
}
