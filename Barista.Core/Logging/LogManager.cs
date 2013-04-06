namespace Barista.Logging
{
  using Barista.Extensions;
  using Barista.Logging.LogProviders;
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Linq;

  public static class LogManager
  {
    private static ILogManager s_currentLogManager;
    private static readonly HashSet<Target> Targets = new HashSet<Target>();

    public static void EnsureValidLogger()
    {
      GetLogger(typeof(LogManager));
    }

    public static ILog GetCurrentClassLogger()
    {
      var stackFrame = new StackFrame(1, false);
      return GetLogger(stackFrame.GetMethod().DeclaringType);
    }

    public static ILogManager CurrentLogManager
    {
      get { return s_currentLogManager ?? (s_currentLogManager = ResolveExternalLogManager()); }
      set { s_currentLogManager = value; }
    }

    public static ILog GetLogger(Type type)
    {
      return GetLogger(type.FullName);
    }

    public static ILog GetLogger(string name)
    {
      var logManager = CurrentLogManager;
      if (logManager == null)
        return new LoggerExecutionWrapper(new NoOpLogger(), name, Targets.ToArray());

      // This can throw in a case of invalid NLog.config file.
      var log = logManager.GetLogger(name);
      return new LoggerExecutionWrapper(log, name, Targets.ToArray());
    }

    private static ILogManager ResolveExternalLogManager()
    {
      if (NLogLogManager.IsLoggerAvailable())
      {
        return new NLogLogManager();
      }
      if (Log4NetLogManager.IsLoggerAvailable())
      {
        return new Log4NetLogManager();
      }
      return null;
    }

    public static void RegisterTarget<T>() where T : Target, new()
    {
      if (Targets.OfType<T>().Any())
        return;

      Targets.Add(new T());
    }

    public static T GetTarget<T>() where T : Target
    {
      return Targets.ToArray().OfType<T>().FirstOrDefault();
    }

    public class NoOpLogger : ILog
    {
      public bool IsDebugEnabled { get { return false; } }

      public bool IsWarnEnabled { get { return false; } }

      public void Log(LogLevel logLevel, Func<string> messageFunc)
      { }

      public void Log<TException>(LogLevel logLevel, Func<string> messageFunc, TException exception)
        where TException : Exception
      { }
    }

    public static IDisposable OpenNestedConext(string context)
    {
      var logManager = CurrentLogManager;
      return logManager == null ? new DisposableAction(() => { }) : logManager.OpenNestedConext(context);
    }

    public static IDisposable OpenMappedContext(string key, string value)
    {
      var logManager = CurrentLogManager;
      return logManager == null ? new DisposableAction(() => { }) : logManager.OpenMappedContext(key, value);
    }
  }

  public abstract class Target
  {
    public abstract void Write(LogEventInfo logEvent);
  }

  public class LogEventInfo
  {
    public LogLevel Level { get; set; }
    public DateTime TimeStamp { get; set; }
    public string FormattedMessage { get; set; }
    public string LoggerName { get; set; }
    public Exception Exception { get; set; }
  }
}
