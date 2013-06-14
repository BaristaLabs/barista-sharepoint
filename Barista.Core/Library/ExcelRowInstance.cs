namespace Barista.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using OfficeOpenXml;
  using System;

  [Serializable]
  public class ExcelRowConstructor : ClrFunction
  {
    public ExcelRowConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "ExcelRow", new ExcelRowInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public ExcelRowInstance Construct()
    {
      return new ExcelRowInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class ExcelRowInstance : ObjectInstance
  {
    private readonly ExcelRow m_excelRow;

    public ExcelRowInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public ExcelRowInstance(ObjectInstance prototype, ExcelRow excelRow)
      : this(prototype)
    {
      if (excelRow == null)
        throw new ArgumentNullException("excelRow");

      m_excelRow = excelRow;
    }

    public ExcelRow ExcelRow
    {
      get { return m_excelRow; }
    }
  }
}
