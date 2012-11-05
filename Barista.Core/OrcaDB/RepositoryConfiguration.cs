namespace OFS.OrcaDB.Core
{
  using System;
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  using System.Configuration;
  using System.Linq;

  public class RepositoryConfiguration
  {
    private static object m_syncRoot = new object();
    private IDocumentStore m_documentStore = null;

    public RepositoryConfiguration()
    {
      this.RegisteredEntityDefinitions = new EntityDefinitionCollection();
      this.RegisteredEntityMigrationStrategies = new EntityMigrationStrategyCollection();

      //Register Built-In Types
      RegisterEntity<ApplicationConfiguration>(Constants.ApplicationConfigurationV1Namespace);
      RegisterEntity<ApplicationLog>(Constants.ApplicationLogV1Namespace);
      RegisterEntity<Index>(Constants.IndexV1Namespace);
      RegisterEntity<Script>(Constants.ScriptV1Namespace);
    }

    public RepositoryConfiguration(IDocumentStore documentStore) : this()
    {
      m_documentStore = documentStore;
    }

    #region Properties
    /// <summary>
    /// Gets or sets the instance of the Document Store that the Repository will use.
    /// </summary>
    /// <remarks>
    /// The Document Store that the Repository instance will utilize can be defined either through this property or through a 'OrcaDB_DocumentStore' configuration element.
    /// </remarks>
    public IDocumentStore DocumentStore
    {
      get
      {
        //Double-Check Locking Pattern
        if (m_documentStore == null)
        {
          lock (m_syncRoot)
          {
            if (m_documentStore == null)
            {
              //If no document store is provided, use the document store defined in configuration.
              m_documentStore = InitializeDocumentStoreFromConfiguration();
            }
          }
        }

        return m_documentStore;
      }
      set
      {
        m_documentStore = value;
      }
    }

    /// <summary>
    /// Gets or sets the title of the container that will be associated with the Repository.
    /// </summary>
    public string ContainerTitle
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the internal collection of registered entity definitions.
    /// </summary>
    internal EntityDefinitionCollection RegisteredEntityDefinitions
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the internal collection of registered entity migration strategies.
    /// </summary>
    internal EntityMigrationStrategyCollection RegisteredEntityMigrationStrategies
    {
      get;
      set;
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Registers the specified entity type with the Repository, associating it with the specified namespace.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="namespace"></param>
    public RepositoryConfiguration RegisterEntity<T>(string @namespace)
    {
      RegisterEntity(@namespace, typeof(T));
      return this;
    }

    /// <summary>
    /// Registers the specified entity type with the Repository, associating it with the specified namespace.
    /// </summary>
    /// <param name="namespace"></param>
    /// <param name="type"></param>
    public RepositoryConfiguration RegisterEntity(string @namespace, Type type)
    {
      if (String.IsNullOrEmpty(@namespace))
        throw new ArgumentNullException("namespace", "A Namespace must be defined when registring an Entity Type.");

      if (type == null)
        throw new ArgumentNullException("type", "The Entity Type must be defined.");

      lock (m_syncRoot)
      {
        if (RegisteredEntityDefinitions.Any(ed => ed.EntityNamespace == @namespace))
          RegisteredEntityDefinitions.Remove(@namespace);

        RegisteredEntityDefinitions.Add(new EntityDefinition()
        {
          EntityNamespace = @namespace,
          EntityType = type,
        });
      }
      return this;
    }

    /// <summary>
    /// Registers and associates the Entity Part Type with the parent Entity Type, associating the entity part with the specified name.
    /// </summary>
    /// <typeparam name="TEntityType"></typeparam>
    /// <typeparam name="TEntityPartType"></typeparam>
    /// <param name="entityPartName"></param>
    public RepositoryConfiguration RegisterEntityPart<TEntityType, TEntityPartType>(string entityPartName)
    {
      RegisterEntityPart(typeof(TEntityType), entityPartName, typeof(TEntityPartType));
      return this;
    }

    /// <summary>
    /// Registers and associates the Entity Part Type with the parent Entity Type, associating the entity part with the specified name.
    /// </summary>
    /// <param name="parentEntityType"></param>
    /// <param name="entityPartName"></param>
    /// <param name="entityPartType"></param>
    public RepositoryConfiguration RegisterEntityPart(Type parentEntityType, string entityPartName, Type entityPartType)
    {
      if (parentEntityType == null)
        throw new ArgumentNullException("When registering an entity part, the parent entity type must be specified.");

      if (String.IsNullOrEmpty(entityPartName))
        throw new ArgumentNullException("An Entity Part Name must be specified.");

      if (entityPartType == null)
        throw new ArgumentNullException("When registering an entity part, the entity part type must be specified.");

      var parentEntityDefinition = RegisteredEntityDefinitions.Where(ed => ed.EntityType == parentEntityType).FirstOrDefault();

      if (parentEntityDefinition == null)
        throw new InvalidOperationException("The Parent Entities' type must first be registered with the Repository prior to being associated with an Entity Part.");

      lock (m_syncRoot)
      {
        if (parentEntityDefinition.EntityPartDefinitions.Any(p => p.EntityPartName == entityPartName))
          parentEntityDefinition.EntityPartDefinitions.Remove(entityPartName);

        //We've run the gauntlet, add the entity part definition.
        parentEntityDefinition.EntityPartDefinitions.Add(new EntityPartDefinition()
        {
          EntityPartName = entityPartName,
          EntityPartType = entityPartType,
        });
      }
      return this;
    }

    /// <summary>
    /// Registers an index that is updated when the specified entity type is updated
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TIndex"></typeparam>
    /// <param name="indexDefinition"></param>
    public RepositoryConfiguration RegisterIndex<TEntity, TIndex>(IndexDefinition indexDefinition)
    {
      var parentEntityDefinition = RegisteredEntityDefinitions.Where(ed => ed.EntityType == typeof(TEntity)).FirstOrDefault();

      if (parentEntityDefinition == null)
        throw new InvalidOperationException("The Entity Type type must first be registered with the Repository prior to being associated with an Index.");

      indexDefinition.IndexType = typeof(TIndex);

      lock (m_syncRoot)
      {
        //Replace any indexes with the same name.
        if (parentEntityDefinition.IndexDefinitions.Any(p => p.Name == indexDefinition.Name))
          parentEntityDefinition.IndexDefinitions.Remove(indexDefinition.Name);

        parentEntityDefinition.IndexDefinitions.Add(indexDefinition);
      }

      return this;
    }

    public RepositoryConfiguration RegisterEntityMigration<TDestinationEntityType>(EntityMigrationStrategy<TDestinationEntityType> entityMigrationDefinition)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Returns a value that indicates if the document store implements the specified interface.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public bool IsDocumentStore<T>()
    {
      return (this.DocumentStore is T);
    }

    /// <summary>
    /// Gets the instance of the document store which implements the specified interface.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T GetDocumentStore<T>()
    {
      if ((this.DocumentStore is T) == false)
      {
        throw new NotImplementedException(String.Format("The current Document Store does not implement the specified operation. ({0})", typeof(T)));
      }
      return (T)this.DocumentStore;
    }
    #endregion

    #region Protected Methods
    internal static bool SettingsDefinedInConfiguration
    {
      get
      {
        return ConfigurationManager.AppSettings.AllKeys.Any(k => k == "OrcaDB_DocumentStore");
      }
    }

    internal static IDocumentStore InitializeDocumentStoreFromConfiguration()
    {
      if (SettingsDefinedInConfiguration == false)
        throw new InvalidOperationException("The DocumentStore instance that the service will use must be defined in a 'OrcaDB_DocumentStore' value contained in the web.config");

      string fullTypeName = ConfigurationManager.AppSettings["OrcaDB_DocumentStore"];
      if (string.IsNullOrEmpty(fullTypeName))
        throw new InvalidOperationException("A OrcaDB_DocumentStore key was specified within web.config, but it did not contain a value.");

      Type documentStoreType = Type.GetType(fullTypeName, true, true);
      if (documentStoreType.GetInterfaces().Any(i => i == typeof(IDocumentStore)) == false)
        throw new InvalidOperationException("The OrcaDB_DocumentStore was specified within web.config, but it does not implement IDocumentStore");

      return Activator.CreateInstance(documentStoreType) as IDocumentStore;
    }
    #endregion
  }
}
