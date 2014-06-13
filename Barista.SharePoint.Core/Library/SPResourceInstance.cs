using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Barista.SharePoint.Library
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Microsoft.SharePoint;

    [Serializable]
    public class SPResourceConstructor : ClrFunction
    {
        public SPResourceConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPResource", engine.Object.InstancePrototype)
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
