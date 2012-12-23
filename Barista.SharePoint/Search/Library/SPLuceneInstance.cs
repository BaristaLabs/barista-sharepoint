using Lucene.Net.Index;
using Lucene.Net.Search;

namespace Barista.SharePoint.Search.Library
{
  using Barista.SharePoint.Library;
  using Barista.SharePoint.Search;
  using Jurassic;
  using Jurassic.Library;
  using Microsoft.SharePoint;
  using System;
  using System.Linq;

  [Serializable]
  public class SPLuceneInstance : ObjectInstance
  {
    public SPLuceneInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    [JSFunction(Name = "hasIndexWriter")]
    public bool HasIndexWriter(object folder)
    {
      var targetFolder = GetFolderFromObject(folder);

      return LuceneHelper.HasIndexWriterSingleton(targetFolder);
    }

    [JSFunction(Name = "getIndexSearcher")]
    public IndexSearcherInstance GetIndexSearcher(object folder, object createIndex)
    {
      var targetFolder = GetFolderFromObject(folder);

      var indexSearcher = LuceneHelper.GetIndexSearcher(targetFolder);

      return new IndexSearcherInstance(this.Engine.Object.InstancePrototype, indexSearcher);
    }

    [JSFunction(Name = "getIndexWriter")]
    public IndexWriterInstance GetIndexWriter(object folder, object createIndex)
    {
      var targetFolder = GetFolderFromObject(folder);

      var bCreateIndex = false;
      if (createIndex != null && createIndex != Null.Value && createIndex != Undefined.Value && createIndex is Boolean)
        bCreateIndex = (bool)createIndex;

      var indexWriter = LuceneHelper.GetIndexWriterSingleton(targetFolder, bCreateIndex);
      return new IndexWriterInstance(this.Engine.Object.InstancePrototype, indexWriter, targetFolder);
    }

    [JSFunction(Name = "createTermQuery")]
    public TermQueryInstance CreateTermQuery(string fieldName, string text)
    {
      return new TermQueryInstance(this.Engine.Object.InstancePrototype, new TermQuery(new Term(fieldName, text)));
    }

    [JSFunction(Name = "closeIndexWriter")]
    public void CloseIndexWriter(object folder)
    {
      var targetFolder = GetFolderFromObject(folder);

      LuceneHelper.CloseIndexWriterSingleton(targetFolder);
    }

    [JSFunction(Name = "search")]
    public ArrayInstance Search(object folder, string query, object n)
    {

      var targetFolder = GetFolderFromObject(folder);

      var intN = 100;
      if (n != null && n != Null.Value && n != Undefined.Value && n is int)
        intN = (int) n;

      var hitInstances = LuceneHelper.Search(targetFolder, query, intN)
                                     .Select(hit => new SearchHitInstance(this.Engine.Object.InstancePrototype, hit))
                                     .ToArray();

      return this.Engine.Array.Construct(hitInstances);
    }

    private SPFolder GetFolderFromObject(object folder)
    {
      if (folder == Null.Value || folder == Undefined.Value || folder == null)
        throw new JavaScriptException(this.Engine, "Error", "A folder must be specified.");

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
