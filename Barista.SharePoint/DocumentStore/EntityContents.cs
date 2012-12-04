namespace Barista.SharePoint.DocumentStore
{
  using Barista.DocumentStore;
  using System;
  using System.Collections.Generic;

  /// <summary>
  /// Represents an Entity Contents Entity Part.
  /// </summary>
  public class EntityContents
  {
    public EntityContents()
    {
      this.EntityParts = new Dictionary<string, EntityPart>();
    }

    public Entity Entity
    {
      get;
      set;
    }

    public Dictionary<String, EntityPart> EntityParts
    {
      get;
      set;
    }
  }
}
