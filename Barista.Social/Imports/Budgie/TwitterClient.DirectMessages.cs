namespace Barista.Social.Imports.Budgie
{
  using Barista.Newtonsoft.Json;
  using Barista.Social.Imports.Budgie.Extensions;
  using Barista.Social.Imports.Budgie.Json;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;

  partial class TwitterClient
  {
    Task<ITwitterResponse<IEnumerable<TwitterStatus>>> GetMessagesAsync(string relativeUri, int? count, long? since)
    {
      relativeUri += (relativeUri.Contains("?") ? "&" : "?") + "include_entities=true&count=" + (count ?? DefaultPageSize);

      if (since.HasValue) relativeUri += "&since_id=" + since;

      return HttpGetAsync(relativeUri).RespondWith<IEnumerable<TwitterStatus>>(content =>
          JsonConvert.DeserializeObject<IEnumerable<JsonDirectMessage>>(content, new TwitterizerDateConverter())
              .Select(j => (TwitterStatus)j)
              .ToList());
    }

    public Task<ITwitterResponse<IEnumerable<TwitterStatus>>> GetDirectMessagesAsync(int? count = null, long? since = null)
    {
      return GetMessagesAsync("direct_messages.json", count, since);
    }

    public Task<ITwitterResponse<IEnumerable<TwitterStatus>>> GetDirectMessagesSentAsync(int? count = null, long? since = null)
    {
      return GetMessagesAsync("direct_messages/sent.json", count, since);
    }

    public Task<ITwitterResponse<TwitterStatus>> GetDirectMessageAsync(long id)
    {
      var relativeUri = "direct_messages/show/" + id + ".json?include_entities=true";

      return HttpGetAsync(relativeUri).RespondWith<TwitterStatus>(content =>
          JsonConvert.DeserializeObject<JsonDirectMessage>(content, new TwitterizerDateConverter()));
    }

    public Task<ITwitterResponse<TwitterStatus>> SendDirectMessageAsync(string screenName, string text)
    {
      var relativeUri = "direct_messages/new.json";
      var parms = "screen_name=" + screenName.ToRfc3986Encoded() + "&text=" + text.ToRfc3986Encoded();

      return HttpPostAsync(relativeUri, parms).RespondWith<TwitterStatus>(content =>
          JsonConvert.DeserializeObject<JsonDirectMessage>(content, new TwitterizerDateConverter()));
    }

    public Task<ITwitterResponse<TwitterStatus>> DeleteDirectMessageAsync(long id)
    {
      var relativeUri = "direct_messages/destroy/" + id + ".json?include_entities=true";

      return HttpPostAsync(relativeUri).RespondWith<TwitterStatus>(content =>
          JsonConvert.DeserializeObject<JsonStatus>(content, new TwitterizerDateConverter()));
    }
  }
}