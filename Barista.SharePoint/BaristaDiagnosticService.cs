namespace Barista.SharePoint
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Runtime.InteropServices;
  using System.Text;
  using Microsoft.SharePoint.Administration;

  public enum BaristaDiagnosticCategory
  {
    None = 0,
    Deployment = 100,
    Provisioning = 200,
    CustomAction = 300,
    Rendering = 400,
    WebPart = 500,
    Runtime = 600,
    Console = 700,
    StartGroup = 800,
    EndGroup = 900,
    PowerShell = 1000,
    JavaScriptException = 1100,
  }

  [Guid("B7D45781-D64D-4B23-ABA3-33398DB1249B")]
  public class BaristaDiagnosticsService : SPDiagnosticsServiceBase
  {
    private static string DiagnosticsAreaName = "Barista";

    public BaristaDiagnosticsService()
    {
    }

    public BaristaDiagnosticsService(string name, SPFarm farm)
      : base(name, farm)
    {

    }

    protected override IEnumerable<SPDiagnosticsArea> ProvideAreas()
    {
      List<SPDiagnosticsCategory> categories = new List<SPDiagnosticsCategory>();
      foreach (string catName in Enum.GetNames(typeof(BaristaDiagnosticCategory)))
      {
        uint catId = (uint)(int)Enum.Parse(typeof(BaristaDiagnosticCategory), catName);
        categories.Add(new SPDiagnosticsCategory(catName, TraceSeverity.Verbose, EventSeverity.Error, 0, catId));
      }

      yield return new SPDiagnosticsArea(DiagnosticsAreaName, categories);
    }

    public static BaristaDiagnosticsService Local
    {
      get
      {
        return SPDiagnosticsServiceBase.GetLocal<BaristaDiagnosticsService>();
      }
    }

    public SPDiagnosticsCategory this[BaristaDiagnosticCategory id]
    {
      get
      {
        return Areas[DiagnosticsAreaName].Categories[id.ToString()];
      }
    }

    public void LogMessage(ushort id, BaristaDiagnosticCategory category,
                               TraceSeverity traceSeverity, string message, params object[] data)
    {
      if (traceSeverity != TraceSeverity.None)
      {
        SPDiagnosticsCategory cat = this[category];
        Local.WriteTrace(id, cat, traceSeverity, message, data);
      }
    }

    public void LogException(Exception ex, BaristaDiagnosticCategory category, string messagePrefix)
    {
      if (messagePrefix == null)
        messagePrefix = String.Empty;

      if (messagePrefix != String.Empty)
        messagePrefix = messagePrefix.TrimEnd() + " ";

      SPDiagnosticsCategory cat = this[category];
      Local.WriteTrace(1, cat, TraceSeverity.Unexpected, messagePrefix + ex.Message, ex.Data);
    }
  }
}
