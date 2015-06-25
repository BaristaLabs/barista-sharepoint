namespace Barista.DocumentStore
{
  using Barista.Logging;
  using System;

  public class ApplicationLogEntry
  {
    public DateTime TimeStamp
    {
      get;
      set;
    }

    public int Thread
    {
      get;
      set;
    }

    public LogLevel Level
    {
      get;
      set;
    }

    public string Logger
    {
      get;
      set;
    }

    public string Message
    {
      get;
      set;
    }

    public string Exception
    {
      get;
      set;
    }

    public string StackTrace
    {
      get;
      set;
    }

    public string CreatedByLoginName
    {
      get;
      set;
    }

    public string Host
    {
      get;
      set;
    }
  }
}
