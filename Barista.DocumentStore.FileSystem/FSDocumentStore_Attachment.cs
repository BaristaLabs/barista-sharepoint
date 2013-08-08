namespace Barista.DocumentStore.FileSystem
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;

  public partial class FSDocumentStore
  {
    /// <summary>
    /// Deletes the attachment.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="entityId">The entity id.</param>
    /// <param name="fileName">Name of the file.</param>
    /// <returns><c>true</c> if the attachment was deleted, <c>false</c> otherwise</returns>
    public bool DeleteAttachment(string containerTitle, Guid entityId, string fileName)
    {
      var packagePath = GetEntityPackagePath(containerTitle, null, entityId);

      using (var entityPackage = EntityPackage.Open(packagePath))
      {
        return entityPackage.DeleteAttachment(fileName);
      }
    }

    /// <summary>
    /// Downloads the attachment.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="entityId">The entity id.</param>
    /// <param name="fileName">Name of the file.</param>
    /// <returns>Stream.</returns>
    /// <exception cref="System.InvalidOperationException">An entity with the specified id does not exist.</exception>
    public Stream DownloadAttachment(string containerTitle, Guid entityId, string fileName)
    {
      var packagePath = GetEntityPackagePath(containerTitle, null, entityId);

      if (File.Exists(packagePath) == false)
        throw new InvalidOperationException("An entity with the specified id does not exist.");

      using (var entityPackage = EntityPackage.Open(packagePath))
      {
        return entityPackage.DownloadAttachmentStream(fileName);
      }
    }

    /// <summary>
    /// Gets the attachment.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="entityId">The entity id.</param>
    /// <param name="fileName">Name of the file.</param>
    /// <returns>Attachment.</returns>
    /// <exception cref="System.InvalidOperationException">An entity with the specified id does not exist.</exception>
    public Attachment GetAttachment(string containerTitle, Guid entityId, string fileName)
    {
      var packagePath = GetEntityPackagePath(containerTitle, null, entityId);

      if (File.Exists(packagePath) == false)
        throw new InvalidOperationException("An entity with the specified id does not exist.");

      using (var entityPackage = EntityPackage.Open(packagePath))
      {
        return entityPackage.GetAttachment(fileName);
      }
    }

    /// <summary>
    /// Lists the attachments.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="entityId">The entity id.</param>
    /// <returns>IList{Attachment}.</returns>
    /// <exception cref="System.InvalidOperationException">An entity with the specified id does not exist.</exception>
    public IList<Attachment> ListAttachments(string containerTitle, Guid entityId)
    {
      var packagePath = GetEntityPackagePath(containerTitle, null, entityId);

      if (File.Exists(packagePath) == false)
        throw new InvalidOperationException("An entity with the specified id does not exist.");

      using (var entityPackage = EntityPackage.Open(packagePath))
      {
        return entityPackage.ListAttachments().ToList();
      }
    }

    /// <summary>
    /// Renames the attachment.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="entityId">The entity id.</param>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="newFileName">New name of the file.</param>
    /// <returns><c>true</c> if the attachment was renamed, <c>false</c> otherwise</returns>
    /// <exception cref="System.InvalidOperationException">An entity with the specified id does not exist.</exception>
    public bool RenameAttachment(string containerTitle, Guid entityId, string fileName, string newFileName)
    {
      var packagePath = GetEntityPackagePath(containerTitle, null, entityId);

      if (File.Exists(packagePath) == false)
        throw new InvalidOperationException("An entity with the specified id does not exist.");

      using (var entityPackage = EntityPackage.Open(packagePath))
      {
        return entityPackage.RenameAttachment(fileName, newFileName);
      }
    }

    /// <summary>
    /// Uploads the attachment.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="entityId">The entity id.</param>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="attachment">The attachment.</param>
    /// <returns>Attachment.</returns>
    /// <exception cref="System.InvalidOperationException">An entity with the specified id does not exist.</exception>
    public Attachment UploadAttachment(string containerTitle, Guid entityId, string fileName, byte[] attachment)
    {
      var packagePath = GetEntityPackagePath(containerTitle, null, entityId);

      if (File.Exists(packagePath) == false)
        throw new InvalidOperationException("An entity with the specified id does not exist.");

      using (var entityPackage = EntityPackage.Open(packagePath))
      {
        using (var ms = new MemoryStream(attachment))
        {
          return entityPackage.CreateAttachment(fileName, null, null, ms);
        }
      }
    }

    /// <summary>
    /// Uploads the attachment.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="entityId">The entity id.</param>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="attachment">The attachment.</param>
    /// <param name="category">The category.</param>
    /// <param name="path">The path.</param>
    /// <returns>Attachment.</returns>
    /// <exception cref="System.InvalidOperationException">An entity with the specified id does not exist.</exception>
    public Attachment UploadAttachment(string containerTitle, Guid entityId, string fileName, byte[] attachment, string category, string path)
    {
      var packagePath = GetEntityPackagePath(containerTitle, null, entityId);

      if (File.Exists(packagePath) == false)
        throw new InvalidOperationException("An entity with the specified id does not exist.");

      using (var entityPackage = EntityPackage.Open(packagePath))
      {
        using (var ms = new MemoryStream(attachment))
        {
          return entityPackage.CreateAttachment(fileName, category, path, ms);
        }
      }
    }

    /// <summary>
    /// Uploads the attachment from source URL.
    /// </summary>
    /// <param name="containerTitle">The container title.</param>
    /// <param name="entityId">The entity id.</param>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="sourceUrl">The source URL.</param>
    /// <param name="category">The category.</param>
    /// <param name="path">The path.</param>
    /// <returns>Attachment.</returns>
    /// <exception cref="System.NotImplementedException"></exception>
    public Attachment UploadAttachmentFromSourceUrl(string containerTitle, Guid entityId, string fileName, string sourceUrl, string category, string path)
    {
      throw new NotImplementedException();
    }
  }
}
