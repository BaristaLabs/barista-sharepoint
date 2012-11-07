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
    /// <typeparam name="T"></typeparam>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="entityId">The entity id.</param>
    /// <param name="partName">Name of the part.</param>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    EntityPart<T> CreateEntityPart<T>(string containerTitle, Guid entityId, string partName, T value);

    /// <summary>
    /// Creates the entity part.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="entityId">The entity id.</param>
    /// <param name="partName">Name of the part.</param>
    /// <param name="category">The category.</param>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    EntityPart<T> CreateEntityPart<T>(string containerTitle, Guid entityId, string partName, string category, T value);

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
    /// Gets the entity part.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="entityId">The entity id.</param>
    /// <param name="partName">Name of the part.</param>
    /// <returns></returns>
    EntityPart<T> GetEntityPart<T>(string containerTitle, Guid entityId, string partName);

    /// <summary>
    /// Gets the ETag of the specified entity part
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="entityId">The entity id.</param>
    /// <returns></returns>
    string GetEntityPartETag(string containerTitle, Guid entityId, string partName);

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
    /// Tries the get entity part.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="entityId">The entity id.</param>
    /// <param name="partName">Name of the part.</param>
    /// <param name="entityPart">The entity part.</param>
    /// <returns></returns>
    bool TryGetEntityPart<T>(string containerTitle, Guid entityId, string partName, out EntityPart<T> entityPart);

    /// <summary>
    /// Updates the entity part.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="entityId">The entity id.</param>
    /// <param name="partName">Name of the part.</param>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    bool UpdateEntityPart<T>(string containerTitle, Guid entityId, string partName, T value);

    /// <summary>
    /// Updates the entity part.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="entityId">The entity id.</param>
    /// <param name="entityPart">The entity part.</param>
    /// <returns></returns>
    bool UpdateEntityPart(string containerTitle, Guid entityId, EntityPart entityPart);

    /// <summary>
    /// Lists the entity parts associated with the specified entity in the specified container.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="entityId">The entity id.</param>
    /// <returns></returns>
    IList<EntityPart> ListEntityParts(string containerTitle, Guid entityId);
    #endregion
  }
}
