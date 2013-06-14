namespace Barista.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using OfficeOpenXml;
  using System;

  [Serializable]
  public class ExcelAddressConstructor : ClrFunction
  {
    public ExcelAddressConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "ExcelAddress", new ExcelAddressInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public ExcelAddressInstance Construct()
    {
      return new ExcelAddressInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class ExcelAddressInstance : ObjectInstance
  {
    private readonly ExcelAddressBase m_excelAddressBase;

    public ExcelAddressInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public ExcelAddressInstance(ObjectInstance prototype, ExcelAddressBase excelAddressBase)
      : this(prototype)
    {
      if (excelAddressBase == null)
        throw new ArgumentNullException("excelAddressBase");

      m_excelAddressBase = excelAddressBase;
    }

    public ExcelAddressBase ExcelAddressBase
    {
      get { return m_excelAddressBase; }
    }

    [JSProperty(Name = "address")]
    public string Address
    {
      get { return m_excelAddressBase.Address; }
    }

    [JSProperty(Name = "end")]
    public ExcelCellAddressInstance End
    {
      get { return new ExcelCellAddressInstance(this.Engine.Object.Prototype, m_excelAddressBase.End); }
    }

    [JSProperty(Name = "isName")]
    public bool IsName
    {
      get { return m_excelAddressBase.IsName; }
    }

    [JSProperty(Name = "start")]
    public ExcelCellAddressInstance Start
    {
      get { return new ExcelCellAddressInstance(this.Engine.Object.Prototype, m_excelAddressBase.Start); }
    }
  }
}
