using RavenDB = Raven.Abstractions;

namespace Barista.Raven.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using System;

  [Serializable]
  public class PutResultConstructor : ClrFunction
  {
    public PutResultConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "PutResult", new PutResultInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public PutResultInstance Construct()
    {
      return new PutResultInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class PutResultInstance : ObjectInstance
  {
    private readonly RavenDB.Data.PutResult m_putResult;

    public PutResultInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public PutResultInstance(ObjectInstance prototype, RavenDB.Data.PutResult putResult)
      : this(prototype)
    {
      if (putResult == null)
        throw new ArgumentNullException("putResult");

     m_putResult = putResult;
    }

    public RavenDB.Data.PutResult PutResult
    {
      get { return m_putResult; }
    }
  }
}
