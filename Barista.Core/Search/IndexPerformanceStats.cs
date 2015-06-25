namespace Barista.Search
{
  using System;

  public class IndexingPerformanceStats
  {
    public string Operation
    {
      get;
      set;
    }

    public int OutputCount
    {
      get;
      set;
    }

    public int InputCount
    {
      get;
      set;
    }

    public TimeSpan Duration
    {
      get;
      set;
    }

    public DateTime Started
    {
      get;
      set;
    }
    public double DurationMilliseconds
    {
      get
      {
        return Math.Round(Duration.TotalMilliseconds, 2);
      }
    }
  }
}
