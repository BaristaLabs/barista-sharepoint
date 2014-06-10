namespace Barista.SharePoint.Workflow
{
    //Complete 6/10/14

    using Barista.Extensions;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using Barista.Library;
    using Barista.SharePoint.Library;
    using Microsoft.SharePoint;
    using Microsoft.SharePoint.Workflow;
    using System;

    [Serializable]
    public class SPWorkflowAssociationConstructor : ClrFunction
    {
        public SPWorkflowAssociationConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPWorkflowAssociation", new SPWorkflowAssociationInstance(engine.Object.InstancePrototype))
        {
            this.PopulateFunctions();
        }

        //[JSConstructorFunction]
        public SPWorkflowAssociationInstance Construct()
        {
            return new SPWorkflowAssociationInstance(this.InstancePrototype);
        }

        [JSFunction(Name = "createListAssociation")]
        public SPWorkflowAssociationInstance CreateListAssociation(SPWorkflowTemplateInstance template, string name, SPListInstance taskList, SPListInstance historyList)
        {
            if (template == null)
                throw new JavaScriptException(this.Engine, "Error", "A workflow template must be supplied as the first argument.");

            if (taskList == null)
                throw new JavaScriptException(this.Engine, "Error", "A task list must be supplied as the third argument.");

            if (historyList == null)
                throw new JavaScriptException(this.Engine, "Error", "A history list must be supplied as the fourth argument.");

            var result = SPWorkflowAssociation.CreateListAssociation(template.SPWorkflowTemplate, name, taskList.List,
                historyList.List);

            return result == null
                ? null
                : new SPWorkflowAssociationInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "createListContentTypeAssociation")]
        public SPWorkflowAssociationInstance CreateListContentTypeAssociation(SPWorkflowTemplateInstance template, string name, SPListInstance taskList, SPListInstance historyList)
        {
            if (template == null)
                throw new JavaScriptException(this.Engine, "Error", "A workflow template must be supplied as the first argument.");

            if (taskList == null)
                throw new JavaScriptException(this.Engine, "Error", "A task list must be supplied as the third argument.");

            if (historyList == null)
                throw new JavaScriptException(this.Engine, "Error", "A history list must be supplied as the fourth argument.");

            var result = SPWorkflowAssociation.CreateListContentTypeAssociation(template.SPWorkflowTemplate, name, taskList.List,
                historyList.List);

            return result == null
                ? null
                : new SPWorkflowAssociationInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "createWebAssociation")]
        public SPWorkflowAssociationInstance CreateWebAssociation(SPWorkflowTemplateInstance template, string name, SPListInstance taskList, SPListInstance historyList)
        {
            if (template == null)
                throw new JavaScriptException(this.Engine, "Error", "A workflow template must be supplied as the first argument.");

            if (taskList == null)
                throw new JavaScriptException(this.Engine, "Error", "A task list must be supplied as the third argument.");

            if (historyList == null)
                throw new JavaScriptException(this.Engine, "Error", "A history list must be supplied as the fourth argument.");

            var result = SPWorkflowAssociation.CreateWebAssociation(template.SPWorkflowTemplate, name, taskList.List,
                historyList.List);

            return result == null
                ? null
                : new SPWorkflowAssociationInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "createWebContentTypeAssociation")]
        public SPWorkflowAssociationInstance CreateWebContentTypeAssociation(SPWorkflowTemplateInstance template, string name, string taskListName, string historyListName)
        {
            if (template == null)
                throw new JavaScriptException(this.Engine, "Error", "A workflow template must be supplied as the first argument.");

            var result = SPWorkflowAssociation.CreateWebContentTypeAssociation(template.SPWorkflowTemplate, name, taskListName,
                historyListName);

            return result == null
                ? null
                : new SPWorkflowAssociationInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "importFromXml")]
        public SPWorkflowAssociationInstance ImportFromXml(SPWebInstance rootWeb, string xml)
        {
            if (rootWeb == null)
                throw new JavaScriptException(this.Engine, "Error", "A web must be supplied as the first argument.");

            var result = SPWorkflowAssociation.ImportFromXml(rootWeb.Web, xml);
            return result == null
                ? null
                : new SPWorkflowAssociationInstance(this.Engine.Object.InstancePrototype, result);
        }


    }

    [Serializable]
    public class SPWorkflowAssociationInstance : ObjectInstance
    {
        private readonly SPWorkflowAssociation m_workflowAssociation;

        public SPWorkflowAssociationInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public SPWorkflowAssociationInstance(ObjectInstance prototype, SPWorkflowAssociation workflowAssociation)
            : this(prototype)
        {
            if (workflowAssociation == null)
                throw new ArgumentNullException("workflowAssociation");

            m_workflowAssociation = workflowAssociation;
        }

        public SPWorkflowAssociation SPWorkflowAssociation
        {
            get
            {
                return m_workflowAssociation;
            }
        }

        [JSProperty(Name = "allowAsyncManualStart")]
        public bool AllowAsyncManualStart
        {
            get
            {
                return m_workflowAssociation.AllowAsyncManualStart;
            }
            set
            {
                m_workflowAssociation.AllowAsyncManualStart = value;
            }
        }

        [JSProperty(Name = "allowManual")]
        public bool AllowManual
        {
            get
            {
                return m_workflowAssociation.AllowManual;
            }
            set
            {
                m_workflowAssociation.AllowManual = value;
            }
        }

        [JSProperty(Name = "associationData")]
        public string AssociationData
        {
            get
            {
                return m_workflowAssociation.AssociationData;
            }
            set
            {
                m_workflowAssociation.AssociationData = value;
            }
        }

        [JSProperty(Name = "author")]
        public int Author
        {
            get
            {
                return m_workflowAssociation.Author;
            }
        }

        [JSProperty(Name = "autoCleanupDays")]
        public int AutoCleanupDays
        {
            get
            {
                return m_workflowAssociation.AutoCleanupDays;
            }
            set
            {
                m_workflowAssociation.AutoCleanupDays = value;
            }
        }

        [JSProperty(Name = "autoStartChange")]
        public bool AutoStartChange
        {
            get
            {
                return m_workflowAssociation.AutoStartChange;
            }
            set
            {
                m_workflowAssociation.AutoStartChange = value;
            }
        }

        [JSProperty(Name = "autoStartCreate")]
        public bool AutoStartCreate
        {
            get
            {
                return m_workflowAssociation.AutoStartCreate;
            }
            set
            {
                m_workflowAssociation.AutoStartCreate = value;
            }
        }

        [JSProperty(Name = "baseId")]
        public GuidInstance BaseId
        {
            get
            {
                return new GuidInstance(this.Engine.Object.InstancePrototype, m_workflowAssociation.BaseId);
            }
        }

        [JSProperty(Name = "baseTemplate")]
        public SPWorkflowTemplateInstance BaseTemplate
        {
            get
            {
                return m_workflowAssociation.BaseTemplate == null
                    ? null
                    : new SPWorkflowTemplateInstance(this.Engine.Object.InstancePrototype, m_workflowAssociation.BaseTemplate);
            }
        }

        [JSProperty(Name = "compressInstanceData")]
        public bool CompressInstanceData
        {
            get
            {
                return m_workflowAssociation.CompressInstanceData;
            }
        }

        [JSProperty(Name = "contentTypePushDown")]
        public bool ContentTypePushDown
        {
            get
            {
                return m_workflowAssociation.ContentTypePushDown;
            }
            set
            {
                m_workflowAssociation.ContentTypePushDown = value;
            }
        }

        [JSProperty(Name = "created")]
        public DateInstance Created
        {
            get
            {
                return JurassicHelper.ToDateInstance(this.Engine, m_workflowAssociation.Created);
            }
        }

        [JSProperty(Name = "description")]
        public string Description
        {
            get
            {
                return m_workflowAssociation.Description;
            }
            set
            {
                m_workflowAssociation.Description = value;
            }
        }

        [JSProperty(Name = "enabled")]
        public bool Enabled
        {
            get
            {
                return m_workflowAssociation.Enabled;
            }
            set
            {
                m_workflowAssociation.Enabled = value;
            }
        }

        [JSProperty(Name = "globallyEnabled")]
        public bool GloballyEnabled
        {
            get
            {
                return m_workflowAssociation.GloballyEnabled;
            }
        }

        [JSProperty(Name = "historyListId")]
        public GuidInstance HistoryListId
        {
            get
            {
                return new GuidInstance(this.Engine.Object.InstancePrototype, m_workflowAssociation.HistoryListId);
            }
        }

        [JSProperty(Name = "historyListTitle")]
        public string HistoryListTitle
        {
            get
            {
                return m_workflowAssociation.HistoryListTitle;
            }
            set
            {
                m_workflowAssociation.HistoryListTitle = value;
            }
        }

        [JSProperty(Name = "id")]
        public GuidInstance Id
        {
            get
            {
                return new GuidInstance(this.Engine.Object.InstancePrototype, m_workflowAssociation.Id);
            }
        }

        [JSProperty(Name = "instantiationUrl")]
        public string InstantiationUrl
        {
            get
            {
                return m_workflowAssociation.InstantiationUrl;
            }
        }

        [JSProperty(Name = "internalName")]
        public string InternalName
        {
            get
            {
                return m_workflowAssociation.InternalName;
            }
        }

        [JSProperty(Name = "isDeclarative")]
        public bool IsDeclarative
        {
            get
            {
                return m_workflowAssociation.IsDeclarative;
            }
        }

        [JSProperty(Name = "lockItem")]
        public bool LockItem
        {
            get
            {
                return m_workflowAssociation.LockItem;
            }
            set
            {
                m_workflowAssociation.LockItem = value;
            }
        }

        [JSProperty(Name = "markedForDelete")]
        public bool MarkedForDelete
        {
            get
            {
                return m_workflowAssociation.MarkedForDelete;
            }
            set
            {
                m_workflowAssociation.MarkedForDelete = value;
            }
        }

        [JSProperty(Name = "modificationUrl")]
        public string ModificationUrl
        {
            get
            {
                return m_workflowAssociation.ModificationUrl;
            }
        }

        [JSProperty(Name = "modified")]
        public DateInstance Modified
        {
            get
            {
                return JurassicHelper.ToDateInstance(this.Engine, m_workflowAssociation.Modified);
            }
        }

        [JSProperty(Name = "name")]
        public string Name
        {
            get
            {
                return m_workflowAssociation.Name;
            }
            set
            {
                m_workflowAssociation.Name = value;
            }
        }

        [JSProperty(Name = "parentAssociationId")]
        public GuidInstance ParentAssociationId
        {
            get
            {
                return new GuidInstance(this.Engine.Object.InstancePrototype, m_workflowAssociation.ParentAssociationId);
            }
        }

        [JSProperty(Name = "parentContentType")]
        public SPContentTypeInstance ParentContentType
        {
            get
            {
                return m_workflowAssociation.ParentContentType == null
                    ? null
                    : new SPContentTypeInstance(this.Engine.Object.InstancePrototype, m_workflowAssociation.ParentContentType);
            }
        }

        [JSProperty(Name = "permissionsManual")]
        public string PermissionsManual
        {
            get
            {
                return m_workflowAssociation.PermissionsManual.ToString();
            }
            set
            {
                SPBasePermissions basePermissions;
                if (!value.TryParseEnum(true, out basePermissions))
                    throw new JavaScriptException(this.Engine, "Error", "Value must be a valid SPBasePermission");

                m_workflowAssociation.PermissionsManual = basePermissions;
            }
        }

        [JSProperty(Name = "runningInstances")]
        public int RunningInstances
        {
            get
            {
                return m_workflowAssociation.RunningInstances;
            }
        }

        [JSProperty(Name = "siteId")]
        public GuidInstance SiteId
        {
            get
            {
                return new GuidInstance(this.Engine.Object.InstancePrototype, m_workflowAssociation.SiteId);
            }
        }

        [JSProperty(Name = "siteOverQuota")]
        public bool SiteOverQuota
        {
            get
            {
                return m_workflowAssociation.SiteOverQuota;
            }
        }

        [JSProperty(Name = "siteWriteLocked")]
        public bool SiteWriteLocked
        {
            get
            {
                return m_workflowAssociation.SiteWriteLocked;
            }
        }

        [JSProperty(Name = "statusColumn")]
        public bool StatusColumn
        {
            get
            {
                return m_workflowAssociation.StatusColumn;
            }
        }

        [JSProperty(Name = "statusUrl")]
        public string StatusUrl
        {
            get
            {
                return m_workflowAssociation.StatusUrl;
            }
        }

        [JSProperty(Name = "taskListContentTypeId")]
        public SPContentTypeIdInstance TaskListContentTypeId
        {
            get
            {
                return new SPContentTypeIdInstance(this.Engine.Object.InstancePrototype, m_workflowAssociation.TaskListContentTypeId);
            }
        }

        [JSProperty(Name = "taskListId")]
        public GuidInstance TaskListId
        {
            get
            {
                return new GuidInstance(this.Engine.Object.InstancePrototype, m_workflowAssociation.TaskListId);
            }
        }

        [JSProperty(Name = "taskListTitle")]
        public string TaskListTitle
        {
            get
            {
                return m_workflowAssociation.TaskListTitle;
            }
            set
            {
                m_workflowAssociation.TaskListTitle = value;
            }
        }

        [JSProperty(Name = "webId")]
        public GuidInstance WebId
        {
            get
            {
                return new GuidInstance(this.Engine.Object.InstancePrototype, m_workflowAssociation.WebId);
            }
        }

        [JSFunction(Name = "exportToXml")]
        public string ExportToXml()
        {
            return m_workflowAssociation.ExportToXml();
        }

        [JSFunction(Name = "getParentList")]
        public SPListInstance GetParentList()
        {
            var result = m_workflowAssociation.ParentList;
            return result == null
                ? null
                : new SPListInstance(this.Engine, null, null, result);
        }

        [JSFunction(Name = "getParentSite")]
        public SPSiteInstance GetParentSite()
        {
            var result = m_workflowAssociation.ParentSite;
            return result == null
                ? null
                : new SPSiteInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "getParentWeb")]
        public SPWebInstance GetParentWeb()
        {
            var result = m_workflowAssociation.ParentWeb;
            return result == null
                ? null
                : new SPWebInstance(this.Engine, result);
        }

        [JSFunction(Name = "getPropertyByName")]
        public object GetPropertyByName(string property)
        {
            return m_workflowAssociation[property];
        }

        [JSFunction(Name = "getSoapXml")]
        public string GetSoapXml()
        {
            return m_workflowAssociation.SoapXml;
        }

        [JSFunction(Name = "setPropertyByName")]
        public void SetPropertyByName(string property, object obj)
        {
            m_workflowAssociation[property] = TypeConverter.ToObject(this.Engine, obj);
        }

        [JSFunction(Name = "setHistoryList")]
        public void SetHistoryList(SPListInstance list)
        {
            if (list == null)
                throw new JavaScriptException(this.Engine, "Error", "A SPList must be supplied as the first argument.");

            m_workflowAssociation.SetHistoryList(list.List);
        }

        [JSFunction(Name = "setTaskList")]
        public void SetTaskList(SPListInstance list)
        {
            if (list == null)
                throw new JavaScriptException(this.Engine, "Error", "A SPList must be supplied as the first argument.");

            m_workflowAssociation.SetTaskList(list.List);
        }
    }
}
