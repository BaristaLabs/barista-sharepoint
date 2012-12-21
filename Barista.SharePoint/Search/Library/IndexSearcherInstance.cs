using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Barista.SharePoint.Search.Library
{
  using Jurassic;
  using Jurassic.Library;
  using Lucene.Net.Search;
  using System;

  [Serializable]
  public class IndexSearcherConstructor : ClrFunction
  {
    public IndexSearcherConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "IndexSearcher", new IndexSearcherInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public IndexSearcherInstance Construct()
    {
      return new IndexSearcherInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class IndexSearcherInstance : ObjectInstance
  {
    private readonly IndexSearcher m_indexSearcher;

    public IndexSearcherInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public IndexSearcherInstance(ObjectInstance prototype, IndexSearcher indexSearcher)
      : this(prototype)
    {
      if (indexSearcher == null)
        throw new ArgumentNullException("indexSearcher");

      m_indexSearcher = indexSearcher;
    }

    public IndexSearcher IndexSearcher
    {
      get { return m_indexSearcher; }
    }

    public void Search()
    {
      throw new NotImplementedException();
    }
  }
}
