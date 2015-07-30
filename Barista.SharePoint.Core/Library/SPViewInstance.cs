namespace Barista.SharePoint.Library
{
  using System;
  using System.Globalization;
  using System.Linq;
  using Jurassic;
  using Jurassic.Library;
  using Microsoft.SharePoint;

  [Serializable]
  public class SPViewConstructor : ClrFunction
  {
    public SPViewConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPView", new SPViewInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPViewInstance Construct(string viewUrl)
    {
      SPView view;

      if (SPHelper.TryGetSPView(viewUrl, out view) == false)
        throw new JavaScriptException(Engine, "Error", "A view is not available at the specified url.");

      return new SPViewInstance(InstancePrototype, view);
    }

    public SPViewInstance Construct(SPView view)
    {
      if (view == null)
        throw new ArgumentNullException("view");

      return new SPViewInstance(InstancePrototype, view);
    }
  }

  [Serializable]
  public class SPViewInstance : ObjectInstance
  {
    private readonly SPView m_view;

    public SPViewInstance(ObjectInstance prototype)
      : base(prototype)
    {
      PopulateFields();
      PopulateFunctions();
    }

    public SPViewInstance(ObjectInstance prototype, SPView view)
      : this(prototype)
    {
      m_view = view;
    }

    #region Properties

    public SPView View
    {
      get { return m_view; }
    }

    [JSProperty(Name = "aggregations")]
    public string Aggregations
    {
      get
      {
        return m_view.Aggregations;
      }
      set
      {
        m_view.Aggregations = value;
      }
    }

    [JSProperty(Name = "aggregationsStatus")]
    public string AggregationsStatus
    {
      get
      {
        return m_view.AggregationsStatus;
      }
      set
      {
        m_view.AggregationsStatus = value;
      }
    }

    [JSProperty(Name = "baseViewId")]
    public string BaseViewId
    {
      get
      {
        return m_view.BaseViewID;
      }
    }

    [JSProperty(Name = "contentTypeId")]
    public SPContentTypeIdInstance ContentTypeId
    {
      get
      {
        return new SPContentTypeIdInstance(Engine.Object.InstancePrototype, m_view.ContentTypeId);
      }
      set
      {
        m_view.ContentTypeId = value.ContentTypeId;
      }
    }

    [JSProperty(Name = "defaultView")]
    public bool DefaultView
    {
      get
      {
        return m_view.DefaultView;
      }
      set
      {
        m_view.DefaultView = value;
      }
    }

    [JSProperty(Name = "defaultViewForContentType")]
    public bool DefaultViewForContentType
    {
      get
      {
        return m_view.DefaultViewForContentType;
      }
      set
      {
        m_view.DefaultViewForContentType = value;
      }
    }

    [JSProperty(Name = "editorModified")]
    public bool EditorModified
    {
      get
      {
        return m_view.EditorModified;
      }
      set
      {
        m_view.EditorModified = value;
      }
    }

    [JSProperty(Name = "formats")]
    public string Formats
    {
      get
      {
        return m_view.Formats;
      }
      set
      {
        m_view.Formats = value;
      }
    }

    [JSProperty(Name = "hidden")]
    public bool Hidden
    {
      get
      {
        return m_view.Hidden;
      }
      set
      {
        m_view.Hidden = value;
      }
    }

    [JSProperty(Name = "id")]
    public string Id
    {
      get
      {
        return m_view.ID.ToString();
      }
    }

    [JSProperty(Name = "imageUrl")]
    public string ImageUrl
    {
      get
      {
        return m_view.ImageUrl;
      }
    }

    [JSProperty(Name = "includeRootFolder")]
    public bool IncludeRootFolder
    {
      get
      {
        return m_view.IncludeRootFolder;
      }
      set
      {
        m_view.IncludeRootFolder = value;
      }
    }

    [JSProperty(Name = "jsLink")]
    public string JSLink
    {
        get
        {
            return m_view.JSLink;
        }
        set
        {
            m_view.JSLink = value;
        }
    }

    [JSProperty(Name = "method")]
    public string Method
    {
      get
      {
        return m_view.Method;
      }
      set
      {
        m_view.Method = value;
      }
    }

    [JSProperty(Name = "mobileDefaultView")]
    public bool MobileDefaultView
    {
      get
      {
        return m_view.MobileDefaultView;
      }
      set
      {
        m_view.MobileDefaultView = value;
      }
    }

    [JSProperty(Name = "mobileView")]
    public bool MobileView
    {
      get
      {
        return m_view.MobileView;
      }
      set
      {
        m_view.MobileView = value;
      }
    }

    [JSProperty(Name = "moderationType")]
    public string ModerationType
    {
      get
      {
        return m_view.ModerationType;
      }
    }

    [JSProperty(Name = "orderedView")]
    public bool OrderedView
    {
      get
      {
        return m_view.OrderedView;
      }
    }

    [JSProperty(Name = "paged")]
    public bool Paged
    {
      get
      {
        return m_view.OrderedView;
      }
      set
      {
        m_view.Paged = value;
      }
    }

    [JSProperty(Name = "personalView")]
    public bool PersonalView
    {
      get
      {
        return m_view.PersonalView;
      }
    }

    [JSProperty(Name = "readOnlyView")]
    public bool ReadOnlyView
    {
      get
      {
        return m_view.ReadOnlyView;
      }
    }

    [JSProperty(Name = "requiresClientIntegration")]
    public bool RequiresClientIntegration
    {
      get
      {
        return m_view.RequiresClientIntegration;
      }
    }

    [JSProperty(Name = "rowLimit")]
    public string RowLimit
    {
      get
      {
        return m_view.RowLimit.ToString(CultureInfo.InvariantCulture);
      }
      set
      {
        m_view.RowLimit = uint.Parse(value);
      }
    }

    [JSProperty(Name = "scope")]
    public string Scope
    {
      get
      {
        return m_view.Scope.ToString();
      }
      set
      {
        m_view.Scope = (SPViewScope)Enum.Parse(typeof(SPViewScope), value);
      }
    }

    [JSProperty(Name = "serverRelativeUrl")]
    public string ServerRelativeUrl
    {
      get
      {
        return m_view.ServerRelativeUrl;
      }
    }

    [JSProperty(Name = "styleId")]
    public string StyleId
    {
      get
      {
        return m_view.StyleID;
      }
    }

    [JSProperty(Name = "threaded")]
    public bool Threaded
    {
      get
      {
        return m_view.Threaded;
      }
    }

    [JSProperty(Name = "title")]
    public string Title
    {
      get
      {
        return m_view.Title;
      }
      set
      {
        m_view.Title = value;
      }
    }

    [JSProperty(Name = "toolbar")]
    public string Toolbar
    {
      get
      {
        return m_view.Toolbar;
      }
      set
      {
        m_view.Toolbar = value;
      }
    }

    [JSProperty(Name = "toolbarTemplateName")]
    public string ToolbarTemplateName
    {
      get
      {
        return m_view.ToolbarTemplateName;
      }
    }

    [JSProperty(Name = "viewData")]
    public string ViewData
    {
      get
      {
        return m_view.ViewData;
      }
      set
      {
        m_view.ViewData = value;
      }
    }

    [JSProperty(Name = "viewFields")]
    [JSDoc("ternPropertyType", "[string]")]
    public ArrayInstance ViewFields
    {
      get
      {
        var result = Engine.Array.Construct();
        foreach(var viewField in m_view.ViewFields.OfType<string>())
        {
          ArrayInstance.Push(result, viewField);
        }

        return result;
      }
    }

    [JSProperty(Name = "viewJoins")]
    public string ViewJoins
    {
      get
      {
        return m_view.Joins;
      }
      set
      {
        m_view.Joins = value;
      }
    }

    [JSProperty(Name = "viewProjectedFields")]
    public string ViewProjectedFields
    {
      get
      {
        return m_view.ProjectedFields;
      }
      set
      {
        m_view.ProjectedFields = value;
      }
    }

    [JSProperty(Name = "viewQuery")]
    public string ViewQuery
    {
      get
      {
        return m_view.Query;
      }
      set
      {
        m_view.Query = value;
      }
    }

    [JSProperty(Name = "viewType")]
    public string ViewType
    {
      get
      {
        return m_view.Type;
      }
    }

    #endregion

    [JSFunction(Name = "delete")]
    public void Delete()
    {
      m_view.ParentList.Views.Delete(m_view.ID);
    }

    [JSFunction(Name = "renderAsHtml")]
    public string RenderAsHtml()
    {
      return m_view.RenderAsHtml();
    }

    [JSFunction(Name = "update")]
    public void Update()
    {
      m_view.Update();
    }

    [JSFunction(Name = "getHtmlSchemaXml")]
    public string GetHtmlSchemaXml()
    {
      return m_view.HtmlSchemaXml;
    }
  }
}
