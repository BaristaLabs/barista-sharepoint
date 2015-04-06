using System;

namespace Barista.NiL.JS.Core.Modules
{
    /// <summary>
    /// Член, помеченный данным аттрибутом, не будет доступен из сценария.
    /// </summary>
#if !PORTABLE
    [Serializable]
#endif
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = false)]
    public sealed class HiddenAttribute : Attribute
    {
    }
}
