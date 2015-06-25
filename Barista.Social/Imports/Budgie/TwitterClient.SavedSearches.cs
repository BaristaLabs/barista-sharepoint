namespace Barista.Social.Imports.Budgie
{
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using Barista.Social.Imports.Budgie.Extensions;
  using Barista.Social.Imports.Budgie.Json;
  using Newtonsoft.Json;

  partial class TwitterClient
  {
    public Task<ITwitterResponse<IEnumerable<TwitterSavedSearch>>> GetSavedSearchesAsync()
    {
      return HttpGetAsync("saved_searches/list.json").RespondWith<IEnumerable<TwitterSavedSearch>>(content =>
          JsonConvert.DeserializeObject<IEnumerable<JsonSavedSearch>>(content, new TwitterizerDateConverter())
              .Select(j => (TwitterSavedSearch)j)
              .ToList());
    }

    public Task<ITwitterResponse<TwitterSavedSearch>> CreateSavedSearchAsync(string query)
    {
      return HttpPostAsync("saved_searches/create.json", "query=" + query.ToRfc3986Encoded()).RespondWith<TwitterSavedSearch>(content =>
          JsonConvert.DeserializeObject<JsonSavedSearch>(content, new TwitterizerDateConverter()));
    }

    public Task<ITwitterResponse<TwitterSavedSearch>> DeleteSavedSearchAsync(long id)
    {
      return HttpPostAsync("saved_searches/destroy/" + id + ".json").RespondWith<TwitterSavedSearch>(content =>
          JsonConvert.DeserializeObject<JsonSavedSearch>(content, new TwitterizerDateConverter()));
    }
  }
}
