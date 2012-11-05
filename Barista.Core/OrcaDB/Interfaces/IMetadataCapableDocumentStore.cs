namespace OFS.OrcaDB.Core
{
  using System;
  using System.Collections.Generic;

  public interface IMetadataCapableDocumentStore
  {
    #region Metadata
    /// <summary>
    /// Gets the attachment metadata.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="entityId">The entity id.</param>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="key">The key.</param>
    /// <returns></returns>
    string GetAttachmentMetadata(string containerTitle, Guid entityId, string fileName, string key);

    /// <summary>
    /// Gets the container metadata.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="key">The key.</param>
    /// <returns></returns>
    string GetContainerMetadata(string containerTitle, string key);

    /// <summary>
    /// Gets the entity metadata.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="entityId">The entity id.</param>
    /// <param name="key">The key.</param>
    /// <returns></returns>
    string GetEntityMetadata(string containerTitle, Guid entityId, string key);

    /// <summary>
    /// Gets the entity part metadata.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="entityId">The entity id.</param>
    /// <param name="partName">Name of the part.</param>
    /// <param name="key">The key.</param>
    /// <returns></returns>
    string GetEntityPartMetadata(string containerTitle, Guid entityId, string partName, string key);

    /// <summary>
    /// Lists the attachment metadata.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="entityId">The entity id.</param>
    /// <param name="fileName">Name of the file.</param>
    /// <returns></returns>
    IDictionary<string, string> ListAttachmentMetadata(string containerTitle, Guid entityId, string fileName);

    /// <summary>
    /// Lists the container metadata.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <returns></returns>
    IDictionary<string, string> ListContainerMetadata(string containerTitle);

    /// <summary>
    /// Lists the entity metadata.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="entityId">The entity id.</param>
    /// <returns></returns>
    IDictionary<string, string> ListEntityMetadata(string containerTitle, Guid entityId);

    /// <summary>
    /// Lists the entity part metadata.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="entityId">The entity id.</param>
    /// <param name="partName">Name of the part.</param>
    /// <returns></returns>
    IDictionary<string, string> ListEntityPartMetadata(string containerTitle, Guid entityId, string partName);

    /// <summary>
    /// Sets the attachment metadata.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="entityId">The entity id.</param>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    bool SetAttachmentMetadata(string containerTitle, Guid entityId, string fileName, string key, string value);

    /// <summary>
    /// Sets the container metadata.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    bool SetContainerMetadata(string containerTitle, string key, string value);

    /// <summary>
    /// Sets the entity metadata.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="entityId">The entity id.</param>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    bool SetEntityMetadata(string containerTitle, Guid entityId, string key, string value);

    /// <summary>
    /// Sets the entity part metadata.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="entityId">The entity id.</param>
    /// <param name="partName">Name of the part.</param>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    bool SetEntityPartMetadata(string containerTitle, Guid entityId, string partName, string key, string value);
    #endregion
  }
}
