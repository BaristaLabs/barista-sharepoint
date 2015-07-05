namespace Barista.TuesPechkin
{
    using System;
    using System.Collections.Generic;

    internal class DelegateRegistry
    {
        private readonly Dictionary<IntPtr, List<Delegate>> m_registry = new Dictionary<IntPtr, List<Delegate>>();

        public void Register(IntPtr converter, Delegate callback)
        {
            List<Delegate> delegates;

            if (!m_registry.TryGetValue(converter, out delegates))
            {
                delegates = new List<Delegate>();
                m_registry.Add(converter, delegates);
            }

            delegates.Add(callback);
        }

        public void Unregister(IntPtr converter)
        {
            m_registry.Remove(converter);
        }
    }
}
