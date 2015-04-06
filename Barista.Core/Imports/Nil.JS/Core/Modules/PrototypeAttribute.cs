﻿using System;

namespace Barista.NiL.JS.Core.Modules
{
    /// <summary>
    /// Указывает, какой тип необходимо представить в цепочке прототипов объекта-прослойки для помеченного типа.
    /// </summary>
#if !PORTABLE
    [Serializable]
#endif
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public sealed class PrototypeAttribute : Attribute
    {
        public Type PrototypeType { get; private set; }

        public PrototypeAttribute(Type prototypeType)
        {
            PrototypeType = prototypeType;
        }
    }
}
