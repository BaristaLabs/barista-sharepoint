namespace Barista.Bundles
{
  using Barista.Library;
  using System;

  [Serializable]
  public class DocumentBundle : IBundle
  {
    public bool IsSystemBundle
    {
      get { return true; }
    }

    public string BundleName
    {
      get { return "Document"; }
    }

    public string BundleDescription
    {
      get { return "Document Bundle. Provides a mechanism to interact with various document formats: CSV, Excel, Yaml, Pdf, Zip."; }
    }

    public object InstallBundle(Jurassic.ScriptEngine engine)
    {
      engine.SetGlobalValue("ExcelDocument", new ExcelDocumentConstructor(engine));
      engine.SetGlobalValue("ZipFile", new ZipFileConstructor(engine));
      engine.SetGlobalValue("PdfAttachment", new PdfAttachmentConstructor(engine));

      return new DocumentInstance(engine);
    }
  }
}
