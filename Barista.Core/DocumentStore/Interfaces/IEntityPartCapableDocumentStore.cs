namespace Barista.DocumentStore
{
  using System;
  using System.Collections.Generic;

  /// <summary>
  /// Represents a document store extension that provides the ability to store/retrieve Entity Parts -- Subsets of entities that are associated with a parent entity.
  /// </summary>
  /// <remarks>
  /// Extension interfaces, such as this one, allow for implementations of DocumentStores that do not require or expose the full functionality.
  /// </remarks>
  public interface IEntityPartCapableDocumentStore
  {
    #region Entity Parts
    /// <summary>
    /// Creates the entity part.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="entityId">The entity id.</param>
    /// <param name="partName">Name of the part.</param>
    /// <param name="category">The category.</param>
    /// <param name="data">The data.</param>
    /// <returns></returns>
    EntityPart CreateEntityPart(string containerTitle, Guid entityId, string partName, string category, string data);

    /// <summary>
    /// Deletes the entity part.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="entityId">The entity id.</param>
    /// <param name="partName">Name of the part.</param>
    /// <returns></returns>
    bool DeleteEntityPart(string containerTitle, Guid entityId, string partName);

    /// <summary>
    /// Gets the entity part.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="entityId">The entity id.</param>
    /// <param name="partName">Name of the part.</param>
    /// <returns></returns>
    EntityPart GetEntityPart(string containerTitle, Guid entityId, string partName);

    /// <summary>
    /// Lists the entity parts associated with the specified entity in the specified container.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="entityId">The entity id.</param>
    /// <returns></returns>
    IList<EntityPart> ListEntityParts(string containerTitle, Guid entityId);

    /// <summary>
    /// Renames the entity part.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="entityId">The entity id.</param>
    /// <param name="partName">Name of the part.</param>
    /// <param name="newPartName">New name of the part.</param>
    /// <returns></returns>
    bool RenameEntityPart(string containerTitle, Guid entityId, string partName, string newPartName);

    /// <summary>
    /// Updates the entity part.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="entityId">The entity id.</param>
    /// <param name="partName"></param>
    /// <param name="category"></param>
    /// <returns></returns>
    EntityPart UpdateEntityPart(string containerTitle, Guid entityId, string partName, string category);

    /// <summary>
    /// Updates the data portion of the specified entity part.
    /// </summary>
    /// <param name="containerTitle"></param>
    /// <param name="entityId"></param>
    /// <param name="partName"></param>
    /// <param name="eTag"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    EntityPart UpdateEntityPartData(string containerTitle, Guid entityId, string partName, string eTag, string data);
    #endregion
  }
}
