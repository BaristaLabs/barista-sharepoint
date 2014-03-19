namespace Barista.Web.Bundles
{
  using System;
  using System.Configuration;
  using System.IO;
  using System.Linq;
  using System.Web;
  using Barista.Bundles;
  using Barista.Extensions;
  using Barista.Jurassic;
  using Barista.Library;

  public class FileWebOptimizationBundle : WebOptimizationBundle
  {
    public override object InstallBundle(ScriptEngine engine)
    {
      var bundlerInstance = new WebOptimizationInstance(engine)
      {
        GetLastModifiedDate = fileName =>
        {
          DateTime? result = null;
          if (fileName.StartsWith("~"))
          {
            var mappedPath = HttpContext.Current.Request.MapPath(fileName);
            if (File.Exists(mappedPath))
            {
              result = File.GetLastWriteTime(mappedPath);
            }
          }
          else
          {
            var path = "API";
            var configPathKey =
              ConfigurationManager.AppSettings.AllKeys.FirstOrDefault(k => k.ToLowerInvariant() == "barista_scriptpath");
            if (configPathKey != null)
            {
              var configPath = ConfigurationManager.AppSettings[configPathKey];
              if (String.IsNullOrEmpty(configPath) == false)
              {
                path = configPath;
              }
            }

            if (path.IsValidPath() && fileName.IsValidPath())
            {
              path = Path.Combine(path, fileName);
              if (File.Exists(path))
              {
                result = File.GetLastWriteTime(path);
              }
            }
          }

          if (result.HasValue == false)
          {
            var error = string.Format("Bundle error: The file '{0}' doesn't exist", fileName);
            throw new JavaScriptException(engine, "Error", error);
          }

          return result.Value;
        },
        ReadAllText = fileName =>
        {
          string result = null;
          if (fileName.StartsWith("~"))
          {
            var mappedPath = HttpContext.Current.Request.MapPath(fileName);
            if (File.Exists(mappedPath))
            {
              result = File.ReadAllText(mappedPath);
            }
          }
          else
          {
            var path = "API";
            var configPathKey =
              ConfigurationManager.AppSettings.AllKeys.FirstOrDefault(k => k.ToLowerInvariant() == "barista_scriptpath");
            if (configPathKey != null)
            {
              var configPath = ConfigurationManager.AppSettings[configPathKey];
              if (String.IsNullOrEmpty(configPath) == false)
              {
                path = configPath;
              }
            }

            if (path.IsValidPath() && fileName.IsValidPath())
            {
              path = Path.Combine(path, fileName);
              if (File.Exists(path))
              {
                result = File.ReadAllText(path);
              }
            }
          }

          if (result == null)
          {
            var error = string.Format("Bundle error: The file '{0}' doesn't exist", fileName);
            throw new JavaScriptException(engine, "Error", error);
          }

          return result;
        }
      };

      return bundlerInstance;
    }
  }
}
