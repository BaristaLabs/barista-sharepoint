namespace Barista.Social.Imports.Budgie
{
  using Barista.Social.Imports.Budgie.Extensions;
  using Barista.Social.Imports.Budgie.Json;
  using Barista.Newtonsoft.Json;
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;

  // just for brevity
  using TimelineResponse = Budgie.ITwitterResponse<System.Collections.Generic.IEnumerable<TwitterStatus>>;

  partial class TwitterClient
  {
    protected Task<TimelineResponse> GetTimelineAsync(string relativeUri, int? count, long? since, long? max)
    {
      relativeUri += (relativeUri.Contains("?") ? "&" : "?")
          + "include_entities=true&count=" + (count ?? DefaultPageSize);

      if (since.HasValue) relativeUri += "&since_id=" + since;
      if (max.HasValue) relativeUri += "&max_id=" + max;

      return HttpGetAsync(relativeUri).RespondWith<IEnumerable<TwitterStatus>>(content =>
          JsonConvert.DeserializeObject<IEnumerable<JsonStatus>>(content, new TwitterizerDateConverter())
              .Select(j => (TwitterStatus)j)
              .ToList());
    }

    public Task<TimelineResponse> GetUserTimelineAsync(string screenName, bool includeRetweets = false, int? count = null, long? since = null, long? max = null)
    {
      if (string.IsNullOrWhiteSpace(screenName)) throw new ArgumentException("screenName must be specified", "screenName");

      var relativeUri = "statuses/user_timeline.json?screen_name=" + screenName.ToRfc3986Encoded();
      if (includeRetweets) relativeUri += "&include_rts=true";

      return GetTimelineAsync(relativeUri, count, since, max);
    }

    public Task<TimelineResponse> GetRetweetsToUserAsync(string screenName, int? count = null, long? since = null, long? max = null)
    {
      if (string.IsNullOrWhiteSpace(screenName)) throw new ArgumentException("screenName must be specified", "screenName");

      return GetTimelineAsync("statuses/retweeted_to_user.json?screen_name=" + screenName.ToRfc3986Encoded(), count, since, max);
    }

    public Task<TimelineResponse> GetRetweetsByUserAsync(string screenName, int? count = null, long? since = null, long? max = null)
    {
      if (string.IsNullOrWhiteSpace(screenName)) throw new ArgumentException("screenName must be specified", "screenName");

      return GetTimelineAsync("statuses/retweeted_by_user.json?screen_name=" + screenName.ToRfc3986Encoded(), count, since, max);
    }

    public Task<TimelineResponse> GetHomeTimelineAsync(int? count = null, long? since = null, long? max = null)
    {
      return GetTimelineAsync("statuses/home_timeline.json", count, since, max);
    }

    public Task<TimelineResponse> GetUserTimelineAsync(int? count = null, long? since = null, long? max = null)
    {
      return GetTimelineAsync("statuses/user_timeline.json", count, since, max);
    }

    public Task<TimelineResponse> GetMentionsAsync(int? count = null, long? since = null, long? max = null)
    {
      return GetTimelineAsync("statuses/mentions_timeline.json", count, since, max);
    }

    public Task<TimelineResponse> GetRetweetsByMeAsync(int? count = null, long? since = null, long? max = null)
    {
      return GetTimelineAsync("statuses/retweeted_by_me.json", count, since, max);
    }

    public Task<TimelineResponse> GetRetweetsToMeAsync(int? count = null, long? since = null, long? max = null)
    {
      return GetTimelineAsync("statuses/retweeted_to_me.json", count, since, max);
    }

    public Task<TimelineResponse> GetRetweetsOfMeAsync(int? count = null, long? since = null, long? max = null)
    {
      return GetTimelineAsync("statuses/retweets_of_me.json", count, since, max);
    }
  }
}
