namespace Barista.Social.Imports.Budgie.Json
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;

  internal class JsonFriendship
  {
    public JsonRelationship relationship { get; set; }

    public static implicit operator TwitterFriendship(JsonFriendship j)
    {
      return new TwitterFriendship
      {
        AllReplies = j.relationship.source.all_replies ?? false,
        CanDirectMessage = j.relationship.source.can_dm ?? false,
        Id = j.relationship.source.id,
        ScreenName = j.relationship.source.screen_name,
        IsBlocking = j.relationship.source.blocking ?? false,
        IsFollowed = j.relationship.source.followed_by ?? false,
        IsFollowing = j.relationship.source.following ?? false,
        IsMarkedAsSpam = j.relationship.source.marked_spam ?? false,
        Retweets = j.relationship.source.want_retweets ?? false,
      };
    }
  }

  internal class JsonRelationship
  {
    public JsonFriendshipSource source { get; set; }
  }

  internal class JsonFriendshipSource
  {
    public bool? can_dm { get; set; }
    public bool? blocking { get; set; }
    public long id { get; set; }
    public bool? want_retweets { get; set; }
    public bool? all_replies { get; set; }
    public bool? marked_spam { get; set; }
    public string screen_name { get; set; }
    public bool? following { get; set; }
    public bool? followed_by { get; set; }
  }
}
