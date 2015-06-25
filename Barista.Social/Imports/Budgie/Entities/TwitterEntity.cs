namespace Barista.Social.Imports.Budgie.Entities
{
  using System;

  public abstract class TwitterEntity
  {
    public string Text { get; internal set; }
    public Tuple<Int32, Int32> Indices { get; internal set; }
  }
}
