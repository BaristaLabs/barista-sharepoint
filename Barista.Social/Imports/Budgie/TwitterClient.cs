namespace Barista.Social.Imports.Budgie
{
  using Barista.Social.Imports.Budgie.Extensions;
  using Barista.Social.Imports.Budgie.Json;
  using Barista.Newtonsoft.Json;
  using System;
  using System.Collections.Generic;
  using System.Net;
  using System.Threading.Tasks;
  using System.IO;
  using SearchResponse = ITwitterResponse<System.Collections.Generic.IEnumerable<TwitterStatus>>;

  public partial class TwitterClient
  {
    private readonly IPlatformAdaptor m_platformAdaptor;
    private readonly string m_consumerKey;
    private readonly string m_consumerSecret;
    private TwitterAccessToken m_accessToken;
    protected Uri BaseUri = new Uri("https://api.twitter.com/1.1/");

    public TwitterClient(IPlatformAdaptor platformAdaptor, string consumerKey, string consumerSecret)
    {
      if (platformAdaptor == null) throw new ArgumentNullException("platformAdaptor");
      if (String.IsNullOrWhiteSpace(consumerKey)) throw new ArgumentException("consumerKey");
      if (String.IsNullOrWhiteSpace(consumerSecret)) throw new ArgumentException("consumerSecret");

      m_platformAdaptor = platformAdaptor;
      m_consumerKey = consumerKey;
      m_consumerSecret = consumerSecret;

      DefaultPageSize = 20;
      Timeout = TimeSpan.FromSeconds(100);
    }

    protected Task<WebResponse> HttpGetAsync(string relativeUri)
    {
      return HttpGetAsync(new Uri(BaseUri, relativeUri));
    }

    protected Task<WebResponse> HttpGetAsync(Uri uri)
    {
      var request = (HttpWebRequest)WebRequest.Create(uri);
      request.Method = "GET";
      request.ContentType = "application/x-www-form-urlencoded";
      OnGetRequest(request);

      return request.GetResponseAsync(Timeout);
    }

    protected Task<WebResponse> HttpPostAsync(string relativeUri, string content = null, ITokenPair tokens = null)
    {
      return HttpPostAsync(new Uri(BaseUri, relativeUri), content, tokens);
    }

    Task<WebResponse> HttpPostAsync(Uri uri, string content = null, ITokenPair tokens = null)
    {
      var request = (HttpWebRequest)WebRequest.Create(uri);
      request.Method = "POST";
      request.ContentType = "application/x-www-form-urlencoded";
      OnPostRequest(request, content, tokens);

      if (content != null)
      {
        // not "using" rs as per CA2202
        Stream rs = null;
        try
        {
          rs = Task.Factory.FromAsync<Stream>(request.BeginGetRequestStream, request.EndGetRequestStream, null).Result;
          using (var sw = new StreamWriter(rs))
          {
            rs = null;
            sw.Write(content);
          }
        }
        finally
        {
          if (rs != null) rs.Dispose();
        }
      }

      return request.GetResponseAsync(Timeout);
    }

    /// <summary>
    /// The number of statuses returned by any method if the count parameter is omitted. Default is 20.
    /// </summary>
    public int DefaultPageSize { get; set; }

    /// <summary>
    /// The timeout for HTTP requests.
    /// </summary>
    public TimeSpan Timeout { get; set; }

    /// <summary>
    /// Searches Twitter using the given search query and returns matching statuses.
    /// </summary>
    /// <param name="query">The query to issue to Twitter.</param>
    /// <param name="count">The maximum number of tweets to return. If omitted, <see cref="DefaultPageSize" /> will be used.</param>
    /// <param name="since">If specified, only tweets created after the tweet with this Id will be returned.</param>
    /// <returns></returns>
    public Task<SearchResponse> SearchAsync(string query, int? count = null, long? since = null)
    {
      var relativeUri = "search/tweets.json?q=" + query.ToRfc3986Encoded() + "&include_entities=true";

      if (count.HasValue)
      {
        relativeUri += "&count=" + count;
      }

      if (since.HasValue)
      {
        relativeUri += "&since_id=" + since;
      }

      return HttpGetAsync(relativeUri).ContinueWith<SearchResponse>(t =>
      {
        var result = t.ToTwitterResponse<IEnumerable<TwitterStatus>>();
        if (result.StatusCode != HttpStatusCode.OK) return result;

        result.Result = ((TwitterSearchResults)JsonConvert.DeserializeObject<JsonSearchResults>(result.RawContent, new TwitterizerDateConverter())).Results;
        return result;
      });
    }
  }
}
