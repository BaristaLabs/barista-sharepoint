namespace Barista.SharePoint.Migration.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using System;
  using Microsoft.SharePoint.Deployment;

  [Serializable]
  public class SPImportConstructor : ClrFunction
  {
    public SPImportConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPImport", new SPImportInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPImportInstance Construct()
    {
      return new SPImportInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class SPImportInstance : ObjectInstance
  {
    private readonly SPImport m_import;

    public SPImportInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SPImportInstance(ObjectInstance prototype, SPImport import)
      : this(prototype)
    {
      if (import == null)
        throw new ArgumentNullException("import");

      m_import = import;
    }

    public SPImport SPImport
    {
      get { return m_import; }
    }
  }
}
