namespace Barista.SharePoint.Workflow
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Microsoft.SharePoint.Workflow;

    [Serializable]
    public class SPWorkflowTemplateCollectionConstructor : ClrFunction
    {
        public SPWorkflowTemplateCollectionConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPWorkflowTemplateCollection", new SPWorkflowTemplateCollectionInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public SPWorkflowTemplateCollectionInstance Construct()
        {
            return new SPWorkflowTemplateCollectionInstance(this.InstancePrototype);
        }
    }

    [Serializable]
    public class SPWorkflowTemplateCollectionInstance : ObjectInstance
    {
        private readonly SPWorkflowTemplateCollection m_workflowTemplateCollection;

        public SPWorkflowTemplateCollectionInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public SPWorkflowTemplateCollectionInstance(ObjectInstance prototype, SPWorkflowTemplateCollection workflowTemplateCollection)
            : this(prototype)
        {
            if (workflowTemplateCollection == null)
                throw new ArgumentNullException("workflowTemplateCollection");

            m_workflowTemplateCollection = workflowTemplateCollection;
        }

        public SPWorkflowTemplateCollection SPWorkflowTemplateCollection
        {
            get
            {
                return m_workflowTemplateCollection;
            }
        }
    }
}
