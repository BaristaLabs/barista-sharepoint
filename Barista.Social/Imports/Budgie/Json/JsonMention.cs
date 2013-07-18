namespace Barista.Social.Imports.Budgie.Json
{
  using System;
  using Barista.Social.Imports.Budgie.Entities;

  internal class JsonMention
  {
    public long id { get; set; }
    //public string id_str { get; set; }
    public string screen_name { get; set; }
    public string name { get; set; }
    public int[] indices { get; set; }

    public static explicit operator TwitterMention(JsonMention j)
    {
      return new TwitterMention
      {
        Id = j.id,
        //IdString = j.id_str,
        Text = j.screen_name,
        Name = j.name,
        Indices = Tuple.Create(j.indices[0], j.indices[1]),
      };
    }
  }
}
