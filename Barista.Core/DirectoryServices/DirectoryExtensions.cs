namespace Barista.DirectoryServices
{
  using System.DirectoryServices;

  /// <summary>
  /// Extension methods for System.DirectoryServices classes.
  /// </summary>
  public static class DirectoryExtensions
  {
    /// <summary>
    /// Lifts a DirectorySearcher object into a queryable DirectorySource for a given entity type.
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    /// <param name="searcher">DirectorySearcher to base the query on.</param>
    /// <returns>Queryable DirectorySource.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
    public static DirectorySource<T> AsQueryable<T>(this DirectorySearcher searcher)
    {
      return new DirectorySource<T>(searcher);
    }

    /// <summary>
    /// Lifts a DirectoryEntry object into a queryable DirectorySource for a given entity type.
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    /// <param name="entry">DirectoryEntry to start the query from.</param>
    /// <param name="scope">Scope for the query.</param>
    /// <returns>Queryable DirectorySource.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
    public static DirectorySource<T> AsQueryable<T>(this DirectoryEntry entry, SearchScope scope)
    {
      return new DirectorySource<T>(entry, scope);
    }
  }
}
