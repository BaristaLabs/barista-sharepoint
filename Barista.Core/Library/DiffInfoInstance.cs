namespace Barista.Library
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;

    [Serializable]
    public class DiffInfoConstructor : ClrFunction
    {
        public DiffInfoConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "DiffInfo", new DiffInfoInstance(engine))
        {
        }

        [JSConstructorFunction]
        public DiffInfoInstance Construct()
        {
            return new DiffInfoInstance(this.Engine);
        }
    }

    [Serializable]
    public class DiffInfoInstance : ObjectInstance
    {
        public DiffInfoInstance(ScriptEngine engine)
            : base(engine)
        {
            this.PopulateFunctions();
        }

        protected DiffInfoInstance(ObjectInstance prototype)
            : base(prototype)
        {
        }

        [JSProperty(Name = "url")]
        public string Url
        {
            get;
            set;
        }

        [JSProperty(Name = "hash")]
        public string Hash
        {
            get;
            set;
        }

        [JSProperty(Name = "timeLastModified")]
        public DateInstance TimeLastModified
        {
            get;
            set;
        }
    }
}
