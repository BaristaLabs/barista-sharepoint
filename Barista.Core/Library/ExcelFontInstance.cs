namespace Barista.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using OfficeOpenXml.Style;
  using System;
  using Barista.Extensions;

  [Serializable]
  public class ExcelFontConstructor : ClrFunction
  {
    public ExcelFontConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "ExcelFont", new ExcelFontInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public ExcelFontInstance Construct()
    {
      return new ExcelFontInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class ExcelFontInstance : ObjectInstance
  {
    private readonly ExcelFont m_excelFont;

    public ExcelFontInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public ExcelFontInstance(ObjectInstance prototype, ExcelFont excelFont)
      : this(prototype)
    {
      if (excelFont == null)
        throw new ArgumentNullException("excelFont");

      m_excelFont = excelFont;
    }

    public ExcelFont ExcelFont
    {
      get { return m_excelFont; }
    }

    [JSProperty(Name = "bold")]
    [JSDoc("Gets or sets a value that indicates if the font is bold.")]
    public bool Bold
    {
      get { return m_excelFont.Bold; }
      set { m_excelFont.Bold = value; }
    }

    [JSProperty(Name = "color")]
    [JSDoc("Gets or sets a value that indicates the font color.")]
    public ExcelColorInstance Color
    {
      get { return new ExcelColorInstance(this.Engine.Object.InstancePrototype, m_excelFont.Color); }
    }

    [JSProperty(Name = "family")]
    [JSDoc("Gets or sets a value that indicates the font family")]
    public int Family
    {
      get { return m_excelFont.Family; }
      set { m_excelFont.Family = value; }
    }

    [JSProperty(Name = "italic")]
    [JSDoc("Gets or sets a value that indicates if the font is italic.")]
    public bool Italic
    {
      get { return m_excelFont.Italic; }
      set { m_excelFont.Italic = value; }
    }

    [JSProperty(Name = "name")]
    [JSDoc("Gets or sets a value that indicates the font name.")]
    public string Name
    {
      get { return m_excelFont.Name; }
      set { m_excelFont.Name = value; }
    }

    [JSProperty(Name = "scheme")]
    [JSDoc("Gets or sets a value that indicates the font scheme.")]
    public string Scheme
    {
      get { return m_excelFont.Scheme; }
      set { m_excelFont.Scheme = value; }
    }

    [JSProperty(Name = "size")]
    [JSDoc("Gets or sets a value that indicates the font size.")]
    public double Size
    {
      get { return m_excelFont.Size; }
      set { m_excelFont.Size = (float)value; }
    }

    [JSProperty(Name = "strike")]
    [JSDoc("Gets or sets a value that indicates if the font is strike-through.")]
    public bool Strike
    {
      get { return m_excelFont.Strike; }
      set { m_excelFont.Strike = value; }
    }

    [JSProperty(Name = "underLine")]
    [JSDoc("Gets or sets a value that indicates if the font is underlined.")]
    public bool Underline
    {
      get { return m_excelFont.UnderLine; }
      set { m_excelFont.UnderLine = value; }
    }

    [JSProperty(Name = "underLineType")]
    [JSDoc("Gets or sets a value that indicates the font's underline type. (None, Single, Double, SingleAccounting, DoubleAccounting)")]
    public string UnderlineType
    {
      get { return m_excelFont.UnderLineType.ToString(); }
      set
      {
        ExcelUnderLineType underLineType;
        if (value.TryParseEnum(true, out underLineType))
          m_excelFont.UnderLineType = underLineType;
      }
    }

    [JSProperty(Name = "verticalAlign")]
    [JSDoc("Gets or sets a value that indicates the font's vertical alignment. (None, Superscript, Subscript)")]
    public string VerticalAlign
    {
      get { return m_excelFont.VerticalAlign.ToString(); }
      set
      {
        ExcelVerticalAlignmentFont verticalAlignmentFont;
        if (value.TryParseEnum(true, out verticalAlignmentFont))
          m_excelFont.VerticalAlign = verticalAlignmentFont;
      }
    }
  }
}
