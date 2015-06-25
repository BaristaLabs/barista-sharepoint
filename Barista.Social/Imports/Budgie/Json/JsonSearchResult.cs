namespace Barista.Social.Imports.Budgie.Json
{
  using System;
  using Barista.Social.Imports.Budgie.Entities;
  using Barista.Social.Imports.Budgie.Extensions;

  internal class JsonSearchResult
  {
    public DateTime created_at { get; set; }
    public JsonEntityCollection entities { get; set; }
    public string from_user { get; set; }
    public long from_user_id { get; set; }
    public string from_user_name { get; set; }
    public long id { get; set; }
    public string profile_image_url { get; set; }
    public string source { get; set; }
    public string text { get; set; }
    public long? to_user_id { get; set; }
    public string to_user_name { get; set; }

    public static explicit operator TwitterStatus(JsonSearchResult j)
    {
      var status = new TwitterStatus
      {
        Id = j.id,
        CreatedAt = j.created_at,
        Text = j.text,
        User = new TwitterUser
        {
          Id = j.from_user_id,
          ScreenName = j.from_user,
          Name = j.from_user_name,
          ProfileImageUri = j.profile_image_url.ToUri(),
        },
        Source = j.source,
        IsSearchResult = true,
        Entities = (TwitterEntityCollection)j.entities,
      };

      if (j.to_user_id.HasValue)
      {
        status.ToUser = new TwitterUser
        {
          Id = j.to_user_id.Value,
          ScreenName = j.to_user_name,
        };
      }

      return status;
    }
  }
}
