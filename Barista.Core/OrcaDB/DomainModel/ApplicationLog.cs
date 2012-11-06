using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Barista.OrcaDB
{
  public class ApplicationLog
  {
    private static object s_syncRoot = new object();

    public static void AddLogEntry(ApplicationLogEntry entry)
    {
      var date = DateTime.Now;
      var configuration = ApplicationConfiguration.GetApplicationConfiguration();

      if (configuration.MinimumLogLevel > entry.Level)
        return;

      using (var repository = Repository.GetRepository())
      {

        lock (s_syncRoot)
        {
          string path = String.Format("{0}/{1}/{2}/{3}", Constants.ApplicationLogFolderName, date.Year, date.Month, date.Day);
          var client = repository.Configuration.GetDocumentStore<IFolderCapableDocumentStore>();

          Folder folder = client.GetFolder(repository.Configuration.ContainerTitle, path);

          if (folder == null)
            folder = client.CreateFolder(repository.Configuration.ContainerTitle, path);

          var dsApplicationLogs = repository.ListEntities<ApplicationLog>(folder.FullPath, null, null);

          Entity<ApplicationLog> dsApplicationLog = dsApplicationLogs.FirstOrDefault();
          if (dsApplicationLog == null)
          {
            dsApplicationLog = client.CreateEntity<ApplicationLog>(repository.Configuration.ContainerTitle, folder.FullPath, Constants.ApplicationLogV1Namespace, new ApplicationLog());
          }

          var entityPartClient = repository.Configuration.GetDocumentStore<IEntityPartCapableDocumentStore>();
          var dsApplicationLogParts = entityPartClient.ListEntityParts(repository.Configuration.ContainerTitle, dsApplicationLog.Id);

          var dsCurrentHourApplicationLogPart = dsApplicationLogParts.Where(alp => alp.Name == date.Hour.ToString()).FirstOrDefault();
          if (dsCurrentHourApplicationLogPart == null)
          {
            ApplicationLogPart part = new ApplicationLogPart();
            part.Entries.Add(entry);

            dsCurrentHourApplicationLogPart = entityPartClient.CreateEntityPart<ApplicationLogPart>(repository.Configuration.ContainerTitle, dsApplicationLog.Id, date.Hour.ToString(), part);
          }
          else
          {
            ApplicationLogPart part = DocumentStoreHelper.DeserializeObjectFromJson<ApplicationLogPart>(dsCurrentHourApplicationLogPart.Data);
            part.Entries.Add(entry);
            entityPartClient.UpdateEntityPart<ApplicationLogPart>(repository.Configuration.ContainerTitle, dsApplicationLog.Id, date.Hour.ToString(), part);
          }
        }
      }
    }

    public static void AddException(Exception ex)
    {
      ApplicationLogEntry entry = new ApplicationLogEntry()
      {
        Exception = ex.ToString(),
        Level = LogLevel.Fatal,
        Logger = "Exception",
        Message = ex.Message, 
        StackTrace = ex.StackTrace,
        TimeStamp = DateTime.Now,
      };

      AddLogEntry(entry);
    }
  }
}
