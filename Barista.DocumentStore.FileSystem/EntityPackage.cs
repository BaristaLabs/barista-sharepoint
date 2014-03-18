namespace Barista.DocumentStore.FileSystem
{
  using System;
  using System.IO;
  using System.IO.Packaging;
  using Barista.DocumentStore.FileSystem.Data;
  using Barista.Extensions;
  using Barista.Framework;
  using Barista.Newtonsoft.Json;

  /// <summary>
  /// Represents a physical representation of an Entity using the System.IO.Packaging namespace.
  /// </summary>
  public sealed partial class EntityPackage : IDisposable
  {
    public static readonly Uri DefaultEntityPartUri = new Uri("/default.json", UriKind.Relative);

    private readonly Package m_package;

    private EntityPackage(Package package)
    {
      if (package == null)
        throw new ArgumentNullException("package");

      m_package = package;
    }

    #region Properties

    /// <summary>
    /// Gets the Id of the package.
    /// </summary>
    [JsonProperty("id")]
    public Guid Id
    {
      get { return new Guid(m_package.PackageProperties.Identifier); }
    }

    /// <summary>
    /// Gets the underlying System.IO.Packaging.Package.
    /// </summary>
    public Package Package
    {
      get
      {
        return m_package;
      }
    }
    #endregion

    /// <summary>
    /// Returns the default entity part.
    /// </summary>
    /// <returns></returns>
    public EntityPart GetDefaultEntityPart()
    {
      var defaultEntityPartPackagePart = GetDefaultEntityPartPackagePart();
      return MapEntityPartFromPackagePart(defaultEntityPartPackagePart);
    }

    /// <summary>
    /// Updates the default entity part's title, description and namespace fields.
    /// </summary>
    /// <param name="title">The title.</param>
    /// <param name="description">The description.</param>
    /// <param name="namespace">The namespace.</param>
    /// <returns>Entity.</returns>
    public Entity UpdateDefaultEntityPart(string title, string description, string @namespace)
    {
      // Update the metadata part in the Package.
      m_package.PackageProperties.Subject = @namespace;
      m_package.PackageProperties.Title = title;
      m_package.PackageProperties.Description = description;

      //Bump the modified by and modified date.
      m_package.PackageProperties.Modified = DateTime.Now;
      m_package.PackageProperties.LastModifiedBy = User.GetCurrentUser().LoginName;

      return MapEntityFromPackage(true);
    }

    public Entity UpdateDefaultEntityPartData(string etag, string data)
    {
      //TODO: Check to see if the eTag matches.

      var defaultEntityPartPackagePart = GetDefaultEntityPartPackagePart();

      //Copy the data to the default entity part 
      using (var fileStream = new StringStream(data))
      {
        fileStream.CopyTo(defaultEntityPartPackagePart.GetStream());
      }

      //Bump the modified by and modified date.
      m_package.PackageProperties.Modified = DateTime.Now;
      m_package.PackageProperties.LastModifiedBy = User.GetCurrentUser().LoginName;

      //TODO: Bump the etag...

      return MapEntityFromPackage(true);
    }

    #region Private Methods

    /// <summary>
    /// Gets the default entity part package part.
    /// </summary>
    /// <returns>PackagePart.</returns>
    /// <exception cref="System.InvalidOperationException">The default entity part for this package could not be located.</exception>
    private PackagePart GetDefaultEntityPartPackagePart()
    {
      if (this.Package.PartExists(DefaultEntityPartUri) == false)
        throw new InvalidOperationException("The default entity part for this package could not be located.");

      var defaultEntityPartPackagePart = this.Package.GetPart(DefaultEntityPartUri);
      return defaultEntityPartPackagePart;
    }
    #endregion

    #region Static Methods
    /// <summary>
    /// Creates a new, in-memory, EntityPackage.
    /// </summary>
    public static EntityPackage Create(string @namespace, string title, string data)
    {
      var ms = new MemoryStream();

      var package = Package.Open(ms, FileMode.Create, FileAccess.ReadWrite);
      InitializeEntityPackage(package, null, @namespace, title, data);

      return new EntityPackage(package);
    }

    /// <summary>
    /// Creates a new, file system based, EntityPackage.
    /// </summary>
    public static EntityPackage Create(string path, string @namespace, string title, string data)
    {
      if (path.IsNullOrWhiteSpace())
        throw new ArgumentNullException("path");

      var package = Package.Open(path, FileMode.Create, FileAccess.ReadWrite);
      InitializeEntityPackage(package, null, @namespace, title, data);

      return new EntityPackage(package);
    }

    /// <summary>
    /// Creates a new, file system based, EntityPackage with the specified id.
    /// </summary>
    public static EntityPackage Create(string path, Guid entityId, string @namespace, string title, string data)
    {
      if (path.IsNullOrWhiteSpace())
        throw new ArgumentNullException("path");

      var package = Package.Open(path, FileMode.Create, FileAccess.ReadWrite);
      InitializeEntityPackage(package, entityId, @namespace, title, data);

      return new EntityPackage(package);
    }

    /// <summary>
    /// Creates a new, stream based, EntityPackage.
    /// </summary>
    public static EntityPackage Create(Stream stream, string @namespace, string title, string data)
    {
      var package = Package.Open(stream, FileMode.Create, FileAccess.ReadWrite);
      InitializeEntityPackage(package, null, @namespace, title, data);

      return new EntityPackage(package);
    }

    /// <summary>
    /// Opens a package at a given path using a given file mode and access setting.
    /// </summary>
    /// <param name="path"></param>
    public static EntityPackage Open(string path)
    {
      if (File.Exists(path) == false)
        return null;

      var package = Package.Open(path, FileMode.Open, FileAccess.ReadWrite);

      if (package.PackageProperties.ContentType != Constants.EntityPackageContentType)
        throw new InvalidOperationException("Unexpected package content type: " + package.PackageProperties.ContentType);

      return new EntityPackage(package);
    }

    /// <summary>
    /// Opens a package with a given IO stream, file mode, and file access setting.
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static EntityPackage Open(Stream stream)
    {
      var package = Package.Open(stream, FileMode.Open, FileAccess.ReadWrite);

      if (package.PackageProperties.ContentType != Constants.EntityPackageContentType)
        throw new InvalidOperationException("Unexpected package content type: " + package.PackageProperties.ContentType);

      return new EntityPackage(package);
    }

    private static void InitializeEntityPackage(Package package, Guid? entityId, string @namespace, string title, string data)
    {
      if (@namespace.IsNullOrWhiteSpace())
        throw new ArgumentException(@"An entity namespace must be specified.", "namespace");


      if (entityId.HasValue == false)
        entityId = Guid.NewGuid();

      if (title.IsNullOrWhiteSpace())
        title = entityId.ToString();

      // Add the metadata part to the Package.
      package.PackageProperties.Identifier = entityId.Value.ToString();
      package.PackageProperties.ContentType = Constants.EntityPackageContentType;
      package.PackageProperties.Subject = @namespace;
      package.PackageProperties.Title = title;
      package.PackageProperties.Created = DateTime.Now;
      package.PackageProperties.Creator = User.GetCurrentUser().LoginName;
      package.PackageProperties.Modified = DateTime.Now;
      package.PackageProperties.LastModifiedBy = User.GetCurrentUser().LoginName;

      // Add the default entity part to the Package.
      var defaultEntityPart =
        package.CreatePart(DefaultEntityPartUri, Constants.EntityPartContentType);

      //Copy the data to the default entity part 
      if (data.IsNullOrWhiteSpace())
        return;

      using (var fileStream = new StringStream(data))
      {
        fileStream.CopyTo(defaultEntityPart.GetStream());
      }
    }
    #endregion

    #region IDisposable
    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
      if (m_package != null)
        ((IDisposable)m_package).Dispose();
    }
    #endregion
  }
}
