namespace Barista.Social.Imports.Budgie.Entities
{
  using System.Linq;
  using System.Collections.Generic;

  public class TwitterEntityCollection : IEnumerable<TwitterEntity>
  {
    public TwitterEntityCollection()
    {
      Media = new List<TwitterMedia>();
      Urls = new List<TwitterUrl>();
      Mentions = new List<TwitterMention>();
      Hashtags = new List<TwitterHashtag>();
    }

    IEnumerator<TwitterEntity> Enumerator
    {
      get
      {
        return Media.Select(e => (TwitterEntity)e).AsEnumerable()
            .Concat(Urls.Select(e => (TwitterEntity)e))
            .Concat(Mentions.Select(e => (TwitterEntity)e))
            .Concat(Hashtags.Select(e => (TwitterEntity)e))
            .OrderBy(e => e.Indices.Item1)
            .GetEnumerator();
      }
    }

    public IEnumerable<TwitterMedia> Media { get; internal set; }
    public IEnumerable<TwitterUrl> Urls { get; internal set; }
    public IEnumerable<TwitterMention> Mentions { get; internal set; }
    public IEnumerable<TwitterHashtag> Hashtags { get; internal set; }

    public IEnumerator<TwitterEntity> GetEnumerator()
    {
      return Enumerator;
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return Enumerator;
    }
  }
}
