namespace Barista.Social.Imports.Budgie
{
  using System;
  using System.Collections.Generic;

  public class TwitterSearchResults
  {
    internal TwitterSearchResults()
    {
    }

    public TimeSpan CompletedIn { get; internal set; }

    public long MaxId { get; internal set; }
    public long SinceId { get; internal set; }

    public int Page { get; internal set; }
    public int PageSize { get; internal set; }

    public string Query { get; internal set; }

    public IEnumerable<TwitterStatus> Results { get; internal set; }
  }
}
