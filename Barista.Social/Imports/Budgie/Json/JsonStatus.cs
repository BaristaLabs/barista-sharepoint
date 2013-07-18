namespace Barista.Social.Imports.Budgie.Json
{
  using System;
  using Barista.Social.Imports.Budgie.Entities;

  internal class JsonStatus
  {
    public long id { get; set; }
    public DateTime created_at { get; set; }
    public string text { get; set; }
    public JsonUser user { get; set; }
    public long? in_reply_to_status_id { get; set; }
    public long? in_reply_to_user_id { get; set; }
    public string in_reply_to_screen_name { get; set; }
    public string source { get; set; }
    public bool favorited { get; set; }
    public JsonStatus retweeted_status { get; set; }
    public JsonEntityCollection entities { get; set; }

    public static implicit operator TwitterStatus(JsonStatus j)
    {
      var status = new TwitterStatus
      {
        Id = j.id,
        CreatedAt = j.created_at,
        Text = j.text,
        User = (TwitterUser)j.user,
        Source = j.source,
        IsFavorite = j.favorited,
        Entities = (TwitterEntityCollection)j.entities,
      };

      if (j.retweeted_status != null)
      {
        status.IsRetweet = true;
        status.Context = (TwitterStatus)j.retweeted_status;
      }
      else if (j.in_reply_to_user_id.HasValue)
      {
        status.ToUser = new TwitterUser
        {
          Id = j.in_reply_to_user_id.Value,
          ScreenName = j.in_reply_to_screen_name,
        };

        if (j.in_reply_to_status_id.HasValue)
        {
          status.IsReply = true;
          status.Context = new TwitterStatus
          {
            Id = j.in_reply_to_status_id.Value,
            User = status.ToUser,
          };
        }
      }
      return status;
    }
  }
}
