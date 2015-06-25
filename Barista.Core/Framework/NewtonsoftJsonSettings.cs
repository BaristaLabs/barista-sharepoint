namespace Barista.Framework
{
  using Barista.Newtonsoft.Json;

  public static class NewtonsoftJsonSettings
  {
    public static JsonSerializer GetSerializer()
    {
      return new Newtonsoft.Json.JsonSerializer
      {
        TypeNameHandling = TypeNameHandling.Auto,
      };
    }

    public static Formatting GetFormatting()
    {
      return Formatting.Indented;
    }
  }
}
