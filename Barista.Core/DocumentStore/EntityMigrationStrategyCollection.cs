namespace Barista.DocumentStore
{
  using System.Collections.ObjectModel;

  public class EntityMigrationStrategyCollection : KeyedCollection<string, IEntityMigrationStrategy>
  {
    protected override string GetKeyForItem(IEntityMigrationStrategy item)
    {
      return item.FromNamespace;
    }
  }
}
