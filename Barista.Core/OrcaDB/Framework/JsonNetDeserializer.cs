namespace OFS.OrcaDB.Core.Framework
{
  using Newtonsoft.Json;
  using RestSharp.Deserializers;

  public class JsonNetDeserializer : IDeserializer
  {
    public string DateFormat
    {
      get;
      set;
    }

    public string Namespace
    {
      get;
      set;
    }

    public string RootElement
    {
      get;
      set;
    }

    public T Deserialize<T>(RestSharp.IRestResponse response)
    {
      return JsonConvert.DeserializeObject<T>(response.Content);
    }
  }
}
