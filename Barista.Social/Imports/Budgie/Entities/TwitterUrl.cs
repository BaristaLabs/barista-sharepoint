namespace Barista.Social.Imports.Budgie.Entities
{
  using System;

  public class TwitterUrl : TwitterEntity
  {
    public Uri Uri { get; internal set; }
    public Uri ExpandedUri { get; internal set; }
  }
}
