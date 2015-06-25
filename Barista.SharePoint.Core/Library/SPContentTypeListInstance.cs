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
    public class SPContentTypeListConstructor : ClrFunction
    {
        public SPContentTypeListConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPContentTypeList", new SPContentTypeListInstance(engine, null))
        {
        }

        [JSConstructorFunction]
        public SPContentTypeListInstance Construct()
        {
            return new SPContentTypeListInstance(this.Engine, null);
        }
    }

    [Serializable]
    public class SPContentTypeListInstance : ListInstance<SPContentTypeInstance, SPContentType>
    {
        public SPContentTypeListInstance(ScriptEngine engine, IList<SPContentType> contentTypeList)
            : base(new ListInstance<SPContentTypeListInstance, SPContentType>(engine))
        {
            this.List = contentTypeList;
            this.PopulateFunctions(this.GetType(),
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
        }

        protected override SPContentTypeInstance Wrap(SPContentType sPContentType)
        {
            return sPContentType == null
                ? null
                : new SPContentTypeInstance(this.Engine.Object.InstancePrototype, sPContentType);
        }

        protected override SPContentType Unwrap(SPContentTypeInstance sPContentTypeInstance)
        {
            return sPContentTypeInstance == null
                ? null
                : sPContentTypeInstance.ContentType;
        }
    }
}
