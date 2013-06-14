namespace Barista.Library
{
  using System.Collections.Generic;
  using System.Linq;
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using OfficeOpenXml;
  using System;
  using Barista.Extensions;

  [Serializable]
  public class ExcelWorksheetConstructor : ClrFunction
  {
    public ExcelWorksheetConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "ExcelWorksheet", new ExcelWorksheetInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public ExcelWorksheetInstance Construct()
    {
      return new ExcelWorksheetInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class ExcelWorksheetInstance : ObjectInstance
  {
    private readonly ExcelWorksheet m_excelWorksheet;

    public ExcelWorksheetInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public ExcelWorksheetInstance(ObjectInstance prototype, ExcelWorksheet excelWorksheet)
      : this(prototype)
    {
      if (excelWorksheet == null)
        throw new ArgumentNullException("excelWorksheet");

      m_excelWorksheet = excelWorksheet;
    }

    public ExcelWorksheet ExcelWorksheet
    {
      get { return m_excelWorksheet; }
    }

    #region Properties

    [JSProperty(Name = "hidden")]
    public string Hidden
    {
      get
      {
        switch (m_excelWorksheet.Hidden)
        {
          case eWorkSheetHidden.Hidden:
            return "Hidden";
          case eWorkSheetHidden.VeryHidden:
            return "VeryHidden";
          case eWorkSheetHidden.Visible:
            return "Visible";
        }
        return null;
      }
      set
      {
        eWorkSheetHidden hiddenValue;
        if (value.TryParseEnum(true, eWorkSheetHidden.Visible, out hiddenValue))
        {
          m_excelWorksheet.Hidden = hiddenValue;
        }
      }
    }

    [JSProperty(Name = "index")]
    public int Index
    {
      get { return m_excelWorksheet.Index; }
    }

    [JSProperty(Name = "name")]
    public string Name
    {
      get { return m_excelWorksheet.Name; }
      set { m_excelWorksheet.Name = value; }
    }
    #endregion

    #region Functions
    [JSFunction(Name = "convertToJson")]
    public ArrayInstance ConvertToJson(object hasHeader)
    {
      if (m_excelWorksheet.Dimension == null)
        return null;

      var bHasHeader = !(hasHeader != Undefined.Value && hasHeader != null && TypeConverter.ToBoolean(hasHeader) == false);

      var result = this.Engine.Array.Construct();
      var startPos = m_excelWorksheet.Dimension.Start.Row;
      
      var propertyNames = new List<string>();
      if (bHasHeader)
      {
        for (var c = m_excelWorksheet.Dimension.Start.Column; c <= m_excelWorksheet.Dimension.End.Column; c++)
        {
          propertyNames.Add(m_excelWorksheet.Cells[startPos, c].GetValue<string>());
        }

        startPos = startPos + 1;
      }
      else
      {
        for (var c = m_excelWorksheet.Dimension.Start.Column; c <= m_excelWorksheet.Dimension.End.Column; c++)
        {
          var iColumnNumber = c;
          var sCol = "";
          do
          {
            sCol = ((char) ('A' + ((iColumnNumber - 1)%26))) + sCol;
            iColumnNumber = (iColumnNumber - ((iColumnNumber - 1)%26))/26;
          } while (iColumnNumber > 0);

          propertyNames.Add(sCol);
        }
      }

      for (var rowPos = startPos; rowPos <= m_excelWorksheet.Dimension.End.Row; rowPos++)
      {
        var rowObject = this.Engine.Object.Construct();

        for (var c = m_excelWorksheet.Dimension.Start.Column; c <= m_excelWorksheet.Dimension.End.Column; c++)
        {
          var cell = m_excelWorksheet.Cells[rowPos, c];
          rowObject.SetPropertyValue(propertyNames[c - 1], cell.Value, false);
        }

        ArrayInstance.Push(result, rowObject);
      }

      return result;
    }

    [JSFunction(Name = "loadFromJson")]
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
          m_excelWorksheet.Cells[currentRow, i].Value = header[i-1];
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

            SetValue(currentRow, c, propertyValue);
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

            SetValue(currentRow, c, propertyValue);
          }
        }

        currentRow++;
      }
    }

    [JSFunction(Name="deleteRow")]
    public void DeleteRow(int rowFrom, int rows, object shiftOtherRowsUp)
    {
      var bShiftOtherRowsUp = JurassicHelper.GetTypedArgumentValue(this.Engine, shiftOtherRowsUp, false);

      m_excelWorksheet.DeleteRow(rowFrom, rows, bShiftOtherRowsUp);
    }

    [JSFunction(Name = "insertRow")]
    public void InsertRow(int rowFrom, int rows, object copyStylesFromRow)
    {
      if (TypeUtilities.IsNumeric(copyStylesFromRow))
        m_excelWorksheet.InsertRow(rowFrom, rows, TypeConverter.ToInteger(copyStylesFromRow));
      else
        m_excelWorksheet.InsertRow(rowFrom, rows);
    }

    [JSFunction(Name = "getCell")]
    public ExcelRangeInstance GetCell(int row, int column)
    {
      return new ExcelRangeInstance(this.Engine.Object.InstancePrototype, m_excelWorksheet.Cells[row, column]);
    }

    [JSFunction(Name = "getAllCells")]
    public ExcelRangeInstance GetAllCells()
    {
      return new ExcelRangeInstance(this.Engine.Object.InstancePrototype, m_excelWorksheet.Cells);
    }

    [JSFunction(Name = "getCellAtAddress")]
    public ExcelRangeInstance GetCellAtAddress(string address)
    {
      return new ExcelRangeInstance(this.Engine.Object.InstancePrototype, m_excelWorksheet.Cells[address]);
    }
    [JSFunction(Name = "getColumn")]
    public ExcelColumnInstance GetColumn(int row)
    {
      return new ExcelColumnInstance(this.Engine.Object.InstancePrototype, m_excelWorksheet.Column(row));
    }

    [JSFunction(Name = "getRow")]
    public ExcelRowInstance GetRow(int row)
    {
      return new ExcelRowInstance(this.Engine.Object.InstancePrototype, m_excelWorksheet.Row(row));
    }

    [JSFunction(Name = "getSelectedRange")]
    public ExcelRangeInstance GetSelectedRange()
    {
      return new ExcelRangeInstance(this.Engine.Object.InstancePrototype, m_excelWorksheet.SelectedRange);
    }

    [JSFunction(Name = "getValue")]
    public object GetValue(int row, int column)
    {
      var value = m_excelWorksheet.GetValue(row, column);
      if (value is int || value is string || value is double || value is bool)
        return value;

      return m_excelWorksheet.GetValue<string>(row, column);
    }

    [JSFunction(Name = "select")]
    public void Select(string address, object selectSheet)
    {
      var bSelectSheet = JurassicHelper.GetTypedArgumentValue(this.Engine, selectSheet, false);
      m_excelWorksheet.Select(address, bSelectSheet);
    }

    [JSFunction(Name = "setValue")]
    public void SetValue(int row, int column, object value)
    {
      if (TypeUtilities.IsPrimitive(value))
        m_excelWorksheet.SetValue(row, column, value);
      else
      {
        var strValue = JSONObject.Stringify(this.Engine, value, null, null);
        m_excelWorksheet.SetValue(row, column, strValue);
      }
    }

    [JSFunction(Name = "setValueAtAddress")]
    public void SetValueAtAddress(string address, object value)
    {
      if (TypeUtilities.IsPrimitive(value))
        m_excelWorksheet.SetValue(address, value);
      else
      {
        var strValue = JSONObject.Stringify(this.Engine, value, null, null);
        m_excelWorksheet.SetValue(address, strValue);
      }
    }

    #endregion
  }
}
