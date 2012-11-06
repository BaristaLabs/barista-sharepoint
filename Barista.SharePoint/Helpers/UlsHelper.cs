namespace Barista.SharePoint.Helpers
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using System.IO;
  using Microsoft.SharePoint.Utilities;

  public static class UlsHelper
  {
    public static IList<UlsHelper.UlsLogEntry> GetLogsEntriesByCorrelationId(Guid correlationId, int daysToLook)
    {
      DirectoryInfo di = new DirectoryInfo(SPUtility.GetGenericSetupPath("LOGS"));

      if (daysToLook < 1)
        daysToLook = 1;

      if (!di.Exists)
        throw new ArgumentException("The system was unable to locate folder '" + di.FullName + "'");

      var ulsFiles = di.GetFiles("*.log").Where(f => f.Name.StartsWith("PSCDiagnostics") == false && f.LastWriteTime > DateTime.Now.AddDays(daysToLook * -1));

      List<UlsHelper.UlsLogEntry> mergedEntries = new List<UlsLogEntry>();

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
        List<UlsLogEntry> entries = new List<UlsLogEntry>();

        FileStream logFile = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using (StreamReader reader = new StreamReader(logFile))
        {
          var line = reader.ReadLine();
          if (!string.IsNullOrEmpty(line))
          {
            while (!reader.EndOfStream)
            {
              string[] fields = reader.ReadLine().Split(new char[] { '\t' });
              UlsLogEntry entry = new UlsLogEntry();
              entry.Process = fields[1].Trim();
              entry.TID = fields[2].Trim();
              entry.Area = fields[3].Trim();
              entry.Category = fields[4].Trim();
              entry.EventID = fields[5].Trim();
              entry.Level = fields[6].Trim();
              entry.Message = fields[7].Trim();
              entry.CorrelationId = fields[8].Trim();

              DateTime timeStamp;
              if (DateTime.TryParse(fields[0].Trim(), out timeStamp))
                entry.Timestamp = timeStamp;

              entries.Add(entry);

              if (fields[0].EndsWith("*"))
              {
                entries[entries.Count - 2].Message = entries[entries.Count - 2].Message.ToString().TrimEnd('.') + entry.Message.ToString().Substring(3);
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
