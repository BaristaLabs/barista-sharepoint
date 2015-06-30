namespace Barista.TuesPechkin
{
    using System;
    using System.Diagnostics;

    internal static class Tracer
    {
        private readonly static TraceSource Source = new TraceSource("pechkin:default");

        public static void Trace(String message)
        {
            Source.TraceInformation(message);
        }

        public static void Warn(String message)
        {
            Source.TraceEvent(TraceEventType.Warning, 0, message);
        }

        public static void Warn(String message, Exception e)
        {
            Source.TraceEvent(TraceEventType.Warning, 0, String.Format(message + "{0}", e));
        }

        public static void Critical(String message, Exception e)
        {
            Source.TraceEvent(TraceEventType.Critical, 0, String.Format(message + "{0}", e));
        }
    }
}