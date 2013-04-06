namespace Barista.Logging
{
  using System;

  public class LoggerExecutionWrapper : ILog
  {
    public const string FailedToGenerateLogMessage = "Failed to generate log message";
    private readonly ILog m_logger;
    private readonly string m_loggerName;
    private readonly Target[] m_targets;

    public LoggerExecutionWrapper(ILog logger, string loggerName, Target[] targets)
    {
      this.m_logger = logger;
      this.m_loggerName = loggerName;
      this.m_targets = targets;
    }

    public ILog WrappedLogger
    {
      get { return m_logger; }
    }

    #region ILog Members

    public bool IsDebugEnabled
    {
      get { return m_logger.IsDebugEnabled; }
    }

    public bool IsWarnEnabled
    {
      get { return m_logger.IsWarnEnabled; }
    }

    public void Log(LogLevel logLevel, Func<string> messageFunc)
    {
      Func<string> wrappedMessageFunc = () =>
      {
        try
        {
          return messageFunc();
        }
        catch (Exception ex)
        {
          Log(LogLevel.Error, () => FailedToGenerateLogMessage, ex);
        }
        return null;
      };
      m_logger.Log(logLevel, wrappedMessageFunc);
      foreach (var target in m_targets)
      {
        target.Write(new LogEventInfo
        {
          Exception = null,
          FormattedMessage = wrappedMessageFunc(),
          Level = logLevel,
          LoggerName = m_loggerName,
          TimeStamp = DateTime.UtcNow,
        });
      }
    }

    public void Log<TException>(LogLevel logLevel, Func<string> messageFunc, TException exception)
      where TException : Exception
    {
      Func<string> wrappedMessageFunc = () =>
      {
        try
        {
          return messageFunc();
        }
        catch (Exception ex)
        {
          Log(LogLevel.Error, () => FailedToGenerateLogMessage, ex);
        }
        return null;
      };
      m_logger.Log(logLevel, wrappedMessageFunc, exception);
      foreach (var target in m_targets)
      {
        target.Write(new LogEventInfo
        {
          Exception = exception,
          FormattedMessage = wrappedMessageFunc(),
          Level = logLevel,
          LoggerName = m_loggerName,
          TimeStamp = DateTime.UtcNow,
        });
      }
    }

    #endregion
  }
}
