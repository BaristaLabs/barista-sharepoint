namespace Barista.SharePoint.Library
{
  using System.Linq;
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using System;
  using Barista.Library;
  using Microsoft.SharePoint.Administration;

  [Serializable]
  public class SPAlternateUrlCollectionConstructor : ClrFunction
  {
    public SPAlternateUrlCollectionConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPAlternateUrlCollection", new SPAlternateUrlCollectionInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPAlternateUrlCollectionInstance Construct()
    {
      return new SPAlternateUrlCollectionInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class SPAlternateUrlCollectionInstance : ObjectInstance
  {
    private readonly SPAlternateUrlCollection m_alternateUrlCollection;

    public SPAlternateUrlCollectionInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SPAlternateUrlCollectionInstance(ObjectInstance prototype, SPAlternateUrlCollection alternateUrlCollection)
      : this(prototype)
    {
      if (alternateUrlCollection == null)
        throw new ArgumentNullException("alternateUrlCollection");

      m_alternateUrlCollection = alternateUrlCollection;
    }

    public SPAlternateUrlCollection SPAlternateUrlCollection
    {
      get { return m_alternateUrlCollection; }
    }

    //TODO: Add the other properties/functions. There's alot for a collection object.
    [JSProperty(Name = "count")]
    public int Count
    {
      get
      {
        return m_alternateUrlCollection.Count;
      }
    }

    [JSFunction(Name = "getAlternateUrlByUrl")]
    public SPAlternateUrlInstance GetAlternateUrlByUrl(object url)
    {
      Uri uri;
      if (url is UriInstance)
        uri = (url as UriInstance).Uri;
      else
        uri = new Uri(TypeConverter.ToString(url));

      var alternateUrl = m_alternateUrlCollection[uri];
      return alternateUrl == null
        ? null
        : new SPAlternateUrlInstance(this.Engine.Object, alternateUrl);
    }

    [JSFunction(Name = "getAlternateUrlByName")]
    public SPAlternateUrlInstance GetAlternateUrlByIncomingUrl(string incomingUrl)
    {
      var alternateUrl = m_alternateUrlCollection[incomingUrl];
      return alternateUrl == null
        ? null
        : new SPAlternateUrlInstance(this.Engine.Object, alternateUrl);
    }

    [JSFunction(Name = "getAlternateUrlByIndex")]
    public SPAlternateUrlInstance GetAlternateUrlByIndex(int index)
    {
      var alternateUrl = m_alternateUrlCollection[index];
      return alternateUrl == null
        ? null
        : new SPAlternateUrlInstance(this.Engine.Object, alternateUrl);
    }

    [JSFunction(Name = "toArray")]
    public ArrayInstance ToArray()
    {
      var result = this.Engine.Array.Construct();
      foreach (var alternateUrl in m_alternateUrlCollection)
      {
        ArrayInstance.Push(result, new SPAlternateUrlInstance(this.Engine.Object.InstancePrototype, alternateUrl));
      }
      return result;
    }
  }
}
