using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Barista.OrcaDB
{
  public interface IEntityMigrationStrategy
  {
    string FromNamespace
    {
      get;
      set;
    }

    string ToNamespace
    {
      get;
      set;
    }
  }

  /// <summary>
  /// Represents a strategy that will migrate entities and any associated entity parts to a newer version.
  /// </summary>
  public class EntityMigrationStrategy<TDestinationEntityType> : IEntityMigrationStrategy
  {
    /// <summary>
    /// Gets or sets the namespace of the entity that, when retrieved, will invoke the migration.
    /// </summary>
    public string FromNamespace
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the new namespace of the entity after migration.
    /// </summary>
    public string ToNamespace
    {
      get;
      set;
    }

    public Func<JObject, TDestinationEntityType> EntityMigration
    {
      get;
      set;
    }

    public IList<EntityPartMigrationStrategy> EntityPartMigrations
    {
      get;
      set;
    }
  }

  public abstract class EntityPartMigrationStrategy
  {
    public string EntityPartName
    {
      get;
      set;
    }
  }

  public class EntityPartMigrationStrategy<TDestinationEntityPartType> : EntityPartMigrationStrategy
  {
    public Func<JObject, TDestinationEntityPartType> EntityPartMigration
    {
      get;
      set;
    }
  }
}
