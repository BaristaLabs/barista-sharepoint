namespace Barista.Smtp
{
    using System.Net.Mail;
    using System.Reflection;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Barista.Library;

    [Serializable]
    public class AlternateViewCollectionConstructor : ClrFunction
    {
        public AlternateViewCollectionConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "AlternateViewCollection", new AlternateViewCollectionInstance(engine, null))
        {
        }

        [JSConstructorFunction]
        public AlternateViewCollectionInstance Construct()
        {
            return new AlternateViewCollectionInstance(this.Engine, null);
        }
    }

    [Serializable]
    public class AlternateViewCollectionInstance : CollectionInstance<AlternateViewCollection, AlternateViewInstance, AlternateView>
    {
        private readonly AlternateViewCollection m_alternateViewCollection;

        public AlternateViewCollectionInstance(ScriptEngine engine, AlternateViewCollection alternateViewCollection)
            : base(new CollectionInstance<AlternateViewCollection, AlternateViewCollectionInstance, AlternateView>(engine))
        {
            this.m_alternateViewCollection = alternateViewCollection;
            Collection = alternateViewCollection;

            this.PopulateFunctions(this.GetType(),
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
        }

        protected override AlternateViewInstance Wrap(AlternateView alternateView)
        {
            return alternateView == null
                ? null
                : new AlternateViewInstance(this.Engine.Object.InstancePrototype, alternateView);
        }

        protected override AlternateView Unwrap(AlternateViewInstance alternateViewInstance)
        {
            return alternateViewInstance == null
                ? null
                : alternateViewInstance.AlternateView;
        }

        [JSFunction(Name = "dispose")]
        public void Dispose()
        {
            m_alternateViewCollection.Dispose();
        }
    }
}
