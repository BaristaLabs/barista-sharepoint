namespace Barista.Search.Library
{
  using Jurassic;
  using Jurassic.Library;
  using Lucene.Net.Search;
  using System;

  [Serializable]
  public class SortConstructor : ClrFunction
  {
    public SortConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "Sort", new SortInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SortInstance Construct()
    {
      return new SortInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class SortInstance : ObjectInstance
  {
    private readonly Sort m_sort;

    public SortInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SortInstance(ObjectInstance prototype, Sort sort)
      : this(prototype)
    {
      if (sort == null)
        throw new ArgumentNullException("sort");

      m_sort = sort;
    }

    public Sort Sort
    {
      get { return m_sort; }
    }
  }
}
