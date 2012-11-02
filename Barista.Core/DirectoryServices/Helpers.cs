namespace Barista.DirectoryServices
{
  using System.DirectoryServices;
  using System.Globalization;

  internal static class Helpers
  {
    /// <summary>
    /// Helper method to clone a DirectorySearcher object and apply the specified filter and properties.
    /// </summary>
    /// <param name="searcher">DirectorySearcher object to clone.</param>
    /// <param name="filter">Search filter.</param>
    /// <param name="properties">Properties to load.</param>
    /// <returns>Cloned DirectorySearcher object with applied filter and properties.</returns>
    public static DirectorySearcher CloneSearcher(DirectorySearcher searcher, string filter, string[] properties)
    {
      DirectorySearcher result = new DirectorySearcher()
      {
        Asynchronous = searcher.Asynchronous,
        AttributeScopeQuery = searcher.AttributeScopeQuery,
        CacheResults = searcher.CacheResults,
        ClientTimeout = searcher.ClientTimeout,
        DerefAlias = searcher.DerefAlias,
        DirectorySynchronization = searcher.DirectorySynchronization,
        ExtendedDN = searcher.ExtendedDN,
        PageSize = searcher.PageSize,
        PropertyNamesOnly = searcher.PropertyNamesOnly,
        ReferralChasing = searcher.ReferralChasing,
        SearchRoot = searcher.SearchRoot,
        SearchScope = searcher.SearchScope,
        SecurityMasks = searcher.SecurityMasks,
        ServerPageTimeLimit = searcher.ServerPageTimeLimit,
        ServerTimeLimit = searcher.ServerTimeLimit,
        SizeLimit = searcher.SizeLimit,
        Tombstone = searcher.Tombstone,
        VirtualListView = searcher.VirtualListView
      };

      result.Filter = filter;
      if (!string.IsNullOrEmpty(filter) && !string.IsNullOrEmpty(searcher.Filter) && searcher.Filter != "(objectClass=*)")
      {
        result.Filter = string.Format(CultureInfo.InvariantCulture, "(&{0}{1})", searcher.Filter, filter);
      }

      foreach (var property in searcher.PropertiesToLoad)
        result.PropertiesToLoad.Add(property);

      if (properties != null)
        result.PropertiesToLoad.AddRange(properties);

      return result;
    }
  }
}
