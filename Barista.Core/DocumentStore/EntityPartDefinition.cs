namespace Barista.DocumentStore
{
  using System;
  using System.Collections.ObjectModel;

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
