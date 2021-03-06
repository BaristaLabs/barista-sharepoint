﻿namespace Barista.SharePoint.Workflow
{
    //Complete 6/10/14

    using System.Linq;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using Barista.Library;
    using Barista.SharePoint.Library;
    using Microsoft.SharePoint.Workflow;
    using System;
    using System.Globalization;

    [Serializable]
    public class SPWorkflowAssociationCollectionConstructor : ClrFunction
    {
        public SPWorkflowAssociationCollectionConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPWorkflowAssociationCollection", new SPWorkflowAssociationCollectionInstance(engine.Object.InstancePrototype))
        {
            PopulateFunctions();
        }

        //[JSConstructorFunction]
        public SPWorkflowAssociationCollectionInstance Construct()
        {
            return new SPWorkflowAssociationCollectionInstance(InstancePrototype);
        }

        [JSFunction(Name = "getAssociationForListItemById")]
        public SPWorkflowAssociationInstance GetWorkflowAssociationByName(SPListItemInstance listItem, object id)
        {
            if (listItem == null)
                throw new JavaScriptException(Engine, "Error", "ListItem must be specified as the first argument.");

            var guidId = GuidInstance.ConvertFromJsObjectToGuid(id);
            var result = SPWorkflowAssociationCollection.GetAssociationForListItemById(listItem.ListItem, guidId);
            return result == null
                ? null
                : new SPWorkflowAssociationInstance(Engine.Object.InstancePrototype, result);
        }
    }

    [Serializable]
    public class SPWorkflowAssociationCollectionInstance : ObjectInstance
    {
        private readonly SPWorkflowAssociationCollection m_workflowAssociationCollection;

        public SPWorkflowAssociationCollectionInstance(ObjectInstance prototype)
            : base(prototype)
        {
            PopulateFields();
            PopulateFunctions();
        }

        public SPWorkflowAssociationCollectionInstance(ObjectInstance prototype, SPWorkflowAssociationCollection workflowAssociationCollection)
            : this(prototype)
        {
            if (workflowAssociationCollection == null)
                throw new ArgumentNullException("workflowAssociationCollection");

            m_workflowAssociationCollection = workflowAssociationCollection;
        }

        public SPWorkflowAssociationCollection SPWorkflowAssociationCollection
        {
            get
            {
                return m_workflowAssociationCollection;
            }
        }

        [JSProperty(Name = "count")]
        public int Count
        {
            get
            {
                return m_workflowAssociationCollection.Count;
            }
        }

        [JSFunction(Name = "add")]
        public SPWorkflowAssociationInstance Add(SPWorkflowAssociationInstance workflowAssociation)
        {
            if (workflowAssociation == null)
                throw new JavaScriptException(Engine, "Error", "Workflow Association must be supplied as first argument.");

            var result = m_workflowAssociationCollection.Add(workflowAssociation.SPWorkflowAssociation);
            return result == null
                ? null
                : new SPWorkflowAssociationInstance(Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "getParentList")]
        public SPListInstance GetParentList()
        {
            var result = m_workflowAssociationCollection.ParentList;
            return result == null
                ? null
                : new SPListInstance(Engine, null, null, result);
        }

        [JSFunction(Name = "getParentSite")]
        public SPSiteInstance GetParentSite()
        {
            var result = m_workflowAssociationCollection.ParentSite;
            return result == null
                ? null
                : new SPSiteInstance(Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "getParentWeb")]
        public SPWebInstance GetParentWeb()
        {
            var result = m_workflowAssociationCollection.ParentWeb;
            return result == null
                ? null
                : new SPWebInstance(Engine, result);
        }

        [JSFunction(Name = "getSoapXml")]
        public string GetSoapXml()
        {
            return m_workflowAssociationCollection.SoapXml;
        }

        [JSFunction(Name = "getWorkflowAssociationByBaseId")]
        public SPWorkflowAssociationInstance GetWorkflowAssociationByBaseId(object baseTemplateId)
        {
            var guidId = GuidInstance.ConvertFromJsObjectToGuid(baseTemplateId);
            var result = m_workflowAssociationCollection.GetAssociationByBaseID(guidId);
            return result == null
                ? null
                : new SPWorkflowAssociationInstance(Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "getWorkflowAssociationById")]
        public SPWorkflowAssociationInstance GetWorkflowAssociationById(object id)
        {
            var guidId = GuidInstance.ConvertFromJsObjectToGuid(id);
            var result = m_workflowAssociationCollection[guidId];
            return result == null
                ? null
                : new SPWorkflowAssociationInstance(Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "getWorkflowAssociationByIndex")]
        public SPWorkflowAssociationInstance GetWorkflowAssociationByIndex(int index)
        {
            var result = m_workflowAssociationCollection[index];
            return result == null
                ? null
                : new SPWorkflowAssociationInstance(Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "getWorkflowAssociationByName")]
        public SPWorkflowAssociationInstance GetWorkflowAssociationByName(string name)
        {
            //TODO: Cultureinfo.
            var result = m_workflowAssociationCollection.GetAssociationByName(name, CultureInfo.CurrentCulture);
            return result == null
                ? null
                : new SPWorkflowAssociationInstance(Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "remove")]
        public void Remove(SPWorkflowAssociationInstance workflowAssociation)
        {
            if (workflowAssociation == null)
                throw new JavaScriptException(Engine, "Error", "Workflow Association must be supplied as first argument.");

            m_workflowAssociationCollection.Remove(workflowAssociation.SPWorkflowAssociation);
        }

        [JSFunction(Name = "removeById")]
        public void RemoveById(object associationId)
        {
            var guidAssociationId = GuidInstance.ConvertFromJsObjectToGuid(associationId);
            m_workflowAssociationCollection.Remove(guidAssociationId);
        }

        [JSFunction(Name = "toArray")]
        [JSDoc("ternReturnType", "[+SPWorkflowAssociation]")]
        public ArrayInstance ToArray()
        {
            var result = Engine.Array.Construct();
            foreach (var assoc in m_workflowAssociationCollection
                .OfType<SPWorkflowAssociation>()
                .Select(a => new SPWorkflowAssociationInstance(Engine.Object.InstancePrototype, a)))
            {
                ArrayInstance.Push(result, assoc);
            }

            return result;
        }

        [JSFunction(Name = "update")]
        public void Update(SPWorkflowAssociationInstance workflowAssociation)
        {
            if (workflowAssociation == null)
                throw new JavaScriptException(Engine, "Error", "Workflow Association must be supplied as first argument.");

            m_workflowAssociationCollection.Update(workflowAssociation.SPWorkflowAssociation);
        }

        [JSFunction(Name = "updateAssociationsToLatestVersion")]
        public bool UpdateAssociationsToLatestVersion()
        {
            return m_workflowAssociationCollection.UpdateAssociationsToLatestVersion();
        }
    }
}
