namespace Barista.SharePoint.Library
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Microsoft.SharePoint;

    [Serializable]
    public class SPCheckedOutFileConstructor : ClrFunction
    {
        public SPCheckedOutFileConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPCheckedOutFile", new SPCheckedOutFileInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public SPCheckedOutFileInstance Construct()
        {
            return new SPCheckedOutFileInstance(this.InstancePrototype);
        }
    }

    [Serializable]
    public class SPCheckedOutFileInstance : ObjectInstance
    {
        private readonly SPCheckedOutFile m_checkedOutFile;

        public SPCheckedOutFileInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public SPCheckedOutFileInstance(ObjectInstance prototype, SPCheckedOutFile checkedOutFile)
            : this(prototype)
        {
            if (checkedOutFile == null)
                throw new ArgumentNullException("checkedOutFile");

            m_checkedOutFile = checkedOutFile;
        }

        public SPCheckedOutFile SPCheckedOutFile
        {
            get { return m_checkedOutFile; }
        }

        [JSProperty(Name = "checkedOutBy")]
        public SPUserInstance CheckedOutBy
        {
            get
            {
                return m_checkedOutFile.CheckedOutBy == null
                    ? null
                    : new SPUserInstance(this.Engine.Object.InstancePrototype, m_checkedOutFile.CheckedOutBy);
            }
        }

        [JSProperty(Name = "checkedOutByEmail")]
        public string CheckedOutByEmail
        {
            get
            {
                return m_checkedOutFile.CheckedOutByEmail;
            }
        }

        [JSProperty(Name = "checkedOutById")]
        public int CheckedOutById
        {
            get
            {
                return m_checkedOutFile.CheckedOutById;
            }
        }

        [JSProperty(Name = "checkedOutByName")]
        public string CheckedOutByName
        {
            get
            {
                return m_checkedOutFile.CheckedOutByName;
            }
        }

        [JSProperty(Name = "dirName")]
        public string DirName
        {
            get
            {
                return m_checkedOutFile.DirName;
            }
        }

        [JSProperty(Name = "imageUrl")]
        public string ImageUrl
        {
            get
            {
                return m_checkedOutFile.ImageUrl;
            }
        }

        [JSProperty(Name = "leafName")]
        public string LeafName
        {
            get
            {
                return m_checkedOutFile.LeafName;
            }
        }

        [JSProperty(Name = "length")]
        public double Length
        {
            get
            {
                return m_checkedOutFile.Length;
            }
        }

        [JSProperty(Name = "listItemId")]
        public int ListItemId
        {
            get
            {
                return m_checkedOutFile.ListItemId;
            }
        }

        [JSProperty(Name = "timeLastModified")]
        public DateInstance TimeLastModified
        {
            get
            {
                return JurassicHelper.ToDateInstance(this.Engine, m_checkedOutFile.TimeLastModified);
            }
        }

        [JSProperty(Name = "url")]
        public string Url
        {
            get
            {
                return m_checkedOutFile.Url;
            }
        }

        [JSFunction(Name = "delete")]
        public void Delete()
        {
            m_checkedOutFile.Delete();
        }

        [JSFunction(Name = "takeOverCheckOut")]
        public void TakeOverCheckOut()
        {
            m_checkedOutFile.TakeOverCheckOut();
        }
    }
}
