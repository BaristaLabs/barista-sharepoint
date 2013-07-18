namespace Barista.Social.Imports.Budgie.Json
{
  using System.Collections.Generic;

  internal class JsonIdCursor
  {
    public long previous_cursor { get; set; }
    public long next_cursor { get; set; }
    public IEnumerable<long> ids { get; set; }
  }

  internal class JsonUserCursor
  {
    public long previous_cursor { get; set; }
    public long next_cursor { get; set; }
    public IEnumerable<JsonUser> users { get; set; }
  }
}
