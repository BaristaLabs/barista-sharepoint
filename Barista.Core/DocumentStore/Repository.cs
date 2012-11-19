namespace Barista.DocumentStore
{
  using System;
  using System.Collections.Generic;
  using System.Configuration;
  using System.IO;
  using System.Linq;
  using System.Threading.Tasks;

  /// <summary>
  /// Represents a class that abstracts retrieval of data from a document store.
  /// </summary>
  /// <remarks>
  /// This should be the main interface to use when interacting with the DocumentStore.
  /// 
  /// By using Repository/RepositoryConfiguration, the underlying document store implementation is abstracted, magic strings are reduced to a bare minimum, and common patterns are enforced and reused.
  /// 
  /// Further, some of the higher level functionality -- Automatic Migration, Indexing, are only available through the repository.
  /// 
  /// Follows the Unit-of-work pattern.
  /// </remarks>
  [Serializable]
  public class Repository : IDisposable
  {
    #region Fields
    private readonly RepositoryConfiguration m_configuration;
    private object m_syncRoot = new object();
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the configuration associated with the repository.
    /// </summary>
    public RepositoryConfiguration Configuration
    {
      get { return m_configuration; }
    }

    /// <summary>
    /// Gets the factory that was responsible for the creation of the repository.
    /// </summary>
    public IRepositoryFactory Factory
    {
      get;
      private set;
    }
    #endregion

    #region Constructors
    public Repository()
    {
      m_configuration = new RepositoryConfiguration();
    }

    public Repository(IDocumentStore documentStore)
    {
      m_configuration = new RepositoryConfiguration(documentStore);
    }
    #endregion

    #region Repository CRUD+L Methods

    #region Containers
    /// <summary>
    /// Lists all containers contained in the document store.
    /// </summary>
    /// <returns></returns>
    public IList<Container> ListContainers()
    {
      var documentStore = this.Configuration.GetDocumentStore<IDocumentStore>();
      return documentStore.ListContainers();
    }

    /// <summary>
    /// Creates a new container with the specified title in the document store.
    /// </summary>
    /// <param name="containerTitle"></param>
    /// <returns></returns>
    public Container CreateContainer(string containerTitle, string description)
    {
      var documentStore = this.Configuration.GetDocumentStore<IDocumentStore>();
      return documentStore.CreateContainer(containerTitle, description);
    }

    /// <summary>
    /// Deletes the specified container.
    /// </summary>
    /// <param name="containerTitle"></param>
    /// <returns></returns>
    public void DeleteContainer(string containerTitle)
    {
      var documentStore = this.Configuration.GetDocumentStore<IDocumentStore>();
      documentStore.DeleteContainer(containerTitle);
    }
    #endregion

    #region Folders
    /// <summary>
    /// Lists all folders in the specified path in the container.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public IList<Folder> ListFolders(string path)
    {
      var documentStore = this.Configuration.GetDocumentStore<IFolderCapableDocumentStore>();
      return documentStore.ListFolders(this.Configuration.ContainerTitle, path);
    }

    /// <summary>
    /// Creates a new folder with the specified path in the container.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public Folder CreateFolder(string path)
    {
      var documentStore = this.Configuration.GetDocumentStore<IFolderCapableDocumentStore>();
      return documentStore.CreateFolder(this.Configuration.ContainerTitle, path);
    }

    /// <summary>
    /// Deletes the folder with the specified path from the container.
    /// </summary>
    /// <param name="path"></param>
    public void DeleteFolder(string path)
    {
      var documentStore = this.Configuration.GetDocumentStore<IFolderCapableDocumentStore>();
      documentStore.DeleteFolder(this.Configuration.ContainerTitle, path);
    }
    #endregion

    #region Entity
    /// <summary>
    /// Creates a new entity in the repository in the specified path with the specified data and returns its value.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="entityNamespace"></param>
    /// <param name="data"></param>
    /// <param name="updateIndex"></param>
    /// <returns></returns>
    public Entity CreateEntity(string path, string entityNamespace, string data)
    {
      var documentStore = this.Configuration.GetDocumentStore<IFolderCapableDocumentStore>();
      var result = documentStore.CreateEntity(this.Configuration.ContainerTitle, path, entityNamespace, data);

      return result;
    }

    /// <summary>
    /// Clones the specified entity located in the source path into the target path.
    /// </summary>
    /// <param name="entityId"></param>
    /// <param name="sourcePath"></param>
    /// <param name="targetPath"></param>
    /// <returns></returns>
    public Entity CloneEntity(Guid entityId, string sourcePath, string targetPath)
    {
      var entity = GetEntity(entityId, sourcePath);
      var newEntity = CreateEntity(targetPath, entity.Namespace, entity.Data);

      if (this.Configuration.DocumentStore is IEntityPartCapableDocumentStore)
      {
        foreach (var entityPart in ListEntityParts(entityId))
        {
          CreateEntityPart(newEntity.Id, entityPart.Name, entityPart.Category, entityPart.Data);
        }
      }

      if (this.Configuration.DocumentStore is IAttachmentCapableDocumentStore)
      {
        foreach (var attachment in ListAttachments(entityId))
        {
          var attachmentStream = DownloadAttachment(entityId, attachment.FileName);
          var attachmentBytes = attachmentStream.ReadToEnd();
          UploadAttachment(newEntity.Id, attachment.FileName, attachmentBytes);
        }
      }

      if (this.Configuration.DocumentStore is IPermissionsCapableDocumentStore)
      {
        //Clone Permissions?
      }

      return newEntity;
    }

    /// <summary>
    /// Gets the specifed entity of the specified type from the underlying repository.
    /// </summary>
    /// <param name="entityId"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public Entity GetEntity(Guid entityId, string path)
    {
      var documentStore = this.Configuration.GetDocumentStore<IFolderCapableDocumentStore>();
      var entity = documentStore.GetEntity(this.Configuration.ContainerTitle, entityId, path);
      return entity;
    }

    /// <summary>
    /// Returns a collection of entities of the specified namespace contained in the specified path.
    /// </summary>
    public IList<Entity> ListEntities(EntityFilterCriteria filterCriteria)
    {
      IList<Entity> result;
      if (filterCriteria.Path == null)
      {
        var documentStore = this.Configuration.GetDocumentStore<IFolderCapableDocumentStore>();
        result = documentStore.ListEntities(this.Configuration.ContainerTitle, filterCriteria.Path, filterCriteria);
      }
      else
      {
        var documentStore = this.Configuration.GetDocumentStore<IDocumentStore>();
        result = documentStore.ListEntities(this.Configuration.ContainerTitle, filterCriteria);
      }

      return result;
    }

    /// <summary>
    /// Gets the first entity with the specified criteria.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="entityNamespace"></param>
    /// <returns></returns>
    public Entity Single(EntityFilterCriteria filterCriteria)
    {
      return ListEntities(new EntityFilterCriteria() {
        Path = filterCriteria.Path,
        Namespace = filterCriteria.Namespace,
        NamespaceMatchType = filterCriteria.NamespaceMatchType,
        Top = 1,
        Skip = filterCriteria.Skip})
        .FirstOrDefault();
    }

    public Entity UpdateEntity(Guid entityId, string entityTitle, string entityDescription, string entityNamespace)
    {
      var documentStore = this.Configuration.GetDocumentStore<IDocumentStore>();

      var result = documentStore.UpdateEntity(this.Configuration.ContainerTitle, entityId, entityTitle, entityDescription, entityNamespace);

      return result;
    }

    public Entity UpdateEntityData(Guid entityId, string data)
    {
      return UpdateEntityData(entityId, null, data);
    }

    public Entity UpdateEntityData(Guid entityId, string eTag, string data)
    {
      var documentStore = this.Configuration.GetDocumentStore<IDocumentStore>();

      Entity result = documentStore.UpdateEntityData(this.Configuration.ContainerTitle, entityId, eTag, data);

      return result;
    }

    /// <summary>
    /// Moves the specified entity to the specified destination path.
    /// </summary>
    /// <param name="entityId"></param>
    /// <param name="destinationPath"></param>
    /// <param name="updateIndex"></param>
    /// <returns></returns>
    public bool MoveEntity(Guid entityId, string destinationPath)
    {
      var folderCapableDocumentStore = this.Configuration.GetDocumentStore<IFolderCapableDocumentStore>();
      var documentStore = this.Configuration.GetDocumentStore<IDocumentStore>();

      var entity = documentStore.GetEntity(this.Configuration.ContainerTitle, entityId);

      var result = folderCapableDocumentStore.MoveEntity(this.Configuration.ContainerTitle, entityId, destinationPath);

      return result;
    }

    /// <summary>
    /// Deletes the specified entity from the repository.
    /// </summary>
    /// <param name="entityId"></param>
    /// <param name="updateIndex"></param>
    /// <returns></returns>
    public bool DeleteEntity(Guid entityId)
    {
      var documentStore = this.Configuration.GetDocumentStore<IDocumentStore>();

      var entity = documentStore.GetEntity(this.Configuration.ContainerTitle, entityId);

      var result = documentStore.DeleteEntity(this.Configuration.ContainerTitle, entityId);

      return result;
    }
    #endregion

    #region Entity Comments
    /// <summary>
    /// Adds the specified string as a comment to the specified entity.
    /// </summary>
    /// <param name="entityId"></param>
    /// <param name="comment"></param>
    /// <param name="updateIndex"></param>
    /// <returns></returns>
    public Comment AddEntityComment(Guid entityId, string comment)
    {
      var entityDocumentStore = this.Configuration.GetDocumentStore<IDocumentStore>();

      var documentStore = this.Configuration.GetDocumentStore<ICommentCapableDocumentStore>();
      
      var entity = entityDocumentStore.GetEntity(this.Configuration.ContainerTitle, entityId);

      var result = documentStore.AddEntityComment(this.Configuration.ContainerTitle, entityId, comment);

      return result;
    }

    /// <summary>
    /// Gets the comments associated with the specified entity.
    /// </summary>
    /// <param name="entityId"></param>
    /// <returns></returns>
    public IList<Comment> ListEntityComments(Guid entityId)
    {
      var documentStore = this.Configuration.GetDocumentStore<ICommentCapableDocumentStore>();
      return documentStore.ListEntityComments(this.Configuration.ContainerTitle, entityId);
    }
    #endregion

    #region Entity Parts

    /// <summary>
    /// Returns a value that indicates if the specified entity contains an entity part of the specified type.
    /// </summary>
    /// <param name="entityId"></param>
    /// <param name="entityPartName"></param>
    /// <returns></returns>
    public bool HasEntityPart(Guid entityId, string entityPartName)
    {
      var entityParts = ListEntityParts(entityId);
      return entityParts.Any(ep => ep.Name == entityPartName);
    }

    /// <summary>
    /// Creates a new entity part with the specified name, category and data that is associated with the specified entity.
    /// </summary>
    /// <param name="entityId"></param>
    /// <param name="partName"></param>
    /// <param name="category"></param>
    /// <param name="data"></param>
    /// <param name="updateIndex"></param>
    /// <returns></returns>
    public EntityPart CreateEntityPart(Guid entityId, string partName, string category, string data)
    {
      var entityDocumentStore = this.Configuration.GetDocumentStore<IDocumentStore>();

      var entity = entityDocumentStore.GetEntity(this.Configuration.ContainerTitle, entityId);

      var documentStore = this.Configuration.GetDocumentStore<IEntityPartCapableDocumentStore>();

      var result = documentStore.CreateEntityPart(this.Configuration.ContainerTitle, entityId, partName, category, data);

      return result;
    }

    /// <summary>
    /// Returns the entity part associated with the specified entity.
    /// </summary>
    /// <param name="entityId"></param>
    /// <param name="partName"></param>
    /// <returns></returns>
    public EntityPart GetEntityPart(Guid entityId, string partName)
    {
      var documentStore = this.Configuration.GetDocumentStore<IEntityPartCapableDocumentStore>();

      return documentStore.GetEntityPart(this.Configuration.ContainerTitle, entityId, partName);
    }


    public EntityPart UpdateEntityPart(Guid entityId, string partName, string category)
    {
      var documentStore = this.Configuration.GetDocumentStore<IEntityPartCapableDocumentStore>();

      return documentStore.UpdateEntityPart(this.Configuration.ContainerTitle, entityId, partName, category);
    }

    /// <summary>
    /// Updates the entity part associated with the specified entity.
    /// </summary>
    /// <param name="entityId"></param>
    /// <param name="partName"></param>
    /// <param name="data"></param>
    /// <param name="updateIndex"></param>
    /// <returns></returns>
    public EntityPart UpdateEntityPartData(Guid entityId, string partName, string data)
    {
      return UpdateEntityPartData(entityId, partName, null, data);
    }

    public EntityPart UpdateEntityPartData(Guid entityId, string partName, string eTag, string data)
    {
      var documentStore = this.Configuration.GetDocumentStore<IEntityPartCapableDocumentStore>();

      return documentStore.UpdateEntityPartData(this.Configuration.ContainerTitle, entityId, partName, eTag, data);
    }

    /// <summary>
    /// Lists the entity parts associated with the specified entity.
    /// </summary>
    /// <param name="entityId"></param>
    /// <returns></returns>
    public IList<EntityPart> ListEntityParts(Guid entityId)
    {
      var documentStore = this.Configuration.GetDocumentStore<IEntityPartCapableDocumentStore>();

      return documentStore.ListEntityParts(this.Configuration.ContainerTitle, entityId);
    }

    /// <summary>
    /// Deletes the specified Entity Part associated with the specified entity.
    /// </summary>
    /// <param name="entityId"></param>
    /// <param name="entityPartName"></param>
    /// <param name="updateIndex"></param>
    /// <returns></returns>
    public bool DeleteEntityPart(Guid entityId, string entityPartName)
    {
      var entityDocumentStore = this.Configuration.GetDocumentStore<IDocumentStore>();

      var entity = entityDocumentStore.GetEntity(this.Configuration.ContainerTitle, entityId);

      var documentStore = this.Configuration.GetDocumentStore<IEntityPartCapableDocumentStore>();

      var result = documentStore.DeleteEntityPart(this.Configuration.ContainerTitle, entityId, entityPartName);

      return result;
    }
    #endregion

    #region Attachments
    public IList<Attachment> ListAttachments(Guid entityId)
    {
      var documentStore = this.Configuration.GetDocumentStore<IAttachmentCapableDocumentStore>();

      return documentStore.ListAttachments(this.Configuration.ContainerTitle, entityId);
    }

    public Attachment GetAttachment(Guid entityId, string fileName)
    {
       var documentStore = this.Configuration.GetDocumentStore<IAttachmentCapableDocumentStore>();

       return documentStore.GetAttachment(this.Configuration.ContainerTitle, entityId, fileName);
    }

    public Attachment UploadAttachment(Guid entityId, string fileName, byte[] attachment)
    {
      var documentStore = this.Configuration.GetDocumentStore<IAttachmentCapableDocumentStore>();

      return documentStore.UploadAttachment(this.Configuration.ContainerTitle, entityId, fileName, attachment);
    }

    public Stream DownloadAttachment(Guid entityId, string fileName)
    {
      var documentStore = this.Configuration.GetDocumentStore<IAttachmentCapableDocumentStore>();

      return documentStore.DownloadAttachment(this.Configuration.ContainerTitle, entityId, fileName);
    }

    public bool DeleteAttachment(Guid entityId, string fileName)
    {
      var documentStore = this.Configuration.GetDocumentStore<IAttachmentCapableDocumentStore>();

      return documentStore.DeleteAttachment(this.Configuration.ContainerTitle, entityId, fileName);
    }
    #endregion

    #region Permissions
    public PrincipalRoleInfo AddPrincipalRoleToEntity(Guid entityId, string principalName, string principalType, string roleName)
    {
      var documentStore = this.Configuration.GetDocumentStore<IPermissionsCapableDocumentStore>();

      return documentStore.AddPrincipalRoleToEntity(this.Configuration.ContainerTitle, entityId, principalName, principalType, roleName);
    }

    public bool RemovePrincipalRoleFromEntity(Guid entityId, string principalName, string principalType, string roleName)
    {
      var documentStore = this.Configuration.GetDocumentStore<IPermissionsCapableDocumentStore>();

      return documentStore.RemovePrincipalRoleFromEntity(this.Configuration.ContainerTitle, entityId, principalName, principalType, roleName);
    }

    public PermissionsInfo ResetEntityPermissions(Guid entityId)
    {
      var documentStore = this.Configuration.GetDocumentStore<IPermissionsCapableDocumentStore>();

      return documentStore.ResetEntityPermissions(this.Configuration.ContainerTitle, entityId);
    }

    public PermissionsInfo GetEntityPermissions(Guid entityId)
    {
        var documentStore = this.Configuration.GetDocumentStore<IPermissionsCapableDocumentStore>();

        return documentStore.GetEntityPermissions(this.Configuration.ContainerTitle, entityId);
    }
    #endregion

    public IEnumerable<Entity> EntityQuery()
    {
      throw new NotImplementedException();
    }
    #endregion

    #region Static Methods
    private static IRepositoryFactory s_repositoryFactory = null;

    /// <summary>
    /// Using the Repository Factory defined in configuration, returns a repository object.
    /// </summary>
    /// <returns></returns>
    public static Repository GetRepository()
    {
      return GetRepository(null, null);
    }

    /// <summary>
    /// Using the specified repository factory, returns a repository object.
    /// </summary>
    /// <param name="factory"></param>
    /// <returns></returns>
    public static Repository GetRepository(IRepositoryFactory factory)
    {
      return GetRepository(factory, null);
    }

    /// <summary>
    /// Using the specified repository factory, returns a repository object.
    /// </summary>
    /// <param name="factory"></param>
    /// <returns></returns>
    public static Repository GetRepository(IRepositoryFactory factory, IDocumentStore documentStore)
    {
      if (factory == null)
        factory = GetRepositoryFactoryFromConfiguration();

      Repository repository = null;
      if (documentStore == null)
        repository = factory.CreateRepository();
      else
        repository = factory.CreateRepository(documentStore);

      if (repository == null)
        throw new InvalidOperationException("The repository object returned by the factory (" + factory.GetType().ToString() + ") was null.");

      repository.Factory = factory;

      return repository;
    }

    private static object s_syncRoot = new object();

    private static IRepositoryFactory GetRepositoryFactoryFromConfiguration()
    {
      if (ConfigurationManager.AppSettings.AllKeys.Any(k => k == "BaristaDS_RepositoryFactory") == false)
        throw new InvalidOperationException("A Repository Factory has not been defined in the app/web.config file. Please add an application setting named [BaristaDS_RepositoryFactory] which is the fully-qualified type name of the object that implements IRepositoryFactory");

      //Double-Check Locking Pattern
      if (s_repositoryFactory == null)
      {
        lock (s_syncRoot)
        {
          if (s_repositoryFactory == null)
          {
            string fullTypeName = ConfigurationManager.AppSettings["BaristaDS_RepositoryFactory"];
            if (string.IsNullOrEmpty(fullTypeName))
              throw new InvalidOperationException("A BaristaDS_RepositoryFactory key was specified within web.config, but it did not contain a value.");

            Type documentStoreType = Type.GetType(fullTypeName, true, true);
            if (documentStoreType.GetInterfaces().Any(i => i == typeof(IRepositoryFactory)) == false)
              throw new InvalidOperationException("The BaristaDS_RepositoryFactory type name was specified within web.config, but it does not implement IRepositoryFactory");

            s_repositoryFactory = Activator.CreateInstance(documentStoreType) as IRepositoryFactory;
          }
        }
      }

      return s_repositoryFactory;
    }
    #endregion

    #region IDisposable
    private bool m_disposed;
    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
      Dispose(true);

      // Use SupressFinalize in case a subclass
      // of this type implements a finalizer.
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
      // If you need thread safety, use a lock around these 
      // operations, as well as in your methods that use the resource.
      lock (m_syncRoot)
      {
        if (!m_disposed)
        {
          if (disposing)
          {
            if (m_configuration != null && m_configuration != null && m_configuration.DocumentStore != null)
            {
              var ds = m_configuration.DocumentStore as IDisposable;

              if (ds != null)
              {
                try
                {
                  ds.Dispose();
                }
                catch
                {
                  //do nothing.
                }
              }
            }
          }
          m_disposed = true;
        }
      }
    }
    #endregion
  }
}
