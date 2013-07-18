namespace Barista.Social.Imports.Budgie.Json
{
  using System.Collections.Generic;
  using System.Linq;
  using Barista.Social.Imports.Budgie.Entities;

  internal class JsonEntityCollection
  {
    public JsonEntityCollection()
    {
      // initialize in case any elements are not present
      media = new List<JsonMedia>();
      urls = new List<JsonUrl>();
      user_mentions = new List<JsonMention>();
      hashtags = new List<JsonHashtag>();
    }

    public IEnumerable<JsonMedia> media { get; set; }
    public IEnumerable<JsonUrl> urls { get; set; }
    public IEnumerable<JsonMention> user_mentions { get; set; }
    public IEnumerable<JsonHashtag> hashtags { get; set; }

    public static explicit operator TwitterEntityCollection(JsonEntityCollection j)
    {
      if (j == null) return new TwitterEntityCollection();

      return new TwitterEntityCollection
      {
        Media = j.media.Select(m => (TwitterMedia)m).Where(u => u != null).ToList(),
        Urls = j.urls.Select(u => (TwitterUrl)u).Where(u => u != null).ToList(),
        Mentions = j.user_mentions.Select(m => (TwitterMention)m).Where(u => u != null).ToList(),
        Hashtags = j.hashtags.Select(h => (TwitterHashtag)h).Where(u => u != null).ToList(),
      };
    }
  }
}
