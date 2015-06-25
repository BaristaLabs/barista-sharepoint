namespace Barista.Social.Imports.Budgie.Json
{
  using System;
  using System.Collections.Generic;
  using System.Linq;

  internal class JsonSearchResults
  {
    public IEnumerable<JsonStatus> statuses { get; set; }
    public JsonSearchMetadata search_metadata { get; set; }

    public static explicit operator TwitterSearchResults(JsonSearchResults j)
    {
      return new TwitterSearchResults
      {
        CompletedIn = TimeSpan.FromSeconds(j.search_metadata.completed_in),
        MaxId = j.search_metadata.max_id,
        SinceId = j.search_metadata.since_id,
        Query = j.search_metadata.query,
        Results = j.statuses.Select(r =>
        {
          var s = (TwitterStatus)r;
          s.IsSearchResult = true;
          return s;
        }).ToList(),
      };
    }
  }

  internal class JsonSearchMetadata
  {
    public long max_id { get; set; }
    public long since_id { get; set; }
    public double completed_in { get; set; }
    public string query { get; set; }
  }

}
