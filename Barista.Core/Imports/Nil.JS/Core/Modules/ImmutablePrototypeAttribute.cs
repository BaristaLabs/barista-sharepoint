﻿using System;

namespace Barista.NiL.JS.Core.Modules
{
    /// <summary>
    /// Объект-прослойка, созданный для типа, помеченного данным аттрибутом, 
    /// не будет допускать создание полей, которые не существуют в помеченном типе.
    /// </summary>
#if !PORTABLE
    [Serializable]
#endif
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public sealed class ImmutablePrototypeAttribute : Attribute
    {
    }
}
