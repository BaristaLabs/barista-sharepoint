namespace Barista.Social.Imports.Budgie.Json
{
  using System;
  using Barista.Social.Imports.Budgie.Entities;

  internal class JsonDirectMessage
  {
    public long id { get; set; }
    public DateTime created_at { get; set; }
    public JsonUser sender { get; set; }
    public string text { get; set; }
    public JsonUser recipient { get; set; }
    public JsonEntityCollection entities { get; set; }

    public static implicit operator TwitterStatus(JsonDirectMessage j)
    {
      return new TwitterStatus
      {
        Id = j.id,
        CreatedAt = j.created_at,
        User = (TwitterUser)j.sender,
        ToUser = (TwitterUser)j.recipient,
        Text = j.text,
        Entities = (TwitterEntityCollection)j.entities,
        Source = "DM",
        IsDirectMessage = true,
      };
    }
  }
}
