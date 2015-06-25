namespace Barista.Logging.LogProviders
{
  using System;
  using System.Linq;
  using System.Reflection;
  using Barista.Helpers;

  public class Log4NetLogManager : LogManagerBase
	{
		private static bool s_providerIsAvailableOverride = true;
		private static readonly Lazy<Type> LazyGetLogManagerType = new Lazy<Type>(GetLogManagerTypeStatic, true);

		public Log4NetLogManager()
			: base(logger => new Log4NetLogger(logger))
		{
			if (!IsLoggerAvailable())
			{
				throw new InvalidOperationException("log4net.LogManager not found");
			}
		}

		public static bool ProviderIsAvailableOverride
		{
			get { return s_providerIsAvailableOverride; }
			set { s_providerIsAvailableOverride = value; }
		}

		public static bool IsLoggerAvailable()
		{
			return ProviderIsAvailableOverride && LazyGetLogManagerType.Value != null;
		}

		protected override Type GetLogManagerType()
		{
			return GetLogManagerTypeStatic();
		}

		protected static Type GetLogManagerTypeStatic()
		{
			var log4NetAssembly = GetLog4NetAssembly();
			return log4NetAssembly != null
				       ? log4NetAssembly.GetType("log4net.LogManager")
				       : Type.GetType("log4net.LogManager, log4net");
		}

		protected override Type GetNdcType()
		{
			var log4NetAssembly = GetLog4NetAssembly();
			return log4NetAssembly != null ? log4NetAssembly.GetType("log4net.NDC") : Type.GetType("log4net.NDC, log4net");
		}

		protected override Type GetMdcType()
		{
			var log4NetAssembly = GetLog4NetAssembly();
			return log4NetAssembly != null ? log4NetAssembly.GetType("log4net.MDC") : Type.GetType("log4net.MDC, log4net");
		}

		private static Assembly GetLog4NetAssembly()
		{
			return AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(assembly => assembly.FullName.StartsWith("log4net,"));
		}

		public class Log4NetLogger : ILog
		{
			private readonly object m_logger;

			internal Log4NetLogger(object logger)
			{
				this.m_logger = logger;
			}

			public bool IsDebugEnabled
			{
        get { return DynamicHelper.GetPropertyValue<bool>(m_logger, "IsDebugEnabled"); }
			}

			public bool IsWarnEnabled
			{
        get { return DynamicHelper.GetPropertyValue<bool>(m_logger, "IsWarnEnabled"); }
			}

			public void Log(LogLevel logLevel, Func<string> messageFunc)
			{
				switch (logLevel)
				{
					case LogLevel.Information:
						if (DynamicHelper.GetPropertyValue<bool>(m_logger, "IsInfoEnabled"))
						{
							DynamicHelper.InvokeMethod(m_logger, "Info", messageFunc());
						}
						break;
					case LogLevel.Warning:
						if (DynamicHelper.GetPropertyValue<bool>(m_logger, "IsWarnEnabled"))
						{
							DynamicHelper.InvokeMethod(m_logger, "Warn", messageFunc());
						}
						break;
					case LogLevel.Error:
						if (DynamicHelper.GetPropertyValue<bool>(m_logger, "IsErrorEnabled"))
						{
							DynamicHelper.InvokeMethod(m_logger, "Error", messageFunc());
						}
						break;
					case LogLevel.Fatal:
						if (DynamicHelper.GetPropertyValue<bool>(m_logger, "IsFatalEnabled"))
						{
							DynamicHelper.InvokeMethod(m_logger, "Fatal", messageFunc());
						}
						break;
					default:
						if (DynamicHelper.GetPropertyValue<bool>(m_logger, "IsDebugEnabled"))
						{
							DynamicHelper.InvokeMethod(m_logger, "Debug", messageFunc());
								// Log4Net doesn't have a 'Trace' level, so all Trace messages are written as 'Debug'
						}
						break;
				}
			}

			public void Log<TException>(LogLevel logLevel, Func<string> messageFunc, TException exception)
				where TException : Exception
			{
				switch (logLevel)
				{
					case LogLevel.Information:
						if (DynamicHelper.GetPropertyValue<bool>(m_logger, "IsDebugEnabled"))
						{
							DynamicHelper.InvokeMethod(m_logger, "Info", messageFunc(), exception);
						}
						break;
					case LogLevel.Warning:
						if (DynamicHelper.GetPropertyValue<bool>(m_logger, "IsWarnEnabled"))
						{
							DynamicHelper.InvokeMethod(m_logger, "Warn", messageFunc(), exception);
						}
						break;
					case LogLevel.Error:
						if (DynamicHelper.GetPropertyValue<bool>(m_logger, "IsErrorEnabled"))
						{
							DynamicHelper.InvokeMethod(m_logger, "Error", messageFunc(), exception);
						}
						break;
					case LogLevel.Fatal:
						if (DynamicHelper.GetPropertyValue<bool>(m_logger, "IsFatalEnabled"))
						{
							DynamicHelper.InvokeMethod(m_logger, "Fatal", messageFunc(), exception);
						}
						break;
					default:
						if (DynamicHelper.GetPropertyValue<bool>(m_logger, "IsDebugEnabled"))
						{
							DynamicHelper.InvokeMethod(m_logger, "Debug", messageFunc(), exception);
						}
						break;
				}
			}
		}
	}
}