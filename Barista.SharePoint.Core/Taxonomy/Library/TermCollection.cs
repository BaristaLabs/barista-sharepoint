namespace Barista.SharePoint.Taxonomy.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using Barista.Library;
  using Microsoft.SharePoint.Taxonomy;
  using System;

  [Serializable]
  public class TermCollectionConstructor : ClrFunction
  {
    public TermCollectionConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "TermCollection", new TermCollectionInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public TermCollectionInstance Construct()
    {
      return new TermCollectionInstance(InstancePrototype);
    }
  }

  [Serializable]
  public class TermCollectionInstance : ObjectInstance
  {
    private readonly TermCollection m_termCollection;

    public TermCollectionInstance(ObjectInstance prototype)
      : base(prototype)
    {
      PopulateFields();
      PopulateFunctions();
    }

    public TermCollectionInstance(ObjectInstance prototype, TermCollection termCollection)
      : this(prototype)
    {
      if (termCollection == null)
        throw new ArgumentNullException("termCollection");

      m_termCollection = termCollection;
    }

    public TermCollection TermCollection
    {
      get { return m_termCollection; }
    }

    [JSProperty(Name = "count")]
    public int Count
    {
      get
      {
        return m_termCollection.Count;
      }
    }

    [JSProperty(Name = "pagingLimit")]
    public int PagingLimit
    {
      get
      {
        return m_termCollection.PagingLimit;
      }
    }

    [JSFunction(Name = "getTermByName")]
    public TermInstance GetTermByName(string name)
    {
      var term = m_termCollection[name];
      return term == null
        ? null
        : new TermInstance(Engine.Object.InstancePrototype, term);
    }

    [JSFunction(Name = "getTermByIndex")]
    public TermInstance GetTermByIndex(int index)
    {
      var term = m_termCollection[index];
      return term == null
        ? null
        : new TermInstance(Engine.Object.InstancePrototype, term);
    }

    [JSFunction(Name = "getTermById")]
    public TermInstance GetTermById(object id)
    {
      var guid = GuidInstance.ConvertFromJsObjectToGuid(id);
      var term = m_termCollection[guid];
      return term == null
        ? null
        : new TermInstance(Engine.Object.InstancePrototype, term);
    }

    [JSFunction(Name = "toArray")]
    [JSDoc("ternReturnType", "[+Term]")]
    public ArrayInstance ToArray()
    {
      var result = Engine.Array.Construct();
      foreach (var term in m_termCollection)
      {
        ArrayInstance.Push(result, new TermInstance(Engine.Object.InstancePrototype, term));
      }
      return result;
    }
  }
}
