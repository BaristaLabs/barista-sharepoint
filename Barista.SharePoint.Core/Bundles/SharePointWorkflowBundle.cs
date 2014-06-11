namespace Barista.SharePoint.Bundles
{
    using System;
    using Barista.Jurassic;
    using Barista.SharePoint.Workflow;

    [Serializable]
    public class SharePointWorkflowBundle : IBundle
    {
        public bool IsSystemBundle
        {
            get { return true; }
        }

        public string BundleName
        {
            get { return "SharePoint Workflow"; }
        }

        public string BundleDescription
        {
            get { return "SharePoint Workflow Bundle. Provides top-level objects to interact with SharePoint Workflows and Workflow Tasks."; }
        }

        public object InstallBundle(Jurassic.ScriptEngine engine)
        {
            engine.SetGlobalValue("SPWorkflowManager", new SPWorkflowManagerConstructor(engine));

            engine.SetGlobalValue("SPWorkflow", new SPWorkflowConstructor(engine));
            engine.SetGlobalValue("SPWorkflowCollection", new SPWorkflowCollectionConstructor(engine));
            engine.SetGlobalValue("SPWorkflowFilter", new SPWorkflowFilterConstructor(engine));
            engine.SetGlobalValue("SPWorkflowAssociation", new SPWorkflowAssociationConstructor(engine));
            engine.SetGlobalValue("SPWorkflowAssociationCollection", new SPWorkflowAssociationCollectionConstructor(engine));

            engine.SetGlobalValue("SPWorkflowTask", new SPWorkflowTaskConstructor(engine));
            engine.SetGlobalValue("SPWorkflowTaskCollection", new SPWorkflowTaskCollectionConstructor(engine));
            engine.SetGlobalValue("SPWorkflowTemplate", new SPWorkflowTemplateConstructor(engine));

            //TODO: finish this.
            return Undefined.Value;
        }
    }
}
