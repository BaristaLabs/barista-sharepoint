namespace Barista.Diagnostics
{
    using Barista.Jurassic;
    using Barista.Jurassic.Library;
    using System;
    using System.Diagnostics;

    [Serializable]
    public class PerformanceCounterConstructor : ClrFunction
    {
        public PerformanceCounterConstructor(ScriptEngine engine)
            : base(engine.Function.InstancePrototype, "PerformanceCounter", new PerformanceCounterInstance(engine))
        {
        }

        [JSConstructorFunction]
        public PerformanceCounterInstance Construct(object categoryName, object counterName, object instanceName, object machineName)
        {
            if (categoryName != Undefined.Value && categoryName != Null.Value &&
                counterName != Undefined.Value && counterName != Null.Value &&
                instanceName != Undefined.Value && instanceName != Null.Value &&
                machineName != Undefined.Value && machineName != Undefined.Value)
                return new PerformanceCounterInstance(this.Engine, new PerformanceCounter(TypeConverter.ToString(categoryName), TypeConverter.ToString(counterName), TypeConverter.ToString(instanceName), TypeConverter.ToString(machineName)));

            if (categoryName != Undefined.Value && categoryName != Null.Value &&
                counterName != Undefined.Value && counterName != Null.Value &&
                instanceName != Undefined.Value && instanceName != Null.Value)
                return new PerformanceCounterInstance(this.Engine, new PerformanceCounter(TypeConverter.ToString(categoryName), TypeConverter.ToString(counterName), TypeConverter.ToString(instanceName)));


            if (categoryName != Undefined.Value && categoryName != Null.Value &&
                counterName != Undefined.Value && counterName != Null.Value)
                return new PerformanceCounterInstance(this.Engine, new PerformanceCounter(TypeConverter.ToString(categoryName), TypeConverter.ToString(counterName)));

            return new PerformanceCounterInstance(this.Engine, new PerformanceCounter());
        }
    }

    [Serializable]
    public class PerformanceCounterInstance : ObjectInstance
    {
        private readonly PerformanceCounter m_performanceCounter;

        internal PerformanceCounterInstance(ScriptEngine engine)
            : base(engine)
        {
            this.PopulateFunctions();
        }

        public PerformanceCounterInstance(ScriptEngine engine, PerformanceCounter performanceCounter)
            : this(engine)
        {
            if (performanceCounter == null)
                throw new JavaScriptException(engine, "Error", "$camelCasedName must be specified.");

            m_performanceCounter = performanceCounter;
        }

        protected PerformanceCounterInstance(ObjectInstance prototype, PerformanceCounter performanceCounter)
            : base(prototype)
        {
            if (performanceCounter == null)
                throw new ArgumentNullException("performanceCounter");

            m_performanceCounter = performanceCounter;
        }

        public PerformanceCounter PerformanceCounter
        {
            get
            {
                return m_performanceCounter;
            }
        }

        [JSProperty(Name = "categoryName")]
        public string CategoryName
        {
            get
            {
                return m_performanceCounter.CategoryName;
            }
        }

        [JSProperty(Name = "counterHelp")]
        public string CounterHelp
        {
            get
            {
                return m_performanceCounter.CounterHelp;
            }
        }

        [JSProperty(Name = "counterName")]
        public string CounterName
        {
            get
            {
                return m_performanceCounter.CounterName;
            }
        }

        [JSProperty(Name = "counterType")]
        public string CounterType
        {
            get
            {
                return m_performanceCounter.CounterType.ToString();
            }
        }

        [JSProperty(Name = "instanceLifetime")]
        public string InstanceLifetime
        {
            get
            {
                return m_performanceCounter.InstanceLifetime.ToString();
            }
        }

        [JSProperty(Name = "instanceName")]
        public string InstanceName
        {
            get
            {
                return m_performanceCounter.InstanceName;
            }
        }

        [JSProperty(Name = "machineName")]
        public string MachineName
        {
            get
            {
                return m_performanceCounter.MachineName;
            }
        }

        [JSProperty(Name = "rawValue")]
        public double RawValue
        {
            get
            {
                return m_performanceCounter.RawValue;
            }
        }

        [JSProperty(Name = "readOnly")]
        public bool ReadOnly
        {
            get
            {
                return m_performanceCounter.ReadOnly;
            }
        }

        [JSFunction(Name = "nextValue")]
        public double NextValue()
        {
            return m_performanceCounter.NextValue();
        }
    }
}