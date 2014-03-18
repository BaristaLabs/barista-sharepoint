namespace Barista.DocumentStore
{
  using Barista.Logging;
  using System;
  using System.Globalization;
  using System.Linq;

  public class ApplicationLog
  {
    private static readonly object SyncRoot = new object();

    public static void AddLogEntry(Repository repository, ApplicationLogEntry entry)
    {
      var date = DateTime.Now;
      var configuration = ApplicationConfiguration.GetApplicationConfiguration(repository);

      if (configuration.MinimumLogLevel > entry.Level)
        return;

      lock (SyncRoot)
      {
        string path = String.Format("{0}/{1}/{2}/{3}", Constants.ApplicationLogFolderName, date.Year, date.Month, date.Day);
        var client = repository.Configuration.GetDocumentStore<IFolderCapableDocumentStore>();

        var folder = client.GetFolder(repository.Configuration.ContainerTitle, path) ??
                     client.CreateFolder(repository.Configuration.ContainerTitle, path);

        var dsApplicationLogs = repository.ListEntities(new EntityFilterCriteria
          {
          Path = folder.FullPath,
          Namespace = Constants.ApplicationLogV1Namespace,
          NamespaceMatchType = NamespaceMatchType.Equals,
        });

        var dsApplicationLog = dsApplicationLogs.FirstOrDefault();
        if (dsApplicationLog == null)
        {
          var data = DocumentStoreHelper.SerializeObjectToJson(new ApplicationLog());
          dsApplicationLog = client.CreateEntity(repository.Configuration.ContainerTitle, folder.FullPath, null, Constants.ApplicationLogV1Namespace, data);
        }

        var entityPartClient = repository.Configuration.GetDocumentStore<IEntityPartCapableDocumentStore>();
        var dsApplicationLogParts = entityPartClient.ListEntityParts(repository.Configuration.ContainerTitle, dsApplicationLog.Id);

        var dsCurrentHourApplicationLogPart = dsApplicationLogParts.FirstOrDefault(alp => alp.Name == date.Hour.ToString(CultureInfo.InvariantCulture));
        if (dsCurrentHourApplicationLogPart == null)
        {
          var part = new ApplicationLogPart();
          part.Entries.Add(entry);

          var data = DocumentStoreHelper.SerializeObjectToJson(part);
          entityPartClient.CreateEntityPart(repository.Configuration.ContainerTitle,
                                            dsApplicationLog.Id,
                                            date.Hour.ToString(
                                              CultureInfo.InvariantCulture), "", data);
        }
        else
        {
          var part =
            DocumentStoreHelper.DeserializeObjectFromJson<ApplicationLogPart>(dsCurrentHourApplicationLogPart.Data);
          var data = DocumentStoreHelper.SerializeObjectToJson(part);
          part.Entries.Add(entry);
          entityPartClient.UpdateEntityPartData(repository.Configuration.ContainerTitle, dsApplicationLog.Id,
                                                date.Hour.ToString(CultureInfo.InvariantCulture), null, data);
        }
      }
    }

    public static void AddException(Repository repository, Exception ex)
    {
      var entry = new ApplicationLogEntry
        {
        Exception = ex.ToString(),
        Level = LogLevel.Fatal,
        Logger = "Exception",
        Message = ex.Message, 
        StackTrace = ex.StackTrace,
        TimeStamp = DateTime.Now,
      };

      AddLogEntry(repository, entry);
    }
  }
}
