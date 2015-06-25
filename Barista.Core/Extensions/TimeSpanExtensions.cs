namespace Barista.Extensions
{
  using System;
  public static class TimeSpanExtensions
  {
    public static string ToString(this TimeSpan span, string format)
    {
      //1.14:30:15
      return String.Format("{0:00}.{1:00}:{2:00}:{3:00}.{4:00}", span.TotalDays, (int)span.TotalHours, span.Minutes, span.Seconds, span.Milliseconds);
    }
  }
}
