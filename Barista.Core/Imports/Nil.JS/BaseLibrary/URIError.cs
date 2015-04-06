using System;
using Barista.NiL.JS.Core;
using Barista.NiL.JS.Core.Modules;

namespace Barista.NiL.JS.BaseLibrary
{
    [Prototype(typeof(Error))]
#if !PORTABLE
    [Serializable]
#endif
    public sealed class URIError : Error
    {
        [DoNotEnumerate]
        public URIError()
        {

        }

        [DoNotEnumerate]
        public URIError(Arguments args)
            : base(args[0].ToString())
        {

        }

        [DoNotEnumerate]
        public URIError(string message)
            : base(message)
        {

        }
    }
}
