namespace Barista.Social.Imports.Budgie.Json
{
  using System;

  internal class JsonSavedSearch
  {
    public long id { get; set; }
    public string name { get; set; }
    public DateTime created_at { get; set; }
    public string query { get; set; }

    public static implicit operator TwitterSavedSearch(JsonSavedSearch j)
    {
      return new TwitterSavedSearch
      {
        Id = j.id,
        Name = j.name,
        CreatedAt = j.created_at,
        Query = j.query,
      };
    }
  }
}
