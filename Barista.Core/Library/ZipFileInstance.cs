namespace Barista.Library
{
  using Barista.Extensions;
  using ICSharpCode.SharpZipLib.Zip;
  using Jurassic;
  using Jurassic.Library;
  using System;
  using System.IO;

  public class ZipFileConstructor : ClrFunction
  {
    public ZipFileConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "ZipFile", new ZipFileInstance(engine.Object.InstancePrototype, null))
    {
    }

    [JSConstructorFunction]
    public ZipFileInstance Construct(object data)
    {
      return new ZipFileInstance(this.InstancePrototype, data);
    }
  }

  public class ZipFileInstance : ObjectInstance
  {
    private MemoryStream m_memoryStream;
    private ZipOutputStream m_zipOutputStream;

    public ZipFileInstance(ObjectInstance prototype, object data)
      : base(prototype)
    {
      m_memoryStream = new MemoryStream();
      m_zipOutputStream = new ZipOutputStream(m_memoryStream);

      if (data is string)
        this.FileName = (data as string);

      this.PopulateFields();
      this.PopulateFunctions();
    }

    [JSProperty(Name = "fileName")]
    public string FileName
    {
      get;
      set;
    }

    [JSFunction(Name = "addFile")]
    public void AddFile(string fileName, Base64EncodedByteArrayInstance fileData)
    {
      ZipEntry entry = new ZipEntry(fileName);
      entry.DateTime = DateTime.Now;
      entry.Size = fileData.Data.LongLength;

      m_zipOutputStream.PutNextEntry(entry);
      m_zipOutputStream.Write(fileData.Data, 0, fileData.Data.Length);
      m_zipOutputStream.CloseEntry();
    }

    [JSFunction(Name = "finish")]
    public Base64EncodedByteArrayInstance Finish()
    {
      m_zipOutputStream.Finish();
      m_zipOutputStream.Flush();

      m_memoryStream.Seek(0, SeekOrigin.Begin);
      byte[] result = new byte[m_memoryStream.Length];

      m_memoryStream.ReadWholeArray(result);
      m_memoryStream.Dispose();
      m_memoryStream = null;
      m_zipOutputStream = null;

      var byteResult = new Base64EncodedByteArrayInstance(this.Engine.Object.InstancePrototype, result);
      byteResult.FileName = this.FileName;
      byteResult.MimeType = StringHelper.GetMimeTypeFromFileName(this.FileName);
      return byteResult;
    }
  }
}
