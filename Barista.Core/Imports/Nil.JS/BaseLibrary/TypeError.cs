﻿using System;
using Barista.NiL.JS.Core;
using Barista.NiL.JS.Core.Modules;

namespace Barista.NiL.JS.BaseLibrary
{
    [Prototype(typeof(Error))]
#if !PORTABLE
    [Serializable]
#endif
    public sealed class TypeError : Error
    {
        [DoNotEnumerate]
        public TypeError(Arguments args)
            : base(args[0].ToString())
        {

        }

        [DoNotEnumerate]
        public TypeError()
        {

        }

        [DoNotEnumerate]
        public TypeError(string message)
            : base(message)
        {
        }
    }
}
