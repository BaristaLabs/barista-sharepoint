namespace Barista.DocumentStore
{
  using System;
  using System.Collections.Generic;
  using System.IO;

  /// <summary>
  /// Represents the interface of a Document Store.
  /// </summary>
  public interface IDocumentStore
  {
    #region Containers
    /// <summary>
    /// Creates a container in the document store with the specified title and description.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="description">The description.</param>
    /// <returns></returns>
    IContainer CreateContainer(string containerTitle, string description);

    /// <summary>
    /// Deletes the container with the specified title from the document store.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    void DeleteContainer(string containerTitle);

    /// <summary>
    /// Gets the container with the specified title from the document store.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <returns></returns>
    IContainer GetContainer(string containerTitle);

    /// <summary>
    /// Lists all containers contained in the document store.
    /// </summary>
    /// <returns></returns>
    IList<IContainer> ListContainers();

    /// <summary>
    /// Updates the container in the document store with new values.
    /// </summary>
    /// <param name="container">The container.</param>
    /// <returns></returns>
    bool UpdateContainer(IContainer container);
    #endregion

    #region Entities

    /// <summary>
    /// Creates a new entity in the document store, contained in the specified container in the specified namespace.
    /// </summary>
    /// <param name="containerTitle">The container title. Required.</param>
    /// <param name="title">The title of the entity. Optiona.</param>
    /// <param name="namespace">The namespace of the entity. Optional.</param>
    /// <param name="data">The data to store with the entity. Optional.</param>
    /// <returns></returns>
    IEntity CreateEntity(string containerTitle, string title, string @namespace, string data);

    /// <summary>
    /// Deletes the specified entity from the specified container.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="entityId">The entity id.</param>
    /// <returns></returns>
    bool DeleteEntity(string containerTitle, Guid entityId);

    /// <summary>
    /// Returns a stream that contains an export of the entity.
    /// </summary>
    /// <remarks>
    /// The export format is determined by the implementing document store.
    /// </remarks>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="entityId">The entity id.</param>
    /// <returns></returns>
    Stream ExportEntity(string containerTitle, Guid entityId);

    /// <summary>
    /// Gets the specified untyped entity.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="entityId">The entity id.</param>
    /// <returns></returns>
    IEntity GetEntity(string containerTitle, Guid entityId);

    /// <summary>
    /// Gets the specified untyped entity without populating the data field.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="entityId">The entity id.</param>
    /// <returns>An entity that does not have its Data property populated.</returns>
    IEntity GetEntityLight(string containerTitle, Guid entityId);

    /// <summary>
    /// Imports an entity from a previous export.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="entityId">The entity id.</param>
    /// <param name="namespace">The @namespace.</param>
    /// <param name="archiveData">The archive data.</param>
    /// <returns></returns>
    IEntity ImportEntity(string containerTitle, Guid entityId, string @namespace, Byte[] archiveData);

    /// <summary>
    /// Lists all entities contained in the container with the specified criteria.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    IList<IEntity> ListEntities(string containerTitle, EntityFilterCriteria criteria);

    /// <summary>
    /// Returns the total number of entities contained in the container with the specified criteria.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    int CountEntities(string containerTitle, EntityFilterCriteria criteria);

    /// <summary>
    /// Updates the entity.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="entityId"></param>
    /// <param name="title"></param>
    /// <param name="description"></param>
    /// <param name="namespace"></param>
    /// <returns></returns>
    IEntity UpdateEntity(string containerTitle, Guid entityId, string title, string description, string @namespace);

    /// <summary>
    /// Updates the data contained in the specified entity.
    /// </summary>
    /// <param name="containerTitle"></param>
    /// <param name="entityId"></param>
    /// <param name="eTag"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    IEntity UpdateEntityData(string containerTitle, Guid entityId, string eTag, string data);
    #endregion
  }
}
