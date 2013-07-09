namespace Barista.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using System;
  using OfficeOpenXml.Style;

  [Serializable]
  public class ExcelBorderConstructor : ClrFunction
  {
    public ExcelBorderConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "ExcelBorder", new ExcelBorderInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public ExcelBorderInstance Construct()
    {
      return new ExcelBorderInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class ExcelBorderInstance : ObjectInstance
  {
    private readonly Border m_border;

    public ExcelBorderInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public ExcelBorderInstance(ObjectInstance prototype, Border border)
      : this(prototype)
    {
      if (border == null)
        throw new ArgumentNullException("border");

      m_border = border;
    }

    public Border Border
    {
      get { return m_border; }
    }

    [JSProperty(Name = "bottom")]
    [JSDoc("Gets or sets the bottom border.")]
    public ExcelBorderItemInstance Bottom
    {
      get { return new ExcelBorderItemInstance(this.Engine.Object.InstancePrototype, m_border.Bottom); }
    }

    [JSProperty(Name = "diagonal")]
    [JSDoc("Gets or sets the diagonal border.")]
    public ExcelBorderItemInstance Diagonal
    {
      get { return new ExcelBorderItemInstance(this.Engine.Object.InstancePrototype, m_border.Diagonal); }
    }

    [JSProperty(Name = "diagonalDown")]
    [JSDoc("Gets or sets a value that indicates if the border is diagonal down.")]
    public bool DiagonalDown
    {
      get { return m_border.DiagonalDown; }
      set { m_border.DiagonalDown = value; }
    }

    [JSProperty(Name = "diagonalUp")]
    [JSDoc("Gets or sets a value that indicates if the border is diagonal up.")]
    public bool DiagonalUp
    {
      get { return m_border.DiagonalUp; }
      set { m_border.DiagonalUp = value; }
    }

    [JSProperty(Name = "left")]
    [JSDoc("Gets or sets the left border.")]
    public ExcelBorderItemInstance Left
    {
      get { return new ExcelBorderItemInstance(this.Engine.Object.InstancePrototype, m_border.Left); }
    }

    [JSProperty(Name = "right")]
    [JSDoc("Gets or sets the right border.")]
    public ExcelBorderItemInstance Right
    {
      get { return new ExcelBorderItemInstance(this.Engine.Object.InstancePrototype, m_border.Right); }
    }

    [JSProperty(Name = "top")]
    [JSDoc("Gets or sets the right border.")]
    public ExcelBorderItemInstance Top
    {
      get { return new ExcelBorderItemInstance(this.Engine.Object.InstancePrototype, m_border.Top); }
    }
  }
}
