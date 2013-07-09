namespace Barista.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using Barista.Extensions;
  using OfficeOpenXml.Style;
  using System;

  [Serializable]
  public class ExcelColorConstructor : ClrFunction
  {
    public ExcelColorConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "ExcelColor", new ExcelColorInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public ExcelColorInstance Construct()
    {
      return new ExcelColorInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class ExcelColorInstance : ObjectInstance
  {
    private readonly ExcelColor m_excelColor;

    public ExcelColorInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public ExcelColorInstance(ObjectInstance prototype, ExcelColor excelColor)
      : this(prototype)
    {
      if (excelColor == null)
        throw new ArgumentNullException("excelColor");

      m_excelColor = excelColor;
    }

    public ExcelColor ExcelColor
    {
      get { return m_excelColor; }
    }

    [JSProperty(Name = "indexed")]
    [JSDoc("Gets or sets the indexed value of the color.")]
    public int Indexed
    {
      get { return m_excelColor.Indexed; }
      set { m_excelColor.Indexed = value; }
    }

    [JSProperty(Name = "rgb")]
    [JSDoc("Gets the rgb value of the color.")]
    public string Rgb
    {
      get { return m_excelColor.Rgb; }
    }

    [JSProperty(Name = "theme")]
    [JSDoc("Gets the theme value of the color.")]
    public string Theme
    {
      get { return m_excelColor.Theme; }
    }

    [JSProperty(Name = "tint")]
    [JSDoc("Gets or sets the tint value of the color.")]
    public double Tint
    {
      get { return (double)m_excelColor.Tint; }
      set { m_excelColor.Tint = (decimal)value; }
    }

    [JSFunction(Name = "setColorFromArgb")]
    [JSDoc("Sets the value of the color.")]
    public void SetColorArgb(int alpha, int red, int green, int blue)
    {
      var color = System.Drawing.Color.FromArgb(alpha, red, green, blue);
      m_excelColor.SetColor(color);
    }

    [JSFunction(Name = "setColorFromRgb")]
    [JSDoc("Sets the value of the color.")]
    public void SetColorRgb(int red, int green, int blue)
    {
      var color = System.Drawing.Color.FromArgb(red, green, blue);
      m_excelColor.SetColor(color);
    }

    [JSFunction(Name = "setColorFromKnownColor")]
    [JSDoc("Sets the value of the color.")]
    public void SetColorFromKnownColor(string knownColorName)
    {
      System.Drawing.KnownColor knownColor;
      if (knownColorName.TryParseEnum(true, out knownColor))
      {
        var color = System.Drawing.Color.FromKnownColor(knownColor);
        m_excelColor.SetColor(color);
      }
    }

    [JSFunction(Name = "setColorFromName")]
    [JSDoc("Sets the value of the color.")]
    public void SetColorFromName(string name)
    {
      var color = System.Drawing.Color.FromName(name);
      m_excelColor.SetColor(color);
    }
  }
}
