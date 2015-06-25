namespace Barista.Social.Imports.Budgie
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using Barista.Social.Imports.Budgie.Extensions;
  using Barista.Social.Imports.Budgie.Json;
  using Newtonsoft.Json;
  using System.Collections.Concurrent;
  using System.Net;

  using UsersResponse = ITwitterResponse<System.Collections.Generic.IEnumerable<TwitterUser>>;

  partial class TwitterClient
  {
    protected Task<UsersResponse> GetUsersByCursor(string relativeUri)
    {
      var items = new ConcurrentBag<TwitterUser>();

      var rel = relativeUri + (relativeUri.Contains("?") ? "&" : "?") + "cursor=";

      return HttpGetAsync(rel + "-1").ContinueWith<UsersResponse>(t =>
      {
        while (true)
        {
          var result = t.ToTwitterResponse<IEnumerable<TwitterUser>>(items);
          if (result.StatusCode != HttpStatusCode.OK) return result;

          var userCursor = JsonConvert.DeserializeObject<JsonUserCursor>(result.RawContent, new TwitterizerDateConverter());

          foreach (var id in userCursor.users)
            items.Add(id);

          if (userCursor.next_cursor == 0) return result;

          t = HttpGetAsync(rel + userCursor.next_cursor);
          t.Wait();
        }
      });
    }

    public Task<ITwitterResponse<IEnumerable<TwitterUser>>> LookupUsersAsync(IEnumerable<long> ids)
    {
      var count = ids.Count();
      var users = new ConcurrentBag<TwitterUser>();
      var page = 0;
      var pageSize = 99;

      var content = "user_id=" + String.Join(",", ids.Take(pageSize)).ToRfc3986Encoded();

      return HttpPostAsync("users/lookup.json", content).ContinueWith<ITwitterResponse<IEnumerable<TwitterUser>>>(t =>
      {
        while (true)
        {
          var result = t.ToTwitterResponse<IEnumerable<TwitterUser>>(users);
          if (result.StatusCode != HttpStatusCode.OK) return result;

          foreach (var u in JsonConvert.DeserializeObject<IEnumerable<JsonUser>>(result.RawContent, new TwitterizerDateConverter()))
          {
            users.Add((TwitterUser)u);
          }

          page++;

          if (count < page * 100) return result;

          content = "user_id=" + String.Join(",", ids.Skip(page * pageSize).Take(pageSize)).ToRfc3986Encoded();
          t = HttpPostAsync("users/lookup.json", content);
          t.Wait();
        }
      });
    }

    public Task<ITwitterResponse<TwitterUser>> GetUserAsync(string screenName)
    {
      if (string.IsNullOrWhiteSpace(screenName)) throw new ArgumentException("screenName must be specified", "screenName");

      return HttpGetAsync("users/show.json?screen_name=" + screenName.ToRfc3986Encoded()).RespondWith<TwitterUser>(content =>
          JsonConvert.DeserializeObject<JsonUser>(content, new TwitterizerDateConverter()));
    }

    public Task<ITwitterResponse<IEnumerable<TwitterUser>>> FindUsersAsync(string query, int? count = null)
    {
      return HttpGetAsync("users/search.json?per_page=" + (count ?? DefaultPageSize)).RespondWith<IEnumerable<TwitterUser>>(content =>
          JsonConvert.DeserializeObject<IEnumerable<JsonUser>>(content, new TwitterizerDateConverter())
              .Select(u => (TwitterUser)u)
              .ToList());
    }

    public Task<ITwitterResponse<TwitterUser>> ReportSpammerAsync(string screenName)
    {
      return HttpPostAsync("report_spam.json", "screen_name=" + screenName.ToRfc3986Encoded()).RespondWith<TwitterUser>(content =>
          JsonConvert.DeserializeObject<JsonUser>(content, new TwitterizerDateConverter()));
    }
  }
}
