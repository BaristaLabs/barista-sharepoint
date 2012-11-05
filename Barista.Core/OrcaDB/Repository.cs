namespace OFS.OrcaDB.Core
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
  public class Repository : IDisposable
  {
    #region Fields
    private const string IndexETagMetadataKey = "__OrcaDBStaleIndexETag";

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

    #region Configuration
    /// <summary>
    /// Registers the specified entity type with the Repository, associating it with the specified namespace.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="namespace"></param>
    public RepositoryConfiguration RegisterEntity<T>(string @namespace)
    {
      return this.Configuration.RegisterEntity<T>(@namespace);
    }

    /// <summary>
    /// Registers the specified entity type with the Repository, associating it with the specified namespace.
    /// </summary>
    /// <param name="namespace"></param>
    /// <param name="type"></param>
    public RepositoryConfiguration RegisterEntity(string @namespace, Type type)
    {
      return this.Configuration.RegisterEntity(@namespace, type);
    }

    /// <summary>
    /// Registers and associates the Entity Part Type with the parent Entity Type, associating the entity part with the specified name.
    /// </summary>
    /// <typeparam name="TEntityType"></typeparam>
    /// <typeparam name="TEntityPartType"></typeparam>
    /// <param name="entityPartName"></param>
    public RepositoryConfiguration RegisterEntityPart<TEntityType, TEntityPartType>(string entityPartName)
    {
      return this.Configuration.RegisterEntityPart<TEntityType, TEntityPartType>(entityPartName);
    }

    /// <summary>
    /// Registers and associates the Entity Part Type with the parent Entity Type, associating the entity part with the specified name.
    /// </summary>
    /// <param name="parentEntityType"></param>
    /// <param name="entityPartName"></param>
    /// <param name="entityPartType"></param>
    public RepositoryConfiguration RegisterEntityPart(Type parentEntityType, string entityPartName, Type entityPartType)
    {
      return this.Configuration.RegisterEntityPart(parentEntityType, entityPartName, entityPartType);
    }

    /// <summary>
    /// Registers an index that is updated when the associated entity is updated, deleted or is invoked manually.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TIndex"></typeparam>
    /// <param name="indexDefinition"></param>
    public RepositoryConfiguration RegisterIndex<TEntity, TIndex>(IndexDefinition indexDefinition)
    {
      return this.Configuration.RegisterIndex<TEntity, TIndex>(indexDefinition);
    }

    /// <summary>
    /// Defines a migration strategy that migrates entities of the 'from' namespace to the 'to' namespace using the defined strategy (func)
    /// </summary>
    /// <typeparam name="TDestinationEntityType"></typeparam>
    /// <param name="entityMigrationDefinition"></param>
    /// <returns></returns>
    public RepositoryConfiguration DefineMigrationStrategy<TDestinationEntityType>(EntityMigrationStrategy<TDestinationEntityType> entityMigrationDefinition)
    {
      throw new NotImplementedException();
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
    public Container CreateContainer(string containerTitle)
    {
      var documentStore = this.Configuration.GetDocumentStore<IDocumentStore>();
      return documentStore.CreateContainer(this.Configuration.ContainerTitle, containerTitle);
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
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <param name="data"></param>
    /// <param name="updateIndex"></param>
    /// <returns></returns>
    public Entity<T> CreateEntity<T>(string path, string data, bool updateIndex = true)
    {
      var entityDefinition = this.Configuration.RegisteredEntityDefinitions.Where(ed => ed.EntityType == typeof(T)).FirstOrDefault();

      if (entityDefinition == null)
        throw new InvalidOperationException("The specified entity type has not been registered with the repository. " + typeof(T));

      var documentStore = this.Configuration.GetDocumentStore<IFolderCapableDocumentStore>();
      var result = documentStore.CreateEntity(this.Configuration.ContainerTitle, path, entityDefinition.EntityNamespace, data);

      if (result != null && updateIndex)
        UpdateEntityIndexes<T>(result.Id);

      return new Entity<T>(result);
    }

    /// <summary>
    /// Creates a new entity in the repository and returns its value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    public Entity<T> CreateEntity<T>(T value, bool updateIndex = true)
    {
      var entityDefinition = this.Configuration.RegisteredEntityDefinitions.Where(ed => ed.EntityType == typeof(T)).FirstOrDefault();

      if (entityDefinition == null)
        throw new InvalidOperationException("The specified entity type has not been registered with the repository. " + typeof(T));

      var documentStore = this.Configuration.GetDocumentStore<IDocumentStore>();
      var result = documentStore.CreateEntity<T>(this.Configuration.ContainerTitle, entityDefinition.EntityNamespace, value);

      if (result != null && updateIndex)
        UpdateEntityIndexes<T>(result.Id);

      return result;
    }

    /// <summary>
    /// Creates a new entity in the repository and returns its value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    public Entity<T> CreateEntity<T>(string path, T value, bool updateIndex = true)
    {
      var entityDefinition = this.Configuration.RegisteredEntityDefinitions.Where(ed => ed.EntityType == typeof(T)).FirstOrDefault();

      if (entityDefinition == null)
        throw new InvalidOperationException("The specified entity type has not been registered with the repository. " + typeof(T));

      var documentStore = this.Configuration.GetDocumentStore<IFolderCapableDocumentStore>();
      var result = documentStore.CreateEntity<T>(this.Configuration.ContainerTitle, path, entityDefinition.EntityNamespace, value);

      if (result != null && updateIndex)
        UpdateEntityIndexes<T>(result.Id);

      return result;
    }

    /// <summary>
    /// Creates a copy of the specified entity.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="entityId"></param>
    /// <param name="sourcePath"></param>
    /// <param name="targetPath"></param>
    /// <returns></returns>
    public Entity<T> CloneEntity<T>(Guid entityId, string sourcePath, string targetPath, bool updateIndex = true)
    {
      var entity = GetEntity<T>(entityId, sourcePath);
      var newEntity = CreateEntity<T>(targetPath, entity.Data, updateIndex: false);

      if (this.Configuration.DocumentStore is IEntityPartCapableDocumentStore)
      {
        foreach(var entityPart in ListEntityParts<T>(entityId))
        {
          CreateEntityPart<T>(newEntity.Id, entityPart.Name, entityPart.Category, entityPart.Data, updateIndex: false);
        }
      }

      if (this.Configuration.DocumentStore is IAttachmentCapableDocumentStore)
      {
        foreach (var attachment in ListAttachments(entityId))
        {
          var attachmentStream = DownloadAttachment(entityId, attachment.FileName);
          var attachmentBytes = ReadFully(attachmentStream, 0);
          UploadAttachment(newEntity.Id, attachment.FileName, attachmentBytes);
        }
      }

      if (this.Configuration.DocumentStore is IPermissionsCapableDocumentStore)
      {
        //Clone Permissions?
      }

      if (updateIndex == true)
        UpdateEntityIndexes<T>(newEntity.Id);

      return GetEntity<T>(newEntity.Id);
    }

    /// <summary>
    /// Gets the specifed entity of the specified type from the underlying repository.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="entityId"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public Entity<T> GetEntity<T>(Guid entityId)
    {
      return GetEntity<T>(entityId, String.Empty);
    }

    /// <summary>
    /// Gets the specifed entity of the specified type from the underlying repository, optionally restricting to a path.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="entityId"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public Entity<T> GetEntity<T>(Guid entityId, string path = "")
    {
      var entityDefinition = this.Configuration.RegisteredEntityDefinitions.Where(ed => ed.EntityType == typeof(T)).FirstOrDefault();

      if (entityDefinition == null)
        throw new InvalidOperationException("The specified entity type has not been registered with the repository. " + typeof(T));

      var documentStore = this.Configuration.GetDocumentStore<IFolderCapableDocumentStore>();
      var entity = documentStore.GetEntity<T>(this.Configuration.ContainerTitle, entityId, path);

      var migrationStrategy = this.Configuration.RegisteredEntityMigrationStrategies.Where( ms => ms.FromNamespace == entity.Namespace ).FirstOrDefault();
      var typedMigrationStrategy = migrationStrategy as EntityMigrationStrategy<T>;
      if (typedMigrationStrategy != null)
      {
        var migratedValue = typedMigrationStrategy.EntityMigration(Newtonsoft.Json.Linq.JObject.Parse(entity.Data));
        entity.Namespace = typedMigrationStrategy.ToNamespace;
        entity.Value = migratedValue;
        //TODO: What about entity parts?
      }

      return entity;
    }

    /// <summary>
    /// Returns a collection of entities of the specified namespace contained in the specified path.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <param name="skip"></param>
    /// <param name="top"></param>
    /// <returns></returns>
    public IList<Entity<T>> ListEntities<T>(string path = "", uint? skip = null, uint? top = null)
    {
      var entityDefinition = this.Configuration.RegisteredEntityDefinitions.Where(ed => ed.EntityType == typeof(T)).FirstOrDefault();

      if (entityDefinition == null)
        throw new InvalidOperationException("The specified entity type has not been registered with the repository. " + typeof(T));

      //TODO: Utilize the index if one exists. (including populating the index)

      var documentStore = this.Configuration.GetDocumentStore<IFolderCapableDocumentStore>();
      return documentStore.ListEntities<T>(this.Configuration.ContainerTitle, path, new EntityFilterCriteria()
      {
        Namespace = entityDefinition.EntityNamespace,
        Skip = skip,
        Top = top,
      });
    }

    internal IList<Entity> ListEntities(EntityDefinition entityDefinition, string path = "", uint? skip = null, uint? top = null)
    {
      if (entityDefinition == null)
        throw new ArgumentNullException("entityDefinition");

      var documentStore = this.Configuration.GetDocumentStore<IFolderCapableDocumentStore>();
      return documentStore.ListEntities(this.Configuration.ContainerTitle, path, new EntityFilterCriteria()
      {
        Namespace = entityDefinition.EntityNamespace,
        Skip = skip,
        Top = top,
      });
    }

    /// <summary>
    /// Gets the first entity of the specified type from the repository.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <returns></returns>
    public Entity<TEntity> GetFirstEntity<TEntity>()
    {
      return GetFirstEntity<TEntity>(String.Empty);
    }

    /// <summary>
    /// Gets the first entity of the specified type from the this.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <returns></returns>
    public Entity<TEntity> GetFirstEntity<TEntity>(string path = "")
    {
      var entityDefinition = this.Configuration.RegisteredEntityDefinitions.Where(ed => ed.EntityType == typeof(TEntity)).FirstOrDefault();

      if (entityDefinition == null)
        throw new InvalidOperationException("The specified entity type has not been registered with the repository. " + typeof(TEntity));

      var client = this.Configuration.GetDocumentStore<IFolderCapableDocumentStore>();

      return client.ListEntities<TEntity>(this.Configuration.ContainerTitle, path, new EntityFilterCriteria() { Namespace = entityDefinition.EntityNamespace, Top = 1 }).FirstOrDefault();
    }

    /// <summary>
    /// Returns an instance of an entity of the given type in the given path. If no entity exists, one is created. If more than one entity of the given type exists in the given path, an exception is thrown.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="path"></param>
    /// <param name="createEntity"></param>
    /// <returns></returns>
    public Entity<TEntity> GetOrCreateEntitySingleton<TEntity>(string path, Func<TEntity> createEntity, bool updateIndex = true)
    {
      var entityDefinition = this.Configuration.RegisteredEntityDefinitions.Where(ed => ed.EntityType == typeof(TEntity)).FirstOrDefault();

      if (entityDefinition == null)
        throw new InvalidOperationException("The specified entity type has not been registered with the repository. " + typeof(TEntity));

      var documentStore = this.Configuration.GetDocumentStore<IFolderCapableDocumentStore>();

      //Double-check locking pattern.
      var result = documentStore.ListEntities<TEntity>(this.Configuration.ContainerTitle, path, new EntityFilterCriteria() { Namespace = entityDefinition.EntityNamespace }).SingleOrDefault();
      if (result == null)
      {
        lock (m_syncRoot)
        {
          result = documentStore.ListEntities<TEntity>(this.Configuration.ContainerTitle, path, new EntityFilterCriteria() { Namespace = entityDefinition.EntityNamespace }).SingleOrDefault();

          if (result == null)
          {
            result = documentStore.CreateEntity<TEntity>(this.Configuration.ContainerTitle, path, entityDefinition.EntityNamespace, createEntity());

            if (result != null && updateIndex)
              UpdateEntityIndexes<TEntity>(result.Id);
          }
        }
      }

      return result;
    }

    public Entity<TEntity> CreateOrUpdateEntitySingleton<TEntity>(string path, string data, bool updateIndex = true)
    {
      TEntity entity = DocumentStoreHelper.DeserializeObjectFromJson<TEntity>(data);
      return CreateOrUpdateEntitySingleton<TEntity>(path, entity, updateIndex);
    }

    public Entity<TEntity> CreateOrUpdateEntitySingleton<TEntity>(string path, TEntity entity, bool updateIndex = true)
    {
      var entityDefinition = this.Configuration.RegisteredEntityDefinitions.Where(ed => ed.EntityType == typeof(TEntity)).FirstOrDefault();

      if (entityDefinition == null)
        throw new InvalidOperationException("The specified entity type has not been registered with the repository. " + typeof(TEntity));

      var documentStore = this.Configuration.GetDocumentStore<IFolderCapableDocumentStore>();

      //Double-check locking pattern.
      var result = documentStore.ListEntities<TEntity>(this.Configuration.ContainerTitle, path, new EntityFilterCriteria() { Namespace = entityDefinition.EntityNamespace }).SingleOrDefault();
      if (result == null)
      {
        lock (m_syncRoot)
        {
          result = documentStore.ListEntities<TEntity>(this.Configuration.ContainerTitle, path, new EntityFilterCriteria() { Namespace = entityDefinition.EntityNamespace }).SingleOrDefault();

          if (result == null)
          {
            result = documentStore.CreateEntity<TEntity>(this.Configuration.ContainerTitle, path, entityDefinition.EntityNamespace, entity);
          }
          else
          {
            var documentStore2 = this.Configuration.GetDocumentStore<IDocumentStore>();
            documentStore2.UpdateEntity<TEntity>(this.Configuration.ContainerTitle, result.Id, entity);
            result = GetEntity<TEntity>(result.Id);
          }
        }
      }
      else
      {
        var documentStore2 = this.Configuration.GetDocumentStore<IDocumentStore>();
        lock (m_syncRoot)
        {
          documentStore2.UpdateEntity<TEntity>(this.Configuration.ContainerTitle, result.Id, entity);
          result = GetEntity<TEntity>(result.Id);
        }
      }

      if (result != null && updateIndex)
        UpdateEntityIndexes<TEntity>(result.Id);

      return result;
    }

    public bool UpdateEntity<TEntity>(Entity<TEntity> entity, bool updateIndex = true)
    {
      var entityDefinition = this.Configuration.RegisteredEntityDefinitions.Where(ed => ed.EntityType == typeof(TEntity)).FirstOrDefault();

      if (entityDefinition == null)
        throw new InvalidOperationException("The specified entity type has not been registered with the repository. " + typeof(TEntity));

      var documentStore = this.Configuration.GetDocumentStore<IDocumentStore>();

      var result = documentStore.UpdateEntity<TEntity>(this.Configuration.ContainerTitle, entity.Id, entity.Value);

      if (result == true && updateIndex)
        UpdateEntityIndexes<TEntity>(entity.Id);

      return result;
    }

    /// <summary>
    /// Moves the specified entity to the specified destination path.
    /// </summary>
    /// <param name="entityId"></param>
    /// <param name="destinationPath"></param>
    /// <returns></returns>
    public bool MoveEntity<TEntity>(Guid entityId, string destinationPath, bool updateIndex = true)
    {
      var documentStore = this.Configuration.GetDocumentStore<IFolderCapableDocumentStore>();

      var result = documentStore.MoveEntity(this.Configuration.ContainerTitle, entityId, destinationPath);

      if (result && updateIndex)
        UpdateEntityIndexes<TEntity>(entityId);

      return result;
    }

    /// <summary>
    /// Deletes the specified entity from the repository.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="entityId"></param>
    /// <param name="updateIndex"></param>
    /// <returns></returns>
    public bool DeleteEntity<TEntity>(Guid entityId, bool updateIndex = true)
    {
      var documentStore = this.Configuration.GetDocumentStore<IDocumentStore>();

      var result = documentStore.DeleteEntity(this.Configuration.ContainerTitle, entityId);

      if (result && updateIndex)
        UpdateEntityIndexes<TEntity>(entityId);

      return result;
    }
    #endregion

    #region Entity Comments
    /// <summary>
    /// Adds the specified string as a comment to the specified entity.
    /// </summary>
    /// <param name="entityId"></param>
    /// <param name="comment"></param>
    /// <returns></returns>
    public Comment AddEntityComment<TEntity>(Guid entityId, string comment, bool updateIndex = true)
    {
      var documentStore = this.Configuration.GetDocumentStore<ICommentCapableDocumentStore>();
      var result = documentStore.AddEntityComment(this.Configuration.ContainerTitle, entityId, comment);

      if (result != null && updateIndex)
        UpdateEntityIndexes<TEntity>(entityId);

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
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TEntityPart"></typeparam>
    /// <param name="entityId"></param>
    /// <returns></returns>
    public bool HasEntityPart<TEntity, TEntityPart>(Guid entityId)
    {
      var entityDefinition = this.Configuration.RegisteredEntityDefinitions.Where(ed => ed.EntityType == typeof(TEntity)).FirstOrDefault();

      if (entityDefinition == null)
        throw new InvalidOperationException("The specified entity type has not been registered with the repository. " + typeof(TEntity));

      var entityPartDefinition = entityDefinition.EntityPartDefinitions.Where(pd => pd.EntityPartType == typeof(TEntityPart)).FirstOrDefault();

      if (entityPartDefinition == null)
        throw new InvalidOperationException("The specified Entity Part Type has not been registered with the repository. " + typeof(TEntityPart).ToString());

      var entityParts = ListEntityParts<TEntity>(entityId);
      return entityParts.Any(ep => ep.Name == entityPartDefinition.EntityPartName);
    }

    /// <summary>
    /// Creates a new entity part with the specified name, category and data that is associated with the specified entity.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="entityId"></param>
    /// <param name="partName"></param>
    /// <param name="category"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public EntityPart CreateEntityPart<TEntity>(Guid entityId, string partName, string category, string data, bool updateIndex = true)
    {
      var documentStore = this.Configuration.GetDocumentStore<IEntityPartCapableDocumentStore>();

      var result = documentStore.CreateEntityPart(this.Configuration.ContainerTitle, entityId, partName, category, data);

      if (result != null && updateIndex)
        UpdateEntityIndexes<TEntity>(entityId);

      return result;
    }

    public EntityPart<TEntityPart> CreateEntityPart<TEntity, TEntityPart>(Guid entityId, string partName, string category, TEntityPart value, bool updateIndex = true)
    {
      var documentStore = this.Configuration.GetDocumentStore<IEntityPartCapableDocumentStore>();

      var result = documentStore.CreateEntityPart<TEntityPart>(this.Configuration.ContainerTitle, entityId, partName, category, value);

      if (result != null && updateIndex)
        UpdateEntityIndexes<TEntity>(entityId);

      return result;
    }

    public EntityPart<TEntityPart> CreateEntityPart<TEntity, TEntityPart>(Guid entityId, string data, bool updateIndex = true)
    {
      var entityDefinition = this.Configuration.RegisteredEntityDefinitions.Where(ed => ed.EntityType == typeof(TEntity)).FirstOrDefault();

      if (entityDefinition == null)
        throw new InvalidOperationException("The specified entity type has not been registered with the repository. " + typeof(TEntity));

      var entityPartDefinition = entityDefinition.EntityPartDefinitions.Where(pd => pd.EntityPartType == typeof(TEntityPart)).FirstOrDefault();

      if (entityPartDefinition == null)
        throw new InvalidOperationException("The specified Entity Part Type has not been registered with the repository. " + typeof(TEntityPart).ToString());

      var value = DocumentStoreHelper.DeserializeObjectFromJson<TEntityPart>(data);

      var documentStore = this.Configuration.GetDocumentStore<IEntityPartCapableDocumentStore>();

      var result = documentStore.CreateEntityPart<TEntityPart>(this.Configuration.ContainerTitle, entityId, entityPartDefinition.EntityPartName, value);

      if (result != null && updateIndex)
        UpdateEntityIndexes<TEntity>(entityId);

      return result;
    }

    /// <summary>
    /// Creates an entity part of the specified type associated with the entity with the specified id.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TEntityPart"></typeparam>
    /// <param name="entityId"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public EntityPart<TEntityPart> CreateEntityPart<TEntity, TEntityPart>(Guid entityId, TEntityPart value, bool updateIndex = true)
    {
      var entityDefinition = this.Configuration.RegisteredEntityDefinitions.Where(ed => ed.EntityType == typeof(TEntity)).FirstOrDefault();

      if (entityDefinition == null)
        throw new InvalidOperationException("The specified entity type has not been registered with the repository. " + typeof(TEntity));

      var entityPartDefinition = entityDefinition.EntityPartDefinitions.Where(pd => pd.EntityPartType == typeof(TEntityPart)).FirstOrDefault();
     
      if (entityPartDefinition == null)
        throw new InvalidOperationException("The specified Entity Part Type has not been registered with the repository. " + typeof(TEntityPart).ToString());

      var documentStore = this.Configuration.GetDocumentStore<IEntityPartCapableDocumentStore>();

      var result = documentStore.CreateEntityPart<TEntityPart>(this.Configuration.ContainerTitle, entityId, entityPartDefinition.EntityPartName, value);

      if (result != null && updateIndex)
        UpdateEntityIndexes<TEntity>(entityId);

      return result;
    }

    /// <summary>
    /// Returns the entity part associated with the specified entity.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="entityId"></param>
    /// <param name="partName"></param>
    /// <returns></returns>
    public EntityPart<TEntityPart> GetEntityPart<TEntity, TEntityPart>(Guid entityId, string partName)
    {
      var entityDefinition = this.Configuration.RegisteredEntityDefinitions.Where(ed => ed.EntityType == typeof(TEntity)).FirstOrDefault();

      if (entityDefinition == null)
        throw new InvalidOperationException("The specified entity type has not been registered with the repository. " + typeof(TEntity));

      var documentStore = this.Configuration.GetDocumentStore<IEntityPartCapableDocumentStore>();

      return documentStore.GetEntityPart<TEntityPart>(this.Configuration.ContainerTitle, entityId, partName);
    }

    /// <summary>
    /// Returns the entity part associated with the specified entity.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="entityId"></param>
    /// <returns></returns>
    public EntityPart<TEntityPart> GetEntityPart<TEntity, TEntityPart>(Guid entityId)
    {
      var entityDefinition = this.Configuration.RegisteredEntityDefinitions.Where(ed => ed.EntityType == typeof(TEntity)).FirstOrDefault();

      if (entityDefinition == null)
        throw new InvalidOperationException("The specified entity type has not been registered with the repository. " + typeof(TEntity));

      var entityPartDefinition = entityDefinition.EntityPartDefinitions.Where(pd => pd.EntityPartType == typeof(TEntityPart)).FirstOrDefault();
     
      if (entityPartDefinition == null)
        throw new InvalidOperationException("The specified Entity Part Type has not been registered with the repository. " + typeof(TEntityPart).ToString());

      var documentStore = this.Configuration.GetDocumentStore<IEntityPartCapableDocumentStore>();

      return documentStore.GetEntityPart<TEntityPart>(this.Configuration.ContainerTitle, entityId, entityPartDefinition.EntityPartName);
    }

    public bool TryGetEntityPart<TEntity, TEntityPart>(Guid entityId, out EntityPart<TEntityPart> entityPart)
    {
      var entityDefinition = this.Configuration.RegisteredEntityDefinitions.Where(ed => ed.EntityType == typeof(TEntity)).FirstOrDefault();

      if (entityDefinition == null)
        throw new InvalidOperationException("The specified entity type has not been registered with the repository. " + typeof(TEntity));

      var entityPartDefinition = entityDefinition.EntityPartDefinitions.Where(pd => pd.EntityPartType == typeof(TEntityPart)).FirstOrDefault();

      if (entityPartDefinition == null)
        throw new InvalidOperationException("The specified Entity Part Type has not been registered with the repository. " + typeof(TEntityPart).ToString());

      var documentStore = this.Configuration.GetDocumentStore<IEntityPartCapableDocumentStore>();

      return documentStore.TryGetEntityPart<TEntityPart>(this.Configuration.ContainerTitle, entityId, entityPartDefinition.EntityPartName, out entityPart);
    }

    /// <summary>
    /// Updates the entity part associated with the specified entity.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TEntityPart"></typeparam>
    /// <param name="entityId"></param>
    /// <returns></returns>
    public bool UpdateEntityPart<TEntity, TEntityPart>(Guid entityId, TEntityPart value, bool updateIndex = true)
    {
      var entityDefinition = this.Configuration.RegisteredEntityDefinitions.Where(ed => ed.EntityType == typeof(TEntity)).FirstOrDefault();

      if (entityDefinition == null)
        throw new InvalidOperationException("The specified Entity Type has not been registered with the repository. " + typeof(TEntity));

      var entityPartDefinition = entityDefinition.EntityPartDefinitions.Where(pd => pd.EntityPartType == typeof(TEntityPart)).FirstOrDefault();

      if (entityPartDefinition == null)
        throw new InvalidOperationException("The specified Entity Part Type has not been registered with the repository. " + typeof(TEntityPart).ToString());

      var result = UpdateEntityPart<TEntity, TEntityPart>(entityId, entityPartDefinition.EntityPartName, value);

      if (result == true && updateIndex)
        UpdateEntityIndexes<TEntity>(entityId);

      return result;
    }

    /// <summary>
    /// Updates the entity part associated with the specified entity.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TEntityPart"></typeparam>
    /// <param name="entityId"></param>
    /// <returns></returns>
    public bool UpdateEntityPart<TEntity, TEntityPart>(Guid entityId, string entityPartName, TEntityPart value, bool updateIndex = false)
    {
      var entityDefinition = this.Configuration.RegisteredEntityDefinitions.Where(ed => ed.EntityType == typeof(TEntity)).FirstOrDefault();

      if (entityDefinition == null)
        throw new InvalidOperationException("The specified Entity Type has not been registered with the repository. " + typeof(TEntity));

      var documentStore = this.Configuration.GetDocumentStore<IEntityPartCapableDocumentStore>();
      var result = documentStore.UpdateEntityPart<TEntityPart>(this.Configuration.ContainerTitle, entityId, entityPartName, value);

      if (result == true && updateIndex)
        UpdateEntityIndexes<TEntity>(entityId);

      return result;
    }

    /// <summary>
    /// Lists the entity parts associated with the specified entity.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="entityId"></param>
    /// <returns></returns>
    public IList<EntityPart> ListEntityParts<TEntity>(Guid entityId)
    {
      var entityDefinition = this.Configuration.RegisteredEntityDefinitions.Where(ed => ed.EntityType == typeof(TEntity)).FirstOrDefault();

      if (entityDefinition == null)
        throw new InvalidOperationException("The specified entity type has not been registered with the repository. " + typeof(TEntity));

      var documentStore = this.Configuration.GetDocumentStore<IEntityPartCapableDocumentStore>();

      return documentStore.ListEntityParts(this.Configuration.ContainerTitle, entityId);
    }

    /// <summary>
    /// Returns the specified entity part associated with the first entity in the repository.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="entityId"></param>
    /// <returns></returns>
    public EntityPart<TEntityPart> GetFirstEntityPart<TEntity, TEntityPart>()
    {
      var entityDefinition = this.Configuration.RegisteredEntityDefinitions.Where(ed => ed.EntityType == typeof(TEntity)).FirstOrDefault();

      if (entityDefinition == null)
        throw new InvalidOperationException("The specified entity type has not been registered with the repository. " + typeof(TEntity));

      var entityPartDefinition = entityDefinition.EntityPartDefinitions.Where(pd => pd.EntityPartType == typeof(TEntityPart)).FirstOrDefault();

      if (entityPartDefinition == null)
        throw new InvalidOperationException("The specified Entity Part Type has not been registered with the repository. " + typeof(TEntityPart).ToString());

      var firstEntity = this.Configuration.DocumentStore.ListEntities<TEntity>(this.Configuration.ContainerTitle, new EntityFilterCriteria() { Namespace = entityDefinition.EntityNamespace, Top = 1 }).FirstOrDefault();

      if (firstEntity == null)
        return null;

      var documentStore = this.Configuration.GetDocumentStore<IEntityPartCapableDocumentStore>();
      return documentStore.GetEntityPart<TEntityPart>(this.Configuration.ContainerTitle, firstEntity.Id, entityPartDefinition.EntityPartName);
    }

    /// <summary>
    /// Deletes the specified Entity Part associated with the specified entity.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TEntityPart"></typeparam>
    /// <param name="entityId"></param>
    /// <returns></returns>
    public bool DeleteEntityPart<TEntity, TEntityPart>(Guid entityId, bool updateIndex = true)
    {
      var entityDefinition = this.Configuration.RegisteredEntityDefinitions.Where(ed => ed.EntityType == typeof(TEntity)).FirstOrDefault();

      if (entityDefinition == null)
        throw new InvalidOperationException("The specified entity type has not been registered with the repository. " + typeof(TEntity));

      var entityPartDefinition = entityDefinition.EntityPartDefinitions.Where(pd => pd.EntityPartType == typeof(TEntityPart)).FirstOrDefault();

      if (entityPartDefinition == null)
        throw new InvalidOperationException("The specified Entity Part Type has not been registered with the repository. " + typeof(TEntityPart).ToString());

      var documentStore = this.Configuration.GetDocumentStore<IEntityPartCapableDocumentStore>();

      var result = documentStore.DeleteEntityPart(this.Configuration.ContainerTitle, entityId, entityPartDefinition.EntityPartName);

      if (result && updateIndex)
        UpdateEntityIndexes<TEntity>(entityId);

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
    #endregion

    public IEnumerable<Entity> EntityQuery()
    {
      throw new NotImplementedException();
    }
    #endregion

    #region Repository Indexes

    /// <summary>
    /// Get all indexes associated with the specified entity, optionally specifying the index, optionally rebuilding the index, and optionally specifying a index source path.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TIndex"></typeparam>
    /// <param name="indexName"></param>
    /// <param name="rebuildIndexIfMissing"></param>
    /// <param name="rebuildPath"></param>
    /// <returns></returns>
    public ICollection<TIndex> GetEntityIndex<TEntity, TIndex>(string indexName = "", bool rebuildIndexIfMissing = true, string rebuildPath = "", bool rebuildIndexIfStale = false, bool reduce = true)
    {
      var entityDefinition = this.Configuration.RegisteredEntityDefinitions.Where(ed => ed.EntityType == typeof(TEntity)).FirstOrDefault();

      if (entityDefinition == null)
        throw new InvalidOperationException("The specified entity type has not been registered with the repository. " + typeof(TEntity));

      var indexDefinitions = entityDefinition.IndexDefinitions.Where(id => id.IndexType == typeof(TIndex));

      IndexDefinition indexDefinition = null;
      if (indexDefinitions.Count() == 1 && String.IsNullOrEmpty(indexName))
        indexDefinition = indexDefinitions.FirstOrDefault();
      else
      {
        if (indexDefinitions.Count() > 1 && String.IsNullOrEmpty(indexName))
          throw new InvalidOperationException("Multiple indexes exist with the specified types. Please specify an index name.");

        indexDefinition = indexDefinitions.Where(id => id.Name == indexName).FirstOrDefault();
      }

      if (indexDefinition == null)
        throw new InvalidOperationException("Unable to locate the index definition for the specified index type: " + typeof(TIndex) + " and/or name: " + indexName);

      var client = Configuration.GetDocumentStore<IFolderCapableDocumentStore>();
      var indexDefinitionFolder = client.GetFolder(Configuration.ContainerTitle, Constants.IndexFolderName + "/" + indexDefinition.Name);

      if (indexDefinitionFolder == null)
      {
        indexDefinitionFolder = client.CreateFolder(Configuration.ContainerTitle, Constants.IndexFolderName + "/" + indexDefinition.Name);

        if (rebuildIndexIfMissing == false)
          return null;

        RebuildEntityIndexes<TEntity>(rebuildPath);
      }

      var dsIndex = GetFirstEntity<Index>(indexDefinitionFolder.FullPath);

      if (dsIndex == null && rebuildIndexIfMissing)
      {
        RebuildEntityIndexes<TEntity>(rebuildPath);
        dsIndex = GetFirstEntity<Index>(indexDefinitionFolder.FullPath);
      }

      if (dsIndex == null)
        return null;

      var epClient = Configuration.GetDocumentStore<IEntityPartCapableDocumentStore>();
      var items = epClient.ListEntityParts(Configuration.ContainerTitle, dsIndex.Id);

      var result = new List<KeyValuePair<Guid, IndexObject>>();
      foreach (var item in items)
      {
        var indexItem = DocumentStoreHelper.DeserializeObjectFromJson<IndexObject>(item.Data);
        result.Add(new KeyValuePair<Guid, IndexObject>(new Guid(item.Name), indexItem));
      }

      //if (dsIndex == null)
      //  return null;
      
      ////If we're in aggressive mode, validate that the index is not stale.
      //if (indexDefinition.IndexETag != null)
      //{
      //  var indexETag = dsIndex.Value.ETag;
      //  var currentIndexETag = indexDefinition.IndexETag(this);

      //  if (indexETag != currentIndexETag)
      //  {
      //    if (HttpContext.Current != null && HttpContext.Current.Response != null)
      //    {
      //      HttpContext.Current.Response.AddHeader("OrcaDBIndexIsStale", "true");
      //    }

      //    if (rebuildIndexIfStale == true)
      //    {
      //      if (lockClient.GetEntityLockStatus(Configuration.ContainerTitle, Constants.IndexFolderName + "/" + indexDefinition.Name, dsIndex.Id))
      //      {
      //        RebuildEntityIndexes<TEntity>(rebuildPath);
      //      }
      //      else
      //      {
      //        lockClient.WaitForEntityLockRelease(Configuration.ContainerTitle, Constants.IndexFolderName + "/" + indexDefinition.Name, dsIndex.Id, 5000);
      //      }

      //      dsIndex = GetFirstEntity<Index>(indexDefinitionFolder.FullPath);
      //    }
      //  }
      //}

      IList<KeyValuePair<Guid, IndexObject>> indexedItems = result;

      if (reduce == true)
      {
        indexedItems = indexDefinition.Reduce(result);
      }

      return indexedItems.Select(i => (TIndex)i.Value.MapResult).ToList();
    }

    /// <summary>
    /// Instruct the repository to rebuild the entity indexes.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public void RebuildEntityIndexes<TEntity>()
    {
      RebuildEntityIndexes<TEntity>(String.Empty);
    }

    /// <summary>
    /// Instruct the repository to rebuild the entity indexes using the specified source path.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="path"></param>
    public void RebuildEntityIndexes<TEntity>(string path = "")
    {
      var entityDefinition = this.Configuration.RegisteredEntityDefinitions.Where(ed => ed.EntityType == typeof(TEntity)).FirstOrDefault();

      if (entityDefinition == null)
        throw new InvalidOperationException("The specified entity type has not been registered with the repository. " + typeof(TEntity));

      RebuildEntityIndexes(entityDefinition, path);
    }

    internal void RebuildEntityIndexes(EntityDefinition entityDefinition, string path = "")
    {
      var client = Configuration.GetDocumentStore<IEntityPartCapableDocumentStore>();
      var metadataClient = Configuration.GetDocumentStore<IMetadataCapableDocumentStore>();

      string isRebuilding = metadataClient.GetContainerMetadata(Configuration.ContainerTitle, "IsIndexRebuilding");

      if (String.IsNullOrEmpty(isRebuilding) == false)
      {
        if (DateTime.Parse(isRebuilding).AddMinutes(15) > DateTime.Now)
          return;
      }

      lock (m_syncRoot)
      {
        metadataClient.SetContainerMetadata(Configuration.ContainerTitle, "IsIndexRebuilding", DateTime.Now.ToString());

        var entities = ListEntities(entityDefinition, path, null, null);
        var currentEntityIds = entities.Select( e => e.Id);

        ApplicationLog.AddLogEntry(new ApplicationLogEntry()
        {
          Level = LogLevel.Information,
          Logger = "System",
          Message = String.Format("Rebuilding Index for type {0}. {1} entities found.", entityDefinition.EntityType.ToString(), entities.Count()),
          TimeStamp = DateTime.Now,
          Host = Environment.MachineName,
        });

        try
        {
          var folderClient = Configuration.GetDocumentStore<IFolderCapableDocumentStore>();
          IDictionary<IndexDefinition, Entity<Index>> indexes = new ConcurrentDictionary<IndexDefinition, Entity<Index>>();

          //For each index definition, ensure that the index folders and index entities are present.
          foreach (var indexDefinition in entityDefinition.IndexDefinitions)
          {
            Entity<Index> dsIndex = null;

            if (indexes == null || indexes.ContainsKey(indexDefinition) == false)
            {
              //Get or create the index entity from the DocumentStore.

              var indexDefinitionFolder = folderClient.GetFolder(Configuration.ContainerTitle, Constants.IndexFolderName + "/" + indexDefinition.Name);

              if (indexDefinitionFolder == null)
                indexDefinitionFolder = folderClient.CreateFolder(Configuration.ContainerTitle, Constants.IndexFolderName + "/" + indexDefinition.Name);

              dsIndex = GetOrCreateEntitySingleton<Index>(indexDefinitionFolder.FullPath, () => new Index());

              indexes.Add(indexDefinition, dsIndex);
            }
            else
            {
              //Retrieve the index entity from the dictionary.
              dsIndex = indexes[indexDefinition];
            }
          }

          if (Configuration.IsDocumentStore<IAsyncExecDocumentStore>())
          {
            var asyncClient = Configuration.GetDocumentStore<IAsyncExecDocumentStore>();
            var tasks = new List<Task>();
            foreach (var entity in entities)
            {
              var currentEntity = entity;
              Task t = asyncClient.ExecAsync(() =>
              {
                using (var repo = Repository.GetRepository(this.Factory, RepositoryConfiguration.InitializeDocumentStoreFromConfiguration()))
                {
                  repo.UpdateEntityIndexes(entityDefinition, currentEntity.Id, currentEntity, ref indexes);
                }
              });
              tasks.Add(t);
            }
            Task.WaitAll(tasks.ToArray());
          }
          else
          {
            foreach (var entity in entities)
            {
              var currentEntity = entity;
              UpdateEntityIndexes(entityDefinition, currentEntity.Id, currentEntity, ref indexes);
            }
          }

          if (indexes != null)
          {
            foreach (var indexDefinition in indexes)
            {
              //Remove any missing entities from the index.
              foreach (var deletedEntity in indexDefinition.Value.Value.Items.Where(io => currentEntityIds.Contains(io.Key) == false).ToList())
              {
                client.DeleteEntityPart(Configuration.ContainerTitle, indexDefinition.Value.Id, deletedEntity.Key.ToString());
              }
            }
          }
        }
        catch (Exception ex)
        {
          ApplicationLog.AddException(ex);
          throw;
        }
        finally
        {
          metadataClient.SetContainerMetadata(Configuration.ContainerTitle, "IsIndexRebuilding", null);
        }

        ApplicationLog.AddLogEntry(new ApplicationLogEntry()
        {
          Level = LogLevel.Information,
          Logger = "System",
          Message = String.Format("Completed rebuilding indexes for {0}.", entityDefinition.EntityType.ToString()),
          TimeStamp = DateTime.Now,
          Host = Environment.MachineName,
        });
      }
    }

    /// <summary>
    /// Updates all indexes associated with the specified entity.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="entity"></param>
    public void UpdateEntityIndexes<TEntity>(Guid entityId, Entity<TEntity> entity = null, string path = "", bool updateAsync = true)
    {
      var entityDefinition = this.Configuration.RegisteredEntityDefinitions.Where(ed => ed.EntityType == typeof(TEntity)).FirstOrDefault();

      if (entityDefinition == null)
        throw new InvalidOperationException("The specified entity type has not been registered with the repository. " + typeof(TEntity));

      //If the entity definition does not have indexes defined, get out of dodge.
      if (entityDefinition.IndexDefinitions == null || entityDefinition.IndexDefinitions.Count == 0)
        return;

      if (entity == null)
      {
        //Wait a bit to allow for stuff to persist.
        System.Threading.Thread.Sleep(300);
        entity = GetEntity<TEntity>(entityId);
      }

      IDictionary<IndexDefinition, Entity<Index>> indexes = null;

      if (updateAsync && Configuration.IsDocumentStore<IAsyncExecDocumentStore>())
      {
        var asyncClient = Configuration.GetDocumentStore<IAsyncExecDocumentStore>();

        IDocumentStore threadLocalDocumentStore = null;
        if (RepositoryConfiguration.SettingsDefinedInConfiguration)
        {
          threadLocalDocumentStore = RepositoryConfiguration.InitializeDocumentStoreFromConfiguration();
        }
        else
        {
          threadLocalDocumentStore = this.Configuration.DocumentStore;
        }

        Task t = asyncClient.ExecAsync(() =>
          {
            using (var repo = Repository.GetRepository(this.Factory, threadLocalDocumentStore))
            {
              repo.UpdateEntityIndexes(entityDefinition, entity.Id, entity, ref indexes);
            }
          });
        t.Wait();
      }
      else
      {
        UpdateEntityIndexes(entityDefinition, entityId, entity, ref indexes);
      }
    }

    /// <summary>
    /// Updates all indexes associated with the specified entity.
    /// </summary>
    /// <param name="entityId"></param>
    /// <param name="entityNamespace"></param>
    /// <param name="path"></param>
    /// <param name="rebuildIndexIfNotExists"></param>
    /// <param name="persistIndexChanges"></param>
    /// <param name="updateAsync"></param>
    public void UpdateEntityIndexes(Guid entityId, string entityNamespace, string path = "", bool rebuildIndexIfNotExists = true, bool updateAsync = true)
    {
      if (entityId == null || entityId == Guid.Empty || entityId == default(Guid))
        throw new ArgumentNullException("entityId");

      if (String.IsNullOrEmpty(entityNamespace))
        throw new ArgumentNullException("entityNamespace");

      var entityDefinition = this.Configuration.RegisteredEntityDefinitions.Where(ed => ed.EntityNamespace == entityNamespace).FirstOrDefault();

      if (entityDefinition == null)
        throw new InvalidOperationException("The specified entity namespace has not been registered with the repository. " + entityNamespace);

      //If the entity definition does not have indexes defined, get out of dodge.
      if (entityDefinition.IndexDefinitions == null || entityDefinition.IndexDefinitions.Count == 0)
        return;

      var documentStore = this.Configuration.GetDocumentStore<IFolderCapableDocumentStore>();
      //Wait a bit to allow for stuff to persist.
      System.Threading.Thread.Sleep(200);
      var entity = documentStore.GetEntity(this.Configuration.ContainerTitle, entityId, path);

      IDictionary<IndexDefinition, Entity<Index>> indexes = null;
      if (updateAsync && Configuration.IsDocumentStore<IAsyncExecDocumentStore>())
      {
        var asyncClient = Configuration.GetDocumentStore<IAsyncExecDocumentStore>();
        Task t = asyncClient.ExecAsync(() =>
        {
          IDocumentStore threadLocalDocumentStore = null;
          if (RepositoryConfiguration.SettingsDefinedInConfiguration)
          {
            threadLocalDocumentStore = RepositoryConfiguration.InitializeDocumentStoreFromConfiguration();
          }
          else
          {
            threadLocalDocumentStore = this.Configuration.DocumentStore;
          }

          using (var repo = Repository.GetRepository(this.Factory, threadLocalDocumentStore))
          {
            repo.UpdateEntityIndexes(entityDefinition, entity.Id, entity, ref indexes);
          }
        });
        t.Wait();
      }
      else
      {
        UpdateEntityIndexes(entityDefinition, entityId, entity, ref indexes);
      }
    }

    internal void UpdateEntityIndexes(EntityDefinition entityDefinition, Guid entityId, Entity entity, ref IDictionary<IndexDefinition, Entity<Index>> indexes)
    {
      if (entityDefinition == null)
        throw new ArgumentNullException("entityDefinition");

      if (entity != null && entity.Id != entityId)
        throw new InvalidOperationException("The entityId and the id of the entity do not match.");

      var client = Configuration.GetDocumentStore<IFolderCapableDocumentStore>();

      //TODO: Make this parallel.
      foreach (var indexDefinition in entityDefinition.IndexDefinitions)
      {
        Entity<Index> dsIndex = null;

        if (indexes == null || indexes.ContainsKey(indexDefinition) == false)
        {
          //Get or create the index entity from the DocumentStore.

          var indexDefinitionFolder = client.GetFolder(Configuration.ContainerTitle, Constants.IndexFolderName + "/" + indexDefinition.Name);

          if (indexDefinitionFolder == null)
            indexDefinitionFolder = client.CreateFolder(Configuration.ContainerTitle, Constants.IndexFolderName + "/" + indexDefinition.Name);

          dsIndex = GetOrCreateEntitySingleton<Index>(indexDefinitionFolder.FullPath, () => new Index());

          if (indexes == null)
            indexes = new Dictionary<IndexDefinition, Entity<Index>>();

          indexes.Add(indexDefinition, dsIndex);
        }
        else
        {
          //Retrieve the index entity from the dictionary.
          dsIndex = indexes[indexDefinition];
        }

        try
        {
          PerformMap(indexDefinition, entityId, entity, dsIndex);
        }
        catch (Exception ex)
        {
          ApplicationLog.AddException(ex);

          /* Do Nothing!! Invalid data results in the value not showing up in the index, rather than the full index failing... */
        }
      }
    }

    /// <summary>
    /// Performs the map operation for the specified index.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="indexDefinition"></param>
    /// <param name="entityId"></param>
    /// <param name="entity"></param>
    /// <param name="dsIndex"></param>
    /// <returns></returns>
    private bool PerformMap(IndexDefinition indexDefinition, Guid entityId, Entity entity, Entity<Index> dsIndex)
    {
      var client = Configuration.GetDocumentStore<IEntityPartCapableDocumentStore>();

      if (indexDefinition == null || indexDefinition.Map == null)
        return false;

      var result = false;

      //If the entity was null, remove the entity from the index.
      if (entity == null)
      {
        try
        {
          client.DeleteEntityPart(Configuration.ContainerTitle, dsIndex.Id, entity.Id.ToString());
        }
        catch { /* Do Nothing */ }
      }
      else
      {
        //TODO: Remap only if the ETags don't match.

        object mapResult = indexDefinition.Map(entity);

        if (mapResult.GetType() != indexDefinition.IndexType && mapResult.GetType().IsSubclassOf(indexDefinition.IndexType) == false)
          throw new InvalidOperationException("The return type of the index map function did not match the specified index type. " + indexDefinition.IndexType);

        if (mapResult != null)
        {
          var indexObject = new IndexObject()
          {
            Metadata = new Dictionary<string, string>() {
              { "ETag", entity.ETag }
            },
            MapResult = mapResult
          };

          client.CreateEntityPart<IndexObject>(Configuration.ContainerTitle, dsIndex.Id, entityId.ToString(), indexObject);

          result = true;
        }
      }

      return result;
    }

    /// <summary>
    /// Perform the reduce operation for a specified index.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="indexDefinition"></param>
    /// <param name="entityId"></param>
    /// <param name="entity"></param>
    /// <param name="dsIndex"></param>
    /// <returns></returns>
    public bool PerformReduce<TEntity>(IndexDefinition indexDefinition, Entity<Index> dsIndex)
    {
      if (indexDefinition == null || indexDefinition.Reduce == null)
        return false;

      var reduction = indexDefinition.Reduce(dsIndex.Value.Items.ToList());
      dsIndex.Value.Items.Clear();
      foreach (var kvp in reduction)
      {
        if (kvp.Value.MapResult.GetType() != indexDefinition.IndexType && kvp.Value.GetType().IsSubclassOf(indexDefinition.IndexType) == false)
          throw new InvalidOperationException("The return type of the index reduce function did not match the specified index type. " + indexDefinition.IndexType);
        dsIndex.Value.Items.Add(kvp);
      }

      return true;
    }
    #endregion Indexes

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
      if (ConfigurationManager.AppSettings.AllKeys.Any(k => k == "OrcaDB_RepositoryFactory") == false)
        throw new InvalidOperationException("A Repository Factory has not been defined in the app/web.config file. Please add an application setting named [OrcaDB_RepositoryFactory] which is the fully-qualified type name of the object that implements IRepositoryFactory");

      //Double-Check Locking Pattern
      if (s_repositoryFactory == null)
      {
        lock (s_syncRoot)
        {
          if (s_repositoryFactory == null)
          {
            string fullTypeName = ConfigurationManager.AppSettings["OrcaDB_RepositoryFactory"];
            if (string.IsNullOrEmpty(fullTypeName))
              throw new InvalidOperationException("A OrcaDB_RepositoryFactory key was specified within web.config, but it did not contain a value.");

            Type documentStoreType = Type.GetType(fullTypeName, true, true);
            if (documentStoreType.GetInterfaces().Any(i => i == typeof(IRepositoryFactory)) == false)
              throw new InvalidOperationException("The OrcaDB_RepositoryFactory type name was specified within web.config, but it does not implement IRepositoryFactory");

            s_repositoryFactory = Activator.CreateInstance(documentStoreType) as IRepositoryFactory;
          }
        }
      }

      return s_repositoryFactory;
    }

    /// <summary>
    /// Reads data from a stream until the end is reached. The
    /// data is returned as a byte array. An IOException is
    /// thrown if any of the underlying IO calls fail.
    /// </summary>
    /// <param name="stream">The stream to read data from</param>
    /// <param name="initialLength">The initial buffer length</param>
    public static byte[] ReadFully(Stream stream, int initialLength)
    {
      // If we've been passed an unhelpful initial length, just
      // use 32K.
      if (initialLength < 1)
      {
        initialLength = 32768;
      }

      byte[] buffer = new byte[initialLength];
      int read = 0;

      int chunk;
      while ((chunk = stream.Read(buffer, read, buffer.Length - read)) > 0)
      {
        read += chunk;

        // If we've reached the end of our buffer, check to see if there's
        // any more information
        if (read == buffer.Length)
        {
          int nextByte = stream.ReadByte();

          // End of stream? If so, we're done
          if (nextByte == -1)
          {
            return buffer;
          }

          // Nope. Resize the buffer, put in the byte we've just
          // read, and continue
          byte[] newBuffer = new byte[buffer.Length * 2];
          Array.Copy(buffer, newBuffer, buffer.Length);
          newBuffer[read] = (byte)nextByte;
          buffer = newBuffer;
          read++;
        }
      }
      // Buffer is now too big. Shrink it.
      byte[] ret = new byte[read];
      Array.Copy(buffer, ret, read);
      return ret;
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
