namespace Barista.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using OfficeOpenXml.Style;
  using System;
  using Barista.Extensions;

  [Serializable]
  public class ExcelStyleConstructor : ClrFunction
  {
    public ExcelStyleConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "ExcelStyle", new ExcelStyleInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public ExcelStyleInstance Construct()
    {
      return new ExcelStyleInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class ExcelStyleInstance : ObjectInstance
  {
    private readonly ExcelStyle m_excelStyle;

    public ExcelStyleInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public ExcelStyleInstance(ObjectInstance prototype, ExcelStyle excelStyle)
      : this(prototype)
    {
      if (excelStyle == null)
        throw new ArgumentNullException("excelStyle");

      m_excelStyle = excelStyle;
    }

    public ExcelStyle ExcelStyle
    {
      get { return m_excelStyle; }
    }

    [JSProperty(Name = "border")]
    [JSDoc("Gets or sets the border.")]
    public ExcelBorderInstance Border
    {
      get { return new ExcelBorderInstance(this.Engine.Object.InstancePrototype, m_excelStyle.Border); }
      set
      {
        if (value != null)
          m_excelStyle.Border = value.Border;
      }
    }

    [JSProperty(Name = "fill")]
    [JSDoc("Gets or sets the fill.")]
    public ExcelFillInstance Fill
    {
      get { return new ExcelFillInstance(this.Engine.Object.InstancePrototype, m_excelStyle.Fill); }
      set
      {
        if (value != null)
          m_excelStyle.Fill = value.ExcelFill;
      }
    }

    [JSProperty(Name = "font")]
    [JSDoc("Gets or sets the font.")]
    public ExcelFontInstance Font
    {
      get { return new ExcelFontInstance(this.Engine.Object.InstancePrototype, m_excelStyle.Font); }
      set
      {
        if (value != null)
          m_excelStyle.Font = value.ExcelFont;
      }
    }

    [JSProperty(Name = "hidden")]
    [JSDoc("Gets or sets a value that indicates if the cell (range) is hidden.")]
    public bool Hidden
    {
      get { return m_excelStyle.Hidden; }
      set { m_excelStyle.Hidden = value; }
    }

    [JSProperty(Name = "horizontalAlignment")]
    [JSDoc("Gets or sets the horizontal alignment of the cell. (Center, CenterContinuous, Distributed, Fill, General, Justify, Left, Right)")]
    public string HorizontalAlignment
    {
      get { return m_excelStyle.HorizontalAlignment.ToString(); }
      set
      {
        ExcelHorizontalAlignment horizontalAlignment;
        if (value.TryParseEnum(true, out horizontalAlignment))
          m_excelStyle.HorizontalAlignment = horizontalAlignment;
      }
    }

    [JSProperty(Name = "indent")]
    [JSDoc("Gets or sets a value that indicates indent of the cell.")]
    public int Indent
    {
      get { return m_excelStyle.Indent; }
      set { m_excelStyle.Indent = value; }
    }

    [JSProperty(Name = "locked")]
    [JSDoc("Gets or sets a value that indicates if the cell is locked.")]
    public bool Locked
    {
      get { return m_excelStyle.Locked; }
      set { m_excelStyle.Locked = value; }
    }

    [JSProperty(Name = "numberFormat")]
    [JSDoc("Gets or sets the number format of the cell.")]
    public ExcelNumberFormatInstance NumberFormat
    {
      get { return new ExcelNumberFormatInstance(this.Engine.Object.InstancePrototype, m_excelStyle.Numberformat); }
      set
      {
        if (value != null)
          m_excelStyle.Numberformat = value.ExcelNumberFormat;
      }
    }

    [JSProperty(Name = "readingOrder")]
    [JSDoc("Gets or sets the reading order of the cell. (ContextDependent, RightToLeft, LeftToRight)")]
    public string ReadingOrder
    {
      get { return m_excelStyle.ReadingOrder.ToString(); }
      set
      {
        ExcelReadingOrder readingOrder;
        if (value.TryParseEnum(true, out readingOrder))
          m_excelStyle.ReadingOrder = readingOrder;
      }
    }

    [JSProperty(Name = "shrinkToFit")]
    [JSDoc("Gets or sets a value that indicates if the cell should shrink to fit.")]
    public bool ShrinkToFit
    {
      get { return m_excelStyle.ShrinkToFit; }
      set { m_excelStyle.ShrinkToFit = value; }
    }

    [JSProperty(Name = "textRotation")]
    [JSDoc("Gets or sets a value that indicates the rotation of the cell's text.")]
    public int TextRotation
    {
      get { return m_excelStyle.TextRotation; }
      set { m_excelStyle.TextRotation = value; }
    }

    [JSProperty(Name = "verticalAlignment")]
    [JSDoc("Gets or sets a value that indicates the vertical alignment of the cell. (Bottom, Center Distributed, Justify, Top)")]
    public string VerticalAlignment
    {
      get { return m_excelStyle.VerticalAlignment.ToString(); }
      set
      {
        ExcelVerticalAlignment verticalAlignment;
        if (value.TryParseEnum(true, out verticalAlignment))
          m_excelStyle.VerticalAlignment = verticalAlignment;
      }
    }

    [JSProperty(Name = "wrapText")]
    [JSDoc("Gets or sets a value that indicates if the cell should wrap text.")]
    public bool WrapText
    {
      get { return m_excelStyle.WrapText; }
      set { m_excelStyle.WrapText = value; }
    }

    [JSProperty(Name = "xfid")]
    [JSDoc("Gets or sets a the excel fid.")]
    public int Xfid
    {
      get { return m_excelStyle.XfId; }
      set { m_excelStyle.XfId = value; }
    }
  }
}
