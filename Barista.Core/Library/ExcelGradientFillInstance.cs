namespace Barista.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using OfficeOpenXml.Style;
  using System;
  using Barista.Extensions;

  [Serializable]
  public class ExcelGradientFillConstructor : ClrFunction
  {
    public ExcelGradientFillConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "ExcelGradientFill", new ExcelGradientFillInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public ExcelGradientFillInstance Construct()
    {
      return new ExcelGradientFillInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class ExcelGradientFillInstance : ObjectInstance
  {
    private readonly ExcelGradientFill m_excelGradientFill;

    public ExcelGradientFillInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public ExcelGradientFillInstance(ObjectInstance prototype, ExcelGradientFill excelGradientFill)
      : this(prototype)
    {
      if (excelGradientFill == null)
        throw new ArgumentNullException("excelGradientFill");

      m_excelGradientFill = excelGradientFill;
    }

    public ExcelGradientFill ExcelGradientFill
    {
      get { return m_excelGradientFill; }
    }

    [JSProperty(Name = "bottom")]
    [JSDoc("Gets or sets a value that specifies in percentage format (from the top to the bottom) the position of the bottom edge of the inner rectangle (color1)")]
    public double Bottom
    {
      get { return m_excelGradientFill.Bottom; }
      set { m_excelGradientFill.Bottom = value; }
    }

    [JSProperty(Name = "color1")]
    [JSDoc("Gets a value that specifies the gradient color1")]
    public ExcelColor Color1
    {
      get { return m_excelGradientFill.Color1; }
    }

    [JSProperty(Name = "color2")]
    [JSDoc("Gets a value that specifies the gradient color2")]
    public ExcelColor Color2
    {
      get { return m_excelGradientFill.Color2; }
    }

    [JSProperty(Name = "degree")]
    [JSDoc("Gets or sets a value that specifies angle of the linear gradient")]
    public double Degree
    {
      get { return m_excelGradientFill.Degree; }
      set { m_excelGradientFill.Degree = value; }
    }

    [JSProperty(Name = "left")]
    [JSDoc("Gets or sets a value that specifies in percentage format (from the left to the right) the position of the left edge of the inner rectangle (color1)")]
    public double Left
    {
      get { return m_excelGradientFill.Left; }
      set { m_excelGradientFill.Left = value; }
    }

    [JSProperty(Name = "right")]
    [JSDoc("Gets or sets a value that specifies in percentage format (from the left to the right) the position of the right edge of the inner rectangle (color1)")]
    public double Right
    {
      get { return m_excelGradientFill.Right; }
      set { m_excelGradientFill.Right = value; }
    }

    [JSProperty(Name = "top")]
    [JSDoc("Gets or sets a value that specifies in percentage format (from the left to the right) the position of the top edge of the inner rectangle (color1)")]
    public double Top
    {
      get { return m_excelGradientFill.Top; }
      set { m_excelGradientFill.Top = value; }
    }

    [JSProperty(Name = "type")]
    [JSDoc("Gets or sets the type of gradient.")]
    public string Type
    {
      get { return m_excelGradientFill.Type.ToString(); }
      set
      {
        ExcelFillGradientType type;
        if (value.TryParseEnum(true, out type))
          m_excelGradientFill.Type = type;
      }
    }
  }
}
