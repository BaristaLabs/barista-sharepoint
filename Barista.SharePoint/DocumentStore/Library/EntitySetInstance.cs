namespace Barista.SharePoint.DocumentStore.Library
{
  using Barista.DocumentStore;
  using Jurassic;
  using Jurassic.Library;
  using System;
  using System.Collections.Generic;
  using System.Linq;

  [Serializable]
  public class EntitySetInstance : ObjectInstance
  {
    private readonly Entity m_entity;
    private readonly IList<EntityPart> m_entityParts;

    public EntitySetInstance(ScriptEngine engine, Entity entity, IList<EntityPart> entityParts)
      : base(engine)
    {
      if (entity == null)
        throw new ArgumentNullException("entity");

      if (entityParts == null)
        throw new ArgumentNullException("entityParts");

      m_entity = entity;
      m_entityParts = entityParts;

      this.PopulateFields();
      this.PopulateFunctions();
    }

    [JSProperty(Name = "entity")]
    public EntityInstance Entity
    {
      get
      {
        return new EntityInstance(this.Engine, m_entity);
      }
    }

    [JSProperty(Name = "entityParts")]
    public ObjectInstance EntityParts
    {
      get
      {
        var result = this.Engine.Object.Construct();
        foreach (var entityPart in m_entityParts.OrderBy(ep => ep.Name))
        {
          result.SetPropertyValue(entityPart.Name, new EntityPartInstance(this.Engine, entityPart), false);
        }
        return result;
      }
    }
  }
}
