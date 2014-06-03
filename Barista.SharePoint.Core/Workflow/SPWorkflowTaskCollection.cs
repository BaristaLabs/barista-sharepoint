namespace Barista.SharePoint.Workflow
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Microsoft.SharePoint.Workflow;

    [Serializable]
    public class SPWorkflowTaskCollectionConstructor : ClrFunction
    {
        public SPWorkflowTaskCollectionConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPWorkflowTaskCollection", new SPWorkflowTaskCollectionInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public SPWorkflowTaskCollectionInstance Construct()
        {
            return new SPWorkflowTaskCollectionInstance(this.InstancePrototype);
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
    }
}
