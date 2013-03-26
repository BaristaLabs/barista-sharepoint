namespace Barista.SharePoint.Library
{
  using System;
  using System.Linq;
  using Jurassic;
  using Jurassic.Library;
  using Microsoft.SharePoint;
  using System.Collections.Generic;
  using Barista.Library;

  [Serializable]
  public class SPFileVersionConstructor : ClrFunction
  {
    public SPFileVersionConstructor(ScriptEngine engine)
      : base(engine.Function.InstancePrototype, "SPFileVersion", new SPFileVersionInstance(engine.Object.InstancePrototype))
    {
    }

    public SPFileVersionInstance Construct(SPFileVersion fileVersion)
    {
      if (fileVersion == null)
        throw new ArgumentNullException("fileVersion");

      return new SPFileVersionInstance(this.InstancePrototype, fileVersion);
    }
  }

  [Serializable]
  public class SPFileVersionInstance : ObjectInstance
  {
    private SPFileVersion m_fileVersion;

    public SPFileVersionInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SPFileVersionInstance(ObjectInstance prototype, SPFileVersion fileVersion)
      : this(prototype)
    {
      this.m_fileVersion = fileVersion;
    }

    #region Properties
    [JSProperty(Name = "checkInComment")]
    public string CheckInComment
    {
      get { return m_fileVersion.CheckInComment; }
    }

    [JSProperty(Name = "Created")]
    public DateInstance Created
    {
      get { return JurassicHelper.ToDateInstance(this.Engine, m_fileVersion.Created); }
    }

    [JSProperty(Name = "createdBy")]
    public SPUserInstance CreatedBy
    {
      get { return new SPUserInstance(this.Engine.Object.InstancePrototype, m_fileVersion.CreatedBy); }
    }

    [JSProperty(Name = "id")]
    public int Id
    {
      get { return m_fileVersion.ID; }
    }

    [JSProperty(Name = "isCurrentVersion")]
    public bool IsCurrentVersion
    {
      get { return m_fileVersion.IsCurrentVersion; }
    }

    [JSProperty(Name = "level")]
    public string Level
    {
      get { return m_fileVersion.Level.ToString(); }
    }

    [JSProperty(Name = "size")]
    public double Size
    {
      get { return m_fileVersion.Size; }
    }

    [JSProperty(Name = "url")]
    public string Url
    {
      get { return m_fileVersion.Url; }
    }

    [JSProperty(Name = "versionLabel")]
    public string VersionLabel
    {
      get { return m_fileVersion.VersionLabel; }
    }
    #endregion

    [JSFunction(Name = "delete")]
    public void Delete()
    {
      m_fileVersion.Delete();
    }

    [JSFunction(Name = "getFile")]
    public SPFileInstance GetFile()
    {
      return new SPFileInstance(this.Engine.Object.InstancePrototype, m_fileVersion.File);
    }

    [JSFunction(Name = "openBinary")]
    public Base64EncodedByteArrayInstance OpenBinary()
    {
      Base64EncodedByteArrayInstance result = new Base64EncodedByteArrayInstance(this.Engine.Object.InstancePrototype, m_fileVersion.OpenBinary());

      result.FileName = m_fileVersion.File.Name;
      result.MimeType = StringHelper.GetMimeTypeFromFileName(m_fileVersion.File.Name);
      return result;
    }

    [JSFunction(Name = "recycle")]
    public void Recycle()
    {
      m_fileVersion.Recycle();
    }
  }
}
