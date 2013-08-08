namespace Barista.DocumentStore.FileSystem
{
  using System;
  using System.Collections.Generic;
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
    /// Creates a new Entity Part in the Entity Package.
    /// </summary>
    /// <param name="partName"></param>
    /// <param name="category"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public EntityPart CreateEntityPart(string partName, string category, string data)
    {
      var entityPartMetadata = new EntityPartMetadata
      {
        Category = category,
        Created = DateTime.Now,
        CreatedBy = User.GetCurrentUser().LoginName,
        Etag = Etag.Empty,
        Modified = DateTime.Now,
        ModifiedBy = User.GetCurrentUser().LoginName,
        Name = partName,
        Properties = new Dictionary<string, string>(),
      };

      return CreateEntityPart(partName, entityPartMetadata, data);
    }

    /// <summary>
    /// Deletes the entity part with the specified name from the package.
    /// </summary>
    /// <param name="partName"></param>
    /// <returns></returns>
    public bool DeleteEntityPart(string partName)
    {
      var defaultEntityPart = GetDefaultEntityPartPackagePart();

      //Delete the EntityPart and the Metadata Part.
      var entityPartRelationships = defaultEntityPart.GetRelationshipsByType(Data.Constants.AttachmentRelationship);
      var entityPartUri = GetEntityPartUriFromEntityPartName(partName);
      var entityPartMetadataUri = GetEntityPartMetadataUriFromEntityPartName(partName);

      var entityPartRelationship = entityPartRelationships.FirstOrDefault(ar => ar.TargetUri == entityPartUri);

      if (m_package.PartExists(entityPartUri))
        m_package.DeletePart(entityPartUri);

      if (m_package.PartExists(entityPartMetadataUri))
        m_package.DeletePart(entityPartMetadataUri);

      return entityPartRelationship != null;
    }

    /// <summary>
    /// Returns the entity part with the specified name that is contained in the entity package.
    /// </summary>
    /// <param name="partName"></param>
    /// <param name="includeData"></param>
    /// <returns></returns>
    public EntityPart GetEntityPart(string partName, bool includeData)
    {
      var entityPartPackagePart = GetEntityPartPackagePart(partName);
      return MapEntityPartFromPackagePart(entityPartPackagePart, includeData);
    }

    /// <summary>
    /// Returns a collection of all non-default entity parts contained within the EntityPackage.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<EntityPart> ListEntityParts()
    {
      var defaultEntityPart = GetDefaultEntityPartPackagePart();

      var entityPartRelationships = defaultEntityPart.GetRelationshipsByType(Constants.EntityPartRelationship);

      return entityPartRelationships
        .Select(rel => m_package.GetPart(rel.TargetUri))
        .Select(MapEntityPartFromPackagePart);
    }

    /// <summary>
    /// Renames the specified entity part to the new name.
    /// </summary>
    /// <param name="partName"></param>
    /// <param name="newPartName"></param>
    /// <returns></returns>
    public bool RenameEntityPart(string partName, string newPartName)
    {
      var metadata = GetEntityPartMetadata(partName);

      try
      {
        var entityPart = GetEntityPart(partName, true);

        metadata.Name = newPartName;
        metadata.Modified = DateTime.Now;
        metadata.ModifiedBy = User.GetCurrentUser().LoginName;

        CreateEntityPart(newPartName, metadata, entityPart.Data);
        DeleteEntityPart(partName);
      }
      catch (Exception)
      {
        DeleteEntityPart(newPartName);
      }

      return true;
    }

    /// <summary>
    /// Updates the entity part with the specified name.
    /// </summary>
    /// <param name="entityPart"></param>
    /// <returns></returns>
    public EntityPart UpdateEntityPart(EntityPart entityPart)
    {
      var entityPartPackagePart = GetEntityPartPackagePart(entityPart.Name);

      //Copy the data to the entity part 
      using (var fileStream = new StringStream(entityPart.Data))
      {
        fileStream.CopyTo(entityPartPackagePart.GetStream());
      }

      //Update the metadata part.
      var entityPartMetadata = GetEntityPartMetadata(entityPart.Name);

      entityPartMetadata.Category = entityPart.Category;

      entityPartMetadata.Modified = DateTime.Now;
      entityPartMetadata.ModifiedBy = User.GetCurrentUser().LoginName;
      entityPartMetadata.Etag.IncrementBy(1);

      var entityPartMetadataPart = GetEntityPartMetadataPackagePart(entityPart.Name);
      var metadata = JsonConvert.SerializeObject(entityPartMetadata, new EtagJsonConverter());

      //Copy the metadata to the entity part metadata part.
      using (var fileStream = new StringStream(metadata))
      {
        fileStream.CopyTo(entityPartMetadataPart.GetStream());
      }

      return GetEntityPart(entityPart.Name, true);
    }

    #region Private Methods

    private EntityPart CreateEntityPart(string partName, EntityPartMetadata entityPartMetadata, string data)
    {
      if (partName.IsNullOrWhiteSpace())
        throw new ArgumentNullException("partName");

      if (entityPartMetadata == null)
        throw new ArgumentNullException("entityPartMetadata");

      var entityPartUri = GetEntityPartUriFromEntityPartName(partName);

      if (m_package.PartExists(entityPartUri))
        throw new InvalidOperationException("An entity part with the specified name already exists within the entity.");

      var entityPart =
        m_package.CreatePart(entityPartUri, Data.Constants.EntityPartContentType);

      //Copy the data to the entity part 
      using (var fileStream = new StringStream(data))
      {
        fileStream.CopyTo(entityPart.GetStream());
      }

      //Create a relationship between the default entity part and the entity part.
      var defaultEntityPart = GetDefaultEntityPartPackagePart();
      defaultEntityPart.CreateRelationship(entityPartUri, TargetMode.Internal, Data.Constants.EntityPartRelationship);

      //Create the entity part metadata object.
      var entityPartMetadataUri = GetEntityPartMetadataUriFromEntityPartName(partName);
      var entityPartMetadataPart = m_package.CreatePart(entityPartMetadataUri, Data.Constants.EntityPartContentType);

      var metadata = JsonConvert.SerializeObject(entityPartMetadata, new EtagJsonConverter());

      //Copy the metadata to the entity part metadata part.
      using (var fileStream = new StringStream(metadata))
      {
        fileStream.CopyTo(entityPartMetadataPart.GetStream());
      }

      //Finally, create the relationship between the entity part and its metadata -- not that we couldn't have found it by convention.
      entityPart.CreateRelationship(entityPartMetadataUri, TargetMode.Internal, Data.Constants.EntityPartMetadataRelationship);

      return MapEntityPartFromPackagePart(entityPart);
    }

    private static Uri GetEntityPartUriFromEntityPartName(string entityPartName)
    {
      var escapedPartName = HttpUtility.UrlEncode(entityPartName);
      return new Uri(Data.Constants.EntityPartV1Namespace + escapedPartName + ".json", UriKind.Relative);
    }

    private static Uri GetEntityPartMetadataUriFromEntityPartName(string entityPartName)
    {
      var escapedPartName = HttpUtility.UrlEncode(entityPartName);
      return new Uri(Data.Constants.EntityPartV1Namespace + escapedPartName + "-metadata.json", UriKind.Relative);
    }

    private PackagePart GetEntityPartPackagePart(string partName)
    {
      var defaultEntityPart = GetDefaultEntityPartPackagePart();

      var entityPartRelationships = defaultEntityPart.GetRelationshipsByType(Data.Constants.EntityPartRelationship);
      var entityPartUri = GetEntityPartMetadataUriFromEntityPartName(partName);

      var entityPartRelationship = entityPartRelationships.FirstOrDefault(ar => ar.TargetUri == entityPartUri);

      if (entityPartRelationship == null)
        throw new InvalidOperationException("An entity part with the specified name does not exist, or has not been set up as a package relationship.");

      return m_package.GetPart(entityPartRelationship.TargetUri);
    }

    private PackagePart GetEntityPartMetadataPackagePart(string partName)
    {
      var entityPartPackagePart = GetEntityPartPackagePart(partName);

      return GetEntityPartMetadataPackagePart(entityPartPackagePart);
    }

    private PackagePart GetEntityPartMetadataPackagePart(PackagePart entityPartPackagePart)
    {
      if (entityPartPackagePart == null)
        throw new ArgumentNullException("entityPartPackagePart");
      var entityPartMetadataRelationships =
        entityPartPackagePart.GetRelationshipsByType(Constants.EntityPartMetadataRelationship);

      if (entityPartMetadataRelationships.Any() == false)
        throw new InvalidOperationException("The specified entity part did not contain an associated metadata part.");

      return m_package.GetPart(entityPartMetadataRelationships.First().TargetUri);
    }

    private EntityPartMetadata GetEntityPartMetadata(string partName)
    {
      var entityMetadataPackagePart = GetEntityPartMetadataPackagePart(partName);

      return GetEntityPartMetadata(entityMetadataPackagePart);
    }

    private EntityPartMetadata GetEntityPartMetadata(PackagePart entityMetadataPackagePart)
    {
      EntityPartMetadata metadata;
      using (var fs = entityMetadataPackagePart.GetStream())
      {
        var bytes = fs.ReadToEnd();
        var json = Encoding.UTF8.GetString(bytes);
        metadata = JsonConvert.DeserializeObject<EntityPartMetadata>(json, new EtagJsonConverter());
      }

      return metadata;
    }
    #endregion
  }
}
