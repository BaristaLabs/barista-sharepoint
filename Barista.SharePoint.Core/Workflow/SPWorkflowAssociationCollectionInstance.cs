namespace Barista.SharePoint.Workflow
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Microsoft.SharePoint.Workflow;

    [Serializable]
    public class SPWorkflowAssociationCollectionConstructor : ClrFunction
    {
        public SPWorkflowAssociationCollectionConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPWorkflowAssociationCollection", new SPWorkflowAssociationCollectionInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public SPWorkflowAssociationCollectionInstance Construct()
        {
            return new SPWorkflowAssociationCollectionInstance(this.InstancePrototype);
        }
    }

    [Serializable]
    public class SPWorkflowAssociationCollectionInstance : ObjectInstance
    {
        private readonly SPWorkflowAssociationCollection m_workflowAssociationCollection;

        public SPWorkflowAssociationCollectionInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public SPWorkflowAssociationCollectionInstance(ObjectInstance prototype, SPWorkflowAssociationCollection workflowAssociationCollection)
            : this(prototype)
        {
            if (workflowAssociationCollection == null)
                throw new ArgumentNullException("workflowAssociationCollection");

            m_workflowAssociationCollection = workflowAssociationCollection;
        }

        public SPWorkflowAssociationCollection SPWorkflowAssociationCollection
        {
            get
            {
                return m_workflowAssociationCollection;
            }
        }
    }
}
