﻿using System;

namespace Barista.NiL.JS.Core.Modules
{
    /// <summary>
    /// Внимание! Будте осторожны при использовании данного аттрибута!
    /// 
    /// Указывает на то, что нестатический метод, 
    /// помеченный данным аттрибутом, способен корректно выполнится, 
    /// будучи вызванным с параметром this указанного типа или производного от него,
    /// включая те случаи, когда указанный тип и тип, объявивший помеченный метод,
    /// не находятся в одной иерархии наследования.
    /// </summary>
#if !PORTABLE
    [Serializable]
#endif
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
#if PORTABLE
    internal
#else
    public 
#endif
        class AllowUnsafeCallAttribute : Attribute
    {
        internal readonly Type baseType;

        /// <summary>
        /// Альтернативный тип для параметра this.
        /// </summary>
        public Type BaseType { get { return baseType; } }

        /// <summary>
        /// Создаёт экземпляр с указанием типа
        /// </summary>
        /// <param name="type">Тип, который следует включит в список допустимых для параметра this.</param>
        public AllowUnsafeCallAttribute(Type type)
        {
            baseType = type;
        }
    }
}
