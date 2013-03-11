namespace Barista.Library
{
  using System;
  using Barista.Jurassic;
  using Jurassic.Library;
  using Newtonsoft.Json;

  [Serializable]
  public class PdfAttachmentConstructor : ClrFunction
  {
    public PdfAttachmentConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "PdfAttachment", new PdfAttachmentInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public PdfAttachmentInstance Construct()
    {
      return new PdfAttachmentInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class PdfAttachmentInstance : ObjectInstance
  {
    public PdfAttachmentInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
    }

    [JSProperty(Name = "fileName")]
    [JsonProperty("fileName")]
    public string FileName
    {
      get;
      set;
    }

    [JSProperty(Name = "fileDisplayName")]
    [JsonProperty("fileDisplayName")]
    public string FileDisplayName
    {
      get;
      set;
    }

    [JSProperty(Name = "description")]
    [JsonProperty("description")]
    public string Description
    {
      get;
      set;
    }

    [JSProperty(Name = "data")]
    [JsonProperty("data")]
    public Base64EncodedByteArrayInstance Data
    {
      get;
      set;
    }
  }
}