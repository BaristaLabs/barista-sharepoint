namespace Barista.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using OfficeOpenXml;
  using System;

  [Serializable]
  public class ExcelWorksheetConstructor : ClrFunction
  {
    public ExcelWorksheetConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "ExcelWorksheet", new ExcelWorksheetInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public ExcelWorksheetInstance Construct()
    {
      return new ExcelWorksheetInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class ExcelWorksheetInstance : ObjectInstance
  {
    private readonly ExcelWorksheet m_excelWorksheet;

    public ExcelWorksheetInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public ExcelWorksheetInstance(ObjectInstance prototype, ExcelWorksheet excelWorksheet)
      : this(prototype)
    {
      if (excelWorksheet == null)
        throw new ArgumentNullException("excelWorksheet");

      m_excelWorksheet = excelWorksheet;
    }

    public ExcelWorksheet ExcelWorksheet
    {
      get { return m_excelWorksheet; }
    }
  }
}
