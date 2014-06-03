namespace Barista.SharePoint.Workflow
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using Barista.Library;
    using Barista.SharePoint.Library;
    using Microsoft.SharePoint.Workflow;
    using System;

    [Serializable]
    public class SPWorkflowConstructor : ClrFunction
    {
        public SPWorkflowConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPWorkflow", new SPWorkflowInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public SPWorkflowInstance Construct()
        {
            return new SPWorkflowInstance(this.InstancePrototype);
        }
    }

    [Serializable]
    public class SPWorkflowInstance : ObjectInstance
    {
        private readonly SPWorkflow m_workflow;

        public SPWorkflowInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public SPWorkflowInstance(ObjectInstance prototype, SPWorkflow workflow)
            : this(prototype)
        {
            if (workflow == null)
                throw new ArgumentNullException("workflow");

            m_workflow = workflow;
        }

        public SPWorkflow SPWorkflow
        {
            get { return m_workflow; }
        }

        [JSProperty(Name = "associationId")]
        public GuidInstance AssociationId
        {
            get
            {
                return new GuidInstance(this.Engine.Object.InstancePrototype, m_workflow.AssociationId);
            }
        }

        [JSProperty(Name = "author")]
        public int Author
        {
            get
            {
                return m_workflow.Author;
            }
        }

        [JSProperty(Name = "authorUser")]
        public SPUserInstance AuthorUser
        {
            get
            {
                return m_workflow.AuthorUser == null
                    ? null
                    : new SPUserInstance(this.Engine.Object.InstancePrototype, m_workflow.AuthorUser);
            }
        }

        [JSProperty(Name = "created")]
        public DateInstance Created
        {
            get
            {
                return JurassicHelper.ToDateInstance(this.Engine, m_workflow.Created);
            }
        }

        [JSProperty(Name = "hasNewEvents")]
        public bool HasNewEvents
        {
            get
            {
                return m_workflow.HasNewEvents;
            }
        }

        [JSProperty(Name = "historyListId")]
        public GuidInstance HistoryListId
        {
            get
            {
                return new GuidInstance(this.Engine.Object.InstancePrototype, m_workflow.HistoryListId);
            }
        }

        [JSProperty(Name = "instanceId")]
        public GuidInstance InstanceId
        {
            get
            {
                return new GuidInstance(this.Engine.Object.InstancePrototype, m_workflow.InstanceId);
            }
        }

        [JSProperty(Name = "internalState")]
        public string InternalState
        {
            get
            {
                return m_workflow.InternalState.ToString();
            }
        }

        [JSProperty(Name = "isCompleted")]
        public bool IsCompleted
        {
            get
            {
                return m_workflow.IsCompleted;
            }
        }

        [JSProperty(Name = "isLocked")]
        public bool IsLocked
        {
            get
            {
                return m_workflow.IsLocked;
            }
        }

        [JSProperty(Name = "itemGuid")]
        public GuidInstance ItemGuid
        {
            get
            {
                return new GuidInstance(this.Engine.Object.InstancePrototype, m_workflow.ItemGuid);
            }
        }

        [JSProperty(Name = "itemId")]
        public int ItemId
        {
            get
            {
                return m_workflow.ItemId;
            }
        }

        [JSProperty(Name = "itemName")]
        public string ItemName
        {
            get
            {
                return m_workflow.ItemName;
            }
        }

        [JSProperty(Name = "listId")]
        public GuidInstance ListId
        {
            get
            {
                return new GuidInstance(this.Engine.Object.InstancePrototype, m_workflow.ListId);
            }
        }

        [JSProperty(Name = "modifications")]
        public SPWorkflowModificationCollectionInstance Modifications
        {
            get
            {
                return m_workflow.Modifications == null
                    ? null
                    : new SPWorkflowModificationCollectionInstance(this.Engine.Object.InstancePrototype, m_workflow.Modifications);
            }
        }

        [JSProperty(Name = "modified")]
        public DateInstance Modified
        {
            get
            {
                return JurassicHelper.ToDateInstance(this.Engine, m_workflow.Modified);
            }
        }

        [JSProperty(Name = "ownerUser")]
        public SPUserInstance OwnerUser
        {
            get
            {
                return m_workflow.OwnerUser == null
                    ? null
                    : new SPUserInstance(this.Engine.Object.InstancePrototype, m_workflow.OwnerUser);
            }
        }

        [JSProperty(Name = "siteId")]
        public GuidInstance SiteId
        {
            get
            {
                return new GuidInstance(this.Engine.Object.InstancePrototype, m_workflow.SiteId);
            }
        }

        [JSProperty(Name = "statusUrl")]
        public string StatusUrl
        {
            get
            {
                return m_workflow.StatusUrl;
            }
        }

        [JSProperty(Name = "taskListId")]
        public GuidInstance TaskListId
        {
            get
            {
                return new GuidInstance(this.Engine.Object.InstancePrototype, m_workflow.TaskListId);
            }
        }

        [JSProperty(Name = "tasks")]
        public SPWorkflowTaskCollectionInstance Tasks
        {
            get
            {
                return new SPWorkflowTaskCollectionInstance(this.Engine.Object.InstancePrototype, m_workflow.Tasks);
            }
        }

        [JSProperty(Name = "visibleParentItem")]
        public bool VisibleParentItem
        {
            get
            {
                return m_workflow.VisibleParentItem;
            }
        }

        [JSProperty(Name = "webId")]
        public GuidInstance WebId
        {
            get
            {
                return new GuidInstance(this.Engine.Object.InstancePrototype, m_workflow.WebId);
            }
        }


        [JSFunction(Name = "getHistoryList")]
        public SPListInstance GetHistoryList()
        {
            return m_workflow.HistoryList == null
                ? null
                : new SPListInstance(this.Engine, m_workflow.HistoryList.ParentWeb.Site, m_workflow.HistoryList.ParentWeb, m_workflow.HistoryList);
        }

        [JSFunction(Name = "getParentAssociation")]
        public SPWorkflowAssociationInstance GetParentAssociation()
        {
            return m_workflow.ParentAssociation == null
                ? null
                : new SPWorkflowAssociationInstance(this.Engine.Object.InstancePrototype, m_workflow.ParentAssociation);
        }

        [JSFunction(Name = "getParentItem")]
        public SPListItemInstance GetParentItem()
        {
            return m_workflow.ParentItem == null
                ? null
                : new SPListItemInstance(this.Engine, m_workflow.ParentItem);
        }

        [JSFunction(Name = "getParentList")]
        public SPListInstance GetParentList()
        {
            return m_workflow.ParentList == null
                ? null
                : new SPListInstance(this.Engine, m_workflow.HistoryList.ParentWeb.Site, m_workflow.HistoryList.ParentWeb, m_workflow.ParentList);
        }

        [JSFunction(Name = "getParentWeb")]
        public SPWebInstance GetParentWeb()
        {
            return m_workflow.ParentWeb == null
                ? null
                : new SPWebInstance(this.Engine,m_workflow.ParentWeb);
        }

        [JSFunction(Name = "getTaskFilter")]
        public SPWorkflowFilterInstance GetTaskFilter()
        {
            return m_workflow.TaskFilter == null
                ? null
                : new SPWorkflowFilterInstance(this.Engine.Object.InstancePrototype, m_workflow.TaskFilter);
        }

        [JSFunction(Name = "getTaskList")]
        public SPListInstance GetTaskList()
        {
             return m_workflow.TaskList == null
                  ? null
                  : new SPListInstance(this.Engine, m_workflow.HistoryList.ParentWeb.Site, m_workflow.HistoryList.ParentWeb, m_workflow.TaskList);
        }

        [JSFunction(Name = "getXml")]
        public string GetXml()
        {
            return m_workflow.Xml;
        }
    }
}
