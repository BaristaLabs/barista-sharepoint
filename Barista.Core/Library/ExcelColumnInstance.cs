namespace Barista.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using OfficeOpenXml;
  using System;

  [Serializable]
  public class ExcelColumnConstructor : ClrFunction
  {
    public ExcelColumnConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "ExcelColumn", new ExcelColumnInstance(engine.Object.InstancePrototype))
    {
    }
    
    [JSConstructorFunction]
    public ExcelColumnInstance Construct()
    {
      return new ExcelColumnInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class ExcelColumnInstance : ObjectInstance
  {
    private readonly ExcelColumn m_excelColumn;

    public ExcelColumnInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public ExcelColumnInstance(ObjectInstance prototype, ExcelColumn excelColumn)
      : this(prototype)
    {
      if (excelColumn == null)
        throw new ArgumentNullException("excelColumn");

      m_excelColumn = excelColumn;
    }

    public ExcelColumn ExcelColumn
    {
      get { return m_excelColumn; }
    }
  }
}
