namespace Barista.SharePoint
{
  using Barista.Configuration;
  using Barista.Extensions;
  using Barista.Newtonsoft.Json;
  using Barista.SharePoint.Search;
  using Microsoft.SharePoint;
  using Microsoft.SharePoint.Administration;
  using Newtonsoft.Json.Linq;
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.ServiceModel.Security;

  /// <summary>
  /// Contains various helper methods involving barista configuration stored in SharePoint.
  /// </summary>
  public static class BaristaHelper
  {
    /// <summary>
    /// For the given index name, obtains the directory object as defined in the farm property bag. If no directory with the given name has been defined, returns null.
    /// </summary>
    /// <param name="indexName"></param>
    /// <returns></returns>
    public static Lucene.Net.Store.Directory GetDirectoryFromIndexName(string indexName)
    {
      if (indexName.IsNullOrWhiteSpace())
        throw new ArgumentNullException("indexName", @"A directory name must be specified.");

      var indexDefinitions = Utilities.GetFarmKeyValue("BaristaSearchIndexDefinitions");
      if (indexDefinitions.IsNullOrWhiteSpace())
        return null;

      var indexDefinitionCollection = JsonConvert.DeserializeObject<IList<IndexDefinition>>(indexDefinitions);
      var indexDefinitionToUse =
        indexDefinitionCollection.FirstOrDefault(
          id => id.IndexName.IsNullOrWhiteSpace() == false && id.IndexName.ToLowerInvariant() == indexName.ToLowerInvariant());

      if (indexDefinitionToUse == null)
        return null;

      //Lets create the Directory object from the index definition!!
      var directoryType = Type.GetType(indexDefinitionToUse.TypeName, false, true);
      if (directoryType == null)
        throw new InvalidOperationException(
          String.Format("An index definition named {0} was located, however, the type {1} could not be found.",
                        indexName, indexDefinitionToUse.TypeName));

      if (typeof(Lucene.Net.Store.Directory).IsAssignableFrom(directoryType) == false)
        throw new InvalidOperationException(
          String.Format("An index definition named {0} was located, however, the type {1} is not a directory type.",
                        indexName, indexDefinitionToUse.TypeName));
      
      //I know, I know, we went to all the trouble of doing DI, only to hard code the types...
      if (directoryType == typeof (Barista.SharePoint.Search.SPDirectory))
      {
        SPSite site = null;
        SPWeb web = null;

        //Test for the existance of the target index.
        try
        {
          SPFolder folder;
          if (SPHelper.TryGetSPFolder(indexDefinitionToUse.IndexStoragePath, out site, out web, out folder) == false)
            throw new InvalidOperationException(
              String.Format(
                "An SharePoint index definition named {0} was located, however, the target index location {1} is not valid.",
                indexName, indexDefinitionToUse.IndexStoragePath));
        }
        finally
        {
          if (web != null)
            web.Dispose();

          if (site != null)
            site.Dispose();
        }

        return new SPDirectory(indexDefinitionToUse.IndexStoragePath);
      }
      
      if (directoryType == typeof (Lucene.Net.Store.SimpleFSDirectory))
      {
        var di = new DirectoryInfo(indexDefinitionToUse.IndexStoragePath);
        if (di.Exists == false)
          di.Create();

        return new Lucene.Net.Store.SimpleFSDirectory(di);
      }

      if (directoryType == typeof (Lucene.Net.Store.RAMDirectory))
      {
        return new Lucene.Net.Store.RAMDirectory();
      }

      //A little bit of extensibility...
      var directory = (Lucene.Net.Store.Directory)Activator.CreateInstance(directoryType, indexDefinitionToUse.IndexStoragePath);
      return directory;
    }

    /// <summary>
    /// Checks the current context against the trusted locations as defined in the farm property bag. If the current location is not trusted, throws an exception.
    /// </summary>
    public static void EnsureExecutionInTrustedLocation()
    {
      var currentUri = new Uri(SPContext.Current.Web.Url.ToLowerInvariant().EnsureEndsWith("/"));

      //CA is always trusted.
      if (SPAdministrationWebApplication.Local.AlternateUrls.Any(u => u != null && u.Uri != null && u.Uri.IsBaseOf(currentUri)))
        return;

      var trusted = false;
      var trustedLocations = Utilities.GetFarmKeyValue("BaristaTrustedLocations");

      if (String.IsNullOrEmpty(trustedLocations) == false)
      {
        var trustedLocationsCollection = JArray.Parse(trustedLocations);
        foreach (var trustedLocation in trustedLocationsCollection.OfType<JObject>())
        {
          var trustedLocationUrl = new Uri(trustedLocation["Url"].ToString().ToLowerInvariant().EnsureEndsWith("/"), UriKind.Absolute);
          var trustChildren = trustedLocation["TrustChildren"].ToObject<Boolean>();

          if (trustChildren)
          {
            if (trustedLocationUrl.IsBaseOf(currentUri))
            {
              trusted = true;
              break;
            }
          }
          else
          {
            if (trustedLocationUrl == currentUri)
            {
              trusted = true;
              break;
            }
          }
        }
      }
      else
        throw new SecurityAccessDeniedException("Cannot execute Barista: Unable to read Farm Property Bag Settings to determine trusted location.");

      if (trusted == false)
        throw new SecurityAccessDeniedException(String.Format("Cannot execute Barista: The current location is not trusted ({0}). Contact your farm administrator to add the current location to the trusted Urls in the management section of the Barista service application.", currentUri));
    }
  }
}
