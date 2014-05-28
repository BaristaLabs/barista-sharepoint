namespace Barista.SharePoint.Workflow
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Microsoft.SharePoint.Workflow;

    [Serializable]
    public class SPWorkflowConstructor : ClrFunction
    {
        public SPWorkflowConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPWorkflow", new SPWorkflowInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public SPWorkflowInstance Construct()
        {
            return new SPWorkflowInstance(this.InstancePrototype);
        }
    }

    [Serializable]
    public class SPWorkflowInstance : ObjectInstance
    {
        private readonly SPWorkflow m_workflow;

        public SPWorkflowInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public SPWorkflowInstance(ObjectInstance prototype, SPWorkflow workflow)
            : this(prototype)
        {
            if (workflow == null)
                throw new ArgumentNullException("workflow");

            m_workflow = workflow;
        }

        public SPWorkflow SPWorkflow
        {
            get { return m_workflow; }
        }
    }
}
