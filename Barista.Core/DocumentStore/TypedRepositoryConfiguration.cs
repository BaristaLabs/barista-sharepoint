namespace Barista.DocumentStore
{
  using System;
  using System.Collections.Generic;
  using System.Configuration;
  using System.Linq;
  using Barista.Extensions;
  using System.Text.RegularExpressions;

  public class TypedRepositoryConfiguration
  {
    private static readonly object SyncRoot = new object();
    private volatile IDocumentStore m_documentStore;

    public TypedRepositoryConfiguration()
    {
      this.RegisteredEntityDefinitions = new EntityDefinitionCollection();
      this.RegisteredEntityMigrationStrategies = new EntityMigrationStrategyCollection();
      this.RegisteredSynchronousEntityTriggers = new Dictionary<string, Action<TriggerProperties>>();
      this.RegisteredSynchronousEntityPartTriggers = new Dictionary<string, Action<TriggerProperties>>();

      //Register Built-In Types
      RegisterEntity<ApplicationConfiguration>(Constants.ApplicationConfigurationV1Namespace);
      RegisterEntity<ApplicationLog>(Constants.ApplicationLogV1Namespace);
      RegisterEntity<Script>(Constants.ScriptV1Namespace);
    }

    public TypedRepositoryConfiguration(IDocumentStore documentStore)
      : this()
    {
      m_documentStore = documentStore;
    }

    #region Properties
    /// <summary>
    /// Gets or sets the instance of the Document Store that the Repository will use.
    /// </summary>
    /// <remarks>
    /// The Document Store that the Repository instance will utilize can be defined either through this property or through a 'BaristaDS_DocumentStore' configuration element.
    /// </remarks>
    public IDocumentStore DocumentStore
    {
      get
      {
        //Double-Check Locking Pattern
        if (m_documentStore == null)
        {
          lock (SyncRoot)
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

    /// <summary>
    /// Gets or sets the interal collection of registered synchronous entity triggers.
    /// </summary>
    internal IDictionary<string, Action<TriggerProperties>> RegisteredSynchronousEntityTriggers
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the interal collection of registered synchronous entity part triggers.
    /// </summary>
    internal IDictionary<string, Action<TriggerProperties>> RegisteredSynchronousEntityPartTriggers
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
    public TypedRepositoryConfiguration RegisterEntity<T>(string @namespace)
    {
      RegisterEntity(@namespace, typeof(T));
      return this;
    }

    /// <summary>
    /// Registers the specified entity type with the Repository, associating it with the specified namespace.
    /// </summary>
    /// <param name="namespace"></param>
    /// <param name="type"></param>
    public TypedRepositoryConfiguration RegisterEntity(string @namespace, Type type)
    {
      if (String.IsNullOrEmpty(@namespace))
        throw new ArgumentNullException("namespace", @"A Namespace must be defined when registring an Entity Type.");

      //Validate the namespace parameter.
      Uri entityNamespaceUri;
      if (Uri.TryCreate(@namespace, UriKind.Absolute, out entityNamespaceUri) == false)
        throw new ArgumentException("The Namespace parameter must conform to a valid absolute Uri.");

      if (type == null)
        throw new ArgumentNullException("type", @"The Entity Type must be defined.");

      lock (SyncRoot)
      {
        if (RegisteredEntityDefinitions.Any(ed => ed.EntityNamespace == @namespace))
          RegisteredEntityDefinitions.Remove(@namespace);

        RegisteredEntityDefinitions.Add(new EntityDefinition
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
    public TypedRepositoryConfiguration RegisterEntityPart<TEntityType, TEntityPartType>(string entityPartName)
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
    public TypedRepositoryConfiguration RegisterEntityPart(Type parentEntityType, string entityPartName, Type entityPartType)
    {
      if (parentEntityType == null)
        throw new ArgumentNullException("parentEntityType", @"When registering an entity part, the parent entity type must be specified.");

      if (String.IsNullOrEmpty(entityPartName))
        throw new ArgumentNullException("entityPartName", @"An Entity Part Name must be specified.");

      if (entityPartType == null)
        throw new ArgumentNullException("entityPartType", @"When registering an entity part, the entity part type must be specified.");

      var parentEntityDefinition = RegisteredEntityDefinitions.FirstOrDefault(ed => ed.EntityType == parentEntityType);

      if (parentEntityDefinition == null)
        throw new InvalidOperationException("The Parent Entities' type must first be registered with the Repository prior to being associated with an Entity Part.");

      lock (SyncRoot)
      {
        if (parentEntityDefinition.EntityPartDefinitions.Any(p => p.EntityPartName == entityPartName))
          parentEntityDefinition.EntityPartDefinitions.Remove(entityPartName);

        //We've run the gauntlet, add the entity part definition.
        parentEntityDefinition.EntityPartDefinitions.Add(new EntityPartDefinition
          {
          EntityPartName = entityPartName,
          EntityPartType = entityPartType,
        });
      }
      return this;
    }

    public TypedRepositoryConfiguration RegisterEntityMigration<TDestinationEntityType>(EntityMigrationStrategy<TDestinationEntityType> entityMigrationDefinition)
    {
      throw new NotImplementedException();
    }

    public TypedRepositoryConfiguration RegisterSynchronousEntityTrigger(string entityNamespacePattern, Action<TriggerProperties> trigger)
    {
      if (entityNamespacePattern.IsNullOrWhiteSpace())
        throw new ArgumentNullException("entityNamespacePattern", @"A namespace pattern must be provided.");

      if (trigger == null)
        throw new ArgumentNullException("trigger", @"A trigger action must be provided.");

      lock (SyncRoot)
      {
        if (this.RegisteredSynchronousEntityTriggers.ContainsKey(entityNamespacePattern))
          this.RegisteredSynchronousEntityTriggers.Remove(entityNamespacePattern);

        this.RegisteredSynchronousEntityTriggers.Add(entityNamespacePattern, trigger);
      }
      return this;
    }

    public TypedRepositoryConfiguration RegisterSynchronousEntityPartTrigger(string entityNamespacePattern, Action<TriggerProperties> trigger)
    {
      if (entityNamespacePattern.IsNullOrWhiteSpace())
        throw new ArgumentNullException("entityNamespacePattern", @"A namespace pattern must be provided.");

      if (trigger == null)
        throw new ArgumentNullException("trigger", @"A trigger action must be provided.");

      lock (SyncRoot)
      {
        if (this.RegisteredSynchronousEntityPartTriggers.ContainsKey(entityNamespacePattern))
          this.RegisteredSynchronousEntityPartTriggers.Remove(entityNamespacePattern);

        this.RegisteredSynchronousEntityPartTriggers.Add(entityNamespacePattern, trigger);
      }

      return this;
    }

    internal IEnumerable<Action<TriggerProperties>> GetEntityTriggersForNamespace(string entityNamespace)
    {
      foreach (var pattern in RegisteredSynchronousEntityTriggers.Keys)
      {
        if (Regex.IsMatch(entityNamespace, pattern, RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.Singleline))
          yield return RegisteredSynchronousEntityTriggers[pattern];
      }
    }

    internal IEnumerable<Action<TriggerProperties>> GetEntityPartTriggersForNamespace(string entityNamespace)
    {
      foreach (var pattern in RegisteredSynchronousEntityPartTriggers.Keys)
      {
        if (Regex.IsMatch(pattern, entityNamespace, RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.Singleline))
          yield return RegisteredSynchronousEntityPartTriggers[pattern];
      }
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
        throw new NotImplementedException(String.Format("The Document Store associated with the repository does not implement the specified operation. ({0})", typeof(T)));
      }
      return (T)this.DocumentStore;
    }
    #endregion

    #region Protected Methods
    internal static bool SettingsDefinedInConfiguration
    {
      get
      {
        return ConfigurationManager.AppSettings.AllKeys.Any(k => k == "BaristaDS_DocumentStore");
      }
    }

    internal static IDocumentStore InitializeDocumentStoreFromConfiguration()
    {
      if (SettingsDefinedInConfiguration == false)
        throw new InvalidOperationException("The DocumentStore instance that the service will use must be defined in a 'BaristaDS_DocumentStore' value contained in the web.config");

      string fullTypeName = ConfigurationManager.AppSettings["BaristaDS_DocumentStore"];
      if (string.IsNullOrEmpty(fullTypeName))
        throw new InvalidOperationException("A BaristaDS_DocumentStore key was specified within web.config, but it did not contain a value.");

      Type documentStoreType = Type.GetType(fullTypeName, true, true);
      if (documentStoreType != null && documentStoreType.GetInterfaces().Any(i => i == typeof(IDocumentStore)) == false)
        throw new InvalidOperationException("The BaristaDS_DocumentStore was specified within web.config, but it does not implement IDocumentStore");

      if (documentStoreType == null)
        throw new InvalidOperationException("Unable to initialize the BaristaDS_DocumentStore.");
      
      return Activator.CreateInstance(documentStoreType) as IDocumentStore;
    }
    #endregion
  }
}
