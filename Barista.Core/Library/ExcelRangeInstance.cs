namespace Barista.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using OfficeOpenXml;
  using System;

  [Serializable]
  public class ExcelRangeConstructor : ClrFunction
  {
    public ExcelRangeConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "ExcelRange", new ExcelRangeInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public ExcelRangeInstance Construct()
    {
      return new ExcelRangeInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class ExcelRangeInstance : ObjectInstance
  {
    private readonly ExcelRange m_excelRange;

    public ExcelRangeInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public ExcelRangeInstance(ObjectInstance prototype, ExcelRange excelRange)
      : this(prototype)
    {
      if (excelRange == null)
        throw new ArgumentNullException("excelRange");

      m_excelRange = excelRange;
    }

    public ExcelRange ExcelRange
    {
      get { return m_excelRange; }
    }

    #region Properties

    [JSProperty(Name = "address")]
    public string Address
    {
      get { return m_excelRange.Address; }
      set { m_excelRange.Address = value; }
    }

    [JSProperty(Name = "autoFilter")]
    public bool AutoFilter
    {
      get { return m_excelRange.AutoFilter; }
      set { m_excelRange.AutoFilter = value; }
    }

    [JSProperty(Name = "formula")]
    public string Formula
    {
      get { return m_excelRange.Formula; }
      set { m_excelRange.Formula = value; }
    }

    [JSProperty(Name = "formulaR1C1")]
    public string FormulaR1C1
    {
      get { return m_excelRange.FormulaR1C1; }
      set { m_excelRange.FormulaR1C1 = value; }
    }

    [JSProperty(Name = "fullAddress")]
    public string FullAddress
    {
      get { return m_excelRange.FullAddress; }
    }

    [JSProperty(Name = "fullAddressAbsolute")]
    public string FullAddressAbsolute
    {
      get { return m_excelRange.FullAddressAbsolute; }
    }

    [JSProperty(Name = "hyperlink")]
    public string Hyperlink
    {
      get { return m_excelRange.Hyperlink.ToString(); }
      set { m_excelRange.Hyperlink = new Uri(value); }
    }

    [JSProperty(Name = "isArrayFormula")]
    public bool IsArrayFormula
    {
      get { return m_excelRange.IsArrayFormula; }
    }

    [JSProperty(Name = "isName")]
    public bool IsName
    {
      get { return m_excelRange.IsName; }
    }

    [JSProperty(Name = "isRichText")]
    public bool IsRichText
    {
      get { return m_excelRange.IsRichText; }
    }

    [JSProperty(Name = "merge")]
    public bool Merge
    {
      get { return m_excelRange.Merge; }
      set { m_excelRange.Merge = value; }
    }

    [JSProperty(Name = "styleId")]
    public int StyleId
    {
      get { return m_excelRange.StyleID; }
      set { m_excelRange.StyleID = value; }
    }

    [JSProperty(Name = "styleName")]
    public string StyleName
    {
      get { return m_excelRange.StyleName; }
      set { m_excelRange.StyleName = value; }
    }

    [JSProperty(Name = "text")]
    public string Text 
    {
      get { return m_excelRange.Text; }
    }
    #endregion

    #region Functions

    [JSFunction(Name="clear")]
    public void Clear()
    {
      m_excelRange.Clear();
    }

    [JSFunction(Name = "reset")]
    public void Reset()
    {
      m_excelRange.Reset();
    }

    [JSFunction(Name = "loadFromCSV")]
    public void LoadFromCSV(string csv)
    {
      m_excelRange.LoadFromText(csv);
    }

    [JSFunction(Name = "getValue")]
    public object GetValue(int row, int column)
    {
      var value = m_excelRange.Value;
      if (value is int || value is string || value is double || value is bool)
        return value;

      return m_excelRange.GetValue<string>();
    }

    [JSFunction(Name = "setValue")]
    public void SetValue(int row, int column, object value)
    {
      if (TypeUtilities.IsPrimitive(value))
        m_excelRange.Value = value;
      else
      {
        var strValue = JSONObject.Stringify(this.Engine, value, null, null);
        m_excelRange.Value = strValue;
      }
    }
    #endregion
  }
}
