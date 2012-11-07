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
    public ContainerInstance CreateContainer(string containerTitle)
    {
      var container = m_repository.CreateContainer(containerTitle);
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
    public EntityInstance CreateEntity(object path, object entityNamespace, object data, bool updateIndex)
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

      var result = m_repository.CreateEntity(stringPath, stringEntityNamespace, stringData, updateIndex);
      return new EntityInstance(this.Engine, result);
    }

    [JSFunction(Name = "cloneEntity")]
    public EntityInstance CloneEntity(string entityId, string sourcePath, string targetPath, bool updateIndex)
    {
      var id = new Guid(entityId);
      var result = m_repository.CloneEntity(id, sourcePath, targetPath, updateIndex);
      return new EntityInstance(this.Engine, result);
    }

    [JSFunction(Name = "deleteEntity")]
    public bool DeleteEntity(string entityId, bool updateIndex)
    {
      var id = new Guid(entityId);
      return m_repository.DeleteEntity(id, updateIndex);
    }

    [JSFunction(Name = "getEntity")]
    public EntityInstance GetEntity(string entityId, [DefaultParameterValue("")]string path)
    {
      var id = new Guid(entityId);
      var result = m_repository.GetEntity(id, path);
      return new EntityInstance(this.Engine, result);
    }

    [JSFunction(Name = "getFirstEntity")]
    public EntityInstance GetFirstEntity(string path, string entityNamespace)
    {
      var result = m_repository.GetFirstEntity(path, entityNamespace);
      return new EntityInstance(this.Engine, result);
    }

    [JSFunction(Name = "listEntities")]
    public ArrayInstance ListEntities(object path, object entityNamespace, int skip, int top)
    {
      string stringPath = string.Empty;
      string stringNamespace = string.Empty;

      if (path is string)
        stringPath = path as string;

      if (entityNamespace is string)
        stringNamespace = entityNamespace as string;

      uint? actualSkip = null;
      if (skip >= 0)
        actualSkip = (uint)skip;

      uint? actualTop = null;
      if (top >= 0)
        actualTop = (uint)top;


      var result = this.Engine.Array.Construct();

      foreach(var entity in m_repository.ListEntities(stringPath, stringNamespace, actualSkip, actualTop))
      {
        ArrayInstance.Push(result, new EntityInstance(this.Engine, entity));
      }

      return result;
    }

    [JSFunction(Name = "moveEntity")]
    public bool MoveEntity(string entityId, string destinationPath, bool updateIndex)
    {
      var id = new Guid(entityId);
      return m_repository.MoveEntity(id, destinationPath, updateIndex);
    }

    [JSFunction(Name = "updateEntity")]
    public bool UpdateEntity(EntityInstance entity, bool updateIndex)
    {
      return m_repository.UpdateEntity(entity.Entity, updateIndex);
    }
    #endregion

    #region EntityComments

    [JSFunction(Name = "addEntityComment")]
    public CommentInstance AddEntityComment(string entityId, string comment, bool updateIndex)
    {
      var id = new Guid(entityId);
      var result = m_repository.AddEntityComment(id, comment, updateIndex);
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
    public EntityPartInstance CreateEntityPart(string entityId, object partName, object category, object data, bool updateIndex)
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
      return new EntityPartInstance(this.Engine, m_repository.CreateEntityPart(id, stringPartName, stringCategory, stringData, updateIndex));
    }

    [JSFunction(Name = "deleteEntityPart")]
    public bool DeleteEntityPart(string entityId, string partName, bool updateIndex)
    {
      var id = new Guid(entityId);
      return m_repository.DeleteEntityPart(id, partName, updateIndex);
    }

    [JSFunction(Name = "getEntityPart")]
    public EntityPartInstance GetEntityPart(string entityId, string partName)
    {
      var id = new Guid(entityId);
      return new EntityPartInstance(this.Engine, m_repository.GetEntityPart(id, partName));
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
    public bool UpdateEntityPart(string entityId, string partName, EntityPartInstance entityPart, bool updateIndex)
    {
      var id = new Guid(entityId);
      return m_repository.UpdateEntityPart(id, entityPart.EntityPart, updateIndex);
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

      var result = new Base64EncodedByteArrayInstance(this.Engine.Object.Prototype, streamResult.ReadToEnd());
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
