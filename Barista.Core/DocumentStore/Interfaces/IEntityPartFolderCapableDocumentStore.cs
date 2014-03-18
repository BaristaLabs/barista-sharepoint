namespace Barista.DocumentStore
{
  using System;
  using System.Collections.Generic;

  public interface IEntityPartFolderCapableDocumentStore
  {
    /// <summary>
    /// Creates the entity part.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="path">The path.</param>
    /// <param name="entityId">The entity id.</param>
    /// <param name="partName">Name of the part.</param>
    /// <param name="category">The category.</param>
    /// <param name="data">The data.</param>
    /// <returns></returns>
    EntityPart CreateEntityPart(string containerTitle, string path, Guid entityId, string partName, string category, string data);

    /// <summary>
    /// Deletes the entity part.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="path">The path.</param>
    /// <param name="entityId">The entity id.</param>
    /// <param name="partName">Name of the part.</param>
    /// <returns></returns>
    bool DeleteEntityPart(string containerTitle, string path, Guid entityId, string partName);

    /// <summary>
    /// Gets the entity part associated with the specified entity in the specified path.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="path">The path.</param>
    /// <param name="entityId">The entity id.</param>
    /// <param name="partName">Name of the part.</param>
    /// <returns></returns>
    EntityPart GetEntityPart(string containerTitle, string path, Guid entityId, string partName);

    /// <summary>
    /// Lists the entity parts associated with the specified entity in the specified container.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="path"></param>
    /// <param name="entityId">The entity id.</param>
    /// <returns></returns>
    IList<EntityPart> ListEntityParts(string containerTitle, string path, Guid entityId);

    /// <summary>
    /// Renames the entity part.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="path">The path.</param>
    /// <param name="entityId">The entity id.</param>
    /// <param name="partName">Name of the part.</param>
    /// <param name="newPartName">New name of the part.</param>
    /// <returns></returns>
    bool RenameEntityPart(string containerTitle, string path, Guid entityId, string partName, string newPartName);

    /// <summary>
    /// Updates the entity part.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="path">The path.</param>
    /// <param name="entityId">The entity id.</param>
    /// <param name="partName"></param>
    /// <param name="category"></param>
    /// <returns></returns>
    EntityPart UpdateEntityPart(string containerTitle, string path, Guid entityId, string partName, string category);

    /// <summary>
    /// Updates the data portion of the specified entity part.
    /// </summary>
    /// <param name="containerTitle"></param>
    /// <param name="path"></param>
    /// <param name="entityId"></param>
    /// <param name="partName"></param>
    /// <param name="eTag"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    EntityPart UpdateEntityPartData(string containerTitle, string path, Guid entityId, string partName, string eTag, string data);
  }
}
