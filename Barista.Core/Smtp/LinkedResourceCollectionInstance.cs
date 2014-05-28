namespace Barista.Smtp
{
    using System.Net.Mail;
    using System.Reflection;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Barista.Library;

    [Serializable]
    public class LinkedResourceCollectionConstructor : ClrFunction
    {
        public LinkedResourceCollectionConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "LinkedResourceCollection", new LinkedResourceCollectionInstance(engine, null))
        {
        }

        [JSConstructorFunction]
        public LinkedResourceCollectionInstance Construct()
        {
            return new LinkedResourceCollectionInstance(this.Engine, null);
        }
    }

    [Serializable]
    public class LinkedResourceCollectionInstance : CollectionInstance<LinkedResourceCollection, LinkedResourceInstance, LinkedResource>
    {
        private readonly LinkedResourceCollection m_linkedResourceCollection;

        public LinkedResourceCollectionInstance(ScriptEngine engine, LinkedResourceCollection linkedResourceCollection)
            : base(new CollectionInstance<LinkedResourceCollection, LinkedResourceCollectionInstance, LinkedResource>(engine))
        {
            this.m_linkedResourceCollection = linkedResourceCollection;
            Collection = linkedResourceCollection;

            this.PopulateFunctions(this.GetType(),
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
        }

        protected override LinkedResourceInstance Wrap(LinkedResource linkedResource)
        {
            return linkedResource == null
                ? null
                : new LinkedResourceInstance(this.Engine, linkedResource);
        }

        protected override LinkedResource Unwrap(LinkedResourceInstance linkedResourceInstance)
        {
            return linkedResourceInstance == null
                ? null
                : linkedResourceInstance.LinkedResource;
        }

        [JSFunction(Name = "dispose")]
        public void Dispose()
        {
            m_linkedResourceCollection.Dispose();
        }
    }
}
