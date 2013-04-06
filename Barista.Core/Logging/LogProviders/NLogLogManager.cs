namespace Barista.Logging.LogProviders
{
  using Barista.Helpers;
  using System;
  using System.Linq;
  using System.Reflection;

  public class NLogLogManager : LogManagerBase
	{
		private static bool s_providerIsAvailableOverride = true;
		private static readonly Lazy<Type> LazyGetLogManagerType = new Lazy<Type>(GetLogManagerTypeStatic, true);

		public NLogLogManager()
			: base(logger => new NLogLogger(logger))
		{
			if (!IsLoggerAvailable())
			{
				throw new InvalidOperationException("NLog.LogManager not found");
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

		private static Type GetLogManagerTypeStatic()
		{
			var nlogAssembly = GetNLogAssembly();
			return nlogAssembly != null ? nlogAssembly.GetType("NLog.LogManager") : Type.GetType("NLog.LogManager, nlog");
		}

		protected override Type GetNdcType()
		{
			var nlogAssembly = GetNLogAssembly();
			return nlogAssembly != null
				       ? nlogAssembly.GetType("NLog.NestedDiagnosticsContext")
					   : Type.GetType("NLog.NestedDiagnosticsContext, nlog");
		}

		private static Assembly GetNLogAssembly()
		{
			return AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(assembly => assembly.FullName.StartsWith("NLog,"));
		}

		protected override Type GetMdcType()
		{
			var log4NetAssembly = GetNLogAssembly();
			return log4NetAssembly != null ? log4NetAssembly.GetType("NLog.MappedDiagnosticsContext") : Type.GetType("NLog.MappedDiagnosticsContext, nlog");
		}

		public class NLogLogger : ILog
		{
			private readonly object m_logger;

			internal NLogLogger(object logger)
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
					case LogLevel.Debug:
						if (DynamicHelper.GetPropertyValue<bool>(m_logger, "IsDebugEnabled"))
						{
							DynamicHelper.InvokeMethod(m_logger, "Debug", messageFunc());
						}
						break;
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
						if (DynamicHelper.GetPropertyValue<bool>(m_logger, "IsTraceEnabled"))
						{
							DynamicHelper.InvokeMethod(m_logger, "Trace", messageFunc());
						}
						break;
				}
			}

			public void Log<TException>(LogLevel logLevel, Func<string> messageFunc, TException exception)
				where TException : Exception
			{
				switch (logLevel)
				{
					case LogLevel.Debug:
						if (DynamicHelper.GetPropertyValue<bool>(m_logger, "IsDebugEnabled"))
						{
							DynamicHelper.InvokeMethod(m_logger, "DebugException", messageFunc(), exception);
						}
						break;
					case LogLevel.Information:
						if (DynamicHelper.GetPropertyValue<bool>(m_logger, "IsInfoEnabled"))
						{
							DynamicHelper.InvokeMethod(m_logger, "InfoException", messageFunc(), exception);
						}
						break;
					case LogLevel.Warning:
						if (DynamicHelper.GetPropertyValue<bool>(m_logger, "IsWarnEnabled"))
						{
							DynamicHelper.InvokeMethod(m_logger, "WarnException", messageFunc(), exception);
						}
						break;
					case LogLevel.Error:
						if (DynamicHelper.GetPropertyValue<bool>(m_logger, "IsErrorEnabled"))
						{
							DynamicHelper.InvokeMethod(m_logger, "ErrorException", messageFunc(), exception);
						}
						break;
					case LogLevel.Fatal:
						if (DynamicHelper.GetPropertyValue<bool>(m_logger, "IsFatalEnabled"))
						{
							DynamicHelper.InvokeMethod(m_logger, "FatalException", messageFunc(), exception);
						}
						break;
					default:
						if (DynamicHelper.GetPropertyValue<bool>(m_logger, "IsTraceEnabled"))
						{
							DynamicHelper.InvokeMethod(m_logger, "TraceException", messageFunc(), exception);
						}
						break;
				}
			}
		}
	}
}