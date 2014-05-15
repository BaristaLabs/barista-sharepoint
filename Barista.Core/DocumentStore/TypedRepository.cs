namespace Barista.DocumentStore
{
  using System;
  using System.Collections.Generic;
  using System.Configuration;
  using System.IO;
  using System.Linq;

  /// <summary>
  /// Represents a class that abstracts retrieval of data from a document store.
  /// </summary>
  /// <remarks>
  /// This should be the main interface to use when interacting with the DocumentStore from the object model.
  /// 
  /// By using Repository/RepositoryConfiguration, the underlying document store implementation is abstracted, magic strings are reduced to a bare minimum, and common patterns are enforced and reused.
  /// 
  /// Further, some of the higher level functionality -- Automatic Migration, Indexing, are only available through the repository.
  /// 
  /// Follows the Unit-of-work pattern.
  /// </remarks>
  public class TypedRepository : IDisposable
  {
    #region Fields
    private readonly TypedRepositoryConfiguration m_configuration;
    private readonly object m_syncRoot = new object();
    private readonly static object StaticSyncRoot = new object();
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the configuration associated with the repository.
    /// </summary>
    public TypedRepositoryConfiguration Configuration
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
    public TypedRepository()
    {
      m_configuration = new TypedRepositoryConfiguration();
    }

    public TypedRepository(IDocumentStore documentStore)
    {
      m_configuration = new TypedRepositoryConfiguration(documentStore);
    }
    #endregion

    #region Configuration
    /// <summary>
    /// Registers the specified entity type with the Repository, associating it with the specified namespace.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="namespace"></param>
    public TypedRepositoryConfiguration RegisterEntity<T>(string @namespace)
    {
      return this.Configuration.RegisterEntity<T>(@namespace);
    }

    /// <summary>
    /// Registers the specified entity type with the Repository, associating it with the specified namespace.
    /// </summary>
    /// <param name="namespace"></param>
    /// <param name="type"></param>
    public TypedRepositoryConfiguration RegisterEntity(string @namespace, Type type)
    {
      return this.Configuration.RegisterEntity(@namespace, type);
    }

    /// <summary>
    /// Registers and associates the Entity Part Type with the parent Entity Type, associating the entity part with the specified name.
    /// </summary>
    /// <typeparam name="TEntityType"></typeparam>
    /// <typeparam name="TEntityPartType"></typeparam>
    /// <param name="entityPartName"></param>
    public TypedRepositoryConfiguration RegisterEntityPart<TEntityType, TEntityPartType>(string entityPartName)
    {
      return this.Configuration.RegisterEntityPart<TEntityType, TEntityPartType>(entityPartName);
    }

    /// <summary>
    /// Registers and associates the Entity Part Type with the parent Entity Type, associating the entity part with the specified name.
    /// </summary>
    /// <param name="parentEntityType"></param>
    /// <param name="entityPartName"></param>
    /// <param name="entityPartType"></param>
    public TypedRepositoryConfiguration RegisterEntityPart(Type parentEntityType, string entityPartName, Type entityPartType)
    {
      return this.Configuration.RegisterEntityPart(parentEntityType, entityPartName, entityPartType);
    }

    /// <summary>
    /// Defines a migration strategy that migrates entities of the 'from' namespace to the 'to' namespace using the defined strategy (func)
    /// </summary>
    /// <typeparam name="TDestinationEntityType"></typeparam>
    /// <param name="entityMigrationDefinition"></param>
    /// <returns></returns>
    public TypedRepositoryConfiguration DefineMigrationStrategy<TDestinationEntityType>(EntityMigrationStrategy<TDestinationEntityType> entityMigrationDefinition)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Registeres a synchronous entity trigger with the repository.
    /// </summary>
    /// <param name="namespacePattern"></param>
    /// <param name="trigger"></param>
    /// <returns></returns>
    public TypedRepositoryConfiguration RegisterSynchronousEntityTrigger(string namespacePattern, Action<TriggerProperties> trigger)
    {
      return this.Configuration.RegisterSynchronousEntityTrigger(namespacePattern, trigger);
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
    /// <typeparam name="T"></typeparam>
    /// <param name="title"></param>
    /// <param name="path"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public Entity<T> CreateEntity<T>(string title, string path, string data)
    {
      var entityDefinition = this.Configuration.RegisteredEntityDefinitions.FirstOrDefault(ed => ed.EntityType == typeof(T));

      if (entityDefinition == null)
        throw new InvalidOperationException("The specified entity type has not been registered with the repository. " + typeof(T));

      var documentStore = this.Configuration.GetDocumentStore<IFolderCapableDocumentStore>();
      var result = documentStore.CreateEntity(this.Configuration.ContainerTitle, path, title, entityDefinition.EntityNamespace, data);

      if (result == null)
        return null;

      var triggerProperties = new TriggerProperties
      {
        TriggerType = TriggerType.EntityCreated,
        EntityId = result.Id,
        Entity = result
      };

      foreach (var trigger in this.Configuration.GetEntityTriggersForNamespace(result.Namespace))
      {
        trigger(triggerProperties);
      }

      return new Entity<T>(result);
    }

    /// <summary>
    /// Creates a new entity in the repository and returns its value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="title"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public Entity<T> CreateEntity<T>(string title, T value)
    {
      var entityDefinition = this.Configuration.RegisteredEntityDefinitions.FirstOrDefault(ed => ed.EntityType == typeof(T));

      if (entityDefinition == null)
        throw new InvalidOperationException("The specified entity type has not been registered with the repository. " + typeof(T));

      var documentStore = this.Configuration.GetDocumentStore<IDocumentStore>();

      var json = DocumentStoreHelper.SerializeObjectToJson(value);
      var result = documentStore.CreateEntity(this.Configuration.ContainerTitle, title, entityDefinition.EntityNamespace, json);

      if (result == null)
        return null;

      var triggerProperties = new TriggerProperties
      {
        TriggerType = TriggerType.EntityCreated,
        EntityId = result.Id,
        Entity = result
      };

      foreach (var trigger in this.Configuration.GetEntityTriggersForNamespace(result.Namespace))
      {
        trigger(triggerProperties);
      }

      return new Entity<T>(result);
    }

    /// <summary>
    /// Creates a new entity in the repository and returns its value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="title"></param>
    /// <param name="path"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public Entity<T> CreateEntity<T>(string title, string path, T value)
    {
      var entityDefinition = this.Configuration.RegisteredEntityDefinitions.FirstOrDefault(ed => ed.EntityType == typeof(T));

      if (entityDefinition == null)
        throw new InvalidOperationException("The specified entity type has not been registered with the repository. " + typeof(T));

      var documentStore = this.Configuration.GetDocumentStore<IFolderCapableDocumentStore>();

      var json = DocumentStoreHelper.SerializeObjectToJson(value);
      var result = documentStore.CreateEntity(this.Configuration.ContainerTitle, path, title, entityDefinition.EntityNamespace, json);

      if (result == null)
        return null;

      var triggerProperties = new TriggerProperties
      {
        TriggerType = TriggerType.EntityCreated,
        EntityId = result.Id,
        Entity = result
      };

      foreach (var trigger in this.Configuration.GetEntityTriggersForNamespace(result.Namespace))
      {
        trigger(triggerProperties);
      }

      return new Entity<T>(result);
    }

    /// <summary>
    /// Creates a copy of the specified entity.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="entityId"></param>
    /// <param name="sourcePath"></param>
    /// <param name="targetPath"></param>
    /// <param name="newTitle"></param>
    /// <returns></returns>
    public Entity<T> CloneEntity<T>(Guid entityId, string sourcePath, string targetPath, string newTitle)
    {
      var entity = GetEntity<T>(entityId, sourcePath);
      var newEntity = CreateEntity<T>(newTitle, targetPath, entity.Data);

      if (this.Configuration.DocumentStore is IEntityPartCapableDocumentStore)
      {
        foreach (var entityPart in ListEntityParts<T>(entityId))
        {
          CreateEntityPart(newEntity.Id, entityPart.Name, entityPart.Category, entityPart.Data);
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

      return GetEntity<T>(newEntity.Id);
    }

    /// <summary>
    /// Gets the specifed entity of the specified type from the underlying repository.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="entityId"></param>
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
    public Entity<T> GetEntity<T>(Guid entityId, string path)
    {
      var entityDefinition = this.Configuration.RegisteredEntityDefinitions.FirstOrDefault(ed => ed.EntityType == typeof(T));

      if (entityDefinition == null)
        throw new InvalidOperationException("The specified entity type has not been registered with the repository. " + typeof(T));

      var documentStore = this.Configuration.GetDocumentStore<IFolderCapableDocumentStore>();
      var entity = documentStore.GetEntity(this.Configuration.ContainerTitle, entityId, path);

      if (entity == null)
        return null;

      return new Entity<T>(entity);
    }

    /// <summary>
    /// Gets the specifed entity of the specified type from the underlying repository, optionally restricting to a path.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="entityId"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public Entity<T> GetEntityLight<T>(Guid entityId, string path)
    {
      var entityDefinition = this.Configuration.RegisteredEntityDefinitions.FirstOrDefault(ed => ed.EntityType == typeof(T));

      if (entityDefinition == null)
        throw new InvalidOperationException("The specified entity type has not been registered with the repository. " + typeof(T));

      var documentStore = this.Configuration.GetDocumentStore<IFolderCapableDocumentStore>();
      var entity = documentStore.GetEntityLight(this.Configuration.ContainerTitle, entityId, path);

      if (entity == null)
        return null;

      return new Entity<T>(entity);
    }

    /// <summary>
    /// Returns an instance of an entity of the given type in the given path. If no entity exists, one is created. If more than one entity of the given type exists in the given path, an exception is thrown.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="path"></param>
    /// <param name="title"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public Entity<TEntity> GetOrCreateEntitySingleton<TEntity>(string path, string title, TEntity defaultValue)
    {
      var entityDefinition = this.Configuration.RegisteredEntityDefinitions.FirstOrDefault(ed => ed.EntityType == typeof(TEntity));

      if (entityDefinition == null)
        throw new InvalidOperationException("The specified entity type has not been registered with the repository. " + typeof(TEntity));

      var documentStore = this.Configuration.GetDocumentStore<IFolderCapableDocumentStore>();

      //Double-check locking pattern.
      var result = documentStore.ListEntities(this.Configuration.ContainerTitle, path, new EntityFilterCriteria { Namespace = entityDefinition.EntityNamespace, Top = 1 }).SingleOrDefault();
      if (result == null)
      {
        lock (StaticSyncRoot)
        {
          result = documentStore.ListEntities(this.Configuration.ContainerTitle, path, new EntityFilterCriteria { Namespace = entityDefinition.EntityNamespace, Top = 1 }).SingleOrDefault();

          if (result == null)
          {
            var json = DocumentStoreHelper.SerializeObjectToJson(defaultValue);
            result = documentStore.CreateEntity(this.Configuration.ContainerTitle, path, title, entityDefinition.EntityNamespace, json);
          }
        }
      }

      if (result == null)
        return null;

      return new Entity<TEntity>(result);
    }

    public Entity<TEntity> CreateOrUpdateEntitySingleton<TEntity>(string path, string title, string data)
    {
      TEntity entity = DocumentStoreHelper.DeserializeObjectFromJson<TEntity>(data);
      return CreateOrUpdateEntitySingleton(path, title, entity);
    }

    public Entity<TEntity> CreateOrUpdateEntitySingleton<TEntity>(string path, string title, TEntity entity)
    {
      var entityDefinition = this.Configuration.RegisteredEntityDefinitions.FirstOrDefault(ed => ed.EntityType == typeof(TEntity));

      if (entityDefinition == null)
        throw new InvalidOperationException("The specified entity type has not been registered with the repository. " + typeof(TEntity));

      var documentStore = this.Configuration.GetDocumentStore<IFolderCapableDocumentStore>();

      //Double-check locking pattern.
      var result = documentStore.ListEntities(this.Configuration.ContainerTitle, path, new EntityFilterCriteria { Namespace = entityDefinition.EntityNamespace, Top = 1 }).SingleOrDefault();
      if (result == null)
      {
        lock (StaticSyncRoot)
        {
          result = documentStore.ListEntities(this.Configuration.ContainerTitle, path, new EntityFilterCriteria { Namespace = entityDefinition.EntityNamespace, Top = 1 }).SingleOrDefault();

          if (result == null)
          {
            var json = DocumentStoreHelper.SerializeObjectToJson(entity);
            result = documentStore.CreateEntity(this.Configuration.ContainerTitle, path, title, entityDefinition.EntityNamespace, json);

            if (result != null)
            {
              var triggerProperties = new TriggerProperties
              {
                TriggerType = TriggerType.EntityCreated,
                EntityId = result.Id,
                Entity = result
              };

              foreach (var trigger in this.Configuration.GetEntityTriggersForNamespace(result.Namespace))
              {
                trigger(triggerProperties);
              }
            }
          }
          else
          {
            var documentStore2 = this.Configuration.GetDocumentStore<IDocumentStore>();
            var json = DocumentStoreHelper.SerializeObjectToJson(entity);
            documentStore2.UpdateEntityData(this.Configuration.ContainerTitle, result.Id, "", json);
            result = GetEntity<TEntity>(result.Id);
          }
        }
      }
      else
      {
        var documentStore2 = this.Configuration.GetDocumentStore<IDocumentStore>();
        lock (StaticSyncRoot)
        {
          var json = DocumentStoreHelper.SerializeObjectToJson(entity);
          documentStore2.UpdateEntityData(this.Configuration.ContainerTitle, result.Id, "", json);
          result = GetEntity<TEntity>(result.Id);
        }
      }

      if (result == null)
        return null;

      return new Entity<TEntity>(result);
    }

    public IList<Entity<TEntity>> ListEntities<TEntity>()
    {
      var entityDefinition = this.Configuration.RegisteredEntityDefinitions.FirstOrDefault(ed => ed.EntityType == typeof(TEntity));

      if (entityDefinition == null)
        throw new InvalidOperationException("The specified entity type has not been registered with the repository. " + typeof(TEntity));

      return ListEntities<TEntity>(new EntityFilterCriteria
        {
          Namespace = entityDefinition.EntityNamespace,
          NamespaceMatchType = NamespaceMatchType.Equals,
        });
    }

    /// <summary>
    /// Returns a collection of entities of the specified namespace contained in the specified path.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public IList<Entity<T>> ListEntities<T>(EntityFilterCriteria filterCriteria)
    {
      IList<Entity> result;
      if (filterCriteria.Path != null)
      {
        var documentStore = this.Configuration.GetDocumentStore<IFolderCapableDocumentStore>();
        result = documentStore.ListEntities(this.Configuration.ContainerTitle, filterCriteria.Path, filterCriteria);
      }
      else
      {
        var documentStore = this.Configuration.GetDocumentStore<IDocumentStore>();
        result = documentStore.ListEntities(this.Configuration.ContainerTitle, filterCriteria);
      }

      if (result == null)
        return null;

      return result.
        Select(e => new Entity<T>(e))
        .ToList();
    }

    /// <summary>
    /// Returns the total number of entities of the specified namespace contained in the specified path.
    /// </summary>
    public int CountEntities(EntityFilterCriteria filterCriteria)
    {
      int result;
      if (filterCriteria.Path == null)
      {
        var documentStore = this.Configuration.GetDocumentStore<IFolderCapableDocumentStore>();
        result = documentStore.CountEntities(this.Configuration.ContainerTitle, filterCriteria.Path, filterCriteria);
      }
      else
      {
        var documentStore = this.Configuration.GetDocumentStore<IDocumentStore>();
        result = documentStore.CountEntities(this.Configuration.ContainerTitle, filterCriteria);
      }

      return result;
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
    /// Gets the first entity of the specified type from the repository.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <returns></returns>
    public Entity<TEntity> GetFirstEntity<TEntity>(string path)
    {
      var entityDefinition = this.Configuration.RegisteredEntityDefinitions.FirstOrDefault(ed => ed.EntityType == typeof(TEntity));

      if (entityDefinition == null)
        throw new InvalidOperationException("The specified entity type has not been registered with the repository. " + typeof(TEntity));

      var client = this.Configuration.GetDocumentStore<IFolderCapableDocumentStore>();

      var entity = client.ListEntities(this.Configuration.ContainerTitle, path, new EntityFilterCriteria { Namespace = entityDefinition.EntityNamespace, Top = 1 }).FirstOrDefault();

      if (entity == null)
        return null;

      return new Entity<TEntity>(entity);
    }

    public Entity UpdateEntity(Guid entityId, string entityTitle, string entityDescription, string entityNamespace)
    {
      var documentStore = this.Configuration.GetDocumentStore<IDocumentStore>();

      var result = documentStore.UpdateEntity(this.Configuration.ContainerTitle, entityId, entityTitle,
                                                entityDescription, entityNamespace);

      var triggerProperties = new TriggerProperties
        {
          TriggerType = TriggerType.EntityUpdated,
          EntityId = result.Id,
          Entity = result
        };

      foreach (var trigger in this.Configuration.GetEntityTriggersForNamespace(result.Namespace))
      {
        trigger(triggerProperties);
      }

      return result;
    }

    /// <summary>
    /// Updates both the metadata and the data of the specified entity.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="entity"></param>
    /// <returns></returns>
    public Entity<TEntity> UpdateEntity<TEntity>(Entity<TEntity> entity)
    {
      var entityDefinition = this.Configuration.RegisteredEntityDefinitions.FirstOrDefault(ed => ed.EntityType == typeof(TEntity));

      if (entityDefinition == null)
        throw new InvalidOperationException("The specified entity type has not been registered with the repository. " + typeof(TEntity));

      var documentStore = this.Configuration.GetDocumentStore<IDocumentStore>();

      documentStore.UpdateEntity(this.Configuration.ContainerTitle, entity.Id, entity.Title,
                                 entity.Description, entity.Namespace);

      var json = DocumentStoreHelper.SerializeObjectToJson(entity.Value);

      //TODO: Fix ETag
      var updatedEntity = documentStore.UpdateEntityData(this.Configuration.ContainerTitle, entity.Id, String.Empty,
                                                         json);

      if (updatedEntity == null)
        return null;

      var triggerProperties = new TriggerProperties
        {
          TriggerType = TriggerType.EntityUpdated,
          EntityId = updatedEntity.Id,
          Entity = updatedEntity
        };

      foreach (var trigger in this.Configuration.GetEntityTriggersForNamespace(updatedEntity.Namespace))
      {
        trigger(triggerProperties);
      }

      return new Entity<TEntity>(updatedEntity);
    }

    public Entity<TEntity> UpdateEntityData<TEntity>(Guid entityId, TEntity value)
    {
      return UpdateEntityData(entityId, null, value);
    }

    public Entity<TEntity> UpdateEntityData<TEntity>(Guid entityId, string eTag, TEntity value)
    {
      var documentStore = this.Configuration.GetDocumentStore<IDocumentStore>();

      var json = DocumentStoreHelper.SerializeObjectToJson(value);

      var result = documentStore.UpdateEntityData(this.Configuration.ContainerTitle, entityId, eTag, json);

      if (result == null)
        return null;

      var triggerProperties = new TriggerProperties
        {
          TriggerType = TriggerType.EntityUpdated,
          EntityId = result.Id,
          Entity = result
        };

      foreach (var trigger in this.Configuration.GetEntityTriggersForNamespace(result.Namespace))
      {
        trigger(triggerProperties);
      }

      return new Entity<TEntity>(result);

    }

    /// <summary>
    /// Moves the specified entity to the specified destination path.
    /// </summary>
    /// <param name="entityId"></param>
    /// <param name="destinationPath"></param>
    /// <returns></returns>
    public bool MoveEntity(Guid entityId, string destinationPath)
    {
      var documentStore = this.Configuration.GetDocumentStore<IFolderCapableDocumentStore>();

      var result = documentStore.MoveEntity(this.Configuration.ContainerTitle, entityId, destinationPath);

      var movedEntity = documentStore.GetEntityLight(this.Configuration.ContainerTitle, entityId, destinationPath);

      var triggerProperties = new TriggerProperties
        {
          TriggerType = TriggerType.EntityMoved,
          EntityId = entityId,
          Entity = movedEntity
        };

      foreach (var trigger in this.Configuration.GetEntityTriggersForNamespace(movedEntity.Namespace))
      {
        trigger(triggerProperties);
      }
      return result;
    }

    /// <summary>
    /// Deletes the specified entity from the repository.
    /// </summary>
    /// <param name="entityId"></param>
    /// <returns></returns>
    public bool DeleteEntity(Guid entityId)
    {
      var documentStore = this.Configuration.GetDocumentStore<IDocumentStore>();

      var entityToBeDeleted = documentStore.GetEntityLight(this.Configuration.ContainerTitle, entityId);

      if (entityToBeDeleted != null)
      {
        var triggerProperties = new TriggerProperties
          {
            TriggerType = TriggerType.EntityDeleting,
            EntityId = entityId,
            Entity = entityToBeDeleted
          };

        foreach (var trigger in this.Configuration.GetEntityTriggersForNamespace(entityToBeDeleted.Namespace))
        {
          trigger(triggerProperties);
        }
      }

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
    /// <returns></returns>
    public Comment AddEntityComment(Guid entityId, string comment)
    {
      var documentStore = this.Configuration.GetDocumentStore<ICommentCapableDocumentStore>();
      var result = documentStore.AddEntityComment(this.Configuration.ContainerTitle, entityId, comment);

      var ds1 = this.Configuration.GetDocumentStore<IDocumentStore>();
      var entity = ds1.GetEntityLight(this.Configuration.ContainerTitle, entityId);

      var triggerProperties = new TriggerProperties
      {
        TriggerType = TriggerType.EntityCommentAdded,
        EntityId = entityId,
        Entity = entity
      };

      foreach (var trigger in this.Configuration.GetEntityTriggersForNamespace(entity.Namespace))
      {
        trigger(triggerProperties);
      }

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

    public IList<Comment> ListEntityComments(Guid entityId, string path)
    {
        var documentStore = this.Configuration.GetDocumentStore<ICommentCapableDocumentStore>();
        return documentStore.ListEntityComments(this.Configuration.ContainerTitle, entityId, path);
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
      var entityDefinition = this.Configuration.RegisteredEntityDefinitions.FirstOrDefault(ed => ed.EntityType == typeof(TEntity));

      if (entityDefinition == null)
        throw new InvalidOperationException("The specified entity type has not been registered with the repository. " + typeof(TEntity));

      var entityPartDefinition = entityDefinition.EntityPartDefinitions.FirstOrDefault(pd => pd.EntityPartType == typeof(TEntityPart));

      if (entityPartDefinition == null)
        throw new InvalidOperationException("The specified Entity Part Type has not been registered with the repository. " + typeof(TEntityPart));

      var entityParts = ListEntityParts<TEntity>(entityId);
      return entityParts.Any(ep => ep.Name == entityPartDefinition.EntityPartName);
    }


    public EntityPart CreateEntityPart(Guid entityId, string partName, string category, string data)
    {
      var documentStore = this.Configuration.GetDocumentStore<IEntityPartCapableDocumentStore>();

      var result = documentStore.CreateEntityPart(this.Configuration.ContainerTitle, entityId, partName, category, data);

      var ds1 = this.Configuration.GetDocumentStore<IDocumentStore>();
      var entity = ds1.GetEntityLight(this.Configuration.ContainerTitle, entityId);

      var triggerProperties = new TriggerProperties
      {
        TriggerType = TriggerType.EntityPartCreated,
        EntityId = entityId,
        Entity = entity,
        EntityPart = result
      };

      foreach (var trigger in this.Configuration.GetEntityTriggersForNamespace(entity.Namespace))
      {
        trigger(triggerProperties);
      }

      foreach (var trigger in this.Configuration.GetEntityPartTriggersForNamespace(entity.Namespace))
      {
        trigger(triggerProperties);
      }

      return result;
    }

    /// <summary>
    /// Creates a new entity part with the specified name, category and data that is associated with the specified entity.
    /// </summary>
    /// <typeparam name="TEntityPart"></typeparam>
    /// <param name="entityId"></param>
    /// <param name="partName"></param>
    /// <param name="category"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public EntityPart<TEntityPart> CreateEntityPart<TEntityPart>(Guid entityId, string partName, string category, TEntityPart value)
    {
      var documentStore = this.Configuration.GetDocumentStore<IEntityPartCapableDocumentStore>();

      var json = DocumentStoreHelper.SerializeObjectToJson(value);
      var result = documentStore.CreateEntityPart(this.Configuration.ContainerTitle, entityId, partName, category, json);

      if (result == null)
        return null;

      var ds1 = this.Configuration.GetDocumentStore<IDocumentStore>();
      var entity = ds1.GetEntityLight(this.Configuration.ContainerTitle, entityId);

      var triggerProperties = new TriggerProperties
      {
        TriggerType = TriggerType.EntityPartCreated,
        EntityId = entityId,
        Entity = entity,
        EntityPart = result,
        EntityPartName = partName
      };

      foreach (var trigger in this.Configuration.GetEntityTriggersForNamespace(entity.Namespace))
      {
        trigger(triggerProperties);
      }

      foreach (var trigger in this.Configuration.GetEntityPartTriggersForNamespace(entity.Namespace))
      {
        trigger(triggerProperties);
      }

      return new EntityPart<TEntityPart>(result);
    }

    public EntityPart<TEntityPart> CreateEntityPart<TEntity, TEntityPart>(Guid entityId, string data)
    {
      var entityDefinition = this.Configuration.RegisteredEntityDefinitions.FirstOrDefault(ed => ed.EntityType == typeof(TEntity));

      if (entityDefinition == null)
        throw new InvalidOperationException("The specified entity type has not been registered with the repository. " + typeof(TEntity));

      var entityPartDefinition = entityDefinition.EntityPartDefinitions.FirstOrDefault(pd => pd.EntityPartType == typeof(TEntityPart));

      if (entityPartDefinition == null)
        throw new InvalidOperationException("The specified Entity Part Type has not been registered with the repository. " + typeof(TEntityPart));

      var value = DocumentStoreHelper.DeserializeObjectFromJson<TEntityPart>(data);

      var documentStore = this.Configuration.GetDocumentStore<IEntityPartCapableDocumentStore>();

      var json = DocumentStoreHelper.SerializeObjectToJson(value);
      var result = documentStore.CreateEntityPart(this.Configuration.ContainerTitle, entityId, entityPartDefinition.EntityPartName, "", json);

      if (result == null)
        return null;

      var ds1 = this.Configuration.GetDocumentStore<IDocumentStore>();
      var entity = ds1.GetEntityLight(this.Configuration.ContainerTitle, entityId);

      var triggerProperties = new TriggerProperties
      {
        TriggerType = TriggerType.EntityPartCreated,
        EntityId = entityId,
        Entity = entity,
        EntityPart = result,
        EntityPartName = entityPartDefinition.EntityPartName,
      };

      foreach (var trigger in this.Configuration.GetEntityTriggersForNamespace(entity.Namespace))
      {
        trigger(triggerProperties);
      }

      foreach (var trigger in this.Configuration.GetEntityPartTriggersForNamespace(entity.Namespace))
      {
        trigger(triggerProperties);
      }

      return new EntityPart<TEntityPart>(result);
    }

    /// <summary>
    /// Creates an entity part of the specified type associated with the entity with the specified id.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TEntityPart"></typeparam>
    /// <param name="entityId"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public EntityPart<TEntityPart> CreateEntityPart<TEntity, TEntityPart>(Guid entityId, TEntityPart value)
    {
      var entityDefinition = this.Configuration.RegisteredEntityDefinitions.FirstOrDefault(ed => ed.EntityType == typeof(TEntity));

      if (entityDefinition == null)
        throw new InvalidOperationException("The specified entity type has not been registered with the repository. " + typeof(TEntity));

      var entityPartDefinition = entityDefinition.EntityPartDefinitions.FirstOrDefault(pd => pd.EntityPartType == typeof(TEntityPart));

      if (entityPartDefinition == null)
        throw new InvalidOperationException("The specified Entity Part Type has not been registered with the repository. " + typeof(TEntityPart));

      var documentStore = this.Configuration.GetDocumentStore<IEntityPartCapableDocumentStore>();

      var json = DocumentStoreHelper.SerializeObjectToJson(value);
      var result = documentStore.CreateEntityPart(this.Configuration.ContainerTitle, entityId, entityPartDefinition.EntityPartName, "", json);

      if (result == null)
        return null;

      var ds1 = this.Configuration.GetDocumentStore<IDocumentStore>();
      var entity = ds1.GetEntityLight(this.Configuration.ContainerTitle, entityId);

      var triggerProperties = new TriggerProperties
      {
        TriggerType = TriggerType.EntityPartCreated,
        EntityId = entityId,
        Entity = entity,
        EntityPart = result,
        EntityPartName = entityPartDefinition.EntityPartName
      };

      foreach (var trigger in this.Configuration.GetEntityTriggersForNamespace(entity.Namespace))
      {
        trigger(triggerProperties);
      }

      foreach (var trigger in this.Configuration.GetEntityPartTriggersForNamespace(entity.Namespace))
      {
        trigger(triggerProperties);
      }

      return new EntityPart<TEntityPart>(result);
    }

    /// <summary>
    /// Returns the entity part associated with the specified entity.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TEntityPart"></typeparam>
    /// <param name="entityId"></param>
    /// <param name="partName"></param>
    /// <returns></returns>
    public EntityPart<TEntityPart> GetEntityPart<TEntity, TEntityPart>(Guid entityId, string partName)
    {
      var entityDefinition = this.Configuration.RegisteredEntityDefinitions.FirstOrDefault(ed => ed.EntityType == typeof(TEntity));

      if (entityDefinition == null)
        throw new InvalidOperationException("The specified entity type has not been registered with the repository. " + typeof(TEntity));

      var documentStore = this.Configuration.GetDocumentStore<IEntityPartCapableDocumentStore>();

      var entityPart = documentStore.GetEntityPart(this.Configuration.ContainerTitle, entityId, partName);

      if (entityPart == null)
        return null;

      return new EntityPart<TEntityPart>(entityPart);
    }

    /// <summary>
    /// Returns the entity part associated with the specified entity.
    /// </summary>
    /// <param name="entityId"></param>
    /// <returns></returns>
    public EntityPart<TEntityPart> GetEntityPart<TEntity, TEntityPart>(Guid entityId)
    {
      var entityDefinition = this.Configuration.RegisteredEntityDefinitions.FirstOrDefault(ed => ed.EntityType == typeof(TEntity));

      if (entityDefinition == null)
        throw new InvalidOperationException("The specified entity type has not been registered with the repository. " + typeof(TEntity));

      var entityPartDefinition = entityDefinition.EntityPartDefinitions.FirstOrDefault(pd => pd.EntityPartType == typeof(TEntityPart));

      if (entityPartDefinition == null)
        throw new InvalidOperationException("The specified Entity Part Type has not been registered with the repository. " + typeof(TEntityPart));

      var documentStore = this.Configuration.GetDocumentStore<IEntityPartCapableDocumentStore>();

      var entityPart = documentStore.GetEntityPart(this.Configuration.ContainerTitle, entityId, entityPartDefinition.EntityPartName);

      if (entityPart == null)
        return null;

      return new EntityPart<TEntityPart>(entityPart);
    }

    public bool TryGetEntityPart<TEntity, TEntityPart>(Guid entityId, out EntityPart<TEntityPart> entityPart)
    {
      var entityDefinition = this.Configuration.RegisteredEntityDefinitions.FirstOrDefault(ed => ed.EntityType == typeof(TEntity));

      if (entityDefinition == null)
        throw new InvalidOperationException("The specified entity type has not been registered with the repository. " + typeof(TEntity));

      var entityPartDefinition = entityDefinition.EntityPartDefinitions.FirstOrDefault(pd => pd.EntityPartType == typeof(TEntityPart));

      if (entityPartDefinition == null)
        throw new InvalidOperationException("The specified Entity Part Type has not been registered with the repository. " + typeof(TEntityPart));

      var documentStore = this.Configuration.GetDocumentStore<IEntityPartCapableDocumentStore>();

      var result = documentStore.GetEntityPart(this.Configuration.ContainerTitle, entityId,
                                         entityPartDefinition.EntityPartName);

      if (result == null)
      {
        entityPart = null;
        return false;
      }

      entityPart = new EntityPart<TEntityPart>(result);
      return true;
    }

    public EntityPart UpdateEntityPart(Guid entityId, string partName, string category)
    {
      var documentStore = this.Configuration.GetDocumentStore<IEntityPartCapableDocumentStore>();

      var result = documentStore.UpdateEntityPart(this.Configuration.ContainerTitle, entityId, partName, category);

      if (result == null)
        return null;

      var ds1 = this.Configuration.GetDocumentStore<IDocumentStore>();
      var entity = ds1.GetEntityLight(this.Configuration.ContainerTitle, entityId);

      var triggerProperties = new TriggerProperties
        {
          TriggerType = TriggerType.EntityPartUpdated,
          EntityId = entityId,
          Entity = entity,
          EntityPart = result,
          EntityPartName = partName,
        };

      foreach (var trigger in this.Configuration.GetEntityTriggersForNamespace(entity.Namespace))
      {
        trigger(triggerProperties);
      }

      foreach (var trigger in this.Configuration.GetEntityPartTriggersForNamespace(entity.Namespace))
      {
        trigger(triggerProperties);
      }

      return result;
    }

    public EntityPart<TEntityPart> UpdateEntityPart<TEntity, TEntityPart>(Guid entityId, string category)
    {
      var entityDefinition = this.Configuration.RegisteredEntityDefinitions.FirstOrDefault(ed => ed.EntityType == typeof(TEntity));

      if (entityDefinition == null)
        throw new InvalidOperationException("The specified Entity Type has not been registered with the repository. " + typeof(TEntity));

      var entityPartDefinition = entityDefinition.EntityPartDefinitions.FirstOrDefault(pd => pd.EntityPartType == typeof(TEntityPart));

      if (entityPartDefinition == null)
        throw new InvalidOperationException("The specified Entity Part Type has not been registered with the repository. " + typeof(TEntityPart));

      var documentStore = this.Configuration.GetDocumentStore<IEntityPartCapableDocumentStore>();

      var result = documentStore.UpdateEntityPart(this.Configuration.ContainerTitle, entityId,
                                                  entityPartDefinition.EntityPartName, category);

      if (result == null)
        return null;

      var ds1 = this.Configuration.GetDocumentStore<IDocumentStore>();
      var entity = ds1.GetEntityLight(this.Configuration.ContainerTitle, entityId);

      var triggerProperties = new TriggerProperties
        {
          TriggerType = TriggerType.EntityPartUpdated,
          EntityId = entityId,
          Entity = entity,
          EntityPart = result,
          EntityPartName = entityPartDefinition.EntityPartName
        };

      foreach (var trigger in this.Configuration.GetEntityTriggersForNamespace(entity.Namespace))
      {
        trigger(triggerProperties);
      }

      foreach (var trigger in this.Configuration.GetEntityPartTriggersForNamespace(entity.Namespace))
      {
        trigger(triggerProperties);
      }

      return new EntityPart<TEntityPart>(result);
    }

    /// <summary>
    /// Updates the entity part associated with the specified entity.
    /// </summary>
    /// <param name="entityId"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public EntityPart<TEntityPart> UpdateEntityPartData<TEntity, TEntityPart>(Guid entityId, TEntityPart value)
    {
      return UpdateEntityPartData<TEntity, TEntityPart>(entityId, null, value);
    }

    public EntityPart<TEntityPart> UpdateEntityPartData<TEntity, TEntityPart>(Guid entityId, string eTag, TEntityPart value)
    {
      var entityDefinition = this.Configuration.RegisteredEntityDefinitions.FirstOrDefault(ed => ed.EntityType == typeof(TEntity));

      if (entityDefinition == null)
        throw new InvalidOperationException("The specified Entity Type has not been registered with the repository. " + typeof(TEntity));

      var entityPartDefinition = entityDefinition.EntityPartDefinitions.FirstOrDefault(pd => pd.EntityPartType == typeof(TEntityPart));

      if (entityPartDefinition == null)
        throw new InvalidOperationException("The specified Entity Part Type has not been registered with the repository. " + typeof(TEntityPart));

      var documentStore = this.Configuration.GetDocumentStore<IEntityPartCapableDocumentStore>();

      var json = DocumentStoreHelper.SerializeObjectToJson(value);

      var result = documentStore.UpdateEntityPartData(this.Configuration.ContainerTitle, entityId,
                                                      entityPartDefinition.EntityPartName, eTag, json);

      if (result == null)
        return null;

      var ds1 = this.Configuration.GetDocumentStore<IDocumentStore>();
      var entity = ds1.GetEntityLight(this.Configuration.ContainerTitle, entityId);

      var triggerProperties = new TriggerProperties
        {
          TriggerType = TriggerType.EntityPartUpdated,
          EntityId = entityId,
          Entity = entity,
          EntityPart = result,
          EntityPartName = entityPartDefinition.EntityPartName
        };

      foreach (var trigger in this.Configuration.GetEntityTriggersForNamespace(entity.Namespace))
      {
        trigger(triggerProperties);
      }

      foreach (var trigger in this.Configuration.GetEntityPartTriggersForNamespace(entity.Namespace))
      {
        trigger(triggerProperties);
      }

      return new EntityPart<TEntityPart>(result);
    }

    /// <summary>
    /// Updates the entity part associated with the specified entity.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TEntityPart"></typeparam>
    /// <param name="entityId"></param>
    /// <param name="entityPartName"></param>
    /// <param name="eTag"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public EntityPart<TEntityPart> UpdateEntityPartData<TEntity, TEntityPart>(Guid entityId, string entityPartName, string eTag, TEntityPart value)
    {
      var entityDefinition = this.Configuration.RegisteredEntityDefinitions.FirstOrDefault(ed => ed.EntityType == typeof(TEntity));

      if (entityDefinition == null)
        throw new InvalidOperationException("The specified Entity Type has not been registered with the repository. " + typeof(TEntity));

      var documentStore = this.Configuration.GetDocumentStore<IEntityPartCapableDocumentStore>();
      var json = DocumentStoreHelper.SerializeObjectToJson(value);

      var result = documentStore.UpdateEntityPartData(this.Configuration.ContainerTitle, entityId, entityPartName,
                                                      eTag, json);

      if (result == null)
        return null;

      var ds1 = this.Configuration.GetDocumentStore<IDocumentStore>();
      var entity = ds1.GetEntityLight(this.Configuration.ContainerTitle, entityId);

      var triggerProperties = new TriggerProperties
        {
          TriggerType = TriggerType.EntityPartUpdated,
          EntityId = entityId,
          Entity = entity,
          EntityPart = result,
          EntityPartName = entityPartName
        };

      foreach (var trigger in this.Configuration.GetEntityTriggersForNamespace(entity.Namespace))
      {
        trigger(triggerProperties);
      }

      foreach (var trigger in this.Configuration.GetEntityPartTriggersForNamespace(entity.Namespace))
      {
        trigger(triggerProperties);
      }

      return new EntityPart<TEntityPart>(result);
    }

    /// <summary>
    /// Lists the entity parts associated with the specified entity.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="entityId"></param>
    /// <returns></returns>
    public IList<EntityPart> ListEntityParts<TEntity>(Guid entityId)
    {
      var entityDefinition = this.Configuration.RegisteredEntityDefinitions.FirstOrDefault(ed => ed.EntityType == typeof(TEntity));

      if (entityDefinition == null)
        throw new InvalidOperationException("The specified entity type has not been registered with the repository. " + typeof(TEntity));

      var documentStore = this.Configuration.GetDocumentStore<IEntityPartCapableDocumentStore>();

      return documentStore.ListEntityParts(this.Configuration.ContainerTitle, entityId);
    }

    /// <summary>
    /// Lists the entity parts associated with the specified entity.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="entityId"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public IList<EntityPart> ListEntityParts<TEntity>(Guid entityId, string path)
    {
      var entityDefinition = this.Configuration.RegisteredEntityDefinitions.FirstOrDefault(ed => ed.EntityType == typeof(TEntity));

      if (entityDefinition == null)
        throw new InvalidOperationException("The specified entity type has not been registered with the repository. " + typeof(TEntity));

      var documentStore = this.Configuration.GetDocumentStore<IEntityPartFolderCapableDocumentStore>();

      return documentStore.ListEntityParts(this.Configuration.ContainerTitle, path, entityId);
    }

    /// <summary>
    /// Returns the specified entity part associated with the first entity in the repository.
    /// </summary>
    /// <returns></returns>
    public EntityPart<TEntityPart> GetFirstEntityPart<TEntity, TEntityPart>()
    {
      var entityDefinition = this.Configuration.RegisteredEntityDefinitions.FirstOrDefault(ed => ed.EntityType == typeof(TEntity));

      if (entityDefinition == null)
        throw new InvalidOperationException("The specified entity type has not been registered with the repository. " + typeof(TEntity));

      var entityPartDefinition = entityDefinition.EntityPartDefinitions.FirstOrDefault(pd => pd.EntityPartType == typeof(TEntityPart));

      if (entityPartDefinition == null)
        throw new InvalidOperationException("The specified Entity Part Type has not been registered with the repository. " + typeof(TEntityPart));

      var firstEntity = this.Configuration.DocumentStore.ListEntities(this.Configuration.ContainerTitle, new EntityFilterCriteria { Namespace = entityDefinition.EntityNamespace, Top = 1 }).FirstOrDefault();

      if (firstEntity == null)
        return null;

      var documentStore = this.Configuration.GetDocumentStore<IEntityPartCapableDocumentStore>();
      var entityPart = documentStore.GetEntityPart(this.Configuration.ContainerTitle, firstEntity.Id, entityPartDefinition.EntityPartName);

      if (entityPart == null)
        return null;

      return new EntityPart<TEntityPart>(entityPart);
    }

    /// <summary>
    /// Deletes the specified Entity Part associated with the specified entity.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TEntityPart"></typeparam>
    /// <param name="entityId"></param>
    /// <returns></returns>
    public bool DeleteEntityPart<TEntity, TEntityPart>(Guid entityId)
    {
      var entityDefinition = this.Configuration.RegisteredEntityDefinitions.FirstOrDefault(ed => ed.EntityType == typeof(TEntity));

      if (entityDefinition == null)
        throw new InvalidOperationException("The specified entity type has not been registered with the repository. " + typeof(TEntity));

      var entityPartDefinition = entityDefinition.EntityPartDefinitions.FirstOrDefault(pd => pd.EntityPartType == typeof(TEntityPart));

      if (entityPartDefinition == null)
        throw new InvalidOperationException("The specified Entity Part Type has not been registered with the repository. " + typeof(TEntityPart));

      var documentStore = this.Configuration.GetDocumentStore<IEntityPartCapableDocumentStore>();

      var result = documentStore.DeleteEntityPart(this.Configuration.ContainerTitle, entityId, entityPartDefinition.EntityPartName);

      var ds1 = this.Configuration.GetDocumentStore<IDocumentStore>();
      var entity = ds1.GetEntityLight(this.Configuration.ContainerTitle, entityId);

      var triggerProperties = new TriggerProperties
      {
        TriggerType = TriggerType.EntityPartDeleted,
        EntityId = entityId,
        Entity = entity,
        EntityPartName = entityPartDefinition.EntityPartName
      };

      foreach (var trigger in this.Configuration.GetEntityTriggersForNamespace(entity.Namespace))
      {
        trigger(triggerProperties);
      }

      foreach (var trigger in this.Configuration.GetEntityPartTriggersForNamespace(entity.Namespace))
      {
        trigger(triggerProperties);
      }

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
      var result = documentStore.UploadAttachment(this.Configuration.ContainerTitle, entityId, fileName, attachment);

      if (result == null)
        return null;

      var ds1 = this.Configuration.GetDocumentStore<IDocumentStore>();
      var entity = ds1.GetEntityLight(this.Configuration.ContainerTitle, entityId);

      var triggerProperties = new TriggerProperties
      {
        TriggerType = TriggerType.AttachmentUploaded,
        EntityId = entityId,
        Entity = entity,
        Attachment = result,
        AttachmentFileName = fileName
      };

      foreach (var trigger in this.Configuration.GetEntityTriggersForNamespace(entity.Namespace))
      {
        trigger(triggerProperties);
      }

      return result;
    }

    public Stream DownloadAttachment(Guid entityId, string fileName)
    {
      var documentStore = this.Configuration.GetDocumentStore<IAttachmentCapableDocumentStore>();

      return documentStore.DownloadAttachment(this.Configuration.ContainerTitle, entityId, fileName);
    }

    public bool DeleteAttachment(Guid entityId, string fileName)
    {
      var documentStore = this.Configuration.GetDocumentStore<IAttachmentCapableDocumentStore>();

      var result = documentStore.DeleteAttachment(this.Configuration.ContainerTitle, entityId, fileName);

      var ds1 = this.Configuration.GetDocumentStore<IDocumentStore>();
      var entity = ds1.GetEntityLight(this.Configuration.ContainerTitle, entityId);

      var triggerProperties = new TriggerProperties
      {
        TriggerType = TriggerType.AttachmentDeleted,
        EntityId = entityId,
        Entity = entity,
        AttachmentFileName = fileName
      };

      foreach (var trigger in this.Configuration.GetEntityTriggersForNamespace(entity.Namespace))
      {
        trigger(triggerProperties);
      }

      return result;
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

    #endregion

    #region Static Methods
    private static readonly object SyncRoot = new object();

    [ThreadStatic]
    private static volatile IRepositoryFactory s_repositoryFactory;

    [ThreadStatic]
    private static volatile IDocumentStore s_documentStore;

    public static IRepositoryFactory RepositoryFactory
    {
      get { return s_repositoryFactory; }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        //Double-Check Locking Pattern
        if (s_repositoryFactory != null)
        {
          lock (SyncRoot)
          {
            if (s_repositoryFactory != null)
            {
              throw new InvalidOperationException(
                "The Repository has already been initialized. Please attempt to set the repository factory before the repository has been used.");
            }
          }
        }

        if (s_repositoryFactory == null)
        {
          lock (SyncRoot)
          {
            if (s_repositoryFactory == null)
            {
              s_repositoryFactory = value;
            }
          }
        }
      }
    }

    public static IDocumentStore DocumentStore
    {
      get { return s_documentStore; }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        //Double-Check Locking Pattern
        if (s_documentStore != null)
        {
          lock (SyncRoot)
          {
            if (s_documentStore != null)
            {
              throw new InvalidOperationException(
                "The Document Store has already been initialized. Please attempt to set the document store before the repository has been used.");
            }
          }
        }

        if (s_documentStore == null)
        {
          lock (SyncRoot)
          {
            if (s_documentStore == null)
            {
              s_documentStore = value;
            }
          }
        }
      }
    }

    /// <summary>
    /// Using the Repository Factory defined in configuration, returns a repository object.
    /// </summary>
    /// <returns></returns>
    public static TypedRepository GetRepository()
    {
      return GetRepository(s_repositoryFactory, s_documentStore);
    }

    /// <summary>
    /// Using the specified repository factory, returns a repository object.
    /// </summary>
    /// <param name="factory"></param>
    /// <returns></returns>
    public static TypedRepository GetRepository(IRepositoryFactory factory)
    {
      return GetRepository(factory, s_documentStore);
    }

    /// <summary>
    /// Using the specified repository factory, returns a repository object.
    /// </summary>
    /// <param name="factory"></param>
    /// <param name="documentStore"></param>
    /// <returns></returns>
    public static TypedRepository GetRepository(IRepositoryFactory factory, IDocumentStore documentStore)
    {
      if (factory == null)
        factory = GetRepositoryFactoryFromConfiguration();

      TypedRepository repository = (documentStore == null ? factory.CreateRepository() : factory.CreateRepository(documentStore)) as TypedRepository;

      if (repository == null)
        throw new InvalidOperationException("The repository object returned by the factory (" + factory.GetType() + ") was null.");

      repository.Factory = factory;

      return repository;
    }

    private static IRepositoryFactory GetRepositoryFactoryFromConfiguration()
    {
      if (ConfigurationManager.AppSettings.AllKeys.Any(k => k == "BaristaDS_RepositoryFactory") == false)
        throw new InvalidOperationException("A Repository Factory has not been defined in the app/web.config file. Please add an application setting named [BaristaDS_RepositoryFactory] which is the fully-qualified type name of the object that implements IRepositoryFactory");

      //Double-Check Locking Pattern
      if (s_repositoryFactory == null)
      {
        lock (SyncRoot)
        {
          if (s_repositoryFactory == null)
          {
            string fullTypeName = ConfigurationManager.AppSettings["BaristaDS_RepositoryFactory"];
            if (string.IsNullOrEmpty(fullTypeName))
              throw new InvalidOperationException("A BaristaDS_RepositoryFactory key was specified within web.config, but it did not contain a value.");

            Type documentStoreType = Type.GetType(fullTypeName, true, true);
            if (documentStoreType != null && documentStoreType.GetInterfaces().Any(i => i == typeof(IRepositoryFactory)) == false)
              throw new InvalidOperationException("The BaristaDS_RepositoryFactory type name was specified within web.config, but it does not implement IRepositoryFactory");

            if (documentStoreType == null)
              throw new InvalidOperationException("The BaristaDS_RepositoryFactory type name was specified within web.config, but was null");

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
