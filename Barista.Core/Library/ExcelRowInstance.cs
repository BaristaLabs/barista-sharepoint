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

    #region Properties
    [JSProperty(Name = "collapsed")]
    [JSDoc("Gets or sets a value that indicates if the row is collapsed.")]
    public bool Collapsed
    {
      get { return m_excelRow.Collapsed; }
      set { m_excelRow.Collapsed = value; }
    }

    [JSProperty(Name = "customHeight")]
    [JSDoc("Gets or sets a value that indicates if the row is autosized. False if the row should autosize.")]
    public bool CustomHeight
    {
      get { return m_excelRow.CustomHeight; }
      set { m_excelRow.CustomHeight = value; }
    }

    [JSProperty(Name = "height")]
    [JSDoc("Gets or sets the height of the row.")]
    public double Height
    {
      get { return m_excelRow.Height; }
      set { m_excelRow.Height = value; }
    }

    [JSProperty(Name = "hidden")]
    [JSDoc("Gets or sets a value that indicates if the row is hidden.")]
    public bool Hidden
    {
      get { return m_excelRow.Hidden; }
      set { m_excelRow.Hidden = value; }
    }

    [JSProperty(Name = "outlineLevel")]
    [JSDoc("Gets or sets a value that indicates the outline level. Zero if no outline.")]
    public int OutlineLevel
    {
      get { return m_excelRow.OutlineLevel; }
      set { m_excelRow.OutlineLevel = value; }
    }

    [JSProperty(Name = "pageBreak")]
    [JSDoc("Gets or sets a value that indicates if there is a manual page-break after the row.")]
    public bool PageBreak
    {
      get { return m_excelRow.PageBreak; }
      set { m_excelRow.PageBreak = value; }
    }

    [JSProperty(Name = "row")]
    [JSDoc("Gets or sets the row number.")]
    public int Row
    {
      get { return m_excelRow.Row; }
      set { m_excelRow.Row = value; }
    }

    [JSProperty(Name = "styleName")]
    [JSDoc("Gets or sets the row style name.")]
    public string StyleName
    {
      get { return m_excelRow.StyleName; }
      set { m_excelRow.StyleName = value; }
    }
    #endregion
  }
}
