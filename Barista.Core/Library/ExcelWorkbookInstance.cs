namespace Barista.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using OfficeOpenXml;
  using System;

  [Serializable]
  public class ExcelWorkbookConstructor : ClrFunction
  {
    public ExcelWorkbookConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "ExcelWorkbook", new ExcelWorkbookInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public ExcelWorkbookInstance Construct()
    {
      return new ExcelWorkbookInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class ExcelWorkbookInstance : ObjectInstance
  {
    private readonly ExcelWorkbook m_excelWorkbook;

    public ExcelWorkbookInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public ExcelWorkbookInstance(ObjectInstance prototype, ExcelWorkbook excelWorkbook)
      : this(prototype)
    {
      if (excelWorkbook == null)
        throw new ArgumentNullException("excelWorkbook");

      m_excelWorkbook = excelWorkbook;
    }

    public ExcelWorkbook ExcelWorkbook
    {
      get { return m_excelWorkbook; }
    }
  }
}
