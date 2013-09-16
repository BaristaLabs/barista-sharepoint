namespace Barista.SharePoint.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using Barista.Library;
  using Microsoft.SharePoint;
  using System;

  [Serializable]
  public class SPWebCollectionConstructor : ClrFunction
  {
    public SPWebCollectionConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPWebCollection", new SPWebCollectionInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPWebCollectionInstance Construct()
    {
      return new SPWebCollectionInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class SPWebCollectionInstance : ObjectInstance
  {
    private readonly SPWebCollection m_webCollection;

    public SPWebCollectionInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SPWebCollectionInstance(ObjectInstance prototype, SPWebCollection webCollection)
      : this(prototype)
    {
      if (webCollection == null)
        throw new ArgumentNullException("webCollection");

      m_webCollection = webCollection;
    }

    public SPWebCollection SPWebCollection
    {
      get { return m_webCollection; }
    }

    [JSProperty(Name = "count")]
    public int Count
    {
      get
      {
        return m_webCollection.Count;
      }
    }

    [JSProperty(Name = "names")]
    public ArrayInstance Names
    {
      get
      {
        // ReSharper disable CoVariantArrayConversion
        return m_webCollection.Names == null
          ? null
          : this.Engine.Array.Construct(m_webCollection.Names);
        // ReSharper restore CoVariantArrayConversion
      }
    }

    [JSFunction(Name = "getWebByGuid")]
    public SPWebInstance GetWebByGuid(object id)
    {
      var guid = GuidInstance.ConvertFromJsObjectToGuid(id);

      var result = m_webCollection[guid];

      return result == null
        ? null
        : new SPWebInstance(this.Engine.Object.Prototype, result);
    }

    [JSFunction(Name = "getWebByName")]
    public SPWebInstance GetWebByName(string name)
    {
      var result = m_webCollection[name];
      return result == null
        ? null
        : new SPWebInstance(this.Engine.Object.Prototype, result);
    }

    [JSFunction(Name = "getWebByIndex")]
    public SPWebInstance GetWebByIndex(int index)
    {
      var result = m_webCollection[index];
      return result == null
        ? null
        : new SPWebInstance(this.Engine.Object.Prototype, result);
    }

    [JSFunction(Name = "toArray")]
    public ArrayInstance ToArray()
    {
      var result = this.Engine.Array.Construct();
      foreach (SPWeb web in m_webCollection)
      {
        ArrayInstance.Push(result, new SPWebInstance(this.Engine.Object.InstancePrototype, web));
      }
      return result;
    }
  }
}
