using System.Globalization;

namespace Barista.SharePoint.Library
{
  using System;
  using Jurassic;
  using Jurassic.Library;
  using Microsoft.SharePoint;

  [Serializable]
  public class SPCamlQueryConstructor : ClrFunction
  {
    public SPCamlQueryConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPCamlQuery", new SPCamlQueryInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPCamlQueryInstance Construct(object objectInstance)
    {
      SPCamlQueryInstance result;

      if (objectInstance is SPViewInstance)
      {
        var view = (objectInstance as SPViewInstance).View;
        result = new SPCamlQueryInstance(this.Engine.Object.InstancePrototype, new SPQuery(view));
      }
      else
      {
        result = new SPCamlQueryInstance(this.Engine.Object.InstancePrototype, new SPQuery());
      }

      return result;
    }

    public SPUserInstance Construct(SPUser user)
    {
      if (user == null)
        throw new ArgumentNullException("user");

      return new SPUserInstance(this.InstancePrototype, user);
    }
  }

  public class SPCamlQueryInstance : ObjectInstance
  {
    private readonly SPQuery m_query;

    public SPCamlQueryInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SPCamlQueryInstance(ObjectInstance prototype, SPQuery query)
      : this(prototype)
    {
      this.m_query = query;
    }

    #region Properties

    public SPQuery SPQuery
    {
      get { return m_query; }
    }

    [JSProperty(Name = "autoHyperlink")]
    public bool AutoHyperlink
    {
      get { return m_query.AutoHyperlink; }
      set { m_query.AutoHyperlink = value; }
    }

    [JSProperty(Name = "calendarDate")]
    public DateInstance CalendarDate
    {
      get { return JurassicHelper.ToDateInstance(this.Engine, m_query.CalendarDate); }
      set { m_query.CalendarDate = DateTime.Parse(value.ToISOString()); }
    }

    [JSProperty(Name="datesInUtc")]
    public bool DatesInUtc
    {
      get { return m_query.DatesInUtc; }
      set { m_query.DatesInUtc = value; }
    }

    [JSProperty(Name = "expandRecurrence")]
    public bool ExpandRecurrence
    {
      get { return m_query.ExpandRecurrence; }
      set { m_query.ExpandRecurrence = value; }
    }

    [JSProperty(Name = "expandUserField")]
    public bool ExpandUserField
    {
      get { return m_query.ExpandUserField; }
      set { m_query.ExpandUserField = value; }
    }

    [JSProperty(Name = "folder")]
    public SPFolderInstance Folder
    {
      get
      {
        if (m_query.Folder == null)
          return null;

        return new SPFolderInstance(this.Engine.Object.InstancePrototype, null, null, m_query.Folder);
      }
      set { m_query.Folder = value.Folder; }
    }

    [JSProperty(Name = "includeAllUserPermissions")]
    public bool IncludeAllUserPermissions
    {
      get { return m_query.IncludeAllUserPermissions; }
      set { m_query.IncludeAllUserPermissions = value; }
    }

    [JSProperty(Name = "includeAttachmentUrls")]
    public bool IncludeAttachmentUrls
    {
      get { return m_query.IncludeAttachmentUrls; }
      set { m_query.IncludeAttachmentUrls = value; }
    }

    [JSProperty(Name = "includeAttachmentVersion")]
    public bool IncludeAttachmentVersion
    {
      get { return m_query.IncludeAttachmentVersion; }
      set { m_query.IncludeAttachmentVersion = value; }
    }

    [JSProperty(Name = "includeMandatoryColumns")]
    public bool IncludeMandatoryColumns
    {
      get { return m_query.IncludeMandatoryColumns; }
      set { m_query.IncludeMandatoryColumns = value; }
    }

    [JSProperty(Name = "includePermissions")]
    public bool IncludePermissions
    {
      get { return m_query.IncludePermissions; }
      set { m_query.IncludePermissions = value; }
    }

    [JSProperty(Name = "individualProperties")]
    public bool IndividualProperties
    {
      get { return m_query.IndividualProperties; }
      set { m_query.IndividualProperties = value; }
    }

    [JSProperty(Name = "itemIdQuery")]
    public bool ItemIdQuery
    {
      get { return m_query.ItemIdQuery; }
      set { m_query.ItemIdQuery = value; }
    }

    [JSProperty(Name = "joins")]
    public string Joins
    {
      get { return m_query.Joins; }
      set { m_query.Joins = value; }
    }

    [JSProperty(Name = "listItemCollectionPosition")]
    public SPListItemCollectionPositionInstance ListItemCollectionPosition
    {
      get
      {
        if (m_query.ListItemCollectionPosition == null)
          return null;

        return new SPListItemCollectionPositionInstance(this.Engine.Object.InstancePrototype, m_query.ListItemCollectionPosition);
      }
      set { m_query.ListItemCollectionPosition = value.ListItemCollectionPosition; }
    }

    [JSProperty(Name = "meetingInstanceId")]
    public int MeetingInstanceId
    {
      get { return m_query.MeetingInstanceId; }
      set { m_query.MeetingInstanceId = value; }
    }

    [JSProperty(Name = "method")]
    public string Method
    {
      get { return m_query.Method; }
      set { m_query.Method = value; }
    }

    [JSProperty(Name = "projectedFields")]
    public string ProjectedFields
    {
      get { return m_query.ProjectedFields; }
      set { m_query.ProjectedFields = value; }
    }

    [JSProperty(Name = "query")]
    public string Query
    {
      get { return m_query.Query; }
      set { m_query.Query = value; }
    }

    [JSProperty(Name = "queryThrottleMode")]
    public string QueryThrottleMode
    {
      get { return m_query.QueryThrottleMode.ToString(); }
      set { m_query.QueryThrottleMode = (SPQueryThrottleOption)Enum.Parse(typeof(SPQueryThrottleOption), value); }
    }

    [JSProperty(Name = "recurrenceOrderBy")]
    public bool RecurrenceOrderBy
    {
      get { return m_query.RecurrenceOrderBy; }
      set { m_query.RecurrenceOrderBy = value; }
    }

    [JSProperty(Name = "rowLimit")]
    public string RowLimit
    {
      get { return m_query.RowLimit.ToString(CultureInfo.InvariantCulture); }
      set { m_query.RowLimit = uint.Parse(value); }
    }

    [JSProperty(Name = "viewAttributes")]
    public string ViewAttributes
    {
      get { return m_query.ViewAttributes; }
      set { m_query.ViewAttributes = value; }
    }

    [JSProperty(Name = "viewFields")]
    public string ViewFields
    {
      get { return m_query.ViewFields; }
      set { m_query.ViewFields = value; }
    }

    [JSProperty(Name = "viewFieldsOnly")]
    public bool ViewFieldsOnly
    {
      get { return m_query.ViewFieldsOnly; }
      set { m_query.ViewFieldsOnly = value; }
    }

    [JSProperty(Name = "viewXml")]
    public string ViewXml
    {
      get { return m_query.ViewXml; }
      set { m_query.ViewXml = value; }
    }
    #endregion
  }
}
