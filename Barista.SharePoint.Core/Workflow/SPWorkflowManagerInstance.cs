namespace Barista.SharePoint.Workflow
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Microsoft.SharePoint.Workflow;

    [Serializable]
    public class SPWorkflowManagerConstructor : ClrFunction
    {
        public SPWorkflowManagerConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPWorkflowManager", new SPWorkflowManagerInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public SPWorkflowManagerInstance Construct()
        {
            return new SPWorkflowManagerInstance(this.InstancePrototype);
        }
    }

    [Serializable]
    public class SPWorkflowManagerInstance : ObjectInstance
    {
        private readonly SPWorkflowManager m_workflowManager;

        public SPWorkflowManagerInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public SPWorkflowManagerInstance(ObjectInstance prototype, SPWorkflowManager workflowManager)
            : this(prototype)
        {
            if (workflowManager == null)
                throw new ArgumentNullException("workflowManager");

            m_workflowManager = workflowManager;
        }

        public SPWorkflowManager SPWorkflowManager
        {
            get { return m_workflowManager; }
        }

        [JSProperty(Name = "shuttingDown")]
        public bool ShuttingDown
        {
            get;
            set;
        }
    }
}
