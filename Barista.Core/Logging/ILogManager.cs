namespace Barista.Logging
{
  using System;

  public interface ILogManager
  {
    ILog GetLogger(string name);

    IDisposable OpenNestedConext(string message);

    IDisposable OpenMappedContext(string key, string value);
  }
}
