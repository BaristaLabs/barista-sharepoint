namespace Barista.SharePoint.Library
{
  using Barista.Library;
  using Jurassic;
  using Jurassic.Library;
  using Microsoft.SharePoint;
  using OfficeOpenXml;
  using System;
  using System.IO;

  [Serializable]
  public class ExcelPackageConstructor : ClrFunction
  {
    public ExcelPackageConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "ExcelPackage", new ExcelPackageInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public ExcelPackageInstance Construct(object excelDocument)
    {
      SPFile file = null;

      if (excelDocument is string)
      {
        var fileUrl = excelDocument as string;
        if (SPHelper.TryGetSPFile(fileUrl, out file) == false)
          throw new JavaScriptException(this.Engine, "Error", "A file with the specified url does not exist.");
      }
      else if (excelDocument is SPFileInstance)
      {
        file = (excelDocument as SPFileInstance).File;
      }
      else if (excelDocument == null || excelDocument == Null.Value || excelDocument == Undefined.Value)
      {
        return new ExcelPackageInstance(this.InstancePrototype);
      }

      if (file == null)
        throw new InvalidOperationException("Could not determine the actual file based on the parameters. Please specify a url, a file instance or an excel document."); 

      return new ExcelPackageInstance(this.InstancePrototype, file);
    }

    public ExcelPackageInstance Construct(SPFile file)
    {
      if (file == null)
        throw new ArgumentNullException("file");

      return new ExcelPackageInstance(this.InstancePrototype, file);
    }
  }

  [Serializable]
  public class ExcelPackageInstance : ObjectInstance, IDisposable
  {
    private readonly SPFile m_sourceFile;
    private ExcelPackage m_excelPackage;

    public ExcelPackageInstance(ObjectInstance prototype)
      : base(prototype)
    {
      m_excelPackage = new ExcelPackage();
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public ExcelPackageInstance(ObjectInstance prototype, SPFile sourceFile)
      : this(prototype)
    {
      m_sourceFile = sourceFile;

      var fileStream = new MemoryStream(sourceFile.OpenBinary());
      m_excelPackage.Load(fileStream);
    }

    #region Properties
    [JSProperty(Name = "doAdjustDrawings")]
    public bool DoAdjustDrawings
    {
      get { return m_excelPackage.DoAdjustDrawings; }
      set { m_excelPackage.DoAdjustDrawings = value; }
    }
    #endregion

    #region Functions

    [JSFunction(Name = "getBytes")]
    public Base64EncodedByteArrayInstance GetBytes()
    {
      byte[] data = m_excelPackage.GetAsByteArray();

      var result = new Base64EncodedByteArrayInstance(this.Engine.Object.InstancePrototype, data);

      if (m_sourceFile != null)
        result.FileName = m_sourceFile.Name;

      result.MimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

      return result;
    }

    #endregion
    public void Dispose()
    {
      if (m_excelPackage != null)
      {
        m_excelPackage.Dispose();
        m_excelPackage = null;
      }
    }
  }
}
