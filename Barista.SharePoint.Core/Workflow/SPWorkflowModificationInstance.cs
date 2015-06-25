namespace Barista.SharePoint.Workflow
{
    //Complete 6/10/14

    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using Barista.Library;
    using Microsoft.SharePoint.Workflow;
    using System;

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

        [JSProperty(Name="contextData")]
        public string ContextData
        {
            get
            {
                return m_workflowModification.ContextData;
            }
        }

        [JSProperty(Name = "id")]
        public GuidInstance Id
        {
            get
            {
                return new GuidInstance(this.Engine.Object.InstancePrototype, m_workflowModification.Id);
            }
        }

        [JSProperty(Name = "nameFormatData")]
        public string NameFormatData
        {
            get
            {
                return m_workflowModification.NameFormatData;
            }
        }

        [JSProperty(Name = "typeId")]
        public GuidInstance TypeId
        {
            get
            {
                return new GuidInstance(this.Engine.Object.InstancePrototype, m_workflowModification.TypeId);
            }
        }
    }
}
