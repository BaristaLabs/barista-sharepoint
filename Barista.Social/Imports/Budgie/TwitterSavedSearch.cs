namespace Barista.Social.Imports.Budgie
{
  using System;

  public class TwitterSavedSearch
  {
    internal TwitterSavedSearch()
    {
    }

    public long Id { get; internal set; }
    public string Name { get; internal set; }
    public DateTime CreatedAt { get; internal set; }
    public string Query { get; internal set; }
  }
}
