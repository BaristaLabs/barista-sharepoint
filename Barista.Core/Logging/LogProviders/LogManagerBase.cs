namespace Barista.Logging.LogProviders
{
  using Barista.Extensions;
  using System;
  using System.Linq.Expressions;
  using System.Reflection;

	public abstract class LogManagerBase : ILogManager
	{
		private readonly Func<object, ILog> m_loggerFactory;
		private readonly Func<string, object> m_getLoggerByNameDelegate;
		private readonly Action<string> m_mdcRemoveMethodCall;
		private readonly Action<string, string> m_mdcSetMethodCall;
		private readonly Func<string, IDisposable> m_ndcPushMethodCall;

		protected LogManagerBase(Func<object, ILog> loggerFactory)
		{
			this.m_loggerFactory = loggerFactory;
			m_getLoggerByNameDelegate = GetGetLoggerMethodCall();
			m_ndcPushMethodCall = GetNdcPushMethodCall();
			m_mdcSetMethodCall = GetMdcSetMethodCall();
			m_mdcRemoveMethodCall = GetMdcRemoveMethodCall();
		}

		public Func<string, object> GetLoggerByNameDelegate
		{
			get { return m_getLoggerByNameDelegate; }
		}
		public Func<string, IDisposable> NdcPushMethodCall
		{
			get { return m_ndcPushMethodCall; }
		}
		public Action<string, string> MdcSetMethodCall
		{
			get { return m_mdcSetMethodCall; }
		}
		public Action<string> MdcRemoveMethodCall
		{
			get { return m_mdcRemoveMethodCall; }
		}

		protected abstract Type GetLogManagerType();

		protected abstract Type GetNdcType();

		protected abstract Type GetMdcType();

		public ILog GetLogger(string name)
		{
			return m_loggerFactory(m_getLoggerByNameDelegate(name));
		}

		public IDisposable OpenNestedConext(string message)
		{
			return m_ndcPushMethodCall(message);
		}

		public IDisposable OpenMappedContext(string key, string value)
		{
			m_mdcSetMethodCall(key, value);
			return new DisposableAction(() => m_mdcRemoveMethodCall(key));
		}

		private Func<string, object> GetGetLoggerMethodCall()
		{
			Type logManagerType = GetLogManagerType();
			MethodInfo method = logManagerType.GetMethod("GetLogger", new[] {typeof (string)});
			ParameterExpression resultValue;
			ParameterExpression keyParam = Expression.Parameter(typeof (string), "key");
			MethodCallExpression methodCall = Expression.Call(null, method, new Expression[] {resultValue = keyParam});
			return Expression.Lambda<Func<string, object>>(methodCall, new[] {resultValue}).Compile();
		}

		private Func<string, IDisposable> GetNdcPushMethodCall()
		{
			Type ndcType = GetNdcType();
			MethodInfo method = ndcType.GetMethod("Push", new[] {typeof (string)});
			ParameterExpression resultValue;
			ParameterExpression keyParam = Expression.Parameter(typeof (string), "key");
			MethodCallExpression methodCall = Expression.Call(null, method, new Expression[] {resultValue = keyParam});
			return Expression.Lambda<Func<string, IDisposable>>(methodCall, new[] {resultValue}).Compile();
		}

		private Action<string, string> GetMdcSetMethodCall()
		{
			Type mdcType = GetMdcType();
			MethodInfo method = mdcType.GetMethod("Set", new[] {typeof (string), typeof (string)});
			ParameterExpression keyParam = Expression.Parameter(typeof (string), "key");
			ParameterExpression valueParam = Expression.Parameter(typeof (string), "value");
			MethodCallExpression methodCall = Expression.Call(null, method, new Expression[] {keyParam, valueParam});
			return Expression.Lambda<Action<string, string>>(methodCall, new[] {keyParam, valueParam}).Compile();
		}

		private Action<string> GetMdcRemoveMethodCall()
		{
			Type mdcType = GetMdcType();
			MethodInfo method = mdcType.GetMethod("Remove", new[] {typeof (string)});
			ParameterExpression keyParam = Expression.Parameter(typeof (string), "key");
			MethodCallExpression methodCall = Expression.Call(null, method, new Expression[] {keyParam});
			return Expression.Lambda<Action<string>>(methodCall, new[] {keyParam}).Compile();
		}
	}
}