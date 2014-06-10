namespace Barista.SharePoint.Workflow
{
    //Complete 6/10/14

    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using Barista.Library;
    using Barista.SharePoint.Library;
    using Microsoft.SharePoint.Workflow;
    using System;
    using System.Linq;

    [Serializable]
    public class SPWorkflowTaskCollectionConstructor : ClrFunction
    {
        public SPWorkflowTaskCollectionConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPWorkflowTaskCollection", new SPWorkflowTaskCollectionInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public SPWorkflowTaskCollectionInstance Construct(object arg1, SPWorkflowFilterInstance filter)
        {
            if (filter == null)
                throw new JavaScriptException(this.Engine, "Error", "A filter must be supplied as the second argument.");
            if (arg1 is SPListItemInstance)
            {
                return new SPWorkflowTaskCollectionInstance(this.Engine.Object.InstancePrototype, new SPWorkflowTaskCollection((arg1 as SPListItemInstance).ListItem, filter.SPWorkflowFilter));
            }

            if (arg1 is SPWorkflowCollectionInstance)
            {
                return new SPWorkflowTaskCollectionInstance(this.Engine.Object.InstancePrototype, new SPWorkflowTaskCollection((arg1 as SPWorkflowCollectionInstance).SPWorkflowCollection, filter.SPWorkflowFilter));
            }

            throw new JavaScriptException(this.Engine, "Error", "The first argument must either be a list item or a workflow collection.");
        }
    }

    [Serializable]
    public class SPWorkflowTaskCollectionInstance : ObjectInstance
    {
        private readonly SPWorkflowTaskCollection m_workflowTaskCollection;

        public SPWorkflowTaskCollectionInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public SPWorkflowTaskCollectionInstance(ObjectInstance prototype, SPWorkflowTaskCollection workflowTaskCollection)
            : this(prototype)
        {
            if (workflowTaskCollection == null)
                throw new ArgumentNullException("workflowTaskCollection");

            m_workflowTaskCollection = workflowTaskCollection;
        }

        public SPWorkflowTaskCollection SPWorkflowTaskCollection
        {
            get
            {
                return m_workflowTaskCollection;
            }
        }

        [JSProperty(Name = "count")]
        public int Count
        {
            get
            {
                return m_workflowTaskCollection.Count;
            }
        }

        [JSFunction(Name = "delete")]
        public void Delete(int index)
        {
            m_workflowTaskCollection.Delete(index);
        }

        [JSFunction(Name = "getTaskById")]
        public SPWorkflowTaskInstance GetWorkflowAssociationById(object id)
        {
            var guidId = GuidInstance.ConvertFromJsObjectToGuid(id);
            var result = m_workflowTaskCollection[guidId];
            return result == null
                ? null
                : new SPWorkflowTaskInstance(this.Engine, result);
        }

        [JSFunction(Name = "getTaskByIndex")]
        public SPWorkflowTaskInstance GetWorkflowAssociationByIndex(int index)
        {
            var result = m_workflowTaskCollection[index];
            return result == null
                ? null
                : new SPWorkflowTaskInstance(this.Engine, result);
        }

        [JSFunction(Name = "getXml")]
        public string GetXml()
        {
            return m_workflowTaskCollection.Xml;
        }

        [JSFunction(Name = "toArray")]
        public ArrayInstance ToArray()
        {
            var result = this.Engine.Array.Construct();
            foreach (var wf in m_workflowTaskCollection
                .OfType<SPWorkflowTask>()
                .Select(a => new SPWorkflowTaskInstance(this.Engine, a)))
            {
                ArrayInstance.Push(this.Engine.Object.InstancePrototype, wf);
            }

            return result;
        }
    }
}
