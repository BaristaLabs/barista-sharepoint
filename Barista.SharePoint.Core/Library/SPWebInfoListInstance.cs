namespace Barista.SharePoint.Library
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using Barista.Library;
    using Microsoft.SharePoint;
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    [Serializable]
    public class SPWebInfoListConstructor : ClrFunction
    {
        public SPWebInfoListConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPWebInfoList", new SPWebInfoListInstance(engine, null))
        {
        }

        [JSConstructorFunction]
        public SPWebInfoListInstance Construct()
        {
            return new SPWebInfoListInstance(this.Engine, null);
        }
    }

    [Serializable]
    public class SPWebInfoListInstance : ListInstance<SPWebInfoInstance, SPWebInfo>
    {
        public SPWebInfoListInstance(ScriptEngine engine, IList<SPWebInfo> webInfoList)
            : base(new ListInstance<SPWebInfoListInstance, SPWebInfo>(engine))
        {
            this.List = webInfoList;
            this.PopulateFunctions(this.GetType(),
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
        }

        protected override SPWebInfoInstance Wrap(SPWebInfo sPWebInfo)
        {
            return sPWebInfo == null
                ? null
                : new SPWebInfoInstance(this.Engine, sPWebInfo);
        }

        protected override SPWebInfo Unwrap(SPWebInfoInstance sPWebInfoInstance)
        {
            return sPWebInfoInstance == null
                ? null
                : sPWebInfoInstance.SPWebInfo;
        }
    }
}
