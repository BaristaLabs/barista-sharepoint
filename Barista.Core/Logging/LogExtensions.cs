﻿namespace Barista.Logging
{
  using System;
  using System.Globalization;

  public static class LogExtensions
  {
    public static void Debug(this ILog logger, Func<string> messageFunc)
    {
      GuardAgainstNullLogger(logger);
      logger.Log(LogLevel.Debug, messageFunc);
    }

    public static void Debug(this ILog logger, string message, params object[] args)
    {
      GuardAgainstNullLogger(logger);
      logger.Log(LogLevel.Debug, () =>
      {
        if (args == null || args.Length == 0)
          return message;
        return string.Format(CultureInfo.InvariantCulture, message, args);
      });
    }

    public static void DebugException(this ILog logger, string message, Exception ex)
    {
      GuardAgainstNullLogger(logger);
      logger.Log(LogLevel.Debug, () => message, ex);
    }

    public static void Error(this ILog logger, string message, params object[] args)
    {
      GuardAgainstNullLogger(logger);
      logger.Log(LogLevel.Error, () =>
      {
        if (args == null || args.Length == 0)
          return message;
        return string.Format(CultureInfo.InvariantCulture, message, args);
      });
    }

    public static void ErrorException(this ILog logger, string message, Exception exception)
    {
      GuardAgainstNullLogger(logger);
      logger.Log(LogLevel.Error, () => message, exception);
    }

    public static void FatalException(this ILog logger, string message, Exception exception)
    {
      GuardAgainstNullLogger(logger);
      logger.Log(LogLevel.Fatal, () => message, exception);
    }

    public static void Info(this ILog logger, Func<string> messageFunc)
    {
      GuardAgainstNullLogger(logger);
      logger.Log(LogLevel.Information, messageFunc);
    }

    public static void Info(this ILog logger, string message, params object[] args)
    {
      GuardAgainstNullLogger(logger);
      logger.Log(LogLevel.Information, () =>
      {
        if (args == null || args.Length == 0)
          return message;
        return string.Format(CultureInfo.InvariantCulture, message, args);
      });
    }

    public static void InfoException(this ILog logger, string message, Exception exception)
    {
      GuardAgainstNullLogger(logger);
      logger.Log(LogLevel.Information, () => message, exception);
    }

    public static void Warn(this ILog logger, Func<string> messageFunc)
    {
      GuardAgainstNullLogger(logger);
      logger.Log(LogLevel.Warning, messageFunc);
    }

    public static void Warn(this ILog logger, string message, params object[] args)
    {
      GuardAgainstNullLogger(logger);
      logger.Log(LogLevel.Warning, () =>
      {
        if (args == null || args.Length == 0)
          return message;
        return string.Format(CultureInfo.InvariantCulture, message, args);
      });
    }

    public static void WarnException(this ILog logger, string message, Exception ex)
    {
      GuardAgainstNullLogger(logger);
      logger.Log(LogLevel.Warning, () => message, ex);
    }

    private static void GuardAgainstNullLogger(ILog logger)
    {
      if (logger == null)
      {
        throw new ArgumentException(@"logger is null", "logger");
      }
    }
  }
}
