namespace Barista.DocumentStore.FileSystem
{
  using Barista.DocumentStore.FileSystem.Data;
  using System;
  using System.IO.Packaging;
  using System.Text;

  /// <summary>
  /// Represents a physical representation of an Entity using the System.IO.Packaging namespace.
  /// </summary>
  public sealed partial class EntityPackage
  {
    /// <summary>
    /// Maps the attachment from package part.
    /// </summary>
    /// <param name="attachmentPackagePart">The attachment package part.</param>
    /// <returns>Attachment.</returns>
    public IAttachment MapAttachmentFromPackagePart(PackagePart attachmentPackagePart)
    {
      var metadata = GetAttachmentMetadata(attachmentPackagePart);

      var result = new Attachment
      {
        Category = metadata.Category,
        Created = metadata.Created,
        CreatedBy = User.GetUser(metadata.CreatedBy),
        ETag = metadata.Etag,
        FileName = metadata.FileName,
        Modified = metadata.Modified,
        ModifiedBy = User.GetUser(metadata.ModifiedBy),
        MimeType = attachmentPackagePart.ContentType,
        Path = metadata.Path,
        Size = metadata.Size,
        TimeLastModified = metadata.TimeLastModified,
        Url = metadata.Url
      };

      return result;
    }

    /// <summary>
    /// Returns an Entity object that is the representation of the specified Package. Optionally include data.
    /// </summary>
    /// <param name="includeData"></param>
    /// <returns></returns>
    public Entity MapEntityFromPackage(bool includeData)
    {
      var result = new Entity
      {
        Id = new Guid(m_package.PackageProperties.Identifier),
        Title = m_package.PackageProperties.Title,
        Namespace = m_package.PackageProperties.Subject,
        Description = m_package.PackageProperties.Description
      };

      if (m_package.PackageProperties.Created.HasValue)
        result.Created = m_package.PackageProperties.Created.Value;
      result.CreatedBy = User.GetUser(m_package.PackageProperties.Creator);


      if (m_package.PackageProperties.Modified.HasValue)
        result.Modified = m_package.PackageProperties.Modified.Value;
      result.CreatedBy = User.GetUser(m_package.PackageProperties.Creator);

      if (m_package.PackageProperties.Modified.HasValue)
        result.Modified = m_package.PackageProperties.Modified.Value;
      result.ModifiedBy = User.GetUser(m_package.PackageProperties.LastModifiedBy);

      if (!includeData)
        return result;

      var defaultEntityPart =
        m_package.GetPart(DefaultEntityPartUri);

      using (var fs = defaultEntityPart.GetStream())
      {
        var bytes = fs.ReadToEnd();
        result.Data = Encoding.UTF8.GetString(bytes);
      }

      return result;
    }

    /// <summary>
    /// Maps the entity part from package part.
    /// </summary>
    /// <param name="entityPartPackagePart">The entity part package part.</param>
    /// <returns>EntityPart.</returns>
    public EntityPart MapEntityPartFromPackagePart(PackagePart entityPartPackagePart)
    {
      return MapEntityPartFromPackagePart(entityPartPackagePart, true);
    }

    /// <summary>
    /// Maps the entity part from package part.
    /// </summary>
    /// <param name="entityPartPackagePart">The entity part package part.</param>
    /// <param name="includeData">if set to <c>true</c> include data with the entity part.</param>
    /// <returns>EntityPart.</returns>
    public EntityPart MapEntityPartFromPackagePart(PackagePart entityPartPackagePart, bool includeData)
    {
      //Get the entity part metadata and deserialize.
      EntityPartMetadata metadata = GetEntityPartMetadata(entityPartPackagePart);

      var result = new EntityPart
      {
        Category = metadata.Category,
        Created = metadata.Created,
        CreatedBy = User.GetUser(metadata.CreatedBy),
        EntityId = new Guid(entityPartPackagePart.Package.PackageProperties.Identifier),
        ETag = metadata.Etag,
        Modified = metadata.Modified,
        ModifiedBy = User.GetUser(metadata.ModifiedBy),
        Name = metadata.Name,
      };

      if (includeData)
      {
        using (var fs = entityPartPackagePart.GetStream())
        {
          var bytes = fs.ReadToEnd();
          result.Data = Encoding.UTF8.GetString(bytes);
        }
      }

      return result;
    }
  }
}
