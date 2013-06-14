namespace Barista.Library
{
  using System.Collections.Generic;
  using System.Linq;
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
    [JSDoc("Gets or sets the address for the range.")]
    public string Address
    {
      get { return m_excelRange.Address; }
      set { m_excelRange.Address = value; }
    }

    [JSProperty(Name = "autoFilter")]
    [JSDoc("Gets or sets the autofilter for the range.")]
    public bool AutoFilter
    {
      get { return m_excelRange.AutoFilter; }
      set { m_excelRange.AutoFilter = value; }
    }

    [JSProperty(Name = "current")]
    [JSDoc("Gets the current Address.")]
    public ExcelAddressInstance Current
    {
      get { return new ExcelAddressInstance(this.Engine.Object.Prototype, m_excelRange.Current); }
    }

    [JSProperty(Name = "end")]
    [JSDoc("Gets the row and column of the bottom-right cell.")]
    public ExcelCellAddressInstance End
    {
      get { return new ExcelCellAddressInstance(this.Engine.Object.Prototype, m_excelRange.End); }
    }

    [JSProperty(Name = "formula")]
    [JSDoc("Gets or sets the formula for the range.")]
    public string Formula
    {
      get { return m_excelRange.Formula; }
      set { m_excelRange.Formula = value; }
    }

    [JSProperty(Name = "formulaR1C1")]
    [JSDoc("Gets or sets a formula in R1C1 format.")]
    public string FormulaR1C1
    {
      get { return m_excelRange.FormulaR1C1; }
      set { m_excelRange.FormulaR1C1 = value; }
    }

    [JSProperty(Name = "fullAddress")]
    [JSDoc("Gets the range address including sheet name.")]
    public string FullAddress
    {
      get { return m_excelRange.FullAddress; }
    }

    [JSProperty(Name = "fullAddressAbsolute")]
    [JSDoc("Gets the range address including sheet name.")]
    public string FullAddressAbsolute
    {
      get { return m_excelRange.FullAddressAbsolute; }
    }

    [JSProperty(Name = "hyperlink")]
    [JSDoc("Gets or sets the hyperlink property for the range.")]
    public string Hyperlink
    {
      get { return m_excelRange.Hyperlink.ToString(); }
      set { m_excelRange.Hyperlink = new Uri(value); }
    }

    [JSProperty(Name = "isArrayFormula")]
    [JSDoc("Gets or sets a value that indicates if the range is part of a range formula.")]
    public bool IsArrayFormula
    {
      get { return m_excelRange.IsArrayFormula; }
    }

    [JSProperty(Name = "isName")]
    [JSDoc("Gets a value that indicates if the range address is a defined name.")]
    public bool IsName
    {
      get { return m_excelRange.IsName; }
    }

    [JSProperty(Name = "isRichText")]
    [JSDoc("Gets a value that indicates if the range is in rich text format.")]
    public bool IsRichText
    {
      get { return m_excelRange.IsRichText; }
    }

    [JSProperty(Name = "merge")]
    [JSDoc("Gets or sets a value that indicates if the cells in the range are merged.")]
    public bool Merge
    {
      get { return m_excelRange.Merge; }
      set { m_excelRange.Merge = value; }
    }

    [JSProperty(Name = "start")]
    [JSDoc("Gets the row and column of the top-left cell.")]
    public ExcelCellAddressInstance Start
    {
      get { return new ExcelCellAddressInstance(this.Engine.Object.Prototype, m_excelRange.Start); }
    }

    [JSProperty(Name = "styleId")]
    public int StyleId
    {
      get { return m_excelRange.StyleID; }
      set { m_excelRange.StyleID = value; }
    }

    [JSProperty(Name = "styleName")]
    [JSDoc("Gets or sets the name of the style that applies to the range.")]
    public string StyleName
    {
      get { return m_excelRange.StyleName; }
      set { m_excelRange.StyleName = value; }
    }

    [JSProperty(Name = "text")]
    [JSDoc("Gets the formatted value.")]
    public string Text 
    {
      get { return m_excelRange.Text; }
    }
    #endregion

    #region Functions

    [JSFunction(Name="clear")]
    [JSDoc("Clears all cells in the range.")]
    public void Clear()
    {
      m_excelRange.Clear();
    }

    [JSFunction(Name = "convertToJson")]
    [JSDoc("Converts the contents of the range to a Json Object.")]
    public ArrayInstance ConvertToJson(object hasHeader)
    {
      var bHasHeader = !(hasHeader != Undefined.Value && hasHeader != null && TypeConverter.ToBoolean(hasHeader) == false);

      var result = this.Engine.Array.Construct();
      var startPos = m_excelRange.Start.Row;

      var propertyNames = new List<string>();
      if (bHasHeader)
      {
        for (var c = m_excelRange.Start.Column; c <= m_excelRange.End.Column; c++)
        {
          propertyNames.Add(m_excelRange[startPos, c].GetValue<string>());
        }

        startPos = startPos + 1;
      }
      else
      {
        for (var c = m_excelRange.Start.Column; c <= m_excelRange.End.Column; c++)
        {
          var iColumnNumber = c;
          var sCol = "";
          do
          {
            sCol = ((char)('A' + ((iColumnNumber - 1) % 26))) + sCol;
            iColumnNumber = (iColumnNumber - ((iColumnNumber - 1) % 26)) / 26;
          } while (iColumnNumber > 0);

          propertyNames.Add(sCol);
        }
      }

      for (var rowPos = startPos; rowPos <= m_excelRange.End.Row; rowPos++)
      {
        var rowObject = this.Engine.Object.Construct();

        for (var c = m_excelRange.Start.Column; c <= m_excelRange.End.Column; c++)
        {
          var cell = m_excelRange[rowPos, c];
          rowObject.SetPropertyValue(propertyNames[c - 1], cell.Value, false);
        }

        ArrayInstance.Push(result, rowObject);
      }

      return result;
    }

    [JSFunction(Name = "reset")]
    public void Reset()
    {
      m_excelRange.Reset();
    }

    [JSFunction(Name = "loadFromCSV")]
    [JSDoc("Loads a CSV string into the range starting from the top left cell.")]
    public void LoadFromCSV(string csv)
    {
      m_excelRange.LoadFromText(csv);
    }

    [JSFunction(Name = "loadFromJson")]
    [JSDoc("Loads an array of json objects into the range starting from the top left cell.")]
    public void LoadFromJson(object array, object hasHeader)
    {
      if (array == null || array == Null.Value || array == Undefined.Value || (array is ArrayInstance) == false ||
          (array as ArrayInstance).Length == 0)
        return;

      var jsonArray = array as ArrayInstance;

      var bHasHeader = JurassicHelper.GetTypedArgumentValue(this.Engine, hasHeader, true);
      List<string> header = null;

      //If we have a header, populate the first row with header info.
      var currentRow = 1;
      if (bHasHeader)
      {
        var firstRecord = jsonArray[0] as ObjectInstance;
        if (firstRecord == null)
          return;

        header = firstRecord.Properties
          .Select(property => property.Name)
          .ToList();

        for (int i = 1; i < header.Count + 1; i++)
          m_excelRange[currentRow, i].Value = header[i - 1];
        currentRow++;
      }

      foreach (var value in jsonArray.ElementValues.OfType<ObjectInstance>())
      {
        if (header == null)
        {
          var properties = value.Properties.ToList();
          for (var c = 1; c < properties.Count + 1; c++)
          {
            var property = properties[c - 1];
            var propertyValue = String.Empty;
            if (property.Value != null)
            {
              propertyValue = property.Value.ToString();
            }

            m_excelRange[currentRow, c].Value = propertyValue;
          }

        }
        else
        {
          for (var c = 1; c < header.Count + 1; c++)
          {
            var key = header[c - 1];
            var propertyValue = String.Empty;
            if (value.HasProperty(key) && value[key] != null)
              propertyValue = value[key].ToString();

            m_excelRange[currentRow, c].Value = propertyValue;
          }
        }

        currentRow++;
      }
    }

    [JSFunction(Name = "getValue")]
    [JSDoc("Returns the value of the range.")]
    public object GetValue()
    {
      var value = m_excelRange.Value;
      if (value is int || value is string || value is double || value is bool)
        return value;

      return m_excelRange.GetValue<string>();
    }

    [JSFunction(Name = "setValue")]
    [JSDoc("Sets the value of the range to the specified value.")]
    public void SetValue(object value)
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
