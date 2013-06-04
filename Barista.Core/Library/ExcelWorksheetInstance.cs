namespace Barista.Library
{
  using System.Collections.Generic;
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using OfficeOpenXml;
  using System;

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

    [JSFunction(Name = "convertToJson")]
    public ArrayInstance ConvertToJson(object hasHeader)
    {
      if (m_excelWorksheet.Dimension == null)
        return null;

      var bHasHeader = true;

      if (hasHeader != Undefined.Value && hasHeader != null && TypeConverter.ToBoolean(hasHeader) == false)
        bHasHeader = false;

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
  }
}
