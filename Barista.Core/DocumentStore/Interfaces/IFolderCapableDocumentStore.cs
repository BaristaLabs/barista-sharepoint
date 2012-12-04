namespace Barista.DocumentStore
{
  using System;
  using System.Collections.Generic;

  public interface IFolderCapableDocumentStore
  {
    #region Folders
    /// <summary>
    /// Creates the folder with the specified path in the container.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="path">The path.</param>
    /// <returns></returns>
    Folder CreateFolder(string containerTitle, string path);

    /// <summary>
    /// Deletes the folder with the specified path from the container.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="path">The path.</param>
    void DeleteFolder(string containerTitle, string path);

    /// <summary>
    /// Gets the folder with the specified path that is contained in the container.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="path">The path.</param>
    /// <returns></returns>
    Folder GetFolder(string containerTitle, string path);

    /// <summary>
    /// Lists all folders contained in the specified container.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="path">The path.</param>
    /// <returns></returns>
    IList<Folder> ListAllFolders(string containerTitle, string path);

    /// <summary>
    /// Lists the folders at the specified level in the container.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="path">The path.</param>
    /// <returns></returns>
    IList<Folder> ListFolders(string containerTitle, string path);

    /// <summary>
    /// Renames the specified folder to the new folder name.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="path">The path.</param>
    /// <param name="newFolderName">New name of the folder.</param>
    /// <returns></returns>
    Folder RenameFolder(string containerTitle, string path, string newFolderName);
    #endregion

    #region Entity Folders
    /// <summary>
    /// Creates a new entity in the document store, contained in the specified container in the specified folder and namespace.
    /// </summary>
    /// <param name="containerTitle">The container title. Required.</param>
    /// <param name="path">The path. Optional.</param>
    /// <param name="namespace">The namespace of the entity. Optional.</param>
    /// <param name="data">The data to store with the entity. Optiona.</param>
    /// <returns></returns>
    Entity CreateEntity(string containerTitle, string path, string @namespace, string data);

    /// <summary>
    /// Gets the specified untyped entity in the specified path.
    /// </summary>
    /// <remarks>
    /// Restricts entity retrieval to those entities that are in the specified path.
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="entityId">The entity id.</param>
    /// <returns></returns>
    Entity GetEntity(string containerTitle, Guid entityId, string path);

    /// <summary>
    /// Imports an entity from a previous export.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="path">The path.</param>
    /// <param name="entityId">The entity id.</param>
    /// <param name="namespace">The @namespace.</param>
    /// <param name="archiveData">The archive data.</param>
    /// <returns></returns>
    Entity ImportEntity(string containerTitle, string path, Guid entityId, string @namespace, Byte[] archiveData);

    /// <summary>
    /// Lists the entities.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="path">The path.</param>
    /// <param name="criteria">The criteria.</param>
    /// <returns></returns>
    IList<Entity> ListEntities(string containerTitle, string path, EntityFilterCriteria criteria);

    /// <summary>
    /// Moves the specified entity to the specified destination folder.
    /// </summary>
    /// <param name="entityId">The entity id.</param>
    /// <param name="destinationPath">The destination path.</param>
    bool MoveEntity(string containerTitle, Guid entityId, string destinationPath);
    #endregion
  }
}
