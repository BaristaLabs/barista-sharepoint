namespace Barista.Extensions
{
    using System;
    using System.Reflection;
    using System.Threading;

    public static class ExceptionExtensions
    {
        public static bool IsFatal(this Exception exception)
        {
            while (exception != null)
            {
                if (exception is OutOfMemoryException && !(exception is InsufficientMemoryException) || exception is ThreadAbortException)
                {
                    return true;
                }
                if (exception is TypeInitializationException || exception is TargetInvocationException)
                {
                    exception = exception.InnerException;
                }
                else
                {
                    if (!(exception is AggregateException))
                    {
                        break;
                    }

                    using (var enumerator = ((AggregateException)exception).InnerExceptions.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            if (!enumerator.Current.IsFatal())
                            {
                                continue;
                            }
                            return true;
                        }
                        break;
                    }
                }
            }
            return false;
        }
    }
}
