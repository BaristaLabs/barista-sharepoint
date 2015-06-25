namespace Barista.Social.Imports.Budgie
{
  public class TwitterFriendship
  {
    internal TwitterFriendship()
    {
    }

    public long Id { get; internal set; }
    public string ScreenName { get; internal set; }

    public bool CanDirectMessage { get; internal set; }
    public bool IsBlocking { get; internal set; }
    public bool IsMarkedAsSpam { get; internal set; }
    public bool Retweets { get; internal set; }
    public bool AllReplies { get; internal set; }

    public bool IsFollowing { get; internal set; }
    public bool IsFollowed { get; internal set; }
  }
}
