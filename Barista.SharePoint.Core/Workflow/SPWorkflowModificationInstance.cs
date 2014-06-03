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
    public class SPWorkflowModificationConstructor : ClrFunction
    {
        public SPWorkflowModificationConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPWorkflowModification", new SPWorkflowModificationInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public SPWorkflowModificationInstance Construct()
        {
            return new SPWorkflowModificationInstance(this.InstancePrototype);
        }
    }

    [Serializable]
    public class SPWorkflowModificationInstance : ObjectInstance
    {
        private readonly SPWorkflowModification m_workflowModification;

        public SPWorkflowModificationInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public SPWorkflowModificationInstance(ObjectInstance prototype, SPWorkflowModification workflowModification)
            : this(prototype)
        {
            if (workflowModification == null)
                throw new ArgumentNullException("workflowModification");

            m_workflowModification = workflowModification;
        }

        public SPWorkflowModification SPWorkflowModification
        {
            get
            {
                return m_workflowModification;
            }
        }
    }
}
