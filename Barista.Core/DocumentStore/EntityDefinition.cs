namespace Barista.DocumentStore
{
  using System;
  using System.Collections.ObjectModel;

  public class EntityDefinition
  {
    public EntityDefinition()
    {
      this.EntityPartDefinitions = new EntityPartDefinitionCollection();
      this.IndexDefinitions = new IndexDefinitionCollection();
    }

    public string EntityNamespace
    {
      get;
      set;
    }

    public Type EntityType
    {
      get;
      set;
    }

    public EntityPartDefinitionCollection EntityPartDefinitions
    {
      get;
      set;
    }

    public IndexDefinitionCollection IndexDefinitions
    {
      get;
      set;
    }
  }

  public class EntityDefinitionCollection : KeyedCollection<string, EntityDefinition>
  {
    protected override string GetKeyForItem(EntityDefinition item)
    {
      return item.EntityNamespace;
    }
  }
}
