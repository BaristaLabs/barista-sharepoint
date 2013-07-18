namespace Barista.Social.Imports.Budgie.Json
{
  using Barista.Social.Imports.Budgie.Entities;

  internal class JsonMediaSizeCollection
  {
    public JsonMediaSize large { get; set; }
    public JsonMediaSize medium { get; set; }
    public JsonMediaSize small { get; set; }
    public JsonMediaSize thumb { get; set; }
  }

  internal class JsonMediaSize
  {
    public int w { get; set; }
    public int h { get; set; }
    public string resize { get; set; }

    public static explicit operator TwitterMediaSize(JsonMediaSize j)
    {
      return new TwitterMediaSize
      {
        Width = j.w,
        Height = j.h,
        ResizeMethod = j.resize == "crop" ? TwitterMediaResizeMethod.Crop : TwitterMediaResizeMethod.Fit,
      };
    }
  }
}
