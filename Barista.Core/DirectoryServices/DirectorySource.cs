namespace Barista.DirectoryServices
{
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.ComponentModel;
  using System.DirectoryServices;
  using System.IO;
  using System.Linq;
  using System.Linq.Expressions;
  using System.Reflection;

  /// <summary>
  /// Represents a source object used to tak to Directory Services.
  /// </summary>
  public interface IDirectorySource
  {
    #region Properties

    /// <summary>
    /// Gets the logger for the source. Will be used in logging query information.
    /// </summary>
    TextWriter Log { get; }

    /// <summary>
    /// Root location captured by this source.
    /// </summary>
    DirectoryEntry Root { get; }

    /// <summary>
    /// Search scope used when running queries against this source.
    /// </summary>
    SearchScope Scope { get; }

    /// <summary>
    /// Entity type this source will query for.
    /// </summary>
    Type OriginalType { get; }

    /// <summary>
    /// Searcher used to perform directory searches.
    /// </summary>
    DirectorySearcher Searcher { get; }

    #endregion

    #region Methods

    /// <summary>
    /// Callback method for entity property change tracking.
    /// </summary>
    void UpdateNotification(object sender, PropertyChangedEventArgs e);

    /// <summary>
    /// Update changes in the underlying data source.
    /// </summary>
    void Update();

    #endregion
  }

  /// <summary>
  /// Represents an LDAP data source. Allows for querying the LDAP data source via LINQ.
  /// </summary>
  /// <typeparam name="T">Entity type in the underlying source.</typeparam>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
  public class DirectorySource<T> : IQueryable<T>, IDirectorySource
  {
    #region Private members

    /// <summary>
    /// Associated directory context if bound to a context; otherwise null.
    /// </summary>
    private DirectoryContext _context;

    /// <summary>
    /// Searcher object to perform the directory search. Captures the search root and options.
    /// </summary>
    private DirectorySearcher _searcher;

    /// <summary>
    /// Logger for diagnostic output.
    /// </summary>
    private TextWriter logger;

    /// <summary>
    /// Update catalog; keeps track of update entity instances.
    /// </summary>
    private Dictionary<object, HashSet<string>> updates = new Dictionary<object, HashSet<string>>();

    #endregion

    #region Constructors

    /// <summary>
    /// Creates a new data source instance for the given directory search root and with a given search scope.
    /// </summary>
    /// <param name="searchRoot">Root location in the directory to start all searches from.</param>
    /// <param name="searchScope">Search scope for all queries performed through this data source.</param>
    public DirectorySource(DirectoryEntry searchRoot, SearchScope searchScope)
    {
      _searcher = new DirectorySearcher(searchRoot) { SearchScope = searchScope };
    }

    /// <summary>
    /// Creates a new data source instance based on the given DirectorySearcher.
    /// </summary>
    /// <param name="searcher">DirectorySearcher object to use for directory searches.</param>
    public DirectorySource(DirectorySearcher searcher)
    {
      _searcher = searcher;
    }

    /// <summary>
    /// Creates a data source from the given context.
    /// </summary>
    /// <param name="context">DirectoryContext that embeds the data source.</param>
    /// <param name="searchScope">Search scope for all queries performed through this data source.</param>
    internal DirectorySource(DirectoryContext context, SearchScope searchScope)
    {
      _context = context;
      _searcher = Helpers.CloneSearcher(context.Searcher, null, null);
      _searcher.SearchScope = searchScope;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Used to configure a logger to print diagnostic information about the query performed.
    /// </summary>
    public TextWriter Log
    {
      get
      {
        if (logger != null)
          return logger;

        // Fall back to parent context if applicable.
        if (_context != null)
          return _context.Log;

        return null;
      }
      set
      {
        // Can override on the leaf level.
        logger = value;
      }
    }

    /// <summary>
    /// Search root.
    /// </summary>
    public DirectoryEntry Root
    {
      get { return Searcher.SearchRoot; }
    }

    /// <summary>
    /// Search scope.
    /// </summary>
    public SearchScope Scope
    {
      get { return Searcher.SearchScope; }
    }

    /// <summary>
    /// Entity element type.
    /// </summary>
    public Type ElementType
    {
      get { return typeof(T); }
    }

    /// <summary>
    /// Original type of objects being queried.
    /// </summary>
    public Type OriginalType
    {
      get { return typeof(T); }
    }

    /// <summary>
    /// Expression representing the data source object.
    /// </summary>
    public Expression Expression
    {
      get { return Expression.Constant(this); }
    }

    /// <summary>
    /// LINQ query provider object.
    /// </summary>
    public IQueryProvider Provider
    {
      get { return new DirectoryQueryProvider(); }
    }

    /// <summary>
    /// Directory searcher used for the directory search.
    /// </summary>
    public DirectorySearcher Searcher
    {
      get { return _searcher; }
    }

    #endregion

    #region Enumeration

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public IEnumerator<T> GetEnumerator()
    {
      return new DirectoryQuery<T>(this.Expression).GetEnumerator();
    }

    #endregion

    #region Updates

    public void Update()
    {
      Type t = typeof(T);
      DirectorySchemaAttribute[] attr = (DirectorySchemaAttribute[])t.GetCustomAttributes(typeof(DirectorySchemaAttribute), false);

      foreach (var e in updates)
      {
        if (e.Key is T && e.Key is DirectoryEntity)
        {
          DirectoryEntry entry = ((DirectoryEntity)e.Key).DirectoryEntry;
          foreach (string property in e.Value)
          {
            PropertyInfo i = t.GetProperty(property);

            DirectoryAttributeAttribute[] da = i.GetCustomAttributes(typeof(DirectoryAttributeAttribute), false) as DirectoryAttributeAttribute[];
            if (da != null && da.Length != 0 && da[0] != null)
            {
              if (da[0].QuerySource == DirectoryAttributeType.ActiveDs)
              {
                if (attr != null && attr.Length != 0)
                  attr[0].ActiveDsHelperType.GetProperty(da[0].Attribute).SetValue(entry.NativeObject, i.GetValue(e.Key, null), null);
                else
                  throw new InvalidOperationException("Missing schema mapping attribute for updates through ADSI.");
              }
              else
                entry.Properties[da[0].Attribute].Value = i.GetValue(e.Key, null);
            }
            else
              entry.Properties[i.Name].Value = i.GetValue(e.Key, null);
          }
          entry.CommitChanges();
        }
        else
          throw new InvalidOperationException("Can't apply update because updates type doesn't match original entity type.");
      }

      updates.Clear();
    }

    public void UpdateNotification(object sender, PropertyChangedEventArgs e)
    {
      T source = (T)sender;

      if (!updates.ContainsKey(source))
        updates.Add(source, new HashSet<string>());

      updates[source].Add(e.PropertyName);
    }

    #endregion
  }

}