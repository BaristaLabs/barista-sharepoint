using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Barista.SharePoint.Workflow
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Microsoft.SharePoint.Workflow;

    [Serializable]
    public class SPWorkflowTaskConstructor : ClrFunction
    {
        public SPWorkflowTaskConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPWorkflowTask", new SPWorkflowTaskInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public SPWorkflowTaskInstance Construct()
        {
            return new SPWorkflowTaskInstance(this.InstancePrototype);
        }
    }

    [Serializable]
    public class SPWorkflowTaskInstance : ObjectInstance
    {
        private readonly SPWorkflowTask m_workflowTask;

        public SPWorkflowTaskInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public SPWorkflowTaskInstance(ObjectInstance prototype, SPWorkflowTask workflowTask)
            : this(prototype)
        {
            if (workflowTask == null)
                throw new ArgumentNullException("workflowTask");

            m_workflowTask = workflowTask;
        }

        public SPWorkflowTask SPWorkflowTask
        {
            get
            {
                return m_workflowTask;
            }
        }
    }
}
