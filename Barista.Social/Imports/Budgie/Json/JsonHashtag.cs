namespace Barista.Social.Imports.Budgie.Json
{
  using System;
  using Barista.Social.Imports.Budgie.Entities;

  internal class JsonHashtag
  {
    public string text { get; set; }
    public int[] indices { get; set; }

    public static explicit operator TwitterHashtag(JsonHashtag j)
    {
      return new TwitterHashtag
      {
        Text = j.text,
        Indices = Tuple.Create(j.indices[0], j.indices[1]),
      };
    }
  }
}
