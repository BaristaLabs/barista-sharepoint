namespace Barista.SharePoint.Library
{
    using Barista.Library;
    using Barista.Newtonsoft.Json;
    using Jurassic;
    using Jurassic.Library;
    using Microsoft.SharePoint;
    using System;

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

            return new SPFileVersionInstance(InstancePrototype, fileVersion);
        }
    }

    [Serializable]
    public class SPFileVersionInstance : ObjectInstance
    {
        private readonly SPFileVersion m_fileVersion;

        public SPFileVersionInstance(ObjectInstance prototype)
            : base(prototype)
        {
            PopulateFields();
            PopulateFunctions();
        }

        public SPFileVersionInstance(ObjectInstance prototype, SPFileVersion fileVersion)
            : this(prototype)
        {
            m_fileVersion = fileVersion;
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
            get { return JurassicHelper.ToDateInstance(Engine, m_fileVersion.Created); }
        }

        [JSProperty(Name = "createdBy")]
        public SPUserInstance CreatedBy
        {
            get { return new SPUserInstance(Engine, m_fileVersion.CreatedBy); }
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

        [JSProperty(Name = "allProperties")]
        public ObjectInstance AllProperties
        {
            get
            {
                var result = Engine.Object.Construct();

                foreach (var key in m_fileVersion.Properties.Keys)
                {
                    string serializedKey;
                    if (key is string)
                        serializedKey = key as string;
                    else
                        serializedKey = JsonConvert.SerializeObject(key);

                    var serializedValue = JsonConvert.SerializeObject(m_fileVersion.Properties[key]);

                    result.SetPropertyValue(serializedKey, JSONObject.Parse(Engine, serializedValue, null), false);
                }

                return result;
            }
        }

        [JSFunction(Name = "delete")]
        public void Delete()
        {
            m_fileVersion.Delete();
        }

        [JSFunction(Name = "getLimitedWebPartManager")]
        public SPLimitedWebPartManagerInstance GetLimitedWebPartManager()
        {
            var result = m_fileVersion.GetLimitedWebPartManager();
            return result == null
                ? null
                : new SPLimitedWebPartManagerInstance(Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "getFile")]
        public SPFileInstance GetFile()
        {
            return new SPFileInstance(Engine.Object.InstancePrototype, m_fileVersion.File);
        }

        [JSFunction(Name = "openBinary")]
        public Base64EncodedByteArrayInstance OpenBinary()
        {
            var result = new Base64EncodedByteArrayInstance(Engine.Object.InstancePrototype, m_fileVersion.OpenBinary())
              {
                  FileName = m_fileVersion.File.Name,
                  MimeType = StringHelper.GetMimeTypeFromFileName(m_fileVersion.File.Name)
              };

            return result;
        }

        [JSFunction(Name = "recycle")]
        public void Recycle()
        {
            m_fileVersion.Recycle();
        }
    }
}
