namespace Barista.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using OfficeOpenXml;
  using System;

  [Serializable]
  public class ExcelCellAddressConstructor : ClrFunction
  {
    public ExcelCellAddressConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "ExcelCellAddress", new ExcelCellAddressInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public ExcelCellAddressInstance Construct()
    {
      return new ExcelCellAddressInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class ExcelCellAddressInstance : ObjectInstance
  {
    private readonly ExcelCellAddress m_excelCellAddress;

    public ExcelCellAddressInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public ExcelCellAddressInstance(ObjectInstance prototype, ExcelCellAddress excelCellAddress)
      : this(prototype)
    {
      if (excelCellAddress == null)
        throw new ArgumentNullException("excelCellAddress");

      m_excelCellAddress = excelCellAddress;
    }

    public ExcelCellAddress ExcelCellAddress
    {
      get { return m_excelCellAddress; }
    }

    [JSProperty(Name = "address")]
    [JSDoc("Gets the address of the cell.")]
    public string Address
    {
      get { return m_excelCellAddress.Address; }
    }

    [JSProperty(Name = "column")]
    [JSDoc("Gets the 1-based column index.")]
    public int Column
    {
      get { return m_excelCellAddress.Column; }
    }

    [JSProperty(Name = "row")]
    [JSDoc("Gets the 1-based row index.")]
    public int Row
    {
      get { return m_excelCellAddress.Row; }
    }

    [JSProperty(Name = "isRef")]
    [JSDoc("Gets a value that indicates if the address is an invalid reference. (#REF!)")]
    public bool IsRef
    {
      get { return m_excelCellAddress.IsRef; }
    }
  }
}
