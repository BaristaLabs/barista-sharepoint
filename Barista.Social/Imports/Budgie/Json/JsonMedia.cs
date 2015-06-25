namespace Barista.Social.Imports.Budgie.Json
{
  using System;
  using Barista.Social.Imports.Budgie.Entities;
  using Barista.Social.Imports.Budgie.Extensions;

  internal class JsonMedia
  {
    public long id { get; set; }
    public string media_url { get; set; }
    public string media_url_https { get; set; }
    public string display_url { get; set; }
    public string expanded_url { get; set; }
    // public JsonMediaSizeCollection sizes { get; set; }
    public int[] indices { get; set; }

    public static explicit operator TwitterMedia(JsonMedia j)
    {
      return new TwitterMedia
      {
        Id = j.id,
        Text = j.display_url,
        //Sizes = j.sizes.Select(s => (TwitterMediaSize)s).ToList(),
        Indices = Tuple.Create(j.indices[0], j.indices[1]),
        Uri = j.media_url.ToUri(),
        SecureUri = j.media_url_https.ToUri(),
        ExpandedUri = j.expanded_url.ToUri(),
      };
    }
  }
}
