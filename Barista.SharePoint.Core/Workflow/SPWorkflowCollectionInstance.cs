namespace Barista.SharePoint.Workflow
{
    //Complete 6/10/14

    using Barista.Extensions;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using Barista.Library;
    using Barista.SharePoint.Library;
    using Microsoft.SharePoint.Workflow;
    using System;
    using System.Linq;

    [Serializable]
    public class SPWorkflowCollectionConstructor : ClrFunction
    {
        public SPWorkflowCollectionConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPWorkflowCollection", new SPWorkflowCollectionInstance(engine.Object.InstancePrototype))
        {
            PopulateFunctions();
        }

        [JSConstructorFunction]
        public SPWorkflowCollectionInstance Construct()
        {
            throw new JavaScriptException(this.Engine, "Error", "Use a factory method instead.");
        }

        [JSFunction(Name = "createWorkflowCollectionFromWeb")]
        public SPWorkflowCollectionInstance CreateWorkflowFromWeb(SPWebInstance web,  object inclusiveFilterStates,
            object exclusiveFilterStates, object rowCountLimit, object descending)
        {
            if (web == null)
                throw new JavaScriptException(this.Engine, "Error", "A web must be supplied.");

            SPWorkflowCollection result;
            if (inclusiveFilterStates == Undefined.Value && exclusiveFilterStates == Undefined.Value &&
                rowCountLimit == Undefined.Value && descending == Undefined.Value)
                result = new SPWorkflowCollection(web.Web);
            else
            {
                SPWorkflowState wsInclusiveFilterStates;
                if (!TypeConverter.ToString(inclusiveFilterStates).TryParseEnum(true, out wsInclusiveFilterStates))
                    throw new JavaScriptException(this.Engine, "Error", "inclusiveFilerStates argument must be a valid SPWorkflowState.");

                SPWorkflowState wsExclusiveFilterStates;
                if (!TypeConverter.ToString(exclusiveFilterStates).TryParseEnum(true, out wsExclusiveFilterStates))
                    throw new JavaScriptException(this.Engine, "Error", "exclusiveFilterStates argument must be a valid SPWorkflowState.");

                result = new SPWorkflowCollection(web.Web, wsInclusiveFilterStates, wsExclusiveFilterStates, TypeConverter.ToInteger(rowCountLimit), TypeConverter.ToBoolean(descending));
            }

            return new SPWorkflowCollectionInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "createWorkflowCollectionFromList")]
        public SPWorkflowCollectionInstance CreateWorkflowFromList(SPListInstance list, object associationId)
        {
            if (list == null)
                throw new JavaScriptException(this.Engine, "Error", "A list must be supplied.");

            var result = associationId == Undefined.Value
                ? new SPWorkflowCollection(list.List)
                : new SPWorkflowCollection(list.List, GuidInstance.ConvertFromJsObjectToGuid(associationId));

            return new SPWorkflowCollectionInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "createWorkflowCollectionFromListItem")]
        public SPWorkflowCollectionInstance CreateWorkflowFromListItem(SPListItemInstance listItem, object inclusiveFilterStates,
            object exclusiveFilterStates, object rowCountLimit, object ascending)
        {
            if (listItem == null)
                throw new JavaScriptException(this.Engine, "Error", "A listItem must be supplied.");

            SPWorkflowCollection result;
            if (inclusiveFilterStates == Undefined.Value && exclusiveFilterStates == Undefined.Value &&
                rowCountLimit == Undefined.Value && ascending == Undefined.Value)
                result = new SPWorkflowCollection(listItem.ListItem);
            else if (rowCountLimit == Undefined.Value && ascending == Undefined.Value)
            {
                SPWorkflowState wsInclusiveFilterStates;
                if (!TypeConverter.ToString(inclusiveFilterStates).TryParseEnum(true, out wsInclusiveFilterStates))
                    throw new JavaScriptException(this.Engine, "Error", "inclusiveFilerStates argument must be a valid SPWorkflowState.");

                SPWorkflowState wsExclusiveFilterStates;
                if (!TypeConverter.ToString(exclusiveFilterStates).TryParseEnum(true, out wsExclusiveFilterStates))
                    throw new JavaScriptException(this.Engine, "Error", "exclusiveFilterStates argument must be a valid SPWorkflowState.");

                result = new SPWorkflowCollection(listItem.ListItem, wsInclusiveFilterStates, wsExclusiveFilterStates);
            }
            else
            {
                SPWorkflowState wsInclusiveFilterStates;
                if (!TypeConverter.ToString(inclusiveFilterStates).TryParseEnum(true, out wsInclusiveFilterStates))
                    throw new JavaScriptException(this.Engine, "Error", "inclusiveFilerStates argument must be a valid SPWorkflowState.");

                SPWorkflowState wsExclusiveFilterStates;
                if (!TypeConverter.ToString(exclusiveFilterStates).TryParseEnum(true, out wsExclusiveFilterStates))
                    throw new JavaScriptException(this.Engine, "Error", "exclusiveFilterStates argument must be a valid SPWorkflowState.");

                result = new SPWorkflowCollection(listItem.ListItem, wsInclusiveFilterStates, wsExclusiveFilterStates, TypeConverter.ToInteger(rowCountLimit), TypeConverter.ToBoolean(ascending));
            }

            return new SPWorkflowCollectionInstance(this.Engine.Object.InstancePrototype, result);
        }
    }

    [Serializable]
    public class SPWorkflowCollectionInstance : ObjectInstance
    {
        private readonly SPWorkflowCollection m_workflowCollection;

        public SPWorkflowCollectionInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public SPWorkflowCollectionInstance(ObjectInstance prototype, SPWorkflowCollection workflowCollection)
            : this(prototype)
        {
            if (workflowCollection == null)
                throw new ArgumentNullException("workflowCollection");

            m_workflowCollection = workflowCollection;
        }

        public SPWorkflowCollection SPWorkflowCollection
        {
            get
            {
                return m_workflowCollection;
            }
        }

        [JSProperty(Name = "count")]
        public int Count
        {
            get
            {
                return m_workflowCollection.Count;
            }
        }

        [JSProperty(Name = "exclusiveFilterStates")]
        public string ExclusiveFilterStates
        {
            get
            {
                return m_workflowCollection.ExclusiveFilterStates.ToString();
            }
        }

        [JSProperty(Name = "inclusiveFilterStates")]
        public string InclusiveFilterStates
        {
            get
            {
                return m_workflowCollection.InclusiveFilterStates.ToString();
            }
        }

        [JSFunction(Name = "getInstanceIds")]
        public ArrayInstance GetInstanceIds()
        {
            var result = this.Engine.Array.Construct();
            foreach (var id in m_workflowCollection.GetInstanceIds()
                .Select(guid => new GuidInstance(this.Engine.Object.InstancePrototype, guid)))
            {
                ArrayInstance.Push(this.Engine.Object.InstancePrototype, id);
            }

            return result;
        }

        [JSFunction(Name = "getWorkflowById")]
        public SPWorkflowInstance GetWorkflowAssociationById(object id)
        {
            var guidId = GuidInstance.ConvertFromJsObjectToGuid(id);
            var result = m_workflowCollection[guidId];
            return result == null
                ? null
                : new SPWorkflowInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "getWorkflowByIndex")]
        public SPWorkflowInstance GetWorkflowAssociationByIndex(int index)
        {
            var result = m_workflowCollection[index];
            return result == null
                ? null
                : new SPWorkflowInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "getXml")]
        public string GetXml()
        {
            return m_workflowCollection.Xml;
        }

        [JSFunction(Name = "toArray")]
        public ArrayInstance ToArray()
        {
            var result = this.Engine.Array.Construct();
            foreach (var wf in m_workflowCollection
                .OfType<SPWorkflow>()
                .Select(a => new SPWorkflowInstance(this.Engine.Object.InstancePrototype, a)))
            {
                ArrayInstance.Push(this.Engine.Object.InstancePrototype, wf);
            }

            return result;
        }
    }
}
