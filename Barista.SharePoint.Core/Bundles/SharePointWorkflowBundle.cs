namespace Barista.SharePoint.Bundles
{
    using System;
    using Barista.Jurassic;

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
            //TODO: finish this.
            return Undefined.Value;
        }
    }
}
