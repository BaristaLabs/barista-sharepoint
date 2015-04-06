using System;

namespace Barista.NiL.JS.Core.Modules
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Event, AllowMultiple = false)]
    public sealed class NotConfigurable : Attribute
    {
    }
}
