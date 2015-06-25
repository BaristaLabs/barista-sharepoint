namespace Barista.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using Barista.Newtonsoft.Json;
  using OfficeOpenXml;
  using System;

  [Serializable]
  public class ExcelWorkbookConstructor : ClrFunction
  {
    public ExcelWorkbookConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "ExcelWorkbook", new ExcelWorkbookInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public ExcelWorkbookInstance Construct()
    {
      return new ExcelWorkbookInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class ExcelWorkbookInstance : ObjectInstance
  {
    private readonly ExcelWorkbook m_excelWorkbook;
    private readonly object m_syncRoot = new object();
    private ExcelWorksheetsInstance m_worksheetsInstance = null;

    public ExcelWorkbookInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public ExcelWorkbookInstance(ObjectInstance prototype, ExcelWorkbook excelWorkbook)
      : this(prototype)
    {
      if (excelWorkbook == null)
        throw new ArgumentNullException("excelWorkbook");

      m_excelWorkbook = excelWorkbook;
    }

    public ExcelWorkbook ExcelWorkbook
    {
      get { return m_excelWorkbook; }
    }

    #region Properties
    [JSProperty(Name = "worksheets")]
    [JSDoc("Provides access to all the worksheets in the workbook.")]
    public ExcelWorksheetsInstance Worksheets
    {
      get
      {
        if (m_worksheetsInstance == null)
        {
          lock (m_syncRoot)
          {
            if (m_worksheetsInstance == null)
            {
// ReSharper disable PossibleMultipleWriteAccessInDoubleCheckLocking
              m_worksheetsInstance = new ExcelWorksheetsInstance(this.Engine.Object.InstancePrototype, m_excelWorkbook.Worksheets);
// ReSharper restore PossibleMultipleWriteAccessInDoubleCheckLocking
            }
          }
        }

        return m_worksheetsInstance;
      }
    }
    #endregion

    #region Functions

    [JSFunction(Name = "getWorkbookRaw")]
    [JSDoc("Provides access to the XML data representing the workbook.")]
    public object GetWorkbookRaw()
    {
      var xml = m_excelWorkbook.WorkbookXml;
      var jsonDocument = JsonConvert.SerializeXmlNode(xml.DocumentElement);
      return JSONObject.Parse(this.Engine, jsonDocument, null);
    }

    [JSFunction(Name = "setWorkbookRaw")]
    [JSDoc("Provides access to the XML data representing the workbook.")]
    public void SetWorkbookRaw(object jsonObject)
    {
      string text;
      if (jsonObject is ObjectInstance)
        text = JSONObject.Stringify(this.Engine, jsonObject, null, null);
      else
        text = jsonObject as string;

      var document = JsonConvert.DeserializeXmlNode(text);
      m_excelWorkbook.WorkbookXml.LoadXml(document.OuterXml);
    }

    #endregion
  }
}
