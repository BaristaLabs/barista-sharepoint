namespace Barista.Library
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Reflection;
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using OfficeOpenXml;

  [Serializable]
  public class ExcelDocumentConstructor : ClrFunction
  {
    public ExcelDocumentConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "ExcelDocument", new ExcelDocumentInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public ExcelDocumentInstance Construct(params object[] parameters)
    {
      var firstArg = parameters.FirstOrDefault();
      ExcelDocumentInstance documentInstance;

      if (firstArg != null)
      {

        if (firstArg is Base64EncodedByteArrayInstance)
        {
          //Create the excel document instance from a byte array.
          var byteArray = firstArg as Base64EncodedByteArrayInstance;
          using (var ms = new MemoryStream(byteArray.Data))
          {
            var secondArg = parameters.ElementAtOrDefault(1);

            if (secondArg != null && TypeUtilities.IsString(secondArg))
            {
              var passParam = TypeConverter.ToString(parameters[1]);
              documentInstance = new ExcelDocumentInstance(this.InstancePrototype, ms, passParam);
            }
            else
            {
              documentInstance = new ExcelDocumentInstance(this.InstancePrototype, ms);
            }
          }
        }
        else if (TypeUtilities.IsString(firstArg))
        {
          //String parameter -- assume to be a url to an excel document.
          throw new NotImplementedException();
        }
        else
        {
          throw new JavaScriptException(this.Engine, "Error",
                                        "An Excel Document cannot be constructed with the specified argument.");
        }
      }
      else
      {
        documentInstance = new ExcelDocumentInstance(this.Engine.Object.Prototype);
      }

      return documentInstance;
    }
  }

  public class ExcelDocumentInstance : ObjectInstance, IDisposable
  {
    protected readonly ExcelPackage m_excelPackage;

    public ExcelDocumentInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions(GetType(), BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

      m_excelPackage = new ExcelPackage();
    }

    public ExcelDocumentInstance(ObjectInstance prototype, Stream stream)
      :this(prototype)
    {
      m_excelPackage = new ExcelPackage(stream);
    }

    public ExcelDocumentInstance(ObjectInstance prototype, Stream stream, string password)
      : this(prototype)
    {
      m_excelPackage = new ExcelPackage(stream, password);
    }

    public ExcelPackage ExcelPackage
    {
      get { return m_excelPackage; }
    }

    #region Properties
    [JSProperty(Name = "doAdjustDrawings")]
    public bool DoAdjustDrawings
    {
      get { return m_excelPackage.DoAdjustDrawings; }
      set { m_excelPackage.DoAdjustDrawings = value; }
    }

    [JSProperty(Name = "workbook")]
    public ExcelWorkbookInstance Workbook
    {
      get { return new ExcelWorkbookInstance(this.Engine.Object.Prototype, m_excelPackage.Workbook); }
    }
    #endregion

    #region Functions

    [JSFunction(Name = "getBytes")]
    public Base64EncodedByteArrayInstance GetBytes()
    {
      var data = m_excelPackage.GetAsByteArray();

      var result = new Base64EncodedByteArrayInstance(this.Engine.Object.InstancePrototype, data)
        {
          MimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        };

      //if (m_sourceFile != null)
      //  result.FileName = m_sourceFile.Name;

      return result;
    }

    [JSFunction(Name = "getWorksheetAsJson")]
    public ArrayInstance GetWorksheetAsJson(object worksheetName, object hasHeader)
    {
      var bHasHeader = true;

      ExcelWorksheet worksheet;
      if (TypeUtilities.IsString(worksheetName))
      {
        var strWorksheetName = TypeConverter.ToString(worksheetName);
        worksheet = m_excelPackage.Workbook.Worksheets.FirstOrDefault(w => w.Name == strWorksheetName);

        if (worksheet == null)
          throw new JavaScriptException(this.Engine, "Error", "A worksheet with the specified name does not exist.");
      }
      else if (TypeUtilities.IsNumeric(worksheetName))
      {
        var index = TypeConverter.ToInteger(worksheetName);
        worksheet = m_excelPackage.Workbook.Worksheets.ElementAtOrDefault(index);

        if (worksheet == null)
          throw new JavaScriptException(this.Engine, "Error", "A worksheet at the specified index does not exist.");
      }
      else
      {
        throw new JavaScriptException(this.Engine, "Error",
                                      "The first parameter must either be the name or index of a worksheet.");
      }

      if (hasHeader != Undefined.Value && hasHeader != null && TypeConverter.ToBoolean(hasHeader) == false)
        bHasHeader = false;

      if (worksheet.Dimension == null)
        return null;

      //Start processing.
      var result = this.Engine.Array.Construct();
      var startPos = worksheet.Dimension.Start.Row;

      var propertyNames = new List<string>();
      if (bHasHeader)
      {
        for (var c = worksheet.Dimension.Start.Column; c < worksheet.Dimension.End.Column; c++)
        {
          propertyNames.Add(worksheet.Cells[startPos, c].GetValue<string>());
        }

        startPos = startPos + 1;
      }
      else
      {
        for (var c = worksheet.Dimension.Start.Column; c <= worksheet.Dimension.End.Column; c++)
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

      for (var rowPos = startPos; rowPos <= worksheet.Dimension.End.Row; rowPos++)
      {
        var rowObject = this.Engine.Object.Construct();

        for (var c = worksheet.Dimension.Start.Column; c <= worksheet.Dimension.End.Column; c++)
        {
          var cell = worksheet.Cells[rowPos, c];
          rowObject.SetPropertyValue(propertyNames[c - 1], cell.Value, false);
        }

        ArrayInstance.Push(result, rowObject);
      }

      return result;
    }

    [JSFunction(Name = "load")]
    public virtual void Load(params object[] args)
    {
      var firstArg = args.FirstOrDefault();
      if (firstArg is Base64EncodedByteArrayInstance)
      {
        var byteArray = firstArg as Base64EncodedByteArrayInstance;

        using (var data = new MemoryStream(byteArray.Data))
        {
          var secondArg = args.ElementAtOrDefault(1);
          if (secondArg != null && TypeUtilities.IsString(secondArg))
            m_excelPackage.Load(data, TypeConverter.ToString(secondArg));
          else
            m_excelPackage.Load(data);
        }
      }
      else
      {
        throw new InvalidOperationException("The supplied parameter must be a Base64EncodedByteArray.");
      }
    }

    #endregion

    public void Dispose()
    {
      if (m_excelPackage == null)
        return;

      m_excelPackage.Dispose();
    }
  }
}
