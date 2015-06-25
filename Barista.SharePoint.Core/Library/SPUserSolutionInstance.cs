namespace Barista.SharePoint.Library
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using Barista.Library;
    using Microsoft.SharePoint;

    [Serializable]
    public class SPUserSolutionConstructor : ClrFunction
    {
        public SPUserSolutionConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "SPUserSolution", new SPUserSolutionInstance(engine.Object.InstancePrototype))
        {
        }

        [JSConstructorFunction]
        public SPUserSolutionInstance Construct()
        {
            return new SPUserSolutionInstance(this.InstancePrototype);
        }
    }

    [Serializable]
    public class SPUserSolutionInstance : ObjectInstance
    {
        private readonly SPUserSolution m_userSolution;

        public SPUserSolutionInstance(ObjectInstance prototype)
            : base(prototype)
        {
            this.PopulateFields();
            this.PopulateFunctions();
        }

        public SPUserSolutionInstance(ObjectInstance prototype, SPUserSolution userSolution)
            : this(prototype)
        {
            if (userSolution == null)
                throw new ArgumentNullException("userSolution");

            m_userSolution = userSolution;
        }

        public SPUserSolution SPUserSolution
        {
            get
            {
                return m_userSolution;
            }
        }

        [JSProperty(Name = "name")]
        public string Name
        {
            get
            {
                return m_userSolution.Name;
            }
        }

        [JSProperty(Name = "hasAssemblies")]
        public bool HasAssemblies
        {
            get
            {
                return m_userSolution.HasAssemblies;
            }
        }

        [JSProperty(Name = "signature")]
        public string Signature
        {
            get
            {
                return m_userSolution.Signature;
            }
        }

        [JSProperty(Name = "solutionId")]
        public GuidInstance SolutionId
        {
            get
            {
                return new GuidInstance(this.Engine.Object.InstancePrototype, m_userSolution.SolutionId);
            }
        }

        [JSProperty(Name = "status")]
        public string Status
        {
            get
            {
                return m_userSolution.Status.ToString();
            }
        }

        [JSFunction(Name = "dispose")]
        public void Dispose()
        {
            m_userSolution.Dispose();
        }
    }
}
