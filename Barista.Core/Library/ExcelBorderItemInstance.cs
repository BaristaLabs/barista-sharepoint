namespace Barista.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using OfficeOpenXml.Style;
  using System;
  using Barista.Extensions;

  [Serializable]
  public class ExcelBorderItemConstructor : ClrFunction
  {
    public ExcelBorderItemConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "ExcelBorderItem", new ExcelBorderItemInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public ExcelBorderItemInstance Construct()
    {
      return new ExcelBorderItemInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class ExcelBorderItemInstance : ObjectInstance
  {
    private readonly ExcelBorderItem m_excelBorderItem;

    public ExcelBorderItemInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public ExcelBorderItemInstance(ObjectInstance prototype, ExcelBorderItem excelBorderItem)
      : this(prototype)
    {
      if (excelBorderItem == null)
        throw new ArgumentNullException("excelBorderItem");

      m_excelBorderItem = excelBorderItem;
    }

    public ExcelBorderItem ExcelBorderItem
    {
      get { return m_excelBorderItem; }
    }

    [JSProperty(Name = "style")]
    [JSDoc("Gets or sets the style of the border.")]
    public string Style
    {
      get { return m_excelBorderItem.Style.ToString(); }
      set
      {
        if (value == null)
          m_excelBorderItem.Style = ExcelBorderStyle.None;
        else
        {
          ExcelBorderStyle style;
          if (value.TryParseEnum(true, out style))
            m_excelBorderItem.Style = style;
        }
      }
    }

    [JSProperty(Name = "color")]
    [JSDoc("Gets the color of the border.")]
    public ExcelColorInstance Color
    {
      get { return new ExcelColorInstance(this.Engine.Object.InstancePrototype, m_excelBorderItem.Color); }
    }
  }
}
