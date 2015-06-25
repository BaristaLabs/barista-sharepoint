namespace Barista.Social.Imports.Budgie.Json
{
  using System.Collections.Generic;

  internal class JsonListCursor
  {
    public long previous_cursor { get; set; }
    public long next_cursor { get; set; }
    public IEnumerable<JsonList> lists { get; set; }
  }
}
