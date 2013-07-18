namespace Barista.Social.Imports.Budgie
{
  using System;

  public interface ITokenPair
  {
    string Token { get; }
    string Secret { get; }
  }

  public class TwitterRequestToken : ITokenPair
  {
    internal TwitterRequestToken(Uri baseUri)
    {
      _baseUri = baseUri;
    }

    Uri _baseUri;

    internal bool IsValid
    {
      get { return !String.IsNullOrWhiteSpace(Token) && !String.IsNullOrWhiteSpace(Secret); }
    }

    public string Token { get; internal set; }
    public string Secret { get; internal set; }

    public Uri AuthorizationUri
    {
      get
      {
        if (!IsValid) return null;

        return new Uri(_baseUri, "/oauth/authorize?oauth_token=" + Token);
      }
    }
  }

  public class TwitterAccessToken : ITokenPair
  {
    internal TwitterAccessToken()
    {
    }

    internal bool IsValid
    {
      get { return !String.IsNullOrWhiteSpace(Token) && !String.IsNullOrWhiteSpace(Secret); }
    }

    public string Token { get; internal set; }
    public string Secret { get; internal set; }

    public string UserId { get; internal set; }
    public string ScreenName { get; internal set; }
  }
}
