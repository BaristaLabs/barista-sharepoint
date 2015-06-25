namespace Barista.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using OfficeOpenXml.Style;
  using System;

  [Serializable]
  public class ExcelNumberFormatConstructor : ClrFunction
  {
    public ExcelNumberFormatConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "ExcelNumberFormat", new ExcelNumberFormatInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public ExcelNumberFormatInstance Construct()
    {
      return new ExcelNumberFormatInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class ExcelNumberFormatInstance : ObjectInstance
  {
    private readonly ExcelNumberFormat m_excelNumberFormat;

    public ExcelNumberFormatInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public ExcelNumberFormatInstance(ObjectInstance prototype, ExcelNumberFormat excelNumberFormat)
      : this(prototype)
    {
      if (excelNumberFormat == null)
        throw new ArgumentNullException("excelNumberFormat");

      m_excelNumberFormat = excelNumberFormat;
    }

    public ExcelNumberFormat ExcelNumberFormat
    {
      get { return m_excelNumberFormat; }
    }

    [JSProperty(Name = "buildIn")]
    [JSDoc("Gets a value that indicates if the number format is built-in.")]
    public bool BuildIn
    {
      get { return m_excelNumberFormat.BuildIn; }
    }

    [JSProperty(Name = "format")]
    [JSDoc("Gets or sets the format. E.g. '#,##0.00;(#,##0.00)'")]
    public string Format
    {
      get { return m_excelNumberFormat.Format; }
      set { m_excelNumberFormat.Format = value; }
    }

    [JSProperty(Name = "numberFormatId")]
    [JSDoc("Gets the number format id.")]
    public int NumberFormatId
    {
      get { return m_excelNumberFormat.NumFmtID; }
    }
  }
}
