namespace Barista.SharePoint.Workflow
{
    //Complete 6/10/14

    using System.Reflection;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Barista.Library;
    using Microsoft.SharePoint.Workflow;
    using Barista.SharePoint.Library;

    [Serializable]
    public class SPWorkflowTaskConstructor : ClrFunction
    {
        public SPWorkflowTaskConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPWorkflowTask", new SPWorkflowTaskInstance(engine, null))
        {
            PopulateFunctions();
        }

        [JSFunction(Name="alterTask")]
        public bool AlterTask(SPListItemInstance task, HashtableInstance htData, bool synchronous)
        {
            if (task == null)
                throw new JavaScriptException(this.Engine, "Error", "A task must be specified.");

            if (htData == null)
                throw new JavaScriptException(this.Engine, "Error", "A hashtable of data must be specified.");

            return SPWorkflowTask.AlterTask(task.ListItem, htData.Hashtable, synchronous);
        }

        [JSFunction(Name = "getExtendedPropertiesAsHashtable")]
        public HashtableInstance GetExtendedPropertiesAsHashtable(SPListItemInstance task)
        {
            if (task == null)
                throw new JavaScriptException(this.Engine, "Error", "A task must be specified.");

            var result = SPWorkflowTask.GetExtendedPropertiesAsHashtable(task.ListItem);
            return result == null
                ? null
                : new HashtableInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "getWorkflowData")]
        public string GetWorkflowData(SPListItemInstance task)
        {
            if (task == null)
                throw new JavaScriptException(this.Engine, "Error", "A task must be specified.");

            return SPWorkflowTask.GetWorkflowData(task.ListItem);
        }
    }

    [Serializable]
    public class SPWorkflowTaskInstance : SPListItemInstance
    {
        private readonly SPWorkflowTask m_workflowTask;

        public SPWorkflowTaskInstance(ScriptEngine engine, SPWorkflowTask workflowTask)
            : base(new SPListItemInstance(engine, workflowTask))
        {
            if (workflowTask == null)
                throw new ArgumentNullException("workflowTask");

            this.m_workflowTask = workflowTask;
            this.ListItem = workflowTask;
            this.SecurableObject = workflowTask;

            this.PopulateFunctions(this.GetType(), BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
        }

        public SPWorkflowTask SPWorkflowTask
        {
            get
            {
                return m_workflowTask;
            }
        }

        [JSProperty(Name = "workflowId")]
        public GuidInstance WorkflowId
        {
            get
            {
                return new GuidInstance(this.Engine.Object.InstancePrototype, m_workflowTask.WorkflowId);
            }
        }

        [JSProperty(Name = "xml")]
        public string Xml
        {
            get
            {
                return m_workflowTask.Xml;
            }
        }
    }
}
