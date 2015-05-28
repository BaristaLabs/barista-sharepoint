namespace Barista.SharePoint.Library
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Microsoft.SharePoint;

    [Serializable]
    public class SPResourceInstance : ObjectInstance
    {
        public SPResourceInstance(ScriptEngine engine)
            : base(engine)
        {
            this.PopulateFunctions();
        }

        //TODO: CultureInfo overload

        [JSFunction(Name = "getString")]
        public string GetString(string name, params object[] values)
        {
            return SPResource.GetString(name, values);
        }
            
    }
}
