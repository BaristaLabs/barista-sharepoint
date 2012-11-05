using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OFS.OrcaDB.Core
{
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
