namespace Barista.Library
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    [Serializable]
    public class DiffResultConstructor : ClrFunction
    {
        public DiffResultConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "DiffResult", new DiffResultInstance(engine))
        {
        }

        [JSConstructorFunction]
        public DiffResultInstance Construct()
        {
            return new DiffResultInstance(this.Engine);
        }
    }

    [Serializable]
    public class DiffResultInstance : ObjectInstance
    {
        public DiffResultInstance(ScriptEngine engine)
            : base(engine)
        {
            this.Identical = this.Engine.Array.Construct();
            this.Changed = this.Engine.Array.Construct();
            this.Added = this.Engine.Array.Construct();
            this.Removed = this.Engine.Array.Construct();

            this.PopulateFunctions();
        }

        protected DiffResultInstance(ObjectInstance prototype)
            : base(prototype)
        {
        }

        [JSProperty(Name = "identical")]
        public ArrayInstance Identical
        {
            get;
            set;
        }

        [JSProperty(Name = "changed")]
        public ArrayInstance Changed
        {
            get;
            set;
        }

        [JSProperty(Name = "added")]
        public ArrayInstance Added
        {
            get;
            set;
        }

        [JSProperty(Name = "removed")]
        public ArrayInstance Removed
        {
            get;
            set;
        }

        public void Process(IList<DiffInfoInstance> source, IList<DiffInfoInstance> target)
        {
            foreach(var sourceDiffInfo in source)
            {
                var info = sourceDiffInfo;
                var targetDiffInfo = target.FirstOrDefault(t => String.Equals(t.Url, info.Url, StringComparison.InvariantCultureIgnoreCase));
                if (targetDiffInfo == null)
                {
                    ArrayInstance.Push(this.Removed, sourceDiffInfo);
                    continue;
                }

                ArrayInstance.Push(
                    String.Equals(sourceDiffInfo.Hash, targetDiffInfo.Hash)
                        ? this.Identical
                        : this.Changed,
                    targetDiffInfo);
            }

            foreach(var targetDiffInfo in target)
            {
                var info = targetDiffInfo;
                var sourceDiffInfo = source.FirstOrDefault(t => String.Equals(t.Url, info.Url, StringComparison.InvariantCultureIgnoreCase));

                if (sourceDiffInfo == null)
                {
                    ArrayInstance.Push(this.Added, targetDiffInfo);
                }
            }
        }
    }
}
