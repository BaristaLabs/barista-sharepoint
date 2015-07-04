namespace Barista.Social.Imports.Budgie
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using Barista.Extensions;
  using Barista.Social.Imports.Budgie.Extensions;
  using Barista.Social.Imports.Budgie.Json;
  using System.Linq;
  using System.Collections.Concurrent;
  using System.Net;
  using Newtonsoft.Json;
  using System;

  using ListsResponse = ITwitterResponse<System.Collections.Generic.IEnumerable<TwitterList>>;

  partial class TwitterClient
  {
    protected Task<ListsResponse> GetListsByCursor(string relativeUri)
    {
      var items = new ConcurrentBag<TwitterList>();

      var rel = relativeUri + (relativeUri.Contains("?") ? "&" : "?") + "cursor=";

      return HttpGetAsync(rel + "-1").ContinueWith<ListsResponse>(t =>
      {
        while (true)
        {
          var result = t.ToTwitterResponse<IEnumerable<TwitterList>>(items);
          if (result.StatusCode != HttpStatusCode.OK) return result;

          var listCursor = JsonConvert.DeserializeObject<JsonListCursor>(result.RawContent, new TwitterizerDateConverter());

          foreach (var item in listCursor.lists)
            items.Add((TwitterList)item);

          if (listCursor.next_cursor == 0) return result;

          t = HttpGetAsync(rel + listCursor.next_cursor);
          t.Wait();
        }
      });
    }

    public Task<ListsResponse> GetListsAsync(string screenName = null)
    {
      var relativeUri = "lists/list.json";
      if (screenName != null) relativeUri += "?screen_name=" + screenName.ToRfc3986Encoded();

      return HttpGetAsync(relativeUri).ContinueWith<ListsResponse>(t =>
      {
        var result = t.ToTwitterResponse<IEnumerable<TwitterList>>();
        if (result.StatusCode != HttpStatusCode.OK) return result;

        result.Result = JsonConvert.DeserializeObject<IEnumerable<JsonList>>(result.RawContent, new TwitterizerDateConverter())
            .Select(j => (TwitterList)j)
            .ToList();
        return result;
      });
    }

    public Task<ITwitterResponse<IEnumerable<TwitterList>>> GetListMembershipsAsync()
    {
      return GetListsByCursor("lists/memberships.json");
    }

    public Task<ListsResponse> GetUserListsAsync(string screenName)
    {
        if (screenName.IsNullOrWhiteSpace()) throw new ArgumentException("screenName must be specified", "screenName");

      var relativeUri = "lists.json?screen_name=" + screenName.ToRfc3986Encoded();

      return GetListsByCursor(relativeUri);
    }

    public virtual Task<ListsResponse> GetListMembershipsAsync(string screenName)
    {
        if (screenName.IsNullOrWhiteSpace()) throw new ArgumentException("screenName must be specified", "screenName");

      var relativeUri = "lists/memberships.json?screen_name=" + screenName.ToRfc3986Encoded();

      return GetListsByCursor(relativeUri);
    }

    public Task<ITwitterResponse<IEnumerable<TwitterStatus>>> GetListTimelineAsync(long id, int? count = null, long? since = null, long? max = null)
    {
      return GetTimelineAsync("lists/statuses.json?list_id=" + id, count, since, max);
    }
  }
}


