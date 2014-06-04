namespace Barista.SharePoint.Workflow
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using Microsoft.SharePoint.Workflow;
    using System;

    [Serializable]
    public class SPWorkflowCollectionConstructor : ClrFunction
    {
        public SPWorkflowCollectionConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPWorkflowCollection", new SPWorkflowCollectionInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public SPWorkflowCollectionInstance Construct()
        {
            return new SPWorkflowCollectionInstance(this.InstancePrototype);
        }
    }

    [Serializable]
    public class SPWorkflowCollectionInstance : ObjectInstance
    {
        private readonly SPWorkflowCollection m_workflowCollection;

        public SPWorkflowCollectionInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public SPWorkflowCollectionInstance(ObjectInstance prototype, SPWorkflowCollection workflowCollection)
            : this(prototype)
        {
            if (workflowCollection == null)
                throw new ArgumentNullException("workflowCollection");

            m_workflowCollection = workflowCollection;
        }

        public SPWorkflowCollection SPWorkflowCollection
        {
            get
            {
                return m_workflowCollection;
            }
        }
    }
}
