namespace Barista.SharePoint.Workflow
{
    using Barista.Extensions;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Barista.Library;
    using Barista.SharePoint.Library;
    using Microsoft.SharePoint;
    using Microsoft.SharePoint.Workflow;

    [Serializable]
    public class SPWorkflowTemplateConstructor : ClrFunction
    {
        public SPWorkflowTemplateConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPWorkflowTemplate", new SPWorkflowTemplateInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public SPWorkflowTemplateInstance Construct()
        {
            return new SPWorkflowTemplateInstance(this.InstancePrototype);
        }
    }

    [Serializable]
    public class SPWorkflowTemplateInstance : ObjectInstance
    {
        private readonly SPWorkflowTemplate m_workflowTemplate;

        public SPWorkflowTemplateInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public SPWorkflowTemplateInstance(ObjectInstance prototype, SPWorkflowTemplate workflowTemplate)
            : this(prototype)
        {
            if (workflowTemplate == null)
                throw new ArgumentNullException("workflowTemplate");

            m_workflowTemplate = workflowTemplate;
        }

        public SPWorkflowTemplate SPWorkflowTemplate
        {
            get
            {
                return m_workflowTemplate;
            }
        }

        [JSProperty(Name = "allowAsyncManualStart")]
        public bool AllowAsyncManualStart
        {
            get
            {
                return m_workflowTemplate.AllowAsyncManualStart;
            }
            set
            {
                m_workflowTemplate.AllowAsyncManualStart = value;
            }
        }

        [JSProperty(Name = "allowDefaultContentApproval")]
        public bool AllowDefaultContentApproval
        {
            get
            {
                return m_workflowTemplate.AllowDefaultContentApproval;
            }
            set
            {
                m_workflowTemplate.AllowDefaultContentApproval = value;
            }
        }

        [JSProperty(Name = "allowManual")]
        public bool AllowManual
        {
            get
            {
                return m_workflowTemplate.AllowManual;
            }
            set
            {
                m_workflowTemplate.AllowManual = value;
            }
        }

        [JSProperty(Name = "associationData")]
        public string AssociationData
        {
            get
            {
                return m_workflowTemplate.AssociationData;
            }
            set
            {
                m_workflowTemplate.AssociationData = value;
            }
        }

        [JSProperty(Name = "associationUrl")]
        public string AssociationUrl
        {
            get
            {
                return m_workflowTemplate.AssociationUrl;
            }
        }

        [JSProperty(Name = "autoCleanupDays")]
        public int AutoCleanupDays
        {
            get
            {
                return m_workflowTemplate.AutoCleanupDays;
            }
            set
            {
                m_workflowTemplate.AutoCleanupDays = value;
            }
        }

        [JSProperty(Name = "autoStartChange")]
        public bool AutoStartChange
        {
            get
            {
                return m_workflowTemplate.AutoStartChange;
            }
            set
            {
                m_workflowTemplate.AutoStartChange = value;
            }
        }

        [JSProperty(Name = "autoStartCreate")]
        public bool AutoStartCreate
        {
            get
            {
                return m_workflowTemplate.AutoStartCreate;
            }
            set
            {
                m_workflowTemplate.AutoStartCreate = value;
            }
        }

        [JSProperty(Name = "baseId")]
        public GuidInstance BaseId
        {
            get
            {
                return new GuidInstance(this.Engine.Object.InstancePrototype, m_workflowTemplate.BaseId);
            }
        }

        [JSProperty(Name = "compressInstanceData")]
        public bool CompressInstanceData
        {
            get
            {
                return m_workflowTemplate.CompressInstanceData;
            }
            set
            {
                m_workflowTemplate.CompressInstanceData = value;
            }
        }

        [JSProperty(Name = "description")]
        public string Description
        {
            get
            {
                return m_workflowTemplate.Description;
            }
            set
            {
                m_workflowTemplate.Description = value;
            }
        }

        [JSProperty(Name = "id")]
        public GuidInstance Id
        {
            get
            {
                return new GuidInstance(this.Engine.Object.InstancePrototype, m_workflowTemplate.Id);
            }
        }

        [JSProperty(Name = "instantiationUrl")]
        public string InstantiationUrl
        {
            get
            {
                return m_workflowTemplate.InstantiationUrl;
            }
        }

        [JSProperty(Name = "isDeclarative")]
        public bool IsDeclarative
        {
            get
            {
                return m_workflowTemplate.IsDeclarative;
            }
        }

        [JSProperty(Name = "modificationUrl")]
        public string ModificationUrl
        {
            get
            {
                return m_workflowTemplate.ModificationUrl;
            }
        }

        [JSProperty(Name = "name")]
        public string Name
        {
            get
            {
                return m_workflowTemplate.Name;
            }
        }

        [JSProperty(Name = "permissionsManual")]
        public string PermissionsManual
        {
            get
            {
                return m_workflowTemplate.PermissionsManual.ToString();
            }
            set
            {
                SPBasePermissions basePermissions;
                if (!value.TryParseEnum(true, out basePermissions))
                    throw new JavaScriptException(this.Engine, "Error", "Value must be a valid SPBasePermission");

                m_workflowTemplate.PermissionsManual = basePermissions;
            }
        }

        [JSProperty(Name = "statusColumn")]
        public bool StatusColumn
        {
            get
            {
                return m_workflowTemplate.StatusColumn;
            }
            set
            {
                m_workflowTemplate.StatusColumn = value;
            }
        }

        [JSProperty(Name = "statusUrl")]
        public string StatusUrl
        {
            get
            {
                return m_workflowTemplate.StatusUrl;
            }
        }

        [JSProperty(Name = "taskListContentTypeId")]
        public SPContentTypeIdInstance TaskListContentTypeId
        {
            get
            {
                return new SPContentTypeIdInstance(this.Engine.Object.InstancePrototype, m_workflowTemplate.TaskListContentTypeId);
            }
        }

        [JSProperty(Name = "xml")]
        public string Xml
        {
            get
            {
                return m_workflowTemplate.Xml;
            }
        }

        [JSFunction(Name = "getPropertyByName")]
        public object GetPropertyByName(string property)
        {
            return m_workflowTemplate[property];
        }

        [JSFunction(Name = "setPropertyByName")]
        public void SetPropertyByName(string property, object obj)
        {
            m_workflowTemplate[property] = TypeConverter.ToObject(this.Engine, obj);
        }
    }
}
