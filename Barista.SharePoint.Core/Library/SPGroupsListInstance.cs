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
    public class SPGroupListConstructor : ClrFunction
    {
        public SPGroupListConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPGroupList", new SPGroupListInstance(engine, null))
        {
        }

        [JSConstructorFunction]
        public SPGroupListInstance Construct()
        {
            return new SPGroupListInstance(this.Engine, null);
        }
    }

    [Serializable]
    public class SPGroupListInstance : ListInstance<SPGroupInstance, SPGroup>
    {
        public SPGroupListInstance(ScriptEngine engine, IList<SPGroup> groupList)
            : base(new ListInstance<SPGroupListInstance, SPGroup>(engine))
        {
            this.List = groupList;
            this.PopulateFunctions(this.GetType(),
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
        }

        protected override SPGroupInstance Wrap(SPGroup group)
        {
            return group == null
                ? null
                : new SPGroupInstance(this.Engine.Object.InstancePrototype, group);
        }

        protected override SPGroup Unwrap(SPGroupInstance groupInstance)
        {
            return groupInstance == null
                ? null
                : groupInstance.Group;
        }
    }
}
