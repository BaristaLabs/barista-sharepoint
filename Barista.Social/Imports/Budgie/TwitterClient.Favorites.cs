namespace Barista.Social.Imports.Budgie
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using Barista.Social.Imports.Budgie.Extensions;
  using Barista.Social.Imports.Budgie.Json;
  using Newtonsoft.Json;

  partial class TwitterClient
  {
    public Task<ITwitterResponse<IEnumerable<TwitterStatus>>> GetFavoritesAsync(int? count = null, long? since = null, long? max = null)
    {
      return GetTimelineAsync("favorites/list.json", count, since, max);
    }

    public Task<ITwitterResponse<TwitterStatus>> FavouriteAsync(long id)
    {
      return HttpPostAsync("favorites/create.json", "id=" + id)
          .RespondWith<TwitterStatus>(content => JsonConvert.DeserializeObject<JsonStatus>(content, new TwitterizerDateConverter()));
    }

    public Task<ITwitterResponse<TwitterStatus>> UnfavouriteAsync(long id)
    {
      return HttpPostAsync("favorites/destroy.json", "id=" + id)
          .RespondWith<TwitterStatus>(content => JsonConvert.DeserializeObject<JsonStatus>(content, new TwitterizerDateConverter()));
    }
  }
}
