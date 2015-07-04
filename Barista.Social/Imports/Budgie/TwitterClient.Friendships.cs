namespace Barista.Social.Imports.Budgie
{
    using Barista.Extensions;
    using Barista.Social.Imports.Budgie.Extensions;
  using Barista.Social.Imports.Budgie.Json;
  using Newtonsoft.Json;
  using System;
  using System.Collections.Concurrent;
  using System.Collections.Generic;
  using System.Net;
  using System.Threading.Tasks;

  using IdsResponse = ITwitterResponse<System.Collections.Generic.IEnumerable<long>>;

  partial class TwitterClient
  {
    protected Task<IdsResponse> GetIdsByCursor(string relativeUri)
    {
      var items = new ConcurrentBag<long>();

      var rel = relativeUri + (relativeUri.Contains("?") ? "&" : "?") + "cursor=";

      return HttpGetAsync(rel + "-1").ContinueWith<IdsResponse>(t =>
      {
        while (true)
        {
          var result = t.ToTwitterResponse<IEnumerable<long>>(items);
          if (result.StatusCode != HttpStatusCode.OK) return result;

          var idCursor = JsonConvert.DeserializeObject<JsonIdCursor>(result.RawContent);

          foreach (var id in idCursor.ids)
            items.Add(id);

          if (idCursor.next_cursor == 0) return result;

          t = HttpGetAsync(rel + idCursor.next_cursor);
          t.Wait();
        }
      });
    }

    public Task<IdsResponse> GetFollowerIdsAsync(string screenName = null)
    {
      var relativeUri = "followers/ids.json";
      if (!screenName.IsNullOrWhiteSpace())
      {
        relativeUri += "?screen_name=" + screenName.ToRfc3986Encoded();
      }

      return GetIdsByCursor(relativeUri);
    }

    public Task<IdsResponse> GetFriendIdsAsync(string screenName = null)
    {
      var relativeUri = "friends/ids.json";
      if (!screenName.IsNullOrWhiteSpace())
      {
        relativeUri += "?screen_name" + screenName.ToRfc3986Encoded();
      }
      return GetIdsByCursor(relativeUri);
    }

    public Task<ITwitterResponse<IEnumerable<TwitterUser>>> GetFriendsAsync()
    {
      return GetUsersByCursor("friends/list.json");
    }

    public Task<ITwitterResponse<TwitterUser>> FollowAsync(string screenName)
    {
      return HttpPostAsync("friendships/create.json", "screen_name=" + screenName.ToRfc3986Encoded()).RespondWith<TwitterUser>(content =>
          JsonConvert.DeserializeObject<JsonUser>(content, new TwitterizerDateConverter()));
    }

    public Task<ITwitterResponse<TwitterUser>> UnfollowAsync(string screenName)
    {
      return HttpPostAsync("friendships/destroy.json", "screen_name=" + screenName.ToRfc3986Encoded()).RespondWith<TwitterUser>(content =>
          JsonConvert.DeserializeObject<JsonUser>(content, new TwitterizerDateConverter()));
    }

    public Task<ITwitterResponse<TwitterFriendship>> GetFriendshipAsync(string sourceScreenName, string targetScreenName)
    {
      return HttpGetAsync("friendships/show.json?source_screen_name=" + sourceScreenName.ToRfc3986Encoded() + "&target_screen_name=" + targetScreenName.ToRfc3986Encoded()).RespondWith<TwitterFriendship>(content =>
          JsonConvert.DeserializeObject<JsonFriendship>(content, new TwitterizerDateConverter()));
    }
  }
}
