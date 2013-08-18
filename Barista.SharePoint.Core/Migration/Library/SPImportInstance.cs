namespace Barista.SharePoint.Migration.Library
{
  using System.IO;
  using System.Linq;
  using Barista.Extensions;
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using System;
  using Microsoft.SharePoint;
  using Microsoft.SharePoint.Deployment;

  [Serializable]
  public class SPImportConstructor : ClrFunction
  {
    public SPImportConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPImport", new SPImportInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public SPImportInstance Construct()
    {
      return new SPImportInstance(this.InstancePrototype, new SPImport());
    }
  }

  [Serializable]
  public class SPImportInstance : ObjectInstance
  {
    private readonly SPImport m_import;
    private string m_dropLocation;

    public SPImportInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SPImportInstance(ObjectInstance prototype, SPImport import)
      : this(prototype)
    {
      if (import == null)
        throw new ArgumentNullException("import");

      m_import = import;
    }

    public SPImport SPImport
    {
      get { return m_import; }
    }

    [JSProperty(Name = "dropLocation")]
    public string DropLocation
    {
      get
      {
        return m_dropLocation;
      }
      set
      {
        m_dropLocation = value;
      }
    }

    [JSFunction(Name = "cancel")]
    public void Cancel()
    {
      m_import.Cancel();
    }

    [JSFunction(Name = "dispose")]
    public void Dispose()
    {
      m_import.Dispose();
    }

    [JSFunction(Name = "run")]
    public void Run()
    {
      //SPExportInstance.CopyFilesToDropLocation(m_export, m_dropLocation);
      m_import.Run();
    }

    public static void CopyFilesFromDropLocationToTempLocation(SPImport import, string dropLocation)
    {
      //If a drop location is specified, copy the files to the target location.
      if (dropLocation.IsNullOrWhiteSpace())
        return;

      SPSite dropSite;
      SPWeb dropWeb;
      SPFolder dropFolder;

      if (!SPHelper.TryGetSPFolder(dropLocation, out dropSite, out dropWeb, out dropFolder))
        return;

      var tempPath = Path.GetTempPath();

      try
      {
        foreach (var fileToCopy in dropFolder.Files.OfType<SPFile>())
        {
          using (var fs = File.Create(Path.Combine(tempPath, fileToCopy.Name)))
          {
            fileToCopy.SaveBinary(fs);
          }
        }
        import.Settings.FileLocation = tempPath;
      }
      finally
      {
        if (dropSite != null)
          dropSite.Dispose();

        if (dropWeb != null)
          dropWeb.Dispose();
      }
    }
  }
}
