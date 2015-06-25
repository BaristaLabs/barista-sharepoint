namespace Barista.Diagnostics
{
    using System.Diagnostics;
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;

    [Serializable]
    public class PerformanceCounterCategoryConstructor : ClrFunction
    {
        public PerformanceCounterCategoryConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "PerformanceCounterCategory", new PerformanceCounterCategoryInstance(engine))
        {
            this.PopulateFunctions();
        }

        [JSConstructorFunction]
        public PerformanceCounterCategoryInstance Construct(object categoryName, object machineName)
        {
            if (categoryName != Undefined.Value && categoryName != Null.Value && machineName != Undefined.Value && machineName != Null.Value)
                return new PerformanceCounterCategoryInstance(this.Engine, new PerformanceCounterCategory(TypeConverter.ToString(categoryName), TypeConverter.ToString(machineName)));
            
            if (categoryName != Undefined.Value && categoryName != Null.Value)
                return new PerformanceCounterCategoryInstance(this.Engine, new PerformanceCounterCategory(TypeConverter.ToString(categoryName)));
            
            return new PerformanceCounterCategoryInstance(this.Engine, new PerformanceCounterCategory());
        }

        [JSFunction(Name = "counterExists")]
        public bool CounterExists(string counterName, string categoryName, object machineName)
        {
            string strMachineName = null;
            if (machineName != Undefined.Value && machineName != Null.Value)
                strMachineName = TypeConverter.ToString(machineName);

            return String.IsNullOrEmpty(strMachineName)
                ? PerformanceCounterCategory.CounterExists(counterName, categoryName)
                : PerformanceCounterCategory.CounterExists(counterName, categoryName, strMachineName);
        }

        //Create, Delete

        [JSFunction(Name = "exists")]
        public bool Exists(string categoryName, object machineName)
        {
            string strMachineName = null;
            if (machineName != Undefined.Value && machineName != Null.Value)
                strMachineName = TypeConverter.ToString(machineName);

            return String.IsNullOrEmpty(strMachineName)
                ? PerformanceCounterCategory.Exists(categoryName)
                : PerformanceCounterCategory.Exists(categoryName, strMachineName);
        }

        [JSFunction(Name = "getCategories")]
        public ArrayInstance GetCategories(object machineName)
        {

            string strMachineName = null;
            if (machineName != Undefined.Value && machineName != Null.Value)
                strMachineName = TypeConverter.ToString(machineName);

            var categories = String.IsNullOrEmpty(strMachineName)
                ? PerformanceCounterCategory.GetCategories()
                : PerformanceCounterCategory.GetCategories(strMachineName);

            var result = this.Engine.Array.Construct();
            foreach (var category in categories)
                ArrayInstance.Push(result, new PerformanceCounterCategoryInstance(this.Engine, category));

            return result;
        }

        [JSFunction(Name = "instanceExists")]
        public bool InstanceExists(string instanceName, string categoryName, object machineName)
        {
            string strMachineName = null;
            if (machineName != Undefined.Value && machineName != Null.Value)
                strMachineName = TypeConverter.ToString(machineName);

            return String.IsNullOrEmpty(strMachineName)
                ? PerformanceCounterCategory.InstanceExists(instanceName, categoryName)
                : PerformanceCounterCategory.InstanceExists(instanceName, categoryName, strMachineName);
        }
    }

    [Serializable]
    public class PerformanceCounterCategoryInstance : ObjectInstance
    {
        private readonly PerformanceCounterCategory m_performanceCounterCategory;

        internal PerformanceCounterCategoryInstance(ScriptEngine engine)
            : base(engine)
        {
            this.PopulateFunctions();
        }

        public PerformanceCounterCategoryInstance(ScriptEngine engine, PerformanceCounterCategory performanceCounterCategory)
            : this(engine)
        {
            if (performanceCounterCategory == null)
                throw new JavaScriptException(engine, "Error", "$camelCasedName must be specified.");

            m_performanceCounterCategory = performanceCounterCategory;
        }

        protected PerformanceCounterCategoryInstance(ObjectInstance prototype, PerformanceCounterCategory performanceCounterCategory)
            : base(prototype)
        {
            if (performanceCounterCategory == null)
                throw new ArgumentNullException("performanceCounterCategory");

            m_performanceCounterCategory = performanceCounterCategory;
        }

        public PerformanceCounterCategory PerformanceCounterCategory
        {
            get
            {
                return m_performanceCounterCategory;
            }
        }

        [JSProperty(Name = "categoryHelp")]
        public string CategoryHelp
        {
            get
            {
                return m_performanceCounterCategory.CategoryHelp;
            }
        }

        [JSProperty(Name = "categoryName")]
        public string CategoryName
        {
            get
            {
                return m_performanceCounterCategory.CategoryName;
            }
        }

        [JSProperty(Name = "categoryType")]
        public string CategoryType
        {
            get
            {
                return m_performanceCounterCategory.CategoryType.ToString();
            }
        }

        [JSProperty(Name = "machineName")]
        public string MachineName
        {
            get
            {
                return m_performanceCounterCategory.MachineName;
            }
        }

        [JSFunction(Name = "counterExists")]
        public bool CounterExists(string counterName)
        {
            return m_performanceCounterCategory.CounterExists(counterName);
        }

        [JSFunction(Name = "getCounters")]
        public ArrayInstance GetCounters(object instanceName)
        {
            string strInstanceName = null;

            if (instanceName != Undefined.Value && instanceName != Null.Value)
                strInstanceName = TypeConverter.ToString(instanceName);

            var counters = String.IsNullOrEmpty(strInstanceName)
                ? m_performanceCounterCategory.GetCounters()
                : m_performanceCounterCategory.GetCounters(strInstanceName);


            var result = this.Engine.Array.Construct();
            foreach (var counter in counters)
                ArrayInstance.Push(result, new PerformanceCounterInstance(this.Engine, counter));

            return result;
        }

        [JSFunction(Name = "getInstanceNames")]
        public ArrayInstance GetInstanceNames()
        {
            var instanceNames = m_performanceCounterCategory.GetInstanceNames();
            var result = this.Engine.Array.Construct();
            foreach (var name in instanceNames)
                ArrayInstance.Push(result, name);

            return result;
        }

        [JSFunction(Name = "instanceExists")]
        public bool InstanceExists(string instanceName)
        {
            return m_performanceCounterCategory.InstanceExists(instanceName);
        }
       
    }
}
