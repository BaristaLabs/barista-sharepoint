namespace Barista.SharePoint.Workflow
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Microsoft.SharePoint.Workflow;

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
            return new SPWorkflowModificationCollectionInstance(this.InstancePrototype);
        }
    }

    [Serializable]
    public class SPWorkflowModificationCollectionInstance : ObjectInstance
    {
        private readonly SPWorkflowModificationCollection m_workflowModificationCollection;

        public SPWorkflowModificationCollectionInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
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
    }
}
