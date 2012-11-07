using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Barista.DocumentStore
{
  public class Index
  {
    public Index()
    {
      this.Items = new Dictionary<Guid, IndexObject>();
    }

    public string ETag
    {
      get;
      set;
    }

    public IDictionary<Guid, IndexObject> Items
    {
      get;
      set;
    }
  }

  internal class Index<TIndex>
  {
    public IDictionary<Guid, IndexObject<TIndex>> Items
    {
      get;
      set;
    }
  }

  public class IndexObject
  {
    public Dictionary<string, string> Metadata
    {
      get;
      set;
    }

    public Object MapResult
    {
      get;
      set;
    }
  }

  public class IndexObject<T>
  {
    public Dictionary<string, string> Metadata
    {
      get;
      set;
    }

    public T MapResult
    {
      get;
      set;
    }
  }
}
