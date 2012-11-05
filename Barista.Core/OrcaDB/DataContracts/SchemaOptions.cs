using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OFS.OrcaDB.Core
{
  [Flags]
  public enum SchemaOptions
  {
    PreventInvalidData = 0,
    ValidateExisting = 1,
    DeleteExistingNonconformant = 2,
    ///Others...
  }
}
