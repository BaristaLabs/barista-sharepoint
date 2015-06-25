namespace Barista.SharePoint.Library
{
    using Barista.Extensions;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Barista.Library;
    using Microsoft.SharePoint;

    [Serializable]
    public class SPUserCustomActionConstructor : ClrFunction
    {
        public SPUserCustomActionConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPUserCustomAction", new SPUserCustomActionInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public SPUserCustomActionInstance Construct()
        {
            return new SPUserCustomActionInstance(this.InstancePrototype);
        }
    }

    [Serializable]
    public class SPUserCustomActionInstance : ObjectInstance
    {
        private readonly SPUserCustomAction m_userCustomAction;

        public SPUserCustomActionInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public SPUserCustomActionInstance(ObjectInstance prototype, SPUserCustomAction userCustomAction)
            : this(prototype)
        {
            if (userCustomAction == null)
                throw new ArgumentNullException("userCustomAction");

            m_userCustomAction = userCustomAction;
        }

        public SPUserCustomAction SPUserCustomAction
        {
            get
            {
                return m_userCustomAction;
            }
        }

        [JSProperty(Name = "commandUIExtension")]
        public string CommandUIExtension
        {
            get
            {
                return m_userCustomAction.CommandUIExtension;
            }
            set
            {
                m_userCustomAction.CommandUIExtension = value;
            }
        }

        //CommandUIExtensionResource

        [JSProperty(Name = "description")]
        public string Description
        {
            get
            {
                return m_userCustomAction.Description;
            }
            set
            {
                m_userCustomAction.Description = value;
            }
        }

        //DescriptionResource

        [JSProperty(Name = "group")]
        public string Group
        {
            get
            {
                return m_userCustomAction.Group;
            }
            set
            {
                m_userCustomAction.Group = value;
            }
        }

        [JSProperty(Name = "id")]
        public GuidInstance Id
        {
            get
            {
                return new GuidInstance(this.Engine.Object.InstancePrototype, m_userCustomAction.Id);
            }
        }

        [JSProperty(Name = "imageUrl")]
        public string ImageUrl
        {
            get
            {
                return m_userCustomAction.ImageUrl;
            }
            set
            {
                m_userCustomAction.ImageUrl = value;
            }
        }

        [JSProperty(Name = "location")]
        public string Location
        {
            get
            {
                return m_userCustomAction.Location;
            }
            set
            {
                m_userCustomAction.Location = value;
            }
        }

        [JSProperty(Name = "name")]
        public string Name
        {
            get
            {
                return m_userCustomAction.Name;
            }
            set
            {
                m_userCustomAction.Name = value;
            }
        }

        [JSProperty(Name = "registrationId")]
        public string RegistrationId
        {
            get
            {
                return m_userCustomAction.RegistrationId;
            }
            set
            {
                m_userCustomAction.RegistrationId = value;
            }
        }

        [JSProperty(Name = "registrationType")]
        public string RegistrationType
        {
            get
            {
                return m_userCustomAction.RegistrationType.ToString();
            }
            set
            {
                SPUserCustomActionRegistrationType registrationType;
                if (!value.TryParseEnum(true, out registrationType))
                    throw new JavaScriptException(this.Engine, "Error", "A valid SPUserCustomActionRegistrationType must be specified.");
                
                m_userCustomAction.RegistrationType = registrationType;
            }
        }

        [JSProperty(Name = "rights")]
        public string Rights
        {
            get
            {
                return m_userCustomAction.Rights.ToString();
            }
            set
            {
                SPBasePermissions rights;
                if (!value.TryParseEnum(true, out rights))
                    throw new JavaScriptException(this.Engine, "Error", "A valid SPBasePermission must be specified.");

                m_userCustomAction.Rights = rights;
            }
        }

        [JSProperty(Name = "scope")]
        public string Scope
        {
            get
            {
                return m_userCustomAction.Scope.ToString();
            }
        }

        [JSProperty(Name = "scriptBlock")]
        public string ScriptBlock
        {
            get
            {
                return m_userCustomAction.ScriptBlock;
            }
            set
            {
                m_userCustomAction.ScriptBlock = value;
            }
        }

        [JSProperty(Name = "scriptSrc")]
        public string ScriptSrc
        {
            get
            {
                return m_userCustomAction.ScriptSrc;
            }
            set
            {
                m_userCustomAction.ScriptSrc = value;
            }
        }

        [JSProperty(Name = "sequence")]
        public int Sequence
        {
            get
            {
                return m_userCustomAction.Sequence;
            }
            set
            {
                m_userCustomAction.Sequence = value;
            }
        }

        [JSProperty(Name = "title")]
        public string Title
        {
            get
            {
                return m_userCustomAction.Title;
            }
            set
            {
                m_userCustomAction.Title = value;
            }
        }

        //TitleResource

        [JSProperty(Name = "url")]
        public string Url
        {
            get
            {
                return m_userCustomAction.Url;
            }
            set
            {
                m_userCustomAction.Url = value;
            }
        }

        [JSProperty(Name = "versionOfUserCustomAction")]
        public string VersionOfUserCustomAction
        {
            get
            {
                return m_userCustomAction.VersionOfUserCustomAction.ToString();
            }
        }

        [JSFunction(Name = "delete")]
        public void Delete()
        {
            m_userCustomAction.Delete();
        }

        [JSFunction(Name = "update")]
        public void Update()
        {
            m_userCustomAction.Update();
        }
    }
}
