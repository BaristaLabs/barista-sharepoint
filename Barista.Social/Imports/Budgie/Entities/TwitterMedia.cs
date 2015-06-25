namespace Barista.Social.Imports.Budgie.Entities
{
  using System;
  using System.Collections.Generic;

  public enum TwitterMediaResizeMethod
  {
    Crop, Fit
  }

  public class TwitterMediaSize
  {
    public int Width { get; internal set; }
    public int Height { get; internal set; }
    public TwitterMediaResizeMethod ResizeMethod { get; internal set; }
  }

  public class TwitterMedia : TwitterEntity
  {
    public long Id { get; internal set; }
    //public string IdString { get; internal set; }
    public Uri Uri { get; internal set; }
    public Uri SecureUri { get; internal set; }
    public Uri ExpandedUri { get; internal set; }
    public IEnumerable<TwitterMediaSize> Sizes { get; internal set; }
  }
}
