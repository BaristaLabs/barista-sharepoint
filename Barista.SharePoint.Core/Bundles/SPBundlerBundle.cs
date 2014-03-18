namespace Barista.SharePoint.Bundles
{
  using System;
  using Barista.Bundles;
  using Barista.Jurassic;
  using Barista.Library;
  using Microsoft.SharePoint;

  public class SPBundlerBundle : BundlerBundle
  {
    public override object InstallBundle(ScriptEngine engine)
    {
      var bundlerInstance = new BundlerInstance(engine)
      {
        GetLastModifiedDate = fileName =>
        {
          DateTime? result;
          Uri codeUri;
          if (Uri.TryCreate(fileName, UriKind.RelativeOrAbsolute, out codeUri))
          {
            SPFile file;
            if (SPHelper.TryGetSPFile(fileName, out file))
            {
              result = file.TimeLastModified;
            }
            else
            {
              var error = string.Format("Bundle error: The file '{0}' doesn't exist", fileName);
              throw new JavaScriptException(engine, "Error", error);
            }
          }
          else
          {
            var error = string.Format("Bundle error: The location of file '{0}' could not be determined", fileName);
            throw new JavaScriptException(engine, "Error", error);
          }

          return result.Value;
        },
        ReadAllText = fileName =>
        {
          string result;
          Uri codeUri;
          if (Uri.TryCreate(fileName, UriKind.RelativeOrAbsolute, out codeUri))
          {
            string scriptFilePath;
            bool isHiveFile;
            String codeFromfile;
            if (SPHelper.TryGetSPFileAsString(fileName, out scriptFilePath, out codeFromfile, out isHiveFile))
            {
              result = codeFromfile;
            }
            else
            {
              var error = string.Format("Bundle error: The file '{0}' doesn't exist", fileName);
              throw new JavaScriptException(engine, "Error", error);
            }
          }
          else
          {
            var error = string.Format("Bundle error: The location of file '{0}' could not be determined", fileName);
            throw new JavaScriptException(engine, "Error", error);
          }

          return result;
        }
      };

      return bundlerInstance;
    }
  }
}
