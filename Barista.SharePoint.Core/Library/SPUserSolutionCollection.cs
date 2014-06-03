namespace Barista.SharePoint.Library
{
    using System.Linq;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Barista.Library;
    using Microsoft.SharePoint;

    [Serializable]
    public class SPUserSolutionCollectionConstructor : ClrFunction
    {
        public SPUserSolutionCollectionConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPUserSolutionCollection", new SPUserSolutionCollectionInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public SPUserSolutionCollectionInstance Construct()
        {
            return new SPUserSolutionCollectionInstance(this.InstancePrototype);
        }
    }

    [Serializable]
    public class SPUserSolutionCollectionInstance : ObjectInstance
    {
        private readonly SPUserSolutionCollection m_userSolutionCollection;

        public SPUserSolutionCollectionInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public SPUserSolutionCollectionInstance(ObjectInstance prototype, SPUserSolutionCollection userSolutionCollection)
            : this(prototype)
        {
            if (userSolutionCollection == null)
                throw new ArgumentNullException("userSolutionCollection");

            m_userSolutionCollection = userSolutionCollection;
        }

        public SPUserSolutionCollection SPUserSolutionCollection
        {
            get
            {
                return m_userSolutionCollection;
            }
        }

        [JSProperty(Name = "count")]
        public int Count
        {
            get
            {
                return m_userSolutionCollection.Count;
            }
        }

        [JSFunction(Name = "add")]
        public SPUserSolutionInstance Add(int solutionGalleryItemId)
        {
            var result = m_userSolutionCollection.Add(solutionGalleryItemId);
            return result == null
                ? null
                : new SPUserSolutionInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "remove")]
        public void Remove(SPUserSolutionInstance solution)
        {
            if (solution == null)
                throw new JavaScriptException(this.Engine, "Error", "Solution must be specified.");

            m_userSolutionCollection.Remove(solution.SPUserSolution);
        }

        [JSFunction(Name = "getBySolutionId")]
        public SPUserSolutionInstance GetBySolutionId(object solutionId)
        {
            var guidSolutionId = GuidInstance.ConvertFromJsObjectToGuid(solutionId);
            var result = m_userSolutionCollection[guidSolutionId];
            return result == null
                ? null
                : new SPUserSolutionInstance(this.Engine.Object.InstancePrototype, result);
        }

        [JSFunction(Name = "toArray")]
        public ArrayInstance ToArray()
        {
            var result = this.Engine.Array.Construct();
            foreach (var sol in m_userSolutionCollection
                .OfType<SPUserSolution>()
                .Select(def => new SPUserSolutionInstance(this.Engine.Object.InstancePrototype, def)))
            {
                ArrayInstance.Push(this.Engine.Object.InstancePrototype, sol);
            }

            return result;
        }
    }
}
