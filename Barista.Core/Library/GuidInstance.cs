﻿namespace Barista.Library
{
  using Jurassic;
  using Jurassic.Library;
  using System;

  public class GuidConstructor : ClrFunction
  {
    public GuidConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "Guid", new GuidInstance(engine.Object.InstancePrototype))
    {
      PopulateFunctions();
    }

    [JSConstructorFunction]
    public GuidInstance Construct()
    {
      return new GuidInstance(this.Engine.Object.InstancePrototype, Guid.NewGuid());
    }

    [JSConstructorFunction]
    public GuidInstance Construct(string g)
    {
      return new GuidInstance(this.Engine.Object.InstancePrototype, g);
    }

    [JSFunction(Name = "Empty")]
    public GuidInstance Empty()
    {
      return new GuidInstance(this.Engine.Object, Guid.Empty);
    }
  }


  public class GuidInstance : ObjectInstance
  {
    Guid m_guid;

    public GuidInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public GuidInstance(ObjectInstance prototype, Guid guid)
      : this(prototype)
    {
      if (guid == null)
        throw new ArgumentNullException("guid");

      m_guid = guid;
    }

    public GuidInstance(ObjectInstance prototype, string g)
      : this(prototype, new Guid(g))
    {
    }

    [JSFunction(Name = "toByteArray")]
    public Base64EncodedByteArrayInstance ToByteArray()
    {
      return new Base64EncodedByteArrayInstance(this.Engine.Object.InstancePrototype, m_guid.ToByteArray());
    }

    [JSFunction(Name = "toJSON", Flags = JSFunctionFlags.HasThisObject)]
    public static object ToJSON(ObjectInstance thisObject, string key)
    {
      return thisObject.ToString();
    }

    [JSFunction(Name = "ToString")]
    public string ToStringJS()
    {
      return this.ToString();
    }

    [JSFunction(Name = "toString")]
    public override string ToString()
    {
      return m_guid.ToString();
    }
  }
}
