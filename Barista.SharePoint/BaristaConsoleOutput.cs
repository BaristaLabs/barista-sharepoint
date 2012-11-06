namespace Barista.SharePoint
{
  using System;
  using System.Text;
  using Jurassic;
  using Jurassic.Library;
  using Microsoft.SharePoint;
  using Microsoft.SharePoint.Administration;

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
      BaristaDiagnosticsService baristaLogService = new BaristaDiagnosticsService("Barista", SPFarm.Local);
      if (baristaLogService != null)
      {
        SPDiagnosticsCategory cat = baristaLogService[BaristaDiagnosticCategory.Console];

        StringBuilder output = new StringBuilder();
        for (int i = 0; i < objects.Length; i++)
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

        var severity = TraceSeverity.None;

        switch (style)
        {
          case FirebugConsoleMessageStyle.Error:
            severity = TraceSeverity.Unexpected;
            break;
          case FirebugConsoleMessageStyle.Information:
            severity = TraceSeverity.Verbose;
            break;
          case FirebugConsoleMessageStyle.Regular:
            severity = TraceSeverity.Medium;
            break;
          case FirebugConsoleMessageStyle.Warning:
            severity = TraceSeverity.Monitorable;
            break;
        }

        baristaLogService.WriteTrace(1, cat, severity, output.ToString(), baristaLogService.TypeName);
      }
    }

    public void Clear()
    {
      //Do nothing...
    }

    public void StartGroup(string title, bool initiallyCollapsed)
    {
      BaristaDiagnosticsService baristaLogService = new BaristaDiagnosticsService("Barista", SPFarm.Local);
      if (baristaLogService != null)
      {
        SPDiagnosticsCategory cat = baristaLogService[BaristaDiagnosticCategory.StartGroup];
        baristaLogService.WriteTrace(1, cat, TraceSeverity.Verbose, "-----Start Group - " + title, initiallyCollapsed);
      }
    }

    public void EndGroup()
    {
      BaristaDiagnosticsService baristaLogService = new BaristaDiagnosticsService("Barista", SPFarm.Local);
      if (baristaLogService != null)
      {
        SPDiagnosticsCategory cat = baristaLogService[BaristaDiagnosticCategory.StartGroup];
        baristaLogService.WriteTrace(1, cat, TraceSeverity.Verbose, "-----End Group");
      }
    }
  }
}
