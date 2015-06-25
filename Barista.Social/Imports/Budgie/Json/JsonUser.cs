namespace Barista.Social.Imports.Budgie.Json
{
  using System;
  using Barista.Social.Imports.Budgie.Extensions;

  internal class JsonUser
  {
    public long id { get; set; }
    public string screen_name { get; set; }
    public string name { get; set; }
    public string location { get; set; }
    public string description { get; set; }
    public DateTime created_at { get; set; }
    public string profile_image_url { get; set; }
    public string url { get; set; }
    public bool @protected { get; set; }
    public int friends_count { get; set; }
    public int followers_count { get; set; }
    public int statuses_count { get; set; }

    public static implicit operator TwitterUser(JsonUser j)
    {
      return new TwitterUser
      {
        Id = j.id,
        ScreenName = j.screen_name,
        Name = j.name,
        Location = j.location,
        Description = j.description,
        CreatedAt = j.created_at,
        IsProtected = j.@protected,
        FriendCount = j.friends_count,
        FollowerCount = j.followers_count,
        StatusCount = j.statuses_count,
        Uri = j.url.ToUri(),
        ProfileImageUri = j.profile_image_url.ToUri(),
      };
    }
  }
}
