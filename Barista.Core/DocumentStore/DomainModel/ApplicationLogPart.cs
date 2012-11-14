﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Barista.DocumentStore
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