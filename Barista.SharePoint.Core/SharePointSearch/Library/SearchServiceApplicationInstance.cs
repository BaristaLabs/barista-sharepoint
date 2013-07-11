namespace Barista.SharePoint.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using Microsoft.Office.Server.Search.Administration;
  using System;
  using Microsoft.SharePoint;

  [Serializable]
  public class SearchServiceApplicationConstructor : ClrFunction
  {
    public SearchServiceApplicationConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SearchServiceApplication", new SearchServiceApplicationInstance(engine.Object.InstancePrototype))
    {
      this.PopulateFunctions();
    }

    [JSConstructorFunction]
    public SearchServiceApplicationInstance Construct()
    {
      return new SearchServiceApplicationInstance(this.InstancePrototype);
    }

    public SearchServiceApplicationInstance GetFromCurrentContext()
    {
      //var remoteScopes = new RemoteScopes(SPServiceContext.GetContext(site));
      //remoteScopes.
     //return new SearchServiceApplication.
      throw new NotImplementedException();
    }
  }

  [Serializable]
  public class SearchServiceApplicationInstance : ObjectInstance
  {
    private readonly SearchServiceApplication m_searchServiceApplication;

    public SearchServiceApplicationInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SearchServiceApplicationInstance(ObjectInstance prototype, SearchServiceApplication searchServiceApplication)
      : this(prototype)
    {
      if (searchServiceApplication == null)
        throw new ArgumentNullException("searchServiceApplication");

      m_searchServiceApplication = searchServiceApplication;
    }

    public SearchServiceApplication SearchServiceApplication
    {
      get { return m_searchServiceApplication; }
    }
  }
}
