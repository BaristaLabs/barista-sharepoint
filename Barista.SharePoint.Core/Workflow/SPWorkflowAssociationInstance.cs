namespace Barista.SharePoint.Workflow
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Microsoft.SharePoint.Workflow;

    [Serializable]
    public class SPWorkflowAssociationConstructor : ClrFunction
    {
        public SPWorkflowAssociationConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPWorkflowAssociation", new SPWorkflowAssociationInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public SPWorkflowAssociationInstance Construct()
        {
            return new SPWorkflowAssociationInstance(this.InstancePrototype);
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
    }
}
