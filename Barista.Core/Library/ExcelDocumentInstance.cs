namespace Barista.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using OfficeOpenXml;
  using System;
  using System.IO;
  using System.Linq;
  using System.Reflection;

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

      if (firstArg != null && firstArg != Undefined.Value)
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
        else
        {
          throw new JavaScriptException(this.Engine, "Error",
                                        "An Excel Document cannot be constructed with the specified argument.");
        }
      }
      else
      {
        documentInstance = new ExcelDocumentInstance(this.Engine.Object.InstancePrototype);
      }

      return documentInstance;
    }
  }

  public class ExcelDocumentInstance : ObjectInstance, IDisposable
  {
    protected readonly ExcelPackage m_excelPackage;
    protected readonly object SyncRoot = new object();
    protected ExcelWorkbookInstance WorkbookInstance = null;

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
    [JSDoc("Returns a reference to the workbook within the Excel document. All worksheets and cells can be accessed through the workbook.")]
    public ExcelWorkbookInstance Workbook
    {
      get
      {
        if (WorkbookInstance == null)
        {
          lock (SyncRoot)
          {
            if (WorkbookInstance == null)
            {
// ReSharper disable PossibleMultipleWriteAccessInDoubleCheckLocking
              WorkbookInstance = new ExcelWorkbookInstance(this.Engine.Object.InstancePrototype, m_excelPackage.Workbook);
// ReSharper restore PossibleMultipleWriteAccessInDoubleCheckLocking
            }
          }
        }

        return WorkbookInstance;
      }
    }
    #endregion

    #region Functions

    [JSFunction(Name = "getBytes")]
    [JSDoc("Saves and returns the Excel file as a Base64EncodedByteArray. Closes the document once complete.")]
    public Base64EncodedByteArrayInstance GetBytes(object fileName)
    {
      var data = m_excelPackage.GetAsByteArray();

      var result = new Base64EncodedByteArrayInstance(this.Engine.Object.InstancePrototype, data)
        {
          MimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        };

      if (fileName != null && fileName != Undefined.Value)
        result.FileName = TypeConverter.ToString(fileName);

      return result;
    }

    [JSFunction(Name = "getWorksheet")]
    [JSDoc("Returns an instance of a Excel Worksheet from the Document with the specified name.")]
    public ExcelWorksheetInstance GetWorksheet(object worksheetName)
    {
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
                                      "The first argument must either be the name or index of a worksheet.");
      }

      var worksheetInstance = new ExcelWorksheetInstance(this.Engine.Object.InstancePrototype, worksheet);
      return worksheetInstance;
    }

    [JSFunction(Name = "getWorksheetAsJson")]
    [JSDoc("Returns a Json representation of the specified worksheet.")]
    public ArrayInstance GetWorksheetAsJson(object worksheetName, object hasHeader)
    {
      var worksheetInstance = GetWorksheet(worksheetName);
      return worksheetInstance.ConvertToJson(hasHeader);
    }

    [JSFunction(Name = "load")]
    [JSDoc("Loads the current document with the specified argument. Argument can currently be a Base64EncodedByteArray.")]
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

    [JSFunction(Name = "save")]
    [JSDoc("Saves the Excel Document. Closes the document once complete.")]
    public virtual void Save(params object[] args)
    {
      var firstArg = args.FirstOrDefault();

      if (TypeUtilities.IsString(firstArg))
        m_excelPackage.Save(TypeConverter.ToString(firstArg));
      else
        m_excelPackage.Save();
    }

    [JSFunction(Name = "saveAs")]
    [JSDoc("Saves the Excel Document to a Base64EncodedByteArray. Closes the document once complete.")]
    public virtual void SaveAs(params object[] args)
    {
      var firstArg = args.FirstOrDefault();

      if (firstArg is Base64EncodedByteArrayInstance)
      {
        var secondArg = args.ElementAtOrDefault(1);

        //This seems odd, but rolling with it.
        using (var ms = new MemoryStream())
        {
          if (TypeUtilities.IsString(secondArg))
            m_excelPackage.SaveAs(ms, TypeConverter.ToString(secondArg));
          else
            m_excelPackage.SaveAs(ms);

          ms.Seek(0, SeekOrigin.Begin);
          (firstArg as Base64EncodedByteArrayInstance).Copy(ms.ToArray());
        }
      }
      else
      {
        throw new JavaScriptException(this.Engine, "Error", "The supplied parameter must be a Base64EncodedByteArray.");
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
