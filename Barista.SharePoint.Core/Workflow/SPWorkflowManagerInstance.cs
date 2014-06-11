namespace Barista.SharePoint.Workflow
{
    using Barista.Extensions;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using Barista.Library;
    using Barista.SharePoint.Library;
    using Microsoft.SharePoint.Workflow;
    using System;

    [Serializable]
    public class SPWorkflowManagerConstructor : ClrFunction
    {
        public SPWorkflowManagerConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPWorkflowManager", new SPWorkflowManagerInstance(engine.Object.InstancePrototype))
        {
            this.PopulateFunctions();
        }

        [JSFunction(Name ="cancelWorkflow")]
        public void CancelWorkflow(SPWorkflowInstance workflow)
        {
            if (workflow == null)
                throw new JavaScriptException(this.Engine, "Error", "An instance of a SPWorkflow object must be supplied as the first argument.");

            SPWorkflowManager.CancelWorkflow(workflow.SPWorkflow);
        }
    }

    [Serializable]
    public class SPWorkflowManagerInstance : ObjectInstance
    {
        private readonly SPWorkflowManager m_workflowManager;

        public SPWorkflowManagerInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public SPWorkflowManagerInstance(ObjectInstance prototype, SPWorkflowManager workflowManager)
            : this(prototype)
        {
            if (workflowManager == null)
                throw new ArgumentNullException("workflowManager");

            m_workflowManager = workflowManager;
        }

        public SPWorkflowManager SPWorkflowManager
        {
            get { return m_workflowManager; }
        }

        [JSProperty(Name = "shuttingDown")]
        public bool ShuttingDown
        {
            get;
            set;
        }

        [JSFunction(Name = "countWorkflowAssociations")]
        public int CountWorkflowAssociations(SPWorkflowTemplateInstance template, SPSiteInstance site)
        {
            if (template == null)
                throw new JavaScriptException(this.Engine, "Error", "An instance of a SPWorkflowTemplate object must be supplied as the first argument.");

            if (site == null)
                throw new JavaScriptException(this.Engine, "Error", "An instance of a SPSite object must be supplied as the second argument.");
            
            return m_workflowManager.CountWorkflowAssociations(template.SPWorkflowTemplate, site.Site);
        }

        [JSFunction(Name = "countWorkflowsInWeb")]
        public ObjectInstance CountWorkflowsInWeb(SPWebInstance web)
        {
            if (web == null)
                throw new JavaScriptException(this.Engine, "Error", "An instance of a SPWeb object must be supplied as the first argument.");

            var result = m_workflowManager.CountWorkflows(web.Web);
            
            var obj = this.Engine.Object.Construct();
            foreach(var r in result)
                obj.SetPropertyValue(r.Key.ToString(), r.Value, false);
            return obj;
        }

        [JSFunction(Name = "countWorkflowsInContentType")]
        public ObjectInstance CountWorkflowsInContentType(SPContentTypeInstance ct)
        {
            if (ct == null)
                throw new JavaScriptException(this.Engine, "Error", "An instance of a SPContentType object must be supplied as the first argument.");

            var result = m_workflowManager.CountWorkflows(ct.ContentType);

            var obj = this.Engine.Object.Construct();
            foreach (var r in result)
                obj.SetPropertyValue(r.Key.ToString(), r.Value, false);
            return obj;
        }

        [JSFunction(Name = "countWorkflowsInList")]
        public ObjectInstance CountWorkflowsInList(SPListInstance list)
        {
            if (list == null)
                throw new JavaScriptException(this.Engine, "Error", "An instance of a SPList object must be supplied as the first argument.");

            var result = m_workflowManager.CountWorkflows(list.List);

            var obj = this.Engine.Object.Construct();
            foreach (var r in result)
                obj.SetPropertyValue(r.Key.ToString(), r.Value, false);
            return obj;
        }

        [JSFunction(Name = "countWorkflowsByAssociation")]
        public int CountWorkflowsByAssociation(SPWorkflowAssociationInstance association)
        {
            if (association == null)
                throw new JavaScriptException(this.Engine, "Error", "An instance of a SPWorkflowAssociation object must be supplied as the first argument.");

            return m_workflowManager.CountWorkflows(association.SPWorkflowAssociation);
        }

        [JSFunction(Name = "countWorkflowsByTemplate")]
        public int CountWorkflowsByTemplate(SPWorkflowTemplateInstance template, SPSiteInstance site)
        {
            if (template == null)
                throw new JavaScriptException(this.Engine, "Error", "An instance of a SPWorkflowTemplate object must be supplied as the first argument.");

            if (site == null)
                throw new JavaScriptException(this.Engine, "Error", "An instance of a SPSite object must be supplied as the second argument.");

            return m_workflowManager.CountWorkflows(template.SPWorkflowTemplate, site.Site);
        }

        [JSFunction(Name = "dispose")]
        public void Dispose()
        {
            m_workflowManager.Dispose();
        }

        [JSFunction(Name = "getItemActiveWorkflows")]
        public SPWorkflowCollectionInstance GetItemActiveWorkflows(SPListItemInstance item)
        {
            if (item == null)
                throw new JavaScriptException(this.Engine, "Error",
                    "An instance of a SPListItem object must be supplied as the first argument.");

            var result = m_workflowManager.GetItemActiveWorkflows(item.ListItem);
            return result == null
                ? null
                : new SPWorkflowCollectionInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "getItemTasks")]
        public SPWorkflowTaskCollectionInstance GetItemTasks(SPListItemInstance item, object filter)
        {
            if (item == null)
                throw new JavaScriptException(this.Engine, "Error",
                    "An instance of a SPListItem object must be supplied as the first argument.");

            SPWorkflowTaskCollection result;
            if (filter == Undefined.Value)
                result = m_workflowManager.GetItemTasks(item.ListItem);
            else
            {
                var wf = filter as SPWorkflowFilterInstance;
                if (wf == null)
                    throw new JavaScriptException(this.Engine, "Error",
                    "An instance of a SPWorkflowFilter object must be supplied as the second argument.");
                result = m_workflowManager.GetItemTasks(item.ListItem, wf.SPWorkflowFilter);
            }

            return result == null
                ? null
                : new SPWorkflowTaskCollectionInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "getItemWorkflows")]
        public SPWorkflowCollectionInstance GetItemWorkflows(SPListItemInstance item, object filter)
        {
            if (item == null)
                throw new JavaScriptException(this.Engine, "Error",
                    "An instance of a SPListItem object must be supplied as the first argument.");

            SPWorkflowCollection result;
            if (filter == Undefined.Value)
                result = m_workflowManager.GetItemWorkflows(item.ListItem);
            else
            {
                var wf = filter as SPWorkflowFilterInstance;
                if (wf == null)
                    throw new JavaScriptException(this.Engine, "Error",
                    "An instance of a SPWorkflowFilter object must be supplied as the second argument.");
                result = m_workflowManager.GetItemWorkflows(item.ListItem, wf.SPWorkflowFilter);
            }

            return result == null
                ? null
                : new SPWorkflowCollectionInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "getWorkflowAvailableRunCount")]
        public int GetWorkflowAvailableRunCount(SPWebInstance web)
        {
            if (web == null)
                throw new JavaScriptException(this.Engine, "Error",
                    "An instance of a SPWeb object must be supplied as the first argument.");

            return m_workflowManager.GetWorkflowAvailableRunCount(web.Web);
        }

        [JSFunction(Name = "getWorkflowAvailableRunCount")]
        public SPWorkflowTaskCollectionInstance GetWorkflowAvailableRunCount(SPListItemInstance listItem, object guidOrWorkflow, object filter)
        {
            if (listItem == null)
                throw new JavaScriptException(this.Engine, "Error",
                    "An instance of a SPListItem object must be supplied as the first argument.");

            SPWorkflowTaskCollection result;
            if (guidOrWorkflow is SPWorkflowInstance)
            {
                if (filter == Undefined.Value)
                    result = m_workflowManager.GetWorkflowTasks(listItem.ListItem,
                        (guidOrWorkflow as SPWorkflowInstance).SPWorkflow);
                else
                {
                    var wf = filter as SPWorkflowFilterInstance;
                    if (wf == null)
                        throw new JavaScriptException(this.Engine, "Error",
                            "An instance of a SPWorkflowFilter object must be supplied as the second argument.");
                    result = m_workflowManager.GetWorkflowTasks(listItem.ListItem,
                        (guidOrWorkflow as SPWorkflowInstance).SPWorkflow, wf.SPWorkflowFilter);
                }
            }
            else
            {
                var guid = GuidInstance.ConvertFromJsObjectToGuid(guidOrWorkflow);
                if (filter == Undefined.Value)
                    result = m_workflowManager.GetWorkflowTasks(listItem.ListItem,
                        guid);
                else
                {
                    var wf = filter as SPWorkflowFilterInstance;
                    if (wf == null)
                        throw new JavaScriptException(this.Engine, "Error",
                            "An instance of a SPWorkflowFilter object must be supplied as the second argument.");
                    result = m_workflowManager.GetWorkflowTasks(listItem.ListItem,
                        guid, wf.SPWorkflowFilter);
                }
            }

            return result == null
                ? null
                : new SPWorkflowTaskCollectionInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "getWorkflowTemplatesByCategory")]
        public SPWorkflowTemplateCollectionInstance GetWorkflowTemplatesByCategory(SPWebInstance web, string strReqCaegs)
        {
            if (web == null)
                throw new JavaScriptException(this.Engine, "Error",
                    "An instance of a SPWeb object must be supplied as the first argument.");

            var result = m_workflowManager.GetWorkflowTemplatesByCategory(web.Web, strReqCaegs);
            return result == null
                ? null
                : new SPWorkflowTemplateCollectionInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "removeWorkflowFromListItem")]
        public void RemoveWorkflowFromListItem(SPWorkflowInstance workflow)
        {
            if (workflow == null)
                throw new JavaScriptException(this.Engine, "Error",
                    "An instance of a SPWorkflow object must be supplied as the first argument.");

            m_workflowManager.RemoveWorkflowFromListItem(workflow.SPWorkflow);
        }

        [JSFunction(Name = "startWorkflowWithContext")]
        public SPWorkflowInstance StartWorkflowWithContext(object context, SPWorkflowAssociationInstance association, string eventData, string runOptions)
        {
            if (association == null)
                throw new JavaScriptException(this.Engine, "Error",
                    "An instance of a SPWorkflowAssociation object must be supplied as the second argument.");

            SPWorkflowRunOptions wfRunOptions;
            if (!runOptions.TryParseEnum(true, out wfRunOptions))
                throw new JavaScriptException(this.Engine, "Error", "The runOptions argument must be convertable to a SPWorkflowRunOptions enum value.");

            var result = m_workflowManager.StartWorkflow(TypeConverter.ToObject(this.Engine, context),
                association.SPWorkflowAssociation, eventData, wfRunOptions);

            return result == null
                ? null
                : new SPWorkflowInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "startWorkflow")]
        public SPWorkflowInstance StartWorkflow(SPListItemInstance listItem, SPWorkflowAssociationInstance association, string eventData, object isAutoStart)
        {
            if (listItem == null)
                throw new JavaScriptException(this.Engine, "Error",
                    "An instance of a SPListItem object must be supplied as the first argument.");

            if (association == null)
                throw new JavaScriptException(this.Engine, "Error",
                    "An instance of a SPWorkflowAssociation object must be supplied as the second argument.");

            //Impersonate the system account.
            SPWorkflow result;
            if (isAutoStart == Undefined.Value)
                result = m_workflowManager.StartWorkflow(listItem.ListItem,
                    association.SPWorkflowAssociation, eventData);
            else
                result = m_workflowManager.StartWorkflow(listItem.ListItem,
                    association.SPWorkflowAssociation, eventData, TypeConverter.ToBoolean(isAutoStart));
            
            return result == null
                ? null
                : new SPWorkflowInstance(this.Engine.Object.InstancePrototype, result);
        }
    }
}
