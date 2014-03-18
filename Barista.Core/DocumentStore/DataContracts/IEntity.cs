namespace Barista.DocumentStore
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Runtime.Serialization;

  public interface IEntity : IDSObject, IDSComments, IDSMetadata, IDSVersions, IDSPermissions
  {
    /// <summary>
    /// Gets or sets the unique identifier of the entity.
    /// </summary>
    [DataMember]
    Guid Id
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the title of the entity.
    /// </summary>
    [DataMember]
    string Title
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets a textual description of the entity.
    /// </summary>
    [DataMember]
    string Description
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the entity namespace -- which can be used to denote type.
    /// </summary>
    [DataMember]
    string Namespace
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the content type of the entity.
    /// </summary>
    [DataMember]
    string ContentType
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the eTag of the entity which can be used to indicate if an entity has been modified.
    /// </summary>
    [DataMember]
    string ETag
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the eTag of the entity data.
    /// </summary>
    [DataMember]
    string ContentsETag
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets a value that indicates when the entity data was last modified.
    /// </summary>
    [DataMember]
    DateTime ContentsModified
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets a value that indicates the path of the location of the entity.
    /// </summary>
    [DataMember]
    string Path
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the data of the entity
    /// </summary>
    [DataMember]
    string Data
    {
      get;
      set;
    }

    #region Attachments
    /// <summary>
    /// Deletes the specified attachment.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <returns></returns>
    bool DeleteAttachment(string fileName);

    /// <summary>
    /// Downloads the attachment as a stream.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <returns></returns>
    Stream DownloadAttachment(string fileName);

    /// <summary>
    /// Gets an attachment associated with the specified entity with the specified filename.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <returns></returns>
    IAttachment GetAttachment(string fileName);

    /// <summary>
    /// Lists the attachments associated with the specified entity.
    /// </summary>
    /// <returns></returns>
    IList<IAttachment> ListAttachments();

    /// <summary>
    /// Renames the attachment with the specified filename to the new filename.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="newFileName">New name of the file.</param>
    /// <returns></returns>
    bool RenameAttachment(string fileName, string newFileName);

    /// <summary>
    /// Uploads the attachment and associates it with the specified entity with the specified filename.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="attachment">The attachment.</param>
    /// <returns></returns>
    IAttachment UploadAttachment(string fileName, byte[] attachment);

    /// <summary>
    /// Uploads the attachment and associates it with the specified entity with the specified filename.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="attachment">The attachment.</param>
    /// <param name="category">The category.</param>
    /// <param name="path">The path.</param>
    /// <returns></returns>
    IAttachment UploadAttachment(string fileName, byte[] attachment, string category, string path);
    #endregion

    #region Entity Parts
    /// <summary>
    /// Creates the entity part.
    /// </summary>
    /// <param name="partName">Name of the part.</param>
    /// <param name="category">The category.</param>
    /// <param name="data">The data.</param>
    /// <returns></returns>
    IEntityPart CreateEntityPart(string partName, string category, string data);

    /// <summary>
    /// Deletes the specified entity part.
    /// </summary>
    /// <param name="partName">Name of the part.</param>
    /// <returns></returns>
    bool DeleteEntityPart(string partName);

    /// <summary>
    /// Gets the entity part.
    /// </summary>
    /// <param name="partName">Name of the part.</param>
    /// <returns></returns>
    IEntityPart GetEntityPart(string partName);

    /// <summary>
    /// Lists the entity parts associated with the specified entity in the specified container.
    /// </summary>
    /// <returns></returns>
    IList<IEntityPart> ListEntityParts();

    /// <summary>
    /// Renames the entity part.
    /// </summary>
    /// <param name="partName">Name of the part.</param>
    /// <param name="newPartName">New name of the part.</param>
    /// <returns></returns>
    bool RenameEntityPart(string partName, string newPartName);

    /// <summary>
    /// Updates the entity part.
    /// </summary>
    /// <param name="partName"></param>
    /// <param name="category"></param>
    /// <returns></returns>
    IEntityPart UpdateEntityPart(string partName, string category);

    /// <summary>
    /// Updates the data portion of the specified entity part.
    /// </summary>
    /// <param name="partName"></param>
    /// <param name="eTag"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    IEntityPart UpdateEntityPartData(string partName, string eTag, string data);
    #endregion
  }
}