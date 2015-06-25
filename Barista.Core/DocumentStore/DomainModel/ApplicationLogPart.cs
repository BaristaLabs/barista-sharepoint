namespace Barista.DocumentStore
{
  using System.Collections.Generic;

  class ApplicationLogPart
  {
    public ApplicationLogPart()
    {
      this.Entries = new List<ApplicationLogEntry>();
    }

    public IList<ApplicationLogEntry> Entries
    {
      get;
      set;
    }
  }
}
