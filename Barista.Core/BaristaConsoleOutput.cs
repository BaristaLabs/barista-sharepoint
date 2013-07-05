namespace Barista
{
  using Barista.Logging;
  using Jurassic;
  using Jurassic.Library;
  using System;
  using System.Text;

  /// <summary>
  /// Represents a Console Output that uses either NLog or Log4Net depending on whether or not each is installed.
  /// </summary>
  [Serializable]
  public class BaristaConsoleOutput : IFirebugConsoleOutput
  {
    public BaristaConsoleOutput(ScriptEngine engine)
    {
      if (engine == null)
        throw new ArgumentNullException("engine");

      this.Engine = engine;
    }

    public ScriptEngine Engine
    {
      get;
      private set;
    }

    public void Log(FirebugConsoleMessageStyle style, object[] objects)
    {
      var logger = LogManager.GetCurrentClassLogger();

      var output = new StringBuilder();
      for (var i = 0; i < objects.Length; i++)
      {
        if (i > 0)
          output.AppendLine();
        if (objects[i] is ObjectInstance)
        {
          output.AppendLine(JSONObject.Stringify(this.Engine, objects[i], null, null));
        }
        else
        {
          output.AppendLine(TypeConverter.ToString(objects[i]));
        }
      }

      var severity = LogLevel.Trace;

      switch (style)
      {
        case FirebugConsoleMessageStyle.Error:
          severity = LogLevel.Error;
          break;
        case FirebugConsoleMessageStyle.Information:
          severity = LogLevel.Information;
          break;
        case FirebugConsoleMessageStyle.Regular:
          severity = LogLevel.Debug;
          break;
        case FirebugConsoleMessageStyle.Warning:
          severity = LogLevel.Warning;
          break;
      }

      logger.Log(severity, output.ToString);
    }

    public void Clear()
    {
      //Do nothing...
    }

    public void StartGroup(string title, bool initiallyCollapsed)
    {
      var logger = LogManager.GetCurrentClassLogger();
      logger.Log(LogLevel.Trace, () => "-----Start Group - " + title);
    }

    public void EndGroup()
    {
      var logger = LogManager.GetCurrentClassLogger();
      logger.Log(LogLevel.Trace, () => "-----End Group");
    }
  }
}
