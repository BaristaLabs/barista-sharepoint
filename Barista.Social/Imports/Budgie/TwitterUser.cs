namespace Barista.Social.Imports.Budgie
{
  using System;

  public class TwitterUser
  {
    internal TwitterUser()
    {
    }

    public long Id { get; internal set; }
    public string ScreenName { get; internal set; }
    public string Name { get; internal set; }

    public string Location { get; internal set; }
    public string Description { get; internal set; }
    public Uri Uri { get; internal set; }

    public DateTime CreatedAt { get; internal set; }

    public Uri ProfileImageUri { get; internal set; }

    public bool IsProtected { get; internal set; }

    public int FriendCount { get; internal set; }
    public int FollowerCount { get; internal set; }
    public int StatusCount { get; internal set; }
  }
}
