namespace Barista.Framework
{
  using System.Text;
  using Barista.Newtonsoft.Json;
  using RestSharp.Deserializers;

  public class JsonNetDeserializer : IDeserializer
  {
    private readonly string m_byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());

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
      var content = response.Content;

      if (content.StartsWith(m_byteOrderMarkUtf8))
        content = content.Remove(0, m_byteOrderMarkUtf8.Length);

      return JsonConvert.DeserializeObject<T>(content);
    }
  }
}