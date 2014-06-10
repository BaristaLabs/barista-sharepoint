namespace Barista.SharePoint.Workflow
{
    //Complete 6/10/14

    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using Barista.Library;
    using Microsoft.SharePoint.Workflow;
    using System;
    using System.Globalization;

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

        [JSProperty(Name = "count")]
        public int Count
        {
            get
            {
                return m_workflowTemplateCollection.Count;
            }
        }

        [JSFunction(Name = "getTemplateByBaseId")]
        public SPWorkflowTemplateInstance GetTemplateByBaseId(object baseTemplateId)
        {
            var guidId = GuidInstance.ConvertFromJsObjectToGuid(baseTemplateId);
            var result = m_workflowTemplateCollection.GetTemplateByBaseID(guidId);
            return result == null
                ? null
                : new SPWorkflowTemplateInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "getTemplateById")]
        public SPWorkflowTemplateInstance GetTemplateById(object id)
        {
            var guidId = GuidInstance.ConvertFromJsObjectToGuid(id);
            var result = m_workflowTemplateCollection[guidId];
            return result == null
                ? null
                : new SPWorkflowTemplateInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "getTemplateByIndex")]
        public SPWorkflowTemplateInstance GetTemplateByIndex(int index)
        {
            var result = m_workflowTemplateCollection[index];
            return result == null
                ? null
                : new SPWorkflowTemplateInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "getTemplateByName")]
        public SPWorkflowTemplateInstance GetWorkflowAssociationByName(string name)
        {
            //TODO: Cultureinfo.
            var result = m_workflowTemplateCollection.GetTemplateByName(name, CultureInfo.CurrentCulture);
            return result == null
                ? null
                : new SPWorkflowTemplateInstance(this.Engine.Object.InstancePrototype, result);
        }
    }
}
