
namespace Barista.Social.Imports.Budgie.Json
{
  internal class JsonList
  {
    public long id { get; set; }
    public string name { get; set; }
    public string uri { get; set; }
    public int subscriber_count { get; set; }
    public int member_count { get; set; }
    public string full_name { get; set; }
    public string description { get; set; }
    public JsonUser user { get; set; }
    public bool following { get; set; }

    public static explicit operator TwitterList(JsonList j)
    {
      return new TwitterList
      {
        Id = j.id,
        Name = j.name,
        Path = j.uri,
        SubscriberCount = j.subscriber_count,
        MemberCount = j.member_count,
        FullName = j.full_name,
        Description = j.description,
        User = (TwitterUser)j.user,
        IsFollowing = j.following,
      };
    }
  }
}
