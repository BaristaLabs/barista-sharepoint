namespace Barista.Social.Imports.Budgie
{
  using System.Net;

  public interface ITwitterResponse<out T>
  {
    HttpStatusCode StatusCode { get; }
    string ErrorMessage { get; }
    TwitterRateLimit RateLimit { get; }
    T Result { get; }
  }

  internal class TwitterResponse<T> : ITwitterResponse<T>
  {
    internal TwitterResponse()
    {
    }

    public HttpStatusCode StatusCode { get; internal set; }
    public string ErrorMessage { get; internal set; }
    public string RawContent { get; internal set; }
    public TwitterRateLimit RateLimit { get; internal set; }
    public T Result { get; internal set; }
  }
}
