namespace Barista.SharePoint.Workflow
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Microsoft.SharePoint.Workflow;

    [Serializable]
    public class SPWorkflowFilterConstructor : ClrFunction
    {
        public SPWorkflowFilterConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPWorkflowFilter", new SPWorkflowFilterInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public SPWorkflowFilterInstance Construct()
        {
            return new SPWorkflowFilterInstance(this.InstancePrototype);
        }
    }

    [Serializable]
    public class SPWorkflowFilterInstance : ObjectInstance
    {
        private readonly SPWorkflowFilter m_workflowFilter;

        public SPWorkflowFilterInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public SPWorkflowFilterInstance(ObjectInstance prototype, SPWorkflowFilter workflowFilter)
            : this(prototype)
        {
            if (workflowFilter == null)
                throw new ArgumentNullException("workflowFilter");

            m_workflowFilter = workflowFilter;
        }

        public SPWorkflowFilter SPWorkflowFilter
        {
            get
            {
                return m_workflowFilter;
            }
        }
    }
}
