namespace Barista.Social.Imports.Budgie.Json
{
  using System;
  using Barista.Social.Imports.Budgie.Entities;
  using Barista.Social.Imports.Budgie.Extensions;

  internal class JsonUrl
  {
    public string url { get; set; }
    public string display_url { get; set; }
    public string expanded_url { get; set; }
    public int[] indices { get; set; }

    public static explicit operator TwitterUrl(JsonUrl j)
    {
      var url = new TwitterUrl
      {
        Text = j.display_url,
        Indices = Tuple.Create(j.indices[0], j.indices[1]),
        ExpandedUri = j.expanded_url.ToUri(),
        Uri = j.url.ToUri(),
      };

      if (url.Uri == null) return null;

      return url;
    }
  }
}
