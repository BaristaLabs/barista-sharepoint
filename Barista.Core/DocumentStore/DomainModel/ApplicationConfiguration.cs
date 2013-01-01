namespace Barista.DocumentStore
{
  using System;

  public class ApplicationConfiguration
  {
    [ThreadStatic]
    private static object s_syncRoot;

    public LogLevel MinimumLogLevel
    {
      get;
      set;
    }

    public static ApplicationConfiguration GetApplicationConfiguration(Repository repository)
    {
      if (s_syncRoot == null)
        s_syncRoot = new object();

      //Double-check locking pattern
      var applicationConfigurationEntity =
        repository.Single(new EntityFilterCriteria
          {
            Path = "",
            Namespace = Constants.ApplicationConfigurationV1Namespace
          });
      if (applicationConfigurationEntity == null)
      {
        lock (SyncRoot)
        {
          applicationConfigurationEntity =
            repository.Single(new EntityFilterCriteria
              {
                Path = "",
                Namespace = Constants.ApplicationConfigurationV1Namespace
              });
          if (applicationConfigurationEntity == null)
          {
            var data = DocumentStoreHelper.SerializeObjectToJson(new ApplicationConfiguration
              {
                MinimumLogLevel = LogLevel.Error,
              });

            applicationConfigurationEntity =
              repository.Configuration.DocumentStore.CreateEntity(repository.Configuration.ContainerTitle,
                                                                  Constants.ApplicationConfigurationV1Namespace, data);
          }
        }
      }

      return DocumentStoreHelper.DeserializeObjectFromJson<ApplicationConfiguration>(applicationConfigurationEntity.Data);
    }
  }
}
