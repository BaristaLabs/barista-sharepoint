namespace Barista.DocumentStore
{
  using System;
  using System.Collections.Generic;

  public interface IFolderCapableDocumentStore
  {
    #region Folders
    
    #endregion

    #region Entity Folders

    /// <summary>
    /// Creates a new entity in the document store, contained in the specified container in the specified folder and namespace.
    /// </summary>
    /// <param name="containerTitle">The container title. Required.</param>
    /// <param name="path">The path. Optional.</param>
    /// <param name="title">The title of the entity. Optional.</param>
    /// <param name="namespace">The namespace of the entity. Optional.</param>
    /// <param name="data">The data to store with the entity. Optiona.</param>
    /// <returns></returns>
    IEntity CreateEntity(string containerTitle, string path, string title, string @namespace, string data);

    /// <summary>
    /// Gets the specified untyped entity in the specified path.
    /// </summary>
    /// <remarks>
    /// Restricts entity retrieval to those entities that are in the specified path.
    /// </remarks>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="entityId">The entity id.</param>
    /// <param name="path"></param>
    /// <returns></returns>
    IEntity GetEntity(string containerTitle, Guid entityId, string path);

    /// <summary>
    /// Gets the specified untyped entity in the specified path without populating the data field.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="entityId">The entity id.</param>
    /// <param name="path"></param>
    /// <returns>An entity that does not have its Data property populated.</returns>
    IEntity GetEntityLight(string containerTitle, Guid entityId, string path);

    /// <summary>
    /// Imports an entity from a previous export.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="path">The path.</param>
    /// <param name="entityId">The entity id.</param>
    /// <param name="namespace">The @namespace.</param>
    /// <param name="archiveData">The archive data.</param>
    /// <returns></returns>
    IEntity ImportEntity(string containerTitle, string path, Guid entityId, string @namespace, Byte[] archiveData);

    /// <summary>
    /// Lists all entities in the specified path in the container with the specified criteria.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="path">The path.</param>
    /// <param name="criteria">The criteria.</param>
    /// <returns></returns>
    IList<IEntity> ListEntities(string containerTitle, string path, EntityFilterCriteria criteria);

    /// <summary>
    /// Returns the total number of entities that correspond to the specified criteria.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="path">The path.</param>
    /// <param name="criteria">The criteria.</param>
    /// <returns></returns>
    int CountEntities(string containerTitle, string path, EntityFilterCriteria criteria);

    /// <summary>
    /// Moves the specified entity to the specified destination folder.
    /// </summary>
    /// <param name="containerTitle"></param>
    /// <param name="entityId">The entity id.</param>
    /// <param name="destinationPath">The destination path.</param>
    bool MoveEntity(string containerTitle, Guid entityId, string destinationPath);
    #endregion
  }
}
