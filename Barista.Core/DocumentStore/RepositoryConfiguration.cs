namespace Barista.DocumentStore
{
  using System;
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  using System.Configuration;
  using System.Linq;

  [Serializable]
  public class RepositoryConfiguration
  {
    private static object m_syncRoot = new object();
    private IDocumentStore m_documentStore = null;

    public RepositoryConfiguration()
    {
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
    /// The Document Store that the Repository instance will utilize can be defined either through this property or through a 'BaristaDS_DocumentStore' configuration element.
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
    #endregion

    #region Public Methods
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
      if (IsDocumentStore<T>() == false)
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
      if (documentStoreType.GetInterfaces().Any(i => i == typeof(IDocumentStore)) == false)
        throw new InvalidOperationException("The BaristaDS_DocumentStore was specified within web.config, but it does not implement IDocumentStore");

      return Activator.CreateInstance(documentStoreType) as IDocumentStore;
    }
    #endregion
  }
}
