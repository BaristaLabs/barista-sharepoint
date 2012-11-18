namespace Barista.SharePoint.Library
{
  using System;
  using System.Linq;
  using Jurassic;
  using Jurassic.Library;
  using Microsoft.SharePoint;
  using Microsoft.Office.DocumentManagement.DocumentSets;

  public class SPDocumentSetConstructor : ClrFunction
  {
    public SPDocumentSetConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPDocumentSet", new SPDocumentSetInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPDocumentSetInstance Construct(object obj)
    {
      SPSite site = null;
      SPWeb web = null;
      DocumentSet result = null;

      if (obj is SPFolderInstance)
      {
        var folderInstance = obj as SPFolderInstance;
        result = DocumentSet.GetDocumentSet(folderInstance.Folder);
      }
      else if (obj is string)
      {
        var url = obj as string;
        SPFolder folder;
        if (SPHelper.TryGetSPFolder(url, out site, out web, out folder) == false)
          throw new JavaScriptException(this.Engine, "Error", "No documentSet is available at the specified url.");
        result = DocumentSet.GetDocumentSet(folder);
      }
      else
        throw new JavaScriptException(this.Engine, "Error", "Cannot create a document set with the specified object: " + TypeConverter.ToString(obj));

      return new SPDocumentSetInstance(this.InstancePrototype, site, web, result);
    }

    public SPDocumentSetInstance Construct(DocumentSet documentSet)
    {
      if (documentSet == null)
        throw new ArgumentNullException("documentSet");

      return new SPDocumentSetInstance(this.InstancePrototype, null, null, documentSet);
    }
  }

  public class SPDocumentSetInstance : ObjectInstance,  IDisposable
  {
    private SPSite m_site;
    private SPWeb m_web;
    private DocumentSet m_documentSet;

    public SPDocumentSetInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SPDocumentSetInstance(ObjectInstance prototype, SPSite site, SPWeb web, DocumentSet documentSet)
      : this(prototype)
    {
      this.m_site = site;
      this.m_web = web;
      this.m_documentSet = documentSet;
    }

    #region Properties
    [JSProperty(Name = "item")]
    public SPListItemInstance Item
    {
      get { return new SPListItemInstance(this.Engine.Object.InstancePrototype, m_documentSet.Item); }
    }

    [JSProperty(Name = "welcomePageUrl")]
    public string WelcomePageUrl
    {
      get { return m_documentSet.WelcomePageUrl; }
    }

    #endregion
    [JSFunction(Name = "getContentType")]
    public SPContentTypeInstance GetContentType()
    {
      return new SPContentTypeInstance(this.Engine.Object.InstancePrototype, m_documentSet.ContentType);
    }

    //TODO: getContentTypeTemplate

    [JSFunction(Name = "getFolder")]
    public SPFolderInstance GetFolder()
    {
      return new SPFolderInstance(this.Engine.Object.InstancePrototype, null, null, m_documentSet.Folder);
    }

    [JSFunction(Name = "getParentFolder")]
    public SPFolderInstance GetParentFolder()
    {
      return new SPFolderInstance(this.Engine.Object.InstancePrototype, null, null, m_documentSet.ParentFolder);
    }

    [JSFunction(Name = "getParentList")]
    public SPListInstance GetParentList()
    {
      return new SPListInstance(this.Engine.Object.InstancePrototype, null, null, m_documentSet.ParentList);
    }

    public void Dispose()
    {
      if (m_site != null)
      {
        m_site.Dispose();
        m_site = null;
      }

      if (m_web != null)
      {
        m_web.Dispose();
        m_web = null;
      }
    }
  }
}
