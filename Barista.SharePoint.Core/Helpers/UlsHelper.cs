namespace Barista.SharePoint.Helpers
{
  using Microsoft.SharePoint.Utilities;
  using System;
  using System.Collections.Generic;
  using System.Globalization;
  using System.IO;
  using System.Linq;

  /// <summary>
  /// Represents a class that contains methods to facilitate interacting with the SharePoint ULS logs.
  /// </summary>
  public static class UlsHelper
  {
    public static IList<UlsHelper.UlsLogEntry> GetLogsEntriesByCorrelationId(Guid correlationId, int daysToLook)
    {
      var di = new DirectoryInfo(SPUtility.GetGenericSetupPath("LOGS"));

      if (daysToLook < 1)
        daysToLook = 1;

      if (!di.Exists)
        throw new ArgumentException("The system was unable to locate folder '" + di.FullName + "'");

      var ulsFiles = di.GetFiles("*.log").Where(f => f.Name.StartsWith("PSCDiagnostics") == false && f.LastWriteTime > DateTime.Now.AddDays(daysToLook * -1));

      var mergedEntries = new List<UlsLogEntry>();

      foreach (var file in ulsFiles)
      {
        var entries = RetrieveLogEntriesFromFile(file.FullName);
        mergedEntries.AddRange(entries);
      }

      mergedEntries = mergedEntries.Where(le => le.CorrelationId == correlationId.ToString()).ToList();
      return mergedEntries;
    }

    public static IList<UlsLogEntry> RetrieveLogEntriesFromFile(string fileName)
    {
      try
      {
        // Generate the table columns
        var entries = new List<UlsLogEntry>();

        var logFile = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using (var reader = new StreamReader(logFile))
        {
          var line = reader.ReadLine();
          if (!string.IsNullOrEmpty(line))
          {
            while (!reader.EndOfStream)
            {
              var readLine = reader.ReadLine();
              if (readLine == null)
                continue;

              string[] fields = readLine.Split(new[] { '\t' });
              UlsLogEntry entry = new UlsLogEntry
                {
                  Process = fields[1].Trim(),
                  TID = fields[2].Trim(),
                  Area = fields[3].Trim(),
                  Category = fields[4].Trim(),
                  EventID = fields[5].Trim(),
                  Level = fields[6].Trim(),
                  Message = fields[7].Trim(),
                  CorrelationId = fields[8].Trim()
                };

              DateTime timeStamp;
              if (DateTime.TryParse(fields[0].Trim(), out timeStamp))
                entry.Timestamp = timeStamp;

              entries.Add(entry);

              if (fields[0].EndsWith("*"))
              {
                entries[entries.Count - 2].Message = entries[entries.Count - 2].Message.ToString(CultureInfo.InvariantCulture).TrimEnd('.') + entry.Message.ToString(CultureInfo.InvariantCulture).Substring(3);
                entries.Remove(entry);
              }
            }
          }
        }
        return entries;
      }
      catch (Exception)
      {
        return null;
      }
    }

    public class UlsLogEntry
    {
      public DateTime Timestamp
      {
        get;
        set;
      }

      public string Process
      {
        get;
        set;
      }

      public string TID
      {
        get;
        set;
      }

      public string Area
      {
        get;
        set;
      }

      public string Category
      {
        get;
        set;
      }

      public string EventID
      {
        get;
        set;
      }

      public string Level
      {
        get;
        set;
      }

      public string Message
      {
        get;
        set;
      }

      public string CorrelationId
      {
        get;
        set;
      }
    }
  }
}
