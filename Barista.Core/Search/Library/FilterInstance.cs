namespace Barista.Search.Library
{
  using Jurassic.Library;
  using System;

  [Serializable]
  public abstract class FilterInstance<T> : ObjectInstance
    where T : Filter
  {
    protected FilterInstance(ObjectInstance prototype)
      : base(prototype)
    {
    }

    public abstract T Filter
    {
      get;
    }

    [JSFunction(Name = "ToString")]
    public string ToStringJS()
    {
      return this.ToString();
    }

    [JSFunction(Name = "toString")]
    public override string ToString()
    {
      return this.Filter.ToString();
    }
  }
}
