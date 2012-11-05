using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OFS.OrcaDB.Core
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
      
      var applicationConfigurationEntity = repository.GetFirstEntity<ApplicationConfiguration>();
      if (applicationConfigurationEntity == null)
      {
        lock (s_syncRoot)
        {
          applicationConfigurationEntity = repository.GetFirstEntity<ApplicationConfiguration>();
          if (applicationConfigurationEntity == null)
          {
            applicationConfigurationEntity = repository.Configuration.DocumentStore.CreateEntity<ApplicationConfiguration>(repository.Configuration.ContainerTitle, Constants.ApplicationConfigurationV1Namespace, new ApplicationConfiguration()
            {
              MinimumLogLevel = LogLevel.Error,
            });
          }
        }
      }

      return applicationConfigurationEntity.Value;
    }
  }
}
