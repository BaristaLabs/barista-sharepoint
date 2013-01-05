﻿namespace Barista.Library
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using Jurassic;
  using Jurassic.Library;

  [Serializable]
  public class Base64EncodedByteArrayConstructor : ClrFunction
  {
    public Base64EncodedByteArrayConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "Base64EncodedByteArray", new Base64EncodedByteArrayInstance(engine.Object.InstancePrototype))
    {
      PopulateFunctions();
    }

    [JSConstructorFunction]
    public Base64EncodedByteArrayInstance Construct(string base64EncodedData)
    {
      return String.IsNullOrEmpty(base64EncodedData)
        ? new Base64EncodedByteArrayInstance(this.Engine.Object.InstancePrototype, null)
        : new Base64EncodedByteArrayInstance(this.Engine.Object.InstancePrototype, StringHelper.StringToByteArray(base64EncodedData));
    }

    [JSFunction(Name = "CreateFromString")]
    public Base64EncodedByteArrayInstance CreateFromString(string data)
    {
      return String.IsNullOrEmpty(data)
        ? new Base64EncodedByteArrayInstance(this.Engine.Object.InstancePrototype, null)
        : new Base64EncodedByteArrayInstance(this.Engine.Object.InstancePrototype, Encoding.UTF8.GetBytes(data));
    }
  }

  [Serializable]
  public class Base64EncodedByteArrayInstance : ObjectInstance
  {
    private readonly List<Byte> m_data = new List<byte>();

    public Base64EncodedByteArrayInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.MimeType = "application/octet-stream";
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public Base64EncodedByteArrayInstance(ObjectInstance prototype, byte[] data)
      : this(prototype)
    {
      if (data != null && data.Length > 0)
        this.m_data = new List<byte>(data);
    }

    public byte[] Data
    {
      get { return m_data.ToArray(); }
    }

    [JSProperty(Name = "mimeType")]
    public string MimeType
    {
      get;
      set;
    }

    [JSProperty(Name = "fileName")]
    public string FileName
    {
      get;
      set;
    }

    [JSProperty(Name = "length")]
    public double Length
    {
      get { return m_data.LongCount(); }
    }

    [JSFunction(Name = "append")]
    public void Append(string data)
    {
      m_data.AddRange(StringHelper.StringToByteArray(data));
    }

    [JSFunction(Name = "getByteAt")]
    public string GetByteAt(int index)
    {
      return StringHelper.ByteArrayToString(new[] { m_data[index] });
    }

    [JSFunction(Name = "setByteAt")]
    public void SetByteAt(int index, string data)
    {
      var byteData = StringHelper.StringToByteArray(data);

      m_data[index] = byteData[0];
    }

    [JSFunction(Name = "toBase64String")]
    public string ToBase64String()
    {
      return this.ToString();
    }

    [JSFunction(Name = "toString")]
    public override string ToString()
    {
      return StringHelper.ByteArrayToString(m_data.ToArray());
    }
  }
}
