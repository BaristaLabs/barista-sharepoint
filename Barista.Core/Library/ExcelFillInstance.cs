namespace Barista.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using OfficeOpenXml.Style;
  using System;
  using Barista.Extensions;

  [Serializable]
  public class ExcelFillConstructor : ClrFunction
  {
    public ExcelFillConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "ExcelFill", new ExcelFillInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public ExcelFillInstance Construct()
    {
      return new ExcelFillInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class ExcelFillInstance : ObjectInstance
  {
    private readonly ExcelFill m_excelFill;

    public ExcelFillInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public ExcelFillInstance(ObjectInstance prototype, ExcelFill excelFill)
      : this(prototype)
    {
      if (excelFill == null)
        throw new ArgumentNullException("excelFill");

      m_excelFill = excelFill;
    }

    public ExcelFill ExcelFill
    {
      get { return m_excelFill; }
    }

    [JSProperty(Name = "backgroundColor")]
    [JSDoc("Gets the background color of the fill.")]
    public ExcelColorInstance BackgroundColor
    {
      get { return new ExcelColorInstance(this.Engine.Object.InstancePrototype, m_excelFill.BackgroundColor); }
    }

    [JSProperty(Name = "gradient")]
    [JSDoc("Gets the gradient of the fill.")]
    public ExcelGradientFillInstance Gradient
    {
      get
      {
        if (m_excelFill.PatternType == ExcelFillStyle.None || m_excelFill.PatternType == ExcelFillStyle.Solid)
          return null;

        return new ExcelGradientFillInstance(this.Engine.Object.InstancePrototype, m_excelFill.Gradient);
      }
    }

    [JSProperty(Name = "patternColor")]
    [JSDoc("Gets the pattern color of the fill.")]
    public ExcelColorInstance PatternColor
    {
      get { return new ExcelColorInstance(this.Engine.Object.InstancePrototype, m_excelFill.PatternColor); }
    }

    [JSProperty(Name = "patternType")]
    [JSDoc("Gets the pattern type of the fill. (None, Solid, DarkDown, DarkGray, DarkGrid, DarkHorizontal, DarkTrellis, DarkUp, DarkVertical, Gray0625, Gray125, LightDown, LightGray, LightGrid, LightHorizontal, LightTrellis, LightUp, LightVertical, MediumGray) ")]
    public string PatternType
    {
      get { return m_excelFill.PatternType.ToString(); }
      set
      {
        ExcelFillStyle patternType;
        if (value.TryParseEnum(true, out patternType))
          m_excelFill.PatternType = patternType;
      }
    }
  }
}
