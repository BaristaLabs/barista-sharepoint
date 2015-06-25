namespace Barista.DocumentStore.FileSystem
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.IO.Packaging;
  using System.Linq;
  using System.Text;
  using System.Web;
  using Barista.DocumentStore.FileSystem.Data;
  using Barista.Extensions;
  using Barista.Framework;
  using Barista.Newtonsoft.Json;

  /// <summary>
  /// Represents a physical representation of an Entity using the System.IO.Packaging namespace.
  /// </summary>
  public sealed partial class EntityPackage
  {
    /// <summary>
    /// Creates a new attachment in the entity package with the specified file name.
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="category"></param>
    /// <param name="path"></param>
    /// <param name="attachmentData"></param>
    /// <returns></returns>
    public Attachment CreateAttachment(string fileName, string category, string path, Stream attachmentData)
    {
      var metadata = new AttachmentMetadata
      {
        Category = category,
        Created = DateTime.Now,
        CreatedBy = User.GetCurrentUser().LoginName,
        Etag = Etag.Empty,
        FileName = fileName,
        Modified = DateTime.Now,
        ModifiedBy = User.GetCurrentUser().LoginName,
        Path = path,
        Properties = new Dictionary<string, string>(),
        Size = attachmentData.Length,
        TimeLastModified = DateTime.Now,
        Url = "",
      };

      return CreateAttachment(fileName, metadata, attachmentData);
    }

    /// <summary>
    /// Deletes the attachment with the specified file name from the package.
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public bool DeleteAttachment(string fileName)
    {
      var defaultEntityPart = GetDefaultEntityPartPackagePart();

      var attachmentRelationships = defaultEntityPart.GetRelationshipsByType(Data.Constants.AttachmentRelationship);
      var attachmentUri = GetAttachmentUriFromAttachmentFileName(fileName);
      var attachmentMetadataUrl = GetAttachmentMetadataUriFromAttachmentFileName(fileName);

      var attachmentRelationship = attachmentRelationships.FirstOrDefault(ar => ar.TargetUri == attachmentUri);

      if (m_package.PartExists(attachmentUri))
        m_package.DeletePart(attachmentUri);

      if (m_package.PartExists(attachmentMetadataUrl))
        m_package.DeletePart(attachmentMetadataUrl);

      return attachmentRelationship != null;
    }

    /// <summary>
    /// Gets a stream that is associated with the specified attachment.
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public Stream DownloadAttachmentStream(string fileName)
    {
      var attachmentPackagePart = GetAttachmentMetadataPackagePart(fileName);

      var memoryStream = new MemoryStream();
      using (var s = attachmentPackagePart.GetStream())
      {
        s.CopyTo(memoryStream);
      }

      return memoryStream;
    }

    /// <summary>
    /// Returns the attachment with the specified file name contained in the entity package.
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public Attachment GetAttachment(string fileName)
    {
      var attachmentPackagePart = GetAttachmentPackagePart(fileName);
      return MapAttachmentFromPackagePart(attachmentPackagePart);
    }

    /// <summary>
    /// Returns a collection of all attachment contained within the EntityPackage
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Attachment> ListAttachments()
    {
      var defaultEntityPart = GetDefaultEntityPartPackagePart();

      var attachmentRelationships = defaultEntityPart.GetRelationshipsByType(Data.Constants.AttachmentRelationship);

      return attachmentRelationships
        .Select(rel => m_package.GetPart(rel.TargetUri))
        .Select(MapAttachmentFromPackagePart);
    }

    /// <summary>
    /// Renames the specified attachment to the new file name.
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="newFileName"></param>
    /// <returns></returns>
    public bool RenameAttachment(string fileName, string newFileName)
    {
      var metadata = GetAttachmentMetadata(fileName);

      try
      {
        using (var stream = DownloadAttachmentStream(fileName))
        {
          metadata.FileName = newFileName;
          metadata.Modified = DateTime.Now;
          metadata.ModifiedBy = User.GetCurrentUser().LoginName;

          CreateAttachment(newFileName, metadata, stream);
          DeleteAttachment(fileName);
        }
      }
      catch (Exception)
      {
        DeleteAttachment(newFileName);
      }

      return true;
    }

    #region Private Methods
    private Attachment CreateAttachment(string fileName, AttachmentMetadata attachment, Stream attachmentData)
    {
      if (fileName.IsNullOrWhiteSpace())
        throw new ArgumentNullException("fileName");

      if (attachment == null)
        throw new ArgumentNullException("attachment");

      var defaultEntityPart = GetDefaultEntityPartPackagePart();

      var attachmentRelationships = defaultEntityPart.GetRelationshipsByType(Data.Constants.AttachmentRelationship);
      var attachmentUri = GetAttachmentUriFromAttachmentFileName(fileName);
      var attachmentMetadataUri = GetAttachmentMetadataUriFromAttachmentFileName(fileName);

      var existingAttachmentRelationship = attachmentRelationships.FirstOrDefault(ar => ar.TargetUri == attachmentUri);
      if (existingAttachmentRelationship != null)
        throw new InvalidOperationException("An attachment with the specified name already exists in the package: " + fileName);

      var attachmentPackagePart =
          m_package.CreatePart(attachmentUri, StringHelper.GetMimeTypeFromFileName(fileName));

      if (attachmentData != null)
      {
        attachmentData.CopyTo(attachmentPackagePart.GetStream());
      }

      //Create a relationship between the default entity part and the attachment part.
      defaultEntityPart.CreateRelationship(attachmentUri, TargetMode.Internal, Data.Constants.EntityPartRelationship);

      //Create the attachment metadata object.
      var attachmentMetadataPackagePart = m_package.CreatePart(attachmentMetadataUri, Data.Constants.EntityPartContentType);

      //Throw the metadata in the file.
      var attachmentMetadata = new AttachmentMetadata
      {
        Category = attachment.Category,
        Created = attachment.Created,
        CreatedBy = attachment.CreatedBy,
        Etag = attachment.Etag.IncrementBy(1),
        FileName = fileName,
        Modified = attachment.Modified,
        ModifiedBy = attachment.ModifiedBy,
        Path = attachment.Path,
        Properties = attachment.Properties,
        Size = attachment.Size,
        TimeLastModified = attachment.TimeLastModified,
        Url = attachment.Url
      };

      var metadata = JsonConvert.SerializeObject(attachmentMetadata, new EtagJsonConverter());

      //Copy the metadata to the entity part metadata part.
      using (var fileStream = new StringStream(metadata))
      {
        fileStream.CopyTo(attachmentMetadataPackagePart.GetStream());
      }

      return MapAttachmentFromPackagePart(attachmentPackagePart);
    }

    private static Uri GetAttachmentUriFromAttachmentFileName(string attachmentFileName)
    {
      var escapedAttachmentFileName = HttpUtility.UrlEncode(attachmentFileName);
      return new Uri(Data.Constants.AttachmentV1Namespace + escapedAttachmentFileName, UriKind.Relative);
    }

    private static Uri GetAttachmentMetadataUriFromAttachmentFileName(string attachmentFileName)
    {
      var escapedPartName = HttpUtility.UrlEncode(attachmentFileName);
      return new Uri(Data.Constants.EntityPartV1Namespace + escapedPartName + "-metadata.json", UriKind.Relative);
    }

    private PackagePart GetAttachmentPackagePart(string fileName)
    {
      var defaultEntityPart = GetDefaultEntityPartPackagePart();

      var attachmentRelationships = defaultEntityPart.GetRelationshipsByType(Data.Constants.AttachmentRelationship);
      var attachmentUri = GetAttachmentUriFromAttachmentFileName(fileName);

      var attachmentRelationship = attachmentRelationships.FirstOrDefault(ar => ar.TargetUri == attachmentUri);

      if (attachmentRelationship == null)
        throw new InvalidOperationException("An attachment with the specified file name does not exist, or has not been set up as a package relationship.");

      return m_package.GetPart(attachmentRelationship.TargetUri);
    }

    private PackagePart GetAttachmentMetadataPackagePart(string fileName)
    {
      var attachmentPackagePart = GetAttachmentPackagePart(fileName);

      return GetAttachmentMetadataPackagePart(attachmentPackagePart);
    }

    private PackagePart GetAttachmentMetadataPackagePart(PackagePart attachmentPackagePart)
    {
      if (attachmentPackagePart == null)
        throw new ArgumentNullException("attachmentPackagePart");
      var attachmentMetadataRelationships =
        attachmentPackagePart.GetRelationshipsByType(Constants.AttachmentMetdataRelationship);

      if (attachmentMetadataRelationships.Any() == false)
        throw new InvalidOperationException("The specified attachment did not contain an associated metadata part");

      return m_package.GetPart(attachmentMetadataRelationships.First().TargetUri);
    }

    private AttachmentMetadata GetAttachmentMetadata(string fileName)
    {
      var attachmentMetadataPackagePart = GetAttachmentMetadataPackagePart(fileName);

      return GetAttachmentMetadata(attachmentMetadataPackagePart);
    }

    private AttachmentMetadata GetAttachmentMetadata(PackagePart attachmentMetadataPackagePart)
    {
      AttachmentMetadata metadata;
      using (var fs = attachmentMetadataPackagePart.GetStream())
      {
        var bytes = fs.ReadToEnd();
        var json = Encoding.UTF8.GetString(bytes);
        metadata = JsonConvert.DeserializeObject<AttachmentMetadata>(json, new EtagJsonConverter());
      }

      return metadata;
    }
    #endregion
  }
}
