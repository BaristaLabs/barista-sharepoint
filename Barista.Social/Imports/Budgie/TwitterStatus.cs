namespace Barista.Social.Imports.Budgie
{
  using System;
  using Barista.Social.Imports.Budgie.Entities;

  public class TwitterStatus
  {
    internal TwitterStatus()
    {
    }

    public long Id { get; internal set; }
    public DateTime CreatedAt { get; internal set; }
    public TwitterUser User { get; internal set; }
    public string Text { get; internal set; }

    public string Source { get; internal set; }
    public bool IsFavorite { get; internal set; }

    public bool IsRetweet { get; internal set; }
    public bool IsReply { get; internal set; }
    public bool IsDirectMessage { get; internal set; }
    public bool IsSearchResult { get; internal set; }

    public TwitterStatus Context { get; internal set; }
    public TwitterUser ToUser { get; internal set; }

    public TwitterEntityCollection Entities { get; internal set; }
  }
}
