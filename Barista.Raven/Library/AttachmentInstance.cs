using RavenDB = Raven;

namespace Barista.Raven.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using System;
  using Barista.Library;
  using Barista.Extensions;

  [Serializable]
  public class AttachmentConstructor : ClrFunction
  {
    public AttachmentConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "Attachment", new AttachmentInstance(engine.Object.InstancePrototype))
    {
    }

    [JSConstructorFunction]
    public AttachmentInstance Construct()
    {
      return new AttachmentInstance(this.InstancePrototype);
    }
  }

  [Serializable]
  public class AttachmentInstance : ObjectInstance
  {
    private readonly RavenDB.Abstractions.Data.Attachment m_attachment;

    public AttachmentInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public AttachmentInstance(ObjectInstance prototype, RavenDB.Abstractions.Data.Attachment attachment)
      : this(prototype)
    {
      if (attachment == null)
        throw new ArgumentNullException("attachment");

      m_attachment = attachment;
    }

    public RavenDB.Abstractions.Data.Attachment Attachment
    {
      get { return m_attachment; }
    }

    [JSProperty(Name = "data")]
    public object Data
    {
      get
      {
        if (m_attachment.Data == null)
          return Null.Value;

        var data = m_attachment.Data().ToByteArray();

        return new Base64EncodedByteArrayInstance(this.Engine.Object.InstancePrototype, data);
      }
    }

    //[JSProperty(Name = "eTag")]
    //public GuidInstance Etag
    //{
    //  get { return new GuidInstance(this.Engine.Object.InstancePrototype, m_attachment.Etag); }
    //  set {
    //    m_attachment.Etag = value == null ? default(Guid) : value.Value;
    //  }
    //}

    [JSProperty(Name = "key")]
    public string Key
    {
      get { return m_attachment.Key; }
      set { m_attachment.Key = value; }
    }

    [JSProperty(Name = "metaData")]
    public object Metadata
    {
      get
      {
        if (m_attachment.Metadata == null)
          return Null.Value;

        return JSONObject.Parse(this.Engine, m_attachment.Metadata.ToString(), null);
      }
      set
      {
        if (value == null || value == Null.Value || value == Undefined.Value)
          m_attachment.Metadata = null;
        else
          m_attachment.Metadata = RavenDB.Json.Linq.RavenJObject.Parse(JSONObject.Stringify(this.Engine, value, null, null));
      }
    }

    [JSProperty(Name = "size")]
    public int Size
    {
      get { return m_attachment.Size; }
      set { m_attachment.Size = value; }
    }
  }
}
