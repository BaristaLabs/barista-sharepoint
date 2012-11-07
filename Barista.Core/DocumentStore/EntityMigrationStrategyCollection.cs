using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Barista.DocumentStore
{
  public class EntityMigrationStrategyCollection : KeyedCollection<string, IEntityMigrationStrategy>
  {
    protected override string GetKeyForItem(IEntityMigrationStrategy item)
    {
      return item.FromNamespace;
    }
  }
}
