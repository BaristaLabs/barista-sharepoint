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

    #region Properties
    [JSProperty(Name = "bestFit")]
    [JSDoc("Gets or sets a value that indicates if the column automagically resized when a user inputs a cell.")]
    public bool BestFit
    {
      get { return m_excelColumn.BestFit; }
      set { m_excelColumn.BestFit = value; }
    }

    [JSProperty(Name = "collapsed")]
    [JSDoc("Gets or sets a value that indicates if the column is collapsed in outline mode.")]
    public bool Collapsed
    {
      get { return m_excelColumn.Collapsed; }
      set { m_excelColumn.Collapsed = value; }
    }

    [JSProperty(Name = "columnMax")]
    [JSDoc("Gets or sets the last column that the definition refers to.")]
    public int ColumnMax
    {
      get { return m_excelColumn.ColumnMax; }
      set { m_excelColumn.ColumnMax = value; }
    }

    [JSProperty(Name = "columnMin")]
    [JSDoc("Gets the first column that the definition refers to.")]
    public int ColumnMin
    {
      get { return m_excelColumn.ColumnMin; }
    }

    [JSProperty(Name = "hidden")]
    [JSDoc("Gets or sets a value that indicates if the column is hidden.")]
    public bool Hidden
    {
      get { return m_excelColumn.Hidden; }
      set { m_excelColumn.Hidden = value; }
    }

    [JSProperty(Name = "outlineLevel")]
    [JSDoc("Gets or sets a value that indicates the outline level. Zero if no outline.")]
    public int OutlineLevel
    {
      get { return m_excelColumn.OutlineLevel; }
      set { m_excelColumn.OutlineLevel = value; }
    }

    [JSProperty(Name = "pageBreak")]
    [JSDoc("Gets or sets a value that indicates if there is a manual page-break after the column.")]
    public bool PageBreak
    {
      get { return m_excelColumn.PageBreak; }
      set { m_excelColumn.PageBreak = value; }
    }

    [JSProperty(Name = "style")]
    [JSDoc("Gets the column style.")]
    public ExcelStyleInstance Style
    {
      get { return new ExcelStyleInstance(this.Engine.Object.InstancePrototype, m_excelColumn.Style); }
    }

    [JSProperty(Name = "styleName")]
    [JSDoc("Gets or sets the column style name.")]
    public string StyleName
    {
      get { return m_excelColumn.StyleName; }
      set { m_excelColumn.StyleName = value; }
    }

    [JSProperty(Name = "width")]
    [JSDoc("Gets or sets the width of the column.")]
    public double Width
    {
      get { return m_excelColumn.Width; }
      set { m_excelColumn.Width = value; }
    }
    #endregion
  }
}
