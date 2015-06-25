namespace Barista.Social.Imports.Budgie
{
  using Barista.Social.Imports.Budgie.Extensions;
  using Barista.Social.Imports.Budgie.Json;
  using Newtonsoft.Json;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using StatusResponse = ITwitterResponse<TwitterStatus>;

  partial class TwitterClient
  {
    public Task<ITwitterResponse<TwitterStatus>> GetStatusAsync(long statusId)
    {
      var relativeUri = "statuses/show/" + statusId + ".json?include_entities=true";

      return HttpGetAsync(relativeUri).RespondWith<TwitterStatus>(content =>
          JsonConvert.DeserializeObject<JsonStatus>(content, new TwitterizerDateConverter()));
    }

    public Task<ITwitterResponse<IEnumerable<TwitterUser>>> GetRetweetersAsync(long statusId, int? count = null)
    {
      var relativeUri = "statuses/" + statusId + "/retweeted_by.json";
      if (count.HasValue) relativeUri += "?count=" + count;

      return HttpGetAsync(relativeUri).RespondWith<IEnumerable<TwitterUser>>(content =>
          JsonConvert.DeserializeObject<IEnumerable<JsonUser>>(content, new TwitterizerDateConverter())
              .Select(j => (TwitterUser)j)
              .ToList());
    }

    public Task<StatusResponse> DeleteStatusAsync(long id)
    {
      var relativeUri = "statuses/destroy/" + id + ".json?include_entities=true";

      return HttpPostAsync(relativeUri).RespondWith<TwitterStatus>(content =>
          JsonConvert.DeserializeObject<JsonStatus>(content, new TwitterizerDateConverter()));
    }

    public Task<StatusResponse> RetweetAsync(long id)
    {
      var relativeUri = "statuses/retweet/" + id + ".json?include_entities=true";

      return HttpPostAsync(relativeUri).RespondWith<TwitterStatus>(content =>
          JsonConvert.DeserializeObject<JsonStatus>(content, new TwitterizerDateConverter()));
    }

    public Task<StatusResponse> PostAsync(string text)
    {
      var relativeUri = "statuses/update.json";
      var parms = "include_entities=true&status=" + text.ToRfc3986Encoded();

      return HttpPostAsync(relativeUri, parms).RespondWith<TwitterStatus>(content =>
          JsonConvert.DeserializeObject<JsonStatus>(content, new TwitterizerDateConverter()));
    }

    public Task<StatusResponse> ReplyToAsync(long id, string text)
    {
      var relativeUri = "statuses/update.json";
      var parms = "include_entities=true&in_reply_to_status_id=" + id + "&status=" + text.ToRfc3986Encoded();

      return HttpPostAsync(relativeUri, parms).RespondWith<TwitterStatus>(content =>
          JsonConvert.DeserializeObject<JsonStatus>(content, new TwitterizerDateConverter()));
    }
  }
}
