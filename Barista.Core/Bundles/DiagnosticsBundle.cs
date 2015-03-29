namespace Barista.Bundles
{
    using Barista.Diagnostics;
    using Barista.Jurassic;
    using System;

    [Serializable]
    public class DiagnosticsBundle : IBundle
    {
        public bool IsSystemBundle
        {
            get
            {
                return true;
            }
        }

        public string BundleName
        {
            get
            {
                return "Diagnostics";
            }
        }

        public string BundleDescription
        {
            get
            {
                return
                    "Diagnostics Bundle. Provides a mechanism to interact with system performance counters and other diagnostic tools";
            }
        }

        public object InstallBundle(Jurassic.ScriptEngine engine)
        {
            engine.SetGlobalValue("PerformanceCounter", new PerformanceCounterConstructor(engine));
            engine.SetGlobalValue("PerformanceCounterCategory", new PerformanceCounterCategoryConstructor(engine));

            return Undefined.Value;
        }
    }
}
