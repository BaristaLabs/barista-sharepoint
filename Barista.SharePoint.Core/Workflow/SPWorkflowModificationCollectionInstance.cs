namespace Barista.SharePoint.Workflow
{
    //Complete 6/10/14

    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using Barista.Library;
    using Microsoft.SharePoint.Workflow;
    using System;
    using System.Linq;

    [Serializable]
    public class SPWorkflowModificationCollectionConstructor : ClrFunction
    {
        public SPWorkflowModificationCollectionConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPWorkflowModificationCollection", new SPWorkflowModificationCollectionInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public SPWorkflowModificationCollectionInstance Construct()
        {
            return new SPWorkflowModificationCollectionInstance(InstancePrototype);
        }
    }

    [Serializable]
    public class SPWorkflowModificationCollectionInstance : ObjectInstance
    {
        private readonly SPWorkflowModificationCollection m_workflowModificationCollection;

        public SPWorkflowModificationCollectionInstance(ObjectInstance prototype)
            : base(prototype)
        {
            PopulateFields();
            PopulateFunctions();
        }

        public SPWorkflowModificationCollectionInstance(ObjectInstance prototype, SPWorkflowModificationCollection workflowModificationCollection)
            : this(prototype)
        {
            if (workflowModificationCollection == null)
                throw new ArgumentNullException("workflowModificationCollection");

            m_workflowModificationCollection = workflowModificationCollection;
        }

        public SPWorkflowModificationCollection SPWorkflowModificationCollection
        {
            get
            {
                return m_workflowModificationCollection;
            }
        }

        [JSProperty(Name = "count")]
        public int Count
        {
            get
            {
                return m_workflowModificationCollection.Count;
            }
        }

        [JSFunction(Name = "getWorkflowModificationById")]
        public SPWorkflowModificationInstance GetWorkflowAssociationById(object id)
        {
            var guidId = GuidInstance.ConvertFromJsObjectToGuid(id);
            var result = m_workflowModificationCollection[guidId];
            return result == null
                ? null
                : new SPWorkflowModificationInstance(Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "getWorkflowModificationByIndex")]
        public SPWorkflowModificationInstance GetWorkflowAssociationByIndex(int index)
        {
            var result = m_workflowModificationCollection[index];
            return result == null
                ? null
                : new SPWorkflowModificationInstance(Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "toArray")]
        [JSDoc("ternReturnType", "[+SPWorkflowModification]")]
        public ArrayInstance ToArray()
        {
            var result = Engine.Array.Construct();
            foreach (var wfm in m_workflowModificationCollection
                .OfType<SPWorkflowModification>()
                .Select(a => new SPWorkflowModificationInstance(Engine.Object.InstancePrototype, a)))
            {
                ArrayInstance.Push(result, wfm);
            }

            return result;
        }
    }
}
