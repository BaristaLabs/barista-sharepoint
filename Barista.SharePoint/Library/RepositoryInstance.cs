namespace Barista.SharePoint.Library
{
  using Barista.Extensions;
  using Barista.DocumentStore;
  using Barista.Library;
  using Barista.SharePoint.Helpers;
  using Jurassic;
  using Jurassic.Library;
  using System;
  using System.Linq;
  using Newtonsoft.Json;

  public class RepositoryInstance : ObjectInstance
  {
    Repository m_repository;

    public RepositoryInstance(ScriptEngine engine, Repository repository)
      : base(engine)
    {
      if (repository == null)
        throw new ArgumentNullException("repository");

      m_repository = repository;

      this.PopulateFields();
      this.PopulateFunctions();
    }

    #region Properties
    [JSProperty(Name = "containerTitle")]
    public string ContainerTitle
    {
      get { return m_repository.Configuration.ContainerTitle; }
      set { m_repository.Configuration.ContainerTitle = value; }
    }
    #endregion

    #region Container
    [JSFunction(Name = "listContainers")]
    public ArrayInstance ListContainers()
    {
      var result = this.Engine.Array.Construct();
      foreach (var container in m_repository.ListContainers())
      {
        ArrayInstance.Push(result, new ContainerInstance(this.Engine, container));
      }

      return result;
    }

    [JSFunction(Name = "createContainer")]
    public ContainerInstance CreateContainer(string containerTitle, string description)
    {
      var container = m_repository.CreateContainer(containerTitle, description);
      return new ContainerInstance(this.Engine, container);
    }

    [JSFunction(Name = "deleteContainer")]
    public void DeleteContainer(string containerTitle)
    {
      m_repository.DeleteContainer(containerTitle);
    }
    #endregion

    #region Folder
    [JSFunction(Name="listFolders")]
    public ArrayInstance ListFolders([DefaultParameterValue("")]object path)
    {
      var stringPath = String.Empty;
      if (path != Undefined.Value && path != Null.Value && path is string)
        stringPath = path as string;

      var result = this.Engine.Array.Construct();

      var folders = m_repository.ListFolders(stringPath);

      if (folders != null)
      {
        foreach (var folder in folders)
        {
          ArrayInstance.Push(result, new FolderInstance(this.Engine, folder));
        }
      }

      return result;
    }

    [JSFunction(Name="createFolder")]
    public FolderInstance CreateFolder(string path)
    {
      var folder = m_repository.CreateFolder(path);
      return new FolderInstance(this.Engine, folder);
    }

    [JSFunction(Name="deleteFolder")]
    public void DeleteFolder(string path)
    {
      m_repository.DeleteFolder(path);
    }

    #endregion

    #region Entity
    [JSFunction(Name = "createEntity")]
    public EntityInstance CreateEntity(object path, object entityNamespace, object data)
    {
      var stringPath = "";
      var stringEntityNamespace = "";
      var stringData = "";

      if (path is string)
        stringPath = path as string;

      if (entityNamespace is string)
        stringEntityNamespace = entityNamespace as string;

      if (data is ObjectInstance)
        stringData = JSONObject.Stringify(this.Engine, data, null, null);
      else if (data is string)
        stringData = data as string;
      else
        stringData = data.ToString();

      var result = m_repository.CreateEntity(stringPath, stringEntityNamespace, stringData);
      return new EntityInstance(this.Engine, result);
    }

    [JSFunction(Name = "cloneEntity")]
    public EntityInstance CloneEntity(string entityId, string sourcePath, string targetPath)
    {
      var id = new Guid(entityId);
      var result = m_repository.CloneEntity(id, sourcePath, targetPath);
      return new EntityInstance(this.Engine, result);
    }

    [JSFunction(Name = "deleteEntity")]
    public bool DeleteEntity(string entityId)
    {
      var id = new Guid(entityId);
      return m_repository.DeleteEntity(id);
    }

    [JSFunction(Name = "getEntity")]
    public object GetEntity(string entityId, [DefaultParameterValue("")]string path)
    {
      var id = new Guid(entityId);
      var result = m_repository.GetEntity(id, path);

      if (result == null)
        return Null.Value;

      return new EntityInstance(this.Engine, result);
    }

    [JSFunction(Name = "single")]
    public EntityInstance Single(object filterCriteria)
    {
      var criteria = new EntityFilterCriteriaInstance(this.Engine.Object.InstancePrototype);

      if (filterCriteria != null && filterCriteria != Null.Value && filterCriteria != Undefined.Value)
        criteria = JurassicHelper.Coerce<EntityFilterCriteriaInstance>(this.Engine, filterCriteria);

      var result = m_repository.Single(criteria.EntityFilterCriteria);
      return new EntityInstance(this.Engine, result);
    }

    [JSFunction(Name = "listEntities")]
    public ArrayInstance ListEntities(object filterCriteria)
    {
      var criteria = new EntityFilterCriteriaInstance(this.Engine.Object.InstancePrototype);

      if (filterCriteria != null && filterCriteria != Null.Value && filterCriteria != Undefined.Value)
        criteria = JurassicHelper.Coerce<EntityFilterCriteriaInstance>(this.Engine, filterCriteria);

      var result = this.Engine.Array.Construct();

      foreach(var entity in m_repository.ListEntities(criteria.EntityFilterCriteria))
      {
        ArrayInstance.Push(result, new EntityInstance(this.Engine, entity));
      }

      return result;
    }

    [JSFunction(Name = "moveEntity")]
    public bool MoveEntity(string entityId, string destinationPath)
    {
      var id = new Guid(entityId);
      return m_repository.MoveEntity(id, destinationPath);
    }

    [JSFunction(Name = "updateEntity")]
    public EntityInstance UpdateEntity(string entityId, object eTag, object data)
    {
      var id = new Guid(entityId);
      var stringData = "";
      var stringETag = "";

      if (eTag is string)
        stringETag = (string)eTag;

      if (data is ObjectInstance)
        stringData = JSONObject.Stringify(this.Engine, data, null, null);
      else if (data is string)
        stringData = data as string;
      else
        stringData = data.ToString();

      var result = m_repository.UpdateEntityData(id, stringETag, stringData);
      return new EntityInstance(this.Engine, result);
    }
    #endregion

    #region EntityComments

    [JSFunction(Name = "addEntityComment")]
    public CommentInstance AddEntityComment(string entityId, string comment)
    {
      var id = new Guid(entityId);
      var result = m_repository.AddEntityComment(id, comment);
      return new CommentInstance(this.Engine, result);
    }

    [JSFunction(Name = "listEntityComments")]
    public ArrayInstance ListEntityComments(string entityId)
    {
      var result = this.Engine.Array.Construct();

      var id = new Guid(entityId);
      foreach (var comment in m_repository.ListEntityComments(id))
      {
        ArrayInstance.Push(result, new CommentInstance(this.Engine, comment));
      }

      return result;
    }
    #endregion

    #region Entity Parts
    [JSFunction(Name = "createEntityPart")]
    public EntityPartInstance CreateEntityPart(string entityId, object partName, object category, object data)
    {
      var stringPartName = "";
      var stringCategory = "";
      var stringData = "";

      if (partName is string)
        stringPartName = partName as string;

      if (category is string)
        stringCategory = category as string;

      if (data is ObjectInstance)
        stringData = JSONObject.Stringify(this.Engine, data, null, null);
      else if (data is string)
        stringData = data as string;
      else
        stringData = data.ToString();

      var id = new Guid(entityId);
      return new EntityPartInstance(this.Engine, m_repository.CreateEntityPart(id, stringPartName, stringCategory, stringData));
    }

    [JSFunction(Name = "createOrUpdateEntityPart")]
    public EntityPartInstance CreateOrUpdateEntityPart(string entityId, object partName, object category, object data)
    {
      var id = new Guid(entityId);
      var stringPartName = "";
      var stringCategory = "";
      var stringData = "";

      if (partName is string)
        stringPartName = partName as string;

      if (category is string)
        stringCategory = category as string;

      if (data is ObjectInstance)
        stringData = JSONObject.Stringify(this.Engine, data, null, null);
      else if (data is string)
        stringData = data as string;
      else
        stringData = data.ToString();

      var entityPart = m_repository.GetEntityPart(id, stringPartName);
      if (entityPart == null)
      {
        entityPart = m_repository.CreateEntityPart(id, stringPartName, stringCategory, stringData);
      }
      else
      {
        entityPart = m_repository.UpdateEntityPartData(id, stringPartName, stringData);
      }

      return new EntityPartInstance(this.Engine, entityPart);
    }

    [JSFunction(Name = "deleteEntityPart")]
    public bool DeleteEntityPart(string entityId, string partName)
    {
      var id = new Guid(entityId);
      return m_repository.DeleteEntityPart(id, partName);
    }

    [JSFunction(Name = "getEntityPart")]
    public object GetEntityPart(string entityId, string partName)
    {
      var id = new Guid(entityId);

      var result = m_repository.GetEntityPart(id, partName);

      if (result == null)
        return Null.Value;

      return new EntityPartInstance(this.Engine, result);
    }

    [JSFunction(Name = "hasEntityPart")]
    public bool HasEntityPart(string entityId, string partName)
    {
      var id = new Guid(entityId);
      return m_repository.HasEntityPart(id, partName);
    }

    [JSFunction(Name = "listEntityParts")]
    public ArrayInstance ListEntityParts(string entityId)
    {
      var result = this.Engine.Array.Construct();

      var id = new Guid(entityId);
      foreach (var entityPart in m_repository.ListEntityParts(id))
      {
        ArrayInstance.Push(result, new EntityPartInstance(this.Engine, entityPart));
      }

      return result;
    }

    [JSFunction(Name = "updateEntityPart")]
    public EntityPartInstance UpdateEntityPart(string entityId, object partName, string eTag, object data)
    {
      var id = new Guid(entityId);
      var stringPartName = "";
      var stringData = "";
      var stringETag = "";

      if (partName is string)
        stringPartName = (string)partName;

      if (eTag is string)
        stringETag = (string)eTag;

      if (data is ObjectInstance)
        stringData = JSONObject.Stringify(this.Engine, data, null, null);
      else if (data is string)
        stringData = data as string;
      else
        stringData = data.ToString();

      var result = m_repository.UpdateEntityPartData(id, stringPartName, stringETag, stringData);
      return new EntityPartInstance(this.Engine, result);
    }
    #endregion

    #region Attachments
    [JSFunction(Name = "listAttachments")]
    public ArrayInstance ListAttachments(string entityId)
    {
      var result = this.Engine.Array.Construct();

      var id = new Guid(entityId);
      foreach (var attachment in m_repository.ListAttachments(id))
      {
        ArrayInstance.Push(result, new AttachmentInstance(this.Engine, attachment));
      }

      return result;
    }

    [JSFunction(Name = "getAttachment")]
    public AttachmentInstance GetAttachment(string entityId, string fileName)
    {
      var id = new Guid(entityId);
      return new AttachmentInstance(this.Engine, m_repository.GetAttachment(id, fileName));
    }

    [JSFunction(Name = "uploadAttachment")]
    public AttachmentInstance UploadAttachment(string entityId, string fileName, Base64EncodedByteArrayInstance attachment)
    {
      var id = new Guid(entityId);
      return new AttachmentInstance(this.Engine, m_repository.UploadAttachment(id, fileName, attachment.Data));
    }

    [JSFunction(Name = "downloadAttachment")]
    public Base64EncodedByteArrayInstance DownloadAttachment(string entityId, string fileName)
    {
      var id = new Guid(entityId);
      var attachment = m_repository.GetAttachment(id, fileName);
      var streamResult = m_repository.DownloadAttachment(id, fileName);

      var result = new Base64EncodedByteArrayInstance(this.Engine.Object.InstancePrototype, streamResult.ReadToEnd());
      result.MimeType = attachment.MimeType;
      result.FileName = attachment.FileName;

      return result;
    }

    [JSFunction(Name = "deleteAttachment")]
    public bool DeleteAttachment(string entityId, string fileName)
    {
      var id = new Guid(entityId);

      return m_repository.DeleteAttachment(id, fileName);
    }
    #endregion

    #region Permissions
    [JSFunction(Name = "addPrincipalRoleToEntity")]
    public PrincipalRoleInfoInstance AddPrincipalRoleToEntity(string entityId, string principalName, string principalType, string roleName)
    {
      var id = new Guid(entityId);

      var result = m_repository.AddPrincipalRoleToEntity(id, principalName, principalType, roleName);

      return new PrincipalRoleInfoInstance(this.Engine, result);
    }

    [JSFunction(Name = "getEntityPermissions")]
    public PermissionsInfoInstance GetEntityPermissions(string entityId)
    {
      var id = new Guid(entityId);

      var result = m_repository.GetEntityPermissions(id);

      return new PermissionsInfoInstance(this.Engine, result);
    }

    [JSFunction(Name = "removePrincipalRoleFromEntity")]
    public bool RemovePrincipalRoleFromEntity(string entityId, string principalName, string principalType, string roleName)
    {
      var id = new Guid(entityId);

      return m_repository.RemovePrincipalRoleFromEntity(id, principalName, principalType, roleName);
    }

    [JSFunction(Name = "resetEntityPermissions")]
    public PermissionsInfoInstance ResetEntityPermissions(string entityId)
    {
      var id = new Guid(entityId);

      return new PermissionsInfoInstance(this.Engine, m_repository.ResetEntityPermissions(id));
    }
    #endregion

    #region EntitySet
    [JSFunction(Name = "getEntitySet")]
    public EntitySetInstance GetEntitySet(string entityId, object path)
    {
      var stringPath = "";
      var id = new Guid(entityId);

      if (path is string)
        stringPath = path as string;

      var entity = m_repository.GetEntity(id, stringPath);
      var entityPart = m_repository.ListEntityParts(id);
      return new EntitySetInstance(this.Engine, entity, entityPart);
    }
    #endregion
  }
}
