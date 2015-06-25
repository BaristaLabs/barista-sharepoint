namespace Barista.DirectoryServices
{
  using System;
  using System.DirectoryServices;
  using System.IO;
  using System.Linq;
  using System.Reflection;

  /// <summary>
  /// Provides context-based provider support.
  /// </summary>
  public class DirectoryContext : IDisposable
  {
    #region Private members

    private DirectorySearcher _searcher;

    #endregion

    #region Constructors

    /// <summary>
    /// Creates a new data source instance for the given directory search root and with a given search scope.
    /// </summary>
    /// <param name="searchRoot">Root location in the directory to start all searches from.</param>
    public DirectoryContext(DirectoryEntry searchRoot)
    {
      _searcher = new DirectorySearcher(searchRoot);
      InitializeSources();
    }

    /// <summary>
    /// Creates a new data source instance based on the given DirectorySearcher.
    /// </summary>
    /// <param name="searcher">DirectorySearcher object to use for directory searches.</param>
    public DirectoryContext(DirectorySearcher searcher)
    {
      _searcher = searcher;
      InitializeSources();
    }

    #endregion

    #region Properties

    /// <summary>
    /// Used to configure a logger to print diagnostic information about the query performed.
    /// </summary>
    public TextWriter Log { get; set; }

    /// <summary>
    /// Searcher object.
    /// </summary>
    public DirectorySearcher Searcher { get { return _searcher; } }

    #endregion

    #region Methods

    /// <summary>
    /// Gets the DirectorySource for the given entity type.
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    /// <param name="scope">Search scope.</param>
    /// <returns>DirectorySource for the given entity type.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
    public DirectorySource<T> GetSource<T>(SearchScope scope)
    {
      return new DirectorySource<T>(this, scope);
    }

    /// <summary>
    /// Updates all data sources captured by the context object recursively (including nested contexts).
    /// </summary>
    public void Update()
    {
      foreach (PropertyInfo property in this.GetType().GetProperties())
      {
        if (typeof(IDirectorySource).IsAssignableFrom(property.PropertyType))
        {
          ((IDirectorySource)property.GetValue(this, null)).Update();
        }
        else if (typeof(DirectoryContext).IsAssignableFrom(property.PropertyType))
        {
          ((DirectoryContext)property.GetValue(this, null)).Update();
        }
      }
    }

    #endregion

    #region Helper methods

    /// <summary>
    /// Helper method to initialize data sources.
    /// </summary>
    private void InitializeSources()
    {
      MethodInfo getSource = this.GetType().GetMethod("GetSource");

      foreach (PropertyInfo property in this.GetType().GetProperties())
      {
        if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(DirectorySource<>))
        {
          var searchOptions = property.GetCustomAttributes(typeof(DirectorySearchOptionsAttribute), true).Cast<DirectorySearchOptionsAttribute>().FirstOrDefault();
          SearchScope scope = searchOptions != null ? searchOptions.Scope : SearchScope.Base;

          Type entity = property.PropertyType.GetGenericArguments()[0];
          property.SetValue(this, getSource.MakeGenericMethod(entity).Invoke(this, new object[] { scope }), null);
        }
        else if (typeof(DirectoryContext).IsAssignableFrom(property.PropertyType))
        {
          var searchPath = property.GetCustomAttributes(typeof(DirectorySearchPathAttribute), true).Cast<DirectorySearchPathAttribute>().FirstOrDefault();

          DirectoryEntry searchRoot = _searcher.SearchRoot;
          if (searchPath != null && !string.IsNullOrEmpty(searchPath.Path))
          {
            try
            {
              searchRoot = _searcher.SearchRoot.Children.Find(searchPath.Path);
            }
            catch (DirectoryServicesCOMException ex)
            {
              throw new InvalidOperationException("Failed to retrieve nested context " + property.Name + " with search path " + searchPath.Path + ".", ex);
            }
          }

          Type subContext = property.PropertyType;
          ConstructorInfo ctor = subContext.GetConstructor(new Type[] { typeof(DirectoryEntry) });
          if (ctor == null)
            throw new InvalidOperationException("Nested context " + property.Name + " does not have a suitable constructor.");

          property.SetValue(this, ctor.Invoke(new object[] { searchRoot }), null);
        }
      }
    }

    #endregion

    #region IDisposable Members

    protected virtual void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (_searcher != null)
        {
          _searcher.Dispose();
          _searcher = null;
        }
      }
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    #endregion
  }
}
