namespace Barista.SharePoint.Taxonomy.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using Barista.Library;
  using Microsoft.SharePoint.Taxonomy;
  using System;

  [Serializable]
  public class TermSetCollectionConstructor : ClrFunction
  {
    public TermSetCollectionConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "TermSetCollection", new TermSetCollectionInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public TermSetCollectionInstance Construct()
    {
      return new TermSetCollectionInstance(InstancePrototype);
    }
  }

  [Serializable]
  public class TermSetCollectionInstance : ObjectInstance
  {
    private readonly TermSetCollection m_termSetCollection;

    public TermSetCollectionInstance(ObjectInstance prototype)
      : base(prototype)
    {
      PopulateFields();
      PopulateFunctions();
    }

    public TermSetCollectionInstance(ObjectInstance prototype, TermSetCollection termSetCollection)
      : this(prototype)
    {
      if (termSetCollection == null)
        throw new ArgumentNullException("termSetCollection");

      m_termSetCollection = termSetCollection;
    }

    public TermSetCollection TermSetCollection
    {
      get { return m_termSetCollection; }
    }

    [JSProperty(Name = "count")]
    public int Count
    {
      get
      {
        return m_termSetCollection.Count;
      }
    }

    [JSFunction(Name = "getTermSetByName")]
    public TermSetInstance GetTermByName(string name)
    {
      var termSet = m_termSetCollection[name];
      return termSet == null
        ? null
        : new TermSetInstance(Engine.Object.InstancePrototype, termSet);
    }

    [JSFunction(Name = "getTermSetByIndex")]
    public TermSetInstance GetTermSetByIndex(int index)
    {
      var termSet = m_termSetCollection[index];
      return termSet == null
        ? null
        : new TermSetInstance(Engine.Object.InstancePrototype, termSet);
    }

    [JSFunction(Name = "getTermSetById")]
    public TermSetInstance GetTermById(object id)
    {
      var guid = GuidInstance.ConvertFromJsObjectToGuid(id);
      var termSet = m_termSetCollection[guid];
      return termSet == null
        ? null
        : new TermSetInstance(Engine.Object.InstancePrototype, termSet);
    }

    [JSFunction(Name = "toArray")]
    [JSDoc("ternReturnType", "[+TermSet]")]
    public ArrayInstance ToArray()
    {
      var result = Engine.Array.Construct();
      foreach (var termSet in m_termSetCollection)
      {
        ArrayInstance.Push(result, new TermSetInstance(Engine.Object.InstancePrototype, termSet));
      }
      return result;
    }
  }
}
