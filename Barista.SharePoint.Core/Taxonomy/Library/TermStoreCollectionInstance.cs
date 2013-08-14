namespace Barista.SharePoint.Taxonomy.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using System;
  using Barista.Library;
  using Microsoft.SharePoint.Taxonomy;

  [Serializable]
  public class TermStoreCollectionConstructor : ClrFunction
  {
    public TermStoreCollectionConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "TermStoreCollection", new TermStoreCollectionInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public TermStoreCollectionInstance Construct()
    {
      return new TermStoreCollectionInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class TermStoreCollectionInstance : ObjectInstance
  {
    private readonly TermStoreCollection m_termStoreCollection;

    public TermStoreCollectionInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public TermStoreCollectionInstance(ObjectInstance prototype, TermStoreCollection termStoreCollection)
      : this(prototype)
    {
      if (termStoreCollection == null)
        throw new ArgumentNullException("termStoreCollection");

      m_termStoreCollection = termStoreCollection;
    }

    public TermStoreCollection TermStoreCollection
    {
      get { return m_termStoreCollection; }
    }

    [JSProperty(Name = "count")]
    public int Count
    {
      get
      {
        return m_termStoreCollection.Count;
      }
    }

    [JSFunction(Name = "getTermStoreByName")]
    public TermStoreInstance GetTermStoreByName(string name)
    {
      var termStore = m_termStoreCollection[name];
      return termStore == null
        ? null
        : new TermStoreInstance(this.Engine.Object.InstancePrototype, termStore);
    }

    [JSFunction(Name = "getTermStoreByIndex")]
    public TermStoreInstance GetTermStoreByIndex(int index)
    {
      var termStore = m_termStoreCollection[index];
      return termStore == null
        ? null
        : new TermStoreInstance(this.Engine.Object.InstancePrototype, termStore);
    }

    [JSFunction(Name = "getTermStoreById")]
    public TermStoreInstance GetTermStoreById(object id)
    {
      var guid = GuidInstance.ConvertFromJsObjectToGuid(id);
      var termStore = m_termStoreCollection[guid];
      return termStore == null
        ? null
        : new TermStoreInstance(this.Engine.Object.InstancePrototype, termStore);
    }

    [JSFunction(Name = "getAllTermStores")]
    public ArrayInstance GetAllTermStores()
    {
      var result = this.Engine.Array.Construct();
      foreach (var termStore in m_termStoreCollection)
      {
        ArrayInstance.Push(result, new TermStoreInstance(this.Engine.Object.InstancePrototype, termStore));
      }
      return result;
    }
  }
}
