using System;

namespace Barista.NiL.JS.Core.Modules
{
    /// <summary>
    /// Член, помеченный данным аттрибутом, не будет удаляться оператором "delete".
    /// </summary>
#if !PORTABLE
    [Serializable]
#endif
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class DoNotDeleteAttribute : Attribute
    {
    }
}
