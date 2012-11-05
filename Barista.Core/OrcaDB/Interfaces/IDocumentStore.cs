﻿namespace OFS.OrcaDB.Core
{
  using System;
  using System.Collections.Generic;
  using System.IO;

  /// <summary>
  /// Represents the interface to a Document Store.
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
    Container CreateContainer(string containerTitle, string description);

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
    Container GetContainer(string containerTitle);

    /// <summary>
    /// Lists all containers contained in the document store.
    /// </summary>
    /// <returns></returns>
    IList<Container> ListContainers();

    /// <summary>
    /// Updates the container in the document store with new values.
    /// </summary>
    /// <param name="container">The container.</param>
    /// <returns></returns>
    bool UpdateContainer(Container container);
    #endregion

    #region Entities
    /// <summary>
    /// Creates a new entity in the document store, contained in the specified container in the specified namespace.
    /// </summary>
    /// <param name="containerTitle">The container title. Required.</param>
    /// <param name="namespace">The namespace of the entity. Optional.</param>
    /// <param name="data">The data to store with the entity. Optional.</param>
    /// <returns></returns>
    Entity CreateEntity(string containerTitle, string @namespace, string data);

    /// <summary>
    /// Creates a new entity in the document store, contained in the specified container in the specified namespace.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="namespace">The @namespace.</param>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    Entity<T> CreateEntity<T>(string containerTitle, string @namespace, T value);

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
    Entity GetEntity(string containerTitle, Guid entityId);

    /// <summary>
    /// Gets the specified typed entity.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="entityId">The entity id.</param>
    /// <returns></returns>
    Entity<T> GetEntity<T>(string containerTitle, Guid entityId);

    /// <summary>
    /// Gets the ETag of the specified Entities' contents.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="entityId">The entity id.</param>
    /// <returns></returns>
    string GetEntityContentsETag(string containerTitle, Guid entityId);

    /// <summary>
    /// Imports an entity from a previous export.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="entityId">The entity id.</param>
    /// <param name="namespace">The @namespace.</param>
    /// <param name="archiveData">The archive data.</param>
    /// <returns></returns>
    Entity ImportEntity(string containerTitle, Guid entityId, string @namespace, Byte[] archiveData);

    /// <summary>
    /// Lists all entities contained in the container with the specified namespace.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="namespace">The @namespace.</param>
    /// <returns></returns>
    IList<Entity> ListEntities(string containerTitle, EntityFilterCriteria criteria);

    /// <summary>
    /// Lists all entities contained in the container with the specified namespace, converting the data to the specified type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="namespace">The @namespace.</param>
    /// <returns></returns>
    IList<Entity<T>> ListEntities<T>(string containerTitle, EntityFilterCriteria criteria);

    /// <summary>
    /// Updates the entity.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="entity">The entity.</param>
    /// <returns></returns>
    bool UpdateEntity(string containerTitle, Entity entity);

    /// <summary>
    /// Updates the entity.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="entityId">The entity id.</param>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    bool UpdateEntity<T>(string containerTitle, Guid entityId, T value);
    #endregion
  }
}
