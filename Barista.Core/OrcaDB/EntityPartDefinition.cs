using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace OFS.OrcaDB.Core
{
  public class EntityPartDefinition
  {
    public string EntityPartName
    {
      get;
      set;
    }

    public Type EntityPartType
    {
      get;
      set;
    }
  }

  public class EntityPartDefinitionCollection : KeyedCollection<string, EntityPartDefinition>
  {
    protected override string GetKeyForItem(EntityPartDefinition item)
    {
      return item.EntityPartName;
    }
  }
}
