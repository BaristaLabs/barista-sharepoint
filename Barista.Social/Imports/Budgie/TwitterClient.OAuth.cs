namespace Barista.Social.Imports.Budgie
{
    using Barista.Extensions;
    using Barista.Social.Imports.Budgie.Extensions;
  using Barista.Social.Imports.Budgie.Json;
  using Newtonsoft.Json;
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Net;
  using System.Text;
  using System.Threading.Tasks;

  partial class TwitterClient
  {
    void OnGetRequest(WebRequest request)
    {
      Authorize(request);
    }

    void OnPostRequest(WebRequest request, string content = null, ITokenPair tokens = null)
    {
      Authorize(request, content, tokens);
    }

    // mad props to http://www.codeproject.com/Articles/247336/Twitter-OAuth-authentication-using-Net

    /// <summary>
    /// Gets an AOuth request token from Twitter.
    /// </summary>
    /// <returns>A valid <see cref="TwitterRequestToken"/>.</returns>
    public Task<TwitterRequestToken> GetRequestTokenAsync(Uri callbackUri = null)
    {
      // this is very similar to all the other OAuth requests except that at this point we don't
      // have any tokens to pass along with the request, so for now I'm keeping this logic separate.

      var request = (HttpWebRequest)WebRequest.Create(new Uri(BaseUri, "/oauth/request_token"));
      request.Method = "POST";
      request.ContentType = "application/x-www-form-urlencoded";

      var oauth_nonce = Convert.ToBase64String(UTF8Encoding.UTF8.GetBytes(DateTime.Now.Ticks.ToString()));
      var timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
      var oauth_timestamp = Convert.ToInt64(timeSpan.TotalSeconds).ToString();
      var resource_url = request.RequestUri.GetComponents(UriComponents.SchemeAndServer | UriComponents.Path, UriFormat.Unescaped);

      var parms = new SortedDictionary<string, string>
            {
                { "oauth_consumer_key",      m_consumerKey },
                { "oauth_nonce",             oauth_nonce },
                { "oauth_signature_method",  "HMAC-SHA1" },
                { "oauth_timestamp",         oauth_timestamp },
                { "oauth_version",           "1.0" },
            };

      if (callbackUri != null) parms.Add("oauth_callback", Uri.EscapeDataString(callbackUri.ToString()));

      if (!String.IsNullOrEmpty(request.RequestUri.Query))
      {
        foreach (var p in request.RequestUri.Query.Substring(1).Split('&'))
        {
          var kv = p.Split('=');
          parms.Add(kv[0], kv[1]);
        }
      }

      var baseString = parms.First().Key + "=" + parms.First().Value;
      foreach (var p in parms.Skip(1))
      {
        baseString += "&" + p.Key + "=" + p.Value;
      }

      baseString = request.Method + "&" + Uri.EscapeDataString(resource_url) + "&" + Uri.EscapeDataString(baseString);

      var compositeKey = Uri.EscapeDataString(m_consumerSecret) + "&";

      var oauth_signature = m_platformAdaptor.ComputeSha1Hash(compositeKey, baseString);
      //using (var hasher = new HMACSHA1(UTF8Encoding.UTF8.GetBytes(compositeKey)))
      //{
      //    oauth_signature = Convert.ToBase64String(hasher.ComputeHash(UTF8Encoding.UTF8.GetBytes(baseString)));
      //}

      string authHeader;
      if (callbackUri == null)
      {
        authHeader = string.Format("OAuth oauth_nonce=\"{0}\", oauth_signature_method=\"HMAC-SHA1\", oauth_timestamp=\"{1}\", oauth_consumer_key=\"{2}\", oauth_signature=\"{3}\", oauth_version=\"1.0\"",
                                Uri.EscapeDataString(oauth_nonce),
                                Uri.EscapeDataString(oauth_timestamp),
                                Uri.EscapeDataString(m_consumerKey),
                                Uri.EscapeDataString(oauth_signature));
      }
      else
      {
        authHeader = string.Format("OAuth oauth_callback=\"{0}\", oauth_nonce=\"{1}\", oauth_signature_method=\"HMAC-SHA1\", oauth_timestamp=\"{2}\", oauth_consumer_key=\"{3}\", oauth_signature=\"{4}\", oauth_version=\"1.0\"",
                                Uri.EscapeDataString(callbackUri.ToString()),
                                Uri.EscapeDataString(oauth_nonce),
                                Uri.EscapeDataString(oauth_timestamp),
                                Uri.EscapeDataString(m_consumerKey),
                                Uri.EscapeDataString(oauth_signature));
      }
      request.Headers["Authorization"] = authHeader;

      return request.GetResponseAsync(Timeout).ContinueWith(t =>
      {
        if (t.IsFaulted) return null;

        var response = t.Result as HttpWebResponse;
        if (response.StatusCode != HttpStatusCode.OK) return null;

        string responseString = response.GetContentString();
        if (responseString.IsNullOrWhiteSpace()) return null;

        var result = new TwitterRequestToken(BaseUri);
        foreach (var i in responseString.Split('&'))
        {
          var kv = i.Split('=');
          if (kv.Length < 2) continue;

          if (kv[0] == "oauth_callback_confirmed")
          {
            if (kv[1] != "true") return null;
          }
          else if (kv[0] == "oauth_token")
          {
            result.Token = kv[1];
          }
          else if (kv[0] == "oauth_token_secret")
          {
            result.Secret = kv[1];
          }
        }

        return result.IsValid ? result : null;
      });
    }

    /// <summary>
    /// Authenticates this <see cref="Budgie.TwitterClient"/> with an already-known OAuth access token and token_secret.
    /// </summary>
    /// <param name="accessToken">An OAuth access token.</param>
    /// <param name="accessTokenSecret">An OAuth access token_secret.</param>
    /// <returns>An instance of <see cref="Budgie.TwitterClient.TwitterAccessToken"/> with the supplied values.</returns>
    public TwitterAccessToken Authenticate(string accessToken, string accessTokenSecret)
    {
      m_accessToken = new TwitterAccessToken
      {
        Token = accessToken,
        Secret = accessTokenSecret,
      };
      return m_accessToken;
    }

    public Task<ITwitterResponse<TwitterUser>> VerifyCredentialsAsync()
    {
      return HttpGetAsync("account/verify_credentials.json?skip_status=true").ContinueWith<ITwitterResponse<TwitterUser>>(t =>
      {
        var result = t.ToTwitterResponse<TwitterUser>();
        if (result.StatusCode != HttpStatusCode.OK) return result;

        result.Result = (TwitterUser)JsonConvert.DeserializeObject<JsonUser>(result.RawContent, new TwitterizerDateConverter());
        return result;
      });
    }

    /// <summary>
    /// Authenticates this <see cref="Budgie.TwitterClient"/> by requesting an OAuth access token and token_secret from Twitter.
    /// </summary>
    /// <param name="requestToken">An OAuth request token.</param>
    /// <param name="verifier">A verification PIN obtained by the user from Twitter.</param>
    /// <returns>An instance of <see cref="TwitterAccessToken"/> with the tokens returned from Twitter.</returns>
    public Task<TwitterAccessToken> AuthenticateAsync(TwitterRequestToken requestToken, string verifier)
    {
      return HttpPostAsync("/oauth/access_token", "oauth_verifier=" + verifier, requestToken).ContinueWith(t =>
      {
        if (t.IsFaulted) return null;

        var response = t.Result as HttpWebResponse;
        if (response.StatusCode != HttpStatusCode.OK) return null;

        string responseString = response.GetContentString();
        if (responseString.IsNullOrWhiteSpace()) return null;

        var result = new TwitterAccessToken();
        foreach (var i in responseString.Split('&'))
        {
          var kv = i.Split('=');
          if (kv.Length < 2) continue;

          if (kv[0] == "oauth_token")
          {
            result.Token = kv[1];
          }
          else if (kv[0] == "oauth_token_secret")
          {
            result.Secret = kv[1];
          }
          else if (kv[0] == "user_id")
          {
            result.UserId = kv[1];
          }
          else if (kv[0] == "screen_name")
          {
            result.ScreenName = kv[1];
          }
        }

        if (!result.IsValid) return null;

        return (m_accessToken = result);
      });
    }

    void Authorize(WebRequest request, string content = null, ITokenPair tokens = null)
    {
      if (tokens == null) tokens = m_accessToken;

      var oauth_nonce = Convert.ToBase64String(UTF8Encoding.UTF8.GetBytes(DateTime.Now.Ticks.ToString()));
      var timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
      var oauth_timestamp = Convert.ToInt64(timeSpan.TotalSeconds).ToString();
      var resource_url = request.RequestUri.GetComponents(UriComponents.SchemeAndServer | UriComponents.Path, UriFormat.Unescaped);

      var parms = new SortedDictionary<string, string>
            {
                { "oauth_consumer_key",      m_consumerKey },
                { "oauth_nonce",             oauth_nonce },
                { "oauth_signature_method",  "HMAC-SHA1" },
                { "oauth_timestamp",         oauth_timestamp },
                { "oauth_version",           "1.0" },
                { "oauth_token",             tokens.Token },
            };

      if (!String.IsNullOrEmpty(request.RequestUri.Query))
      {
        foreach (var p in request.RequestUri.Query.Substring(1).Split('&'))
        {
          var kv = p.Split('=');
          parms.Add(kv[0], kv[1]);
        }
      }

      if (content != null)
      {
        foreach (var p in content.Split('&'))
        {
          var kv = p.Split('=');
          parms.Add(kv[0], kv[1]);
        }
      }

      var baseString = parms.First().Key + "=" + parms.First().Value;
      foreach (var p in parms.Skip(1))
      {
        baseString += "&" + p.Key + "=" + p.Value;
      }

      baseString = request.Method + "&" + Uri.EscapeDataString(resource_url) + "&" + Uri.EscapeDataString(baseString);

      var compositeKey = Uri.EscapeDataString(m_consumerSecret) + "&" + Uri.EscapeDataString(tokens.Secret);

      string oauth_signature;
      oauth_signature = m_platformAdaptor.ComputeSha1Hash(compositeKey, baseString);
      //using (var hasher = new HMACSHA1(UTF8Encoding.UTF8.GetBytes(compositeKey)))
      //{
      //    oauth_signature = Convert.ToBase64String(hasher.ComputeHash(UTF8Encoding.UTF8.GetBytes(baseString)));
      //}

      var authHeader = string.Format("OAuth oauth_nonce=\"{0}\", oauth_signature_method=\"HMAC-SHA1\", oauth_timestamp=\"{1}\", oauth_consumer_key=\"{2}\", oauth_signature=\"{3}\", oauth_token=\"{4}\", oauth_version=\"1.0\"",
                              Uri.EscapeDataString(oauth_nonce),
                              Uri.EscapeDataString(oauth_timestamp),
                              Uri.EscapeDataString(m_consumerKey),
                              Uri.EscapeDataString(oauth_signature),
                              Uri.EscapeDataString(tokens.Token));
      request.Headers["Authorization"] = authHeader;
    }
  }
}
