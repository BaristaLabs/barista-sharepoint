namespace Barista.SharePoint.Library
{
  using Jurassic;
  using Jurassic.Library;
  using Microsoft.SharePoint;
  using Barista.SharePoint.Search;
  using Newtonsoft.Json.Linq;
  using System.Linq;
  using System;

  [Serializable]
  public class SPLuceneInstance : ObjectInstance
  {
    public SPLuceneInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    [JSFunction(Name = "addObjectToIndex")]
    public void AddObjectToIndex(object folder, object obj, object createIndex)
    {
      var targetFolder = GetFolderFromObject(folder);

      var objString = JSONObject.Stringify(this.Engine, obj, null, null);
      var objectToAdd = JObject.Parse(objString);

      bool bCreateIndex = false;
      if (createIndex != null && createIndex != Null.Value && createIndex != Undefined.Value && createIndex is Boolean)
        bCreateIndex = (bool)createIndex;
 
      LuceneHelper.AddObjectToIndex(targetFolder, bCreateIndex, objectToAdd);
    }

    [JSFunction(Name = "search")]
    public ArrayInstance Search(object folder, string query)
    {
      var targetFolder = GetFolderFromObject(folder);

      var searchResults = LuceneHelper.Search(targetFolder, query);

      var hitInstances = searchResults
        .Select(hit => new SearchHitInstance(this.Engine.Object.InstancePrototype, hit))
        .ToArray();

      return this.Engine.Array.Construct(hitInstances);
    }

    private SPFolder GetFolderFromObject(object folder)
    {
      SPFolder targetFolder;
      if (folder is SPFolderInstance)
      {
        var folderInstance = folder as SPFolderInstance;
        targetFolder = folderInstance.Folder;
      }
      else if (folder is string)
      {
        SPSite site;
        SPWeb web;

        var url = folder as string;
        if (SPHelper.TryGetSPFolder(url, out site, out web, out targetFolder) == false)
          throw new JavaScriptException(this.Engine, "Error", "A folder is not available at the specified url.");
      }
      else
        throw new JavaScriptException(this.Engine, "Error",
                                      "Cannot create a folder with the specified object: " +
                                      TypeConverter.ToString(folder));

      return targetFolder;
    }
  }
}
