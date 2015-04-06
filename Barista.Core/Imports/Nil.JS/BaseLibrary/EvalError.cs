using System;
using Barista.NiL.JS.Core;
using Barista.NiL.JS.Core.Modules;

namespace Barista.NiL.JS.BaseLibrary
{
#if !PORTABLE
    [Serializable]
#endif
    public sealed class EvalError : Error
    {
        [DoNotEnumerate]
        public EvalError()
        {

        }

        [DoNotEnumerate]
        public EvalError(Arguments args)
            : base(args[0].ToString())
        {

        }

        [DoNotEnumerate]
        public EvalError(string message)
            : base(message)
        {

        }
    }
}
