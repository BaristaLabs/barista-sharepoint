namespace Barista.SharePoint.Helpers
{
  using Microsoft.SharePoint.Administration;
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
      return GetLogsEntriesByCorrelationId(correlationId, daysToLook, false);
    }

    public static IList<UlsHelper.UlsLogEntry> GetLogsEntriesByCorrelationId(Guid correlationId, int daysToLook, bool onlyRecent)
    {
      if (daysToLook < 1)
        daysToLook = 1;

      var config = SPDiagnosticsService.Local;

      var logLocation = Environment.ExpandEnvironmentVariables(config.LogLocation);

      var di = new DirectoryInfo(logLocation);

      if (!di.Exists)
        throw new ArgumentException("The system was unable to locate folder '" + di.FullName + "'");

      var ulsFiles = di.GetFiles("*.log")
        .Where(
          f => f.Name.StartsWith("PSCDiagnostics") == false && f.LastWriteTime > DateTime.Now.AddDays(daysToLook*-1));

      //If onlyrecent, look only at the last two log files.
      if (onlyRecent)
      {
        ulsFiles = ulsFiles.OrderByDescending(f => f.LastWriteTime)
          .Take(2);
      }

      var mergedEntries = new List<UlsLogEntry>();

      var strCorrelationId = correlationId.ToString().ToLowerInvariant();

      foreach (var entries in ulsFiles
        .Where(f => f.Exists)
        .Select(file => RetrieveLogEntriesFromFile(file.FullName, le => le.CorrelationId == strCorrelationId))
        .Where(entries => entries != null))
      {
        mergedEntries.AddRange(entries);
      }

      return mergedEntries;
    }

    public static IList<UlsLogEntry> RetrieveLogEntriesFromFile(string fileName)
    {
      return RetrieveLogEntriesFromFile(fileName, e => true);
    }

    public static IList<UlsLogEntry> RetrieveLogEntriesFromFile(string fileName, Predicate<UlsLogEntry> predicate)
    {
      try
      {
        // Generate the table columns
        var entries = new List<UlsLogEntry>();

        var logFile = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using (var reader = new StreamReader(logFile))
        {
          var line = reader.ReadLine();
          if (String.IsNullOrEmpty(line))
            return entries;

          while (!reader.EndOfStream)
          {
            var readLine = reader.ReadLine();
            if (readLine == null)
              continue;

            var fields = readLine.Split(new[] { '\t' });
            var entry = new UlsLogEntry
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

            if (!predicate(entry))
              continue;

            entries.Add(entry);

            if (!fields[0].EndsWith("*"))
              continue;

            entries[entries.Count - 2].Message =
              entries[entries.Count - 2].Message.ToString(CultureInfo.InvariantCulture).TrimEnd('.') +
              entry.Message.ToString(CultureInfo.InvariantCulture).Substring(3);

            entries.Remove(entry);
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

// ReSharper disable InconsistentNaming
      public string TID
// ReSharper restore InconsistentNaming
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

// ReSharper disable InconsistentNaming
      public string EventID
// ReSharper restore InconsistentNaming
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
