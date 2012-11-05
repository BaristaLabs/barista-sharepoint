using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OFS.OrcaDB.Core
{
  public sealed class EntityFilterCriteria
  {
    public string Namespace
    {
      get;
      set;
    }

    public uint? Skip
    {
      get;
      set;
    }

    public uint? Top
    {
      get;
      set;
    }

    public IDictionary<string, string> FieldValues
    {
      get;
      set;
    }
  }
}
