namespace Barista.SharePoint.OrcaDB
{
  using System;
  using Barista.OrcaDB;
  using Microsoft.SharePoint;
  using System.Collections.Generic;

  public class SPEntityPart : EntityPart
  {
    private object m_syncRoot = new object();

    private SPFile m_file = null;
    private bool m_hasDataBeenSet = false;
    private string m_data = null;

    public SPEntityPart(SPFile file)
    {
      if (file == null)
        throw new ArgumentNullException("file", "When creating an SPEntityPart, the SPFile that represents the entity part must not be null.");

      m_file = file;
      MapFileToProperties();
    }

    public override string Data
    {
      get
      {
        if (m_data == null && m_hasDataBeenSet == false)
        {
          lock (m_syncRoot)
          {
            if (m_data == null && m_hasDataBeenSet == false)
            {
              m_data = System.Text.Encoding.UTF8.GetString(m_file.OpenBinary(SPOpenBinaryOptions.SkipVirusScan));
            }
          }
        }
        return m_data;
      }
      set
      {
        m_data = value;
        m_hasDataBeenSet = true;
      }
    }

    private void MapFileToProperties()
    {
      try
      {
        string id = m_file.Item[Constants.DocumentEntityGuidFieldId] as string;

        this.EntityId = new Guid(id);
      }
      catch
      {
        //Do Nothing...
      }

      this.Category = m_file.Item["Category"] as string;
      this.ETag = m_file.ETag;
      this.Name = m_file.Name.Substring(0, m_file.Name.Length - Constants.DocumentSetEntityPartExtension.Length);
      this.Created = (DateTime)m_file.Item[SPBuiltInFieldId.Created];
      this.Modified = (DateTime)m_file.Item[SPBuiltInFieldId.Modified];

      var createdByUserValue = m_file.Item[SPBuiltInFieldId.Author] as string;
      SPFieldUserValue createdByUser = new SPFieldUserValue(m_file.Web, createdByUserValue);

      if (createdByUser != null)
      {
        this.CreatedBy = new User()
        {
          Email = createdByUser.User.Email,
          LoginName = createdByUser.User.LoginName,
          Name = createdByUser.User.Name,
        };
      }

      var modifiedByUserValue = m_file.Item[SPBuiltInFieldId.Editor] as string;
      SPFieldUserValue modifiedByUser = new SPFieldUserValue(m_file.Web, createdByUserValue);

      if (modifiedByUser != null)
      {
        this.ModifiedBy = new User()
        {
          Email = modifiedByUser.User.Email,
          LoginName = modifiedByUser.User.LoginName,
          Name = modifiedByUser.User.Name,
        };
      }
    }
  }

  public class SPEntityPart<T> : EntityPart<T>
  {
    private object m_syncRoot = new object();

    SPEntityPart m_entityPart = null;
    private bool m_hasValueBeenSet = false;
    private T m_value = default(T);

    public SPEntityPart(SPEntityPart entityPart)
      : base()
    {
      if (entityPart == null)
        throw new ArgumentNullException("entityPart", "When creating an typed SPEntityPart, the SPEntityPart parameter must not be null.");

      this.Category = entityPart.Category;
      this.Created = entityPart.Created;
      this.CreatedBy = entityPart.CreatedBy;
      this.Data = entityPart.Data;
      this.EntityId = entityPart.EntityId;
      this.ETag = entityPart.ETag;
      this.Modified = entityPart.Modified;
      this.ModifiedBy = entityPart.ModifiedBy;
      this.Name = entityPart.Name;

      m_entityPart = entityPart;
    }

    public override string Data
    {
      get
      {
        return m_entityPart.Data;
      }
      set
      {
        m_entityPart.Data = value;
      }
    }

    public override T Value
    {
      get
      {
        if (EqualityComparer<T>.Default.Equals(m_value, default(T)) && m_hasValueBeenSet == false)
        {
          lock (m_syncRoot)
          {
            if (EqualityComparer<T>.Default.Equals(m_value, default(T)) && m_hasValueBeenSet == false)
            {
              m_value = DocumentStoreHelper.DeserializeObjectFromJson<T>(m_entityPart.Data);
            }
          }
        }

        return m_value;
      }
      set
      {
        m_value = value;
        m_hasValueBeenSet = true;
      }
    } 
  }
}
