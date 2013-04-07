namespace Barista.Library
{
  using System.Collections.Generic;
  using System.Collections.Specialized;
  using Barista.Imports.Linq2Rest;
  using Jurassic;
  using Jurassic.Library;
  using System;
  using System.Collections;

  [Serializable]
  public class LinqFilterConstructor : ClrFunction
  {
    public LinqFilterConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "LinqFilter", new LinqFilterInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public LinqFilterInstance Construct()
    {
      return new LinqFilterInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class LinqFilterInstance : ObjectInstance
  {
    public LinqFilterInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    [JSFunction(Name = "filterEnumerable")]
    public ArrayInstance Filter(object source, ObjectInstance query)
    {
      //TODO: There should be an interface that instance objects implement to allow them to be filtered in this manner....
      //INativeEnumerable
      if ((source is IEnumerable) == false)
        throw new JavaScriptException(this.Engine, "Error", "Expected the source parameter to be a (native) IEnumerable<T> object.");

      var valueCollection = new NameValueCollection();
      foreach (var property in query.Properties)
      {
        valueCollection.Add(property.Name, query.GetPropertyValue(property.Name).ToString());
      }

      var stuffs = new List<String>();
      stuffs.Filter(valueCollection);

      throw new NotImplementedException();
    }
  }
}
