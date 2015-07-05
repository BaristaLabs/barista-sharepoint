namespace Barista.Jurassic
{
    using System;
    using Barista.Jurassic.Compiler;

    /// <summary>
    /// Represents the result of compiling a script.
    /// </summary>
    public sealed class CompiledScript
    {
        private readonly GlobalMethodGenerator m_methodGen;

        internal CompiledScript(GlobalMethodGenerator methodGen)
        {
            if (methodGen == null)
                throw new ArgumentNullException("methodGen");
            m_methodGen = methodGen;
        }

        /// <summary>
        /// Executes the compiled script.
        /// </summary>
        public void Execute()
        {
            m_methodGen.Execute();
        }
    }
}
