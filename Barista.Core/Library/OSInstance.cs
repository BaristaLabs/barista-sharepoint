namespace Barista.Library
{
  using Barista.Helpers;
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using System;

  [Serializable]
  public class OSConstructor : ClrFunction
  {
    public OSConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "OS", new OSInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public OSInstance Construct()
    {
      return new OSInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class OSInstance : ObjectInstance
  {
    private readonly OS m_oS;

    public OSInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public OSInstance(ObjectInstance prototype, OS oS)
      : this(prototype)
    {
      if (oS == null)
        throw new ArgumentNullException("oS");

      m_oS = oS;
    }

    public OS OS
    {
      get { return m_oS; }
    }

    [JSProperty(Name = "family")]
    public string Family
    {
      get { return m_oS.Family; }
    }

    [JSProperty(Name = "major")]
    public string Major
    {
      get { return m_oS.Major; }
    }

    [JSProperty(Name = "minor")]
    public string Minor
    {
      get { return m_oS.Minor; }
    }

    [JSProperty(Name = "patch")]
    public string Patch
    {
      get { return m_oS.Patch; }
    }

    [JSProperty(Name = "patchMinor")]
    public string PatchMinor
    {
      get { return m_oS.PatchMinor; }
    }
  }
}
