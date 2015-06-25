namespace Barista.SharePoint.Workflow
{
    //Complete 6/10/14

    using Barista.Extensions;
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
        public SPWorkflowFilterInstance Construct(object inclusiveFilterStates, object exclusiveFilterStates, object filterAssignedTo)
        {
            if (inclusiveFilterStates == Undefined.Value && exclusiveFilterStates == Undefined.Value && filterAssignedTo == Undefined.Value)
                return new SPWorkflowFilterInstance(this.InstancePrototype, new SPWorkflowFilter());
            
            if (filterAssignedTo == Undefined.Value)
            {
                SPWorkflowState wsInclusiveFilterStates;
                if (!TypeConverter.ToString(inclusiveFilterStates).TryParseEnum(true, out wsInclusiveFilterStates))
                    throw new JavaScriptException(this.Engine, "Error", "inclusiveFilerStates argument must be a valid SPWorkflowState.");

                SPWorkflowState wsExclusiveFilterStates;
                if (!TypeConverter.ToString(exclusiveFilterStates).TryParseEnum(true, out wsExclusiveFilterStates))
                    throw new JavaScriptException(this.Engine, "Error", "exclusiveFilterStates argument must be a valid SPWorkflowState.");

                return new SPWorkflowFilterInstance(this.InstancePrototype, new SPWorkflowFilter(wsInclusiveFilterStates, wsExclusiveFilterStates));
            }
            else
            {
                SPWorkflowState wsInclusiveFilterStates;
                if (!TypeConverter.ToString(inclusiveFilterStates).TryParseEnum(true, out wsInclusiveFilterStates))
                    throw new JavaScriptException(this.Engine, "Error", "inclusiveFilerStates argument must be a valid SPWorkflowState.");

                SPWorkflowState wsExclusiveFilterStates;
                if (!TypeConverter.ToString(exclusiveFilterStates).TryParseEnum(true, out wsExclusiveFilterStates))
                    throw new JavaScriptException(this.Engine, "Error", "exclusiveFilterStates argument must be a valid SPWorkflowState.");

                SPWorkflowAssignedToFilter watfFilterAssignedTo;
                if (!TypeConverter.ToString(filterAssignedTo).TryParseEnum(true, out watfFilterAssignedTo))
                    throw new JavaScriptException(this.Engine, "Error", "filterAssignedTo argument must be a valid SPWorkflowAssignedToFilter.");

                return new SPWorkflowFilterInstance(this.InstancePrototype, new SPWorkflowFilter(wsInclusiveFilterStates, wsExclusiveFilterStates, watfFilterAssignedTo));
            }

        }
    }

    [Serializable]
    public class SPWorkflowFilterInstance : ObjectInstance
    {
        private readonly SPWorkflowFilter m_workflowFilter;

        public SPWorkflowFilterInstance(ObjectInstance prototype)
            : base(prototype)
        {
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

        [JSProperty(Name = "assignedTo")]
        public string AssignedTo
        {
            get
            {
                return m_workflowFilter.AssignedTo.ToString();
            }
            set
            {
                SPWorkflowAssignedToFilter watfFilterAssignedTo;
                if (!value.TryParseEnum(true, out watfFilterAssignedTo))
                    throw new JavaScriptException(this.Engine, "Error", "value must be a valid SPWorkflowAssignedToFilter.");

                m_workflowFilter.AssignedTo = watfFilterAssignedTo;

            }
        }

        [JSProperty(Name = "exclusiveFilterStates")]
        public string ExclusiveFilterStates
        {
            get
            {
                return m_workflowFilter.ExclusiveFilterStates.ToString();
            }
            set
            {
                SPWorkflowState wsValue;
                if (!value.TryParseEnum(true, out wsValue))
                    throw new JavaScriptException(this.Engine, "Error", "value must be a valid SPWorkflowState.");

                m_workflowFilter.ExclusiveFilterStates = wsValue;

            }
        }

        [JSProperty(Name = "inclusiveFilterStates")]
        public string InclusiveFilterStates
        {
            get
            {
                return m_workflowFilter.InclusiveFilterStates.ToString();
            }
            set
            {
                SPWorkflowState wsValue;
                if (!value.TryParseEnum(true, out wsValue))
                    throw new JavaScriptException(this.Engine, "Error", "value must be a valid SPWorkflowState.");

                m_workflowFilter.InclusiveFilterStates = wsValue;

            }
        }
    }
}
