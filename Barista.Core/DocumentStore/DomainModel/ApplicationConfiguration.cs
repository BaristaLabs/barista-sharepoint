using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Barista.DocumentStore
{
  public class ApplicationConfiguration
  {
    private static object s_syncRoot = new object();

    public LogLevel MinimumLogLevel
    {
      get;
      set;
    }

    public static ApplicationConfiguration GetApplicationConfiguration()
    {
      var repository = Repository.GetRepository();

      //Double-check locking pattern

      var applicationConfigurationEntity = repository.Single(new EntityFilterCriteria() { Path = "", Namespace = Constants.ApplicationConfigurationV1Namespace });
      if (applicationConfigurationEntity == null)
      {
        lock (s_syncRoot)
        {
          applicationConfigurationEntity = repository.Single(new EntityFilterCriteria() { Path = "", Namespace = Constants.ApplicationConfigurationV1Namespace });
          if (applicationConfigurationEntity == null)
          {
            var data = DocumentStoreHelper.SerializeObjectToJson(new ApplicationConfiguration()
            {
              MinimumLogLevel = LogLevel.Error,
            });

            applicationConfigurationEntity = repository.Configuration.DocumentStore.CreateEntity(repository.Configuration.ContainerTitle, Constants.ApplicationConfigurationV1Namespace, data);
          }
        }
      }

      return DocumentStoreHelper.DeserializeObjectFromJson<ApplicationConfiguration>(applicationConfigurationEntity.Data);
    }
  }
}
