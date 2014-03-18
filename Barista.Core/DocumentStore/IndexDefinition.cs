namespace Barista.DocumentStore
{
  using System;
  using System.Collections.Generic;
  using System.Collections.ObjectModel;

  /// <summary>
  /// Represents an index definition which defines the map/reduce fuctions when an entity is created/updated/modified.
  /// </summary>
  public class IndexDefinition
  {
    /// <summary>
    /// Gets or sets the name of the index.
    /// </summary>
    public string Name
    {
      get;
      set;
    }

    internal Type IndexType
    {
      get;
      set;
    }

    public Func<IEntity, object> Map
    {
      get;
      set;
    }

    public Func<IList<KeyValuePair<Guid, IndexObject>>, IList<KeyValuePair<Guid, IndexObject>>> Reduce
    {
      get;
      set;
    }

    public Func<Repository, string> IndexETag
    {
      get;
      set;
    }

  }

  public class IndexDefinitionCollection : KeyedCollection<string, IndexDefinition>
  {
    protected override string GetKeyForItem(IndexDefinition item)
    {
      return item.Name;
    }
  }
}
