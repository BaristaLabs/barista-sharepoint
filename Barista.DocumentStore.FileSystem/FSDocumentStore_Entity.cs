namespace Barista.DocumentStore.FileSystem
{
  using Barista.Framework;
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.IO.Packaging;
  using System.Linq;

  public partial class FSDocumentStore
  {
    public Entity CreateEntity(string containerTitle, string title, string @namespace, string data)
    {
      return CreateEntity(containerTitle, null, title, @namespace, data);
    }

    protected Entity CreateEntity(string containerTitle, string path, string title, string @namespace, string data)
    {
      var newId = Guid.NewGuid();

      var packagePath = GetEntityPackagePath(containerTitle, path, newId);

      if (File.Exists(packagePath))
        throw new InvalidOperationException("An entity with the specified id already exists.");

      using (var package =
        Package.Open(packagePath, FileMode.Create))
      {
        // Add the metadata part to the Package.
        package.PackageProperties.Identifier = newId.ToString();
        package.PackageProperties.ContentType = "application/barista-entity";
        package.PackageProperties.Subject = @namespace;
        package.PackageProperties.Title = title;
        package.PackageProperties.Created = DateTime.Now;
        package.PackageProperties.Creator = User.GetCurrentUser().LoginName;
        package.PackageProperties.Modified = DateTime.Now;
        package.PackageProperties.LastModifiedBy = User.GetCurrentUser().LoginName;

        // Add the default entity part to the Package.
        var defaultEntityPart =
            package.CreatePart(new Uri(Barista.DocumentStore.Constants.EntityPartV1Namespace + "default.dsep", UriKind.Relative), 
                           "application/json");

        if (defaultEntityPart == null)
          throw new InvalidOperationException("The Default Entity Part Document could not be created.");

        //Copy the data to the default entity part 
        using (var fileStream = new StringStream(data))
        {
          fileStream.CopyTo(defaultEntityPart.GetStream());
        }

        return FSDocumentStoreHelper.MapEntityFromPackage(package, true);
      }
    }

    public bool DeleteEntity(string containerTitle, Guid entityId)
    {
      return DeleteEntity(containerTitle, null, entityId);
    }

    public bool DeleteEntity(string containerTitle, string path, Guid entityId)
    {
      var packagePath = GetEntityPackagePath(containerTitle, path, entityId);

      if (File.Exists(packagePath) == false)
        return false;

      File.Delete(packagePath);
      return true;
    }

    public System.IO.Stream ExportEntity(string containerTitle, Guid entityId)
    {
      throw new NotImplementedException();
    }

    public Entity GetEntity(string containerTitle, Guid entityId)
    {
      return GetEntity(containerTitle, null, entityId);
    }

    public Entity GetEntity(string containerTitle, string path, Guid entityId)
    {
      using (var package = GetEntityPackage(containerTitle, path, entityId))
      {
        return package == null
          ? null
          : FSDocumentStoreHelper.MapEntityFromPackage(package, true);
      }
    }

    public Entity GetEntityLight(string containerTitle, Guid entityId)
    {
      return GetEntityLight(containerTitle, null, entityId);
    }

    public Entity GetEntityLight(string containerTitle, string path, Guid entityId)
    {
      using (var package = GetEntityPackage(containerTitle, path, entityId))
      {
        return package == null
          ? null
          : FSDocumentStoreHelper.MapEntityFromPackage(package, false);
      }
    }

    public Entity ImportEntity(string containerTitle, Guid entityId, string @namespace, byte[] archiveData)
    {
      throw new NotImplementedException();
    }

    public IList<Entity> ListEntities(string containerTitle,EntityFilterCriteria criteria)
    {
      var targetPath = GetContainerPath(containerTitle);
      if (String.IsNullOrWhiteSpace(criteria.Path) == false)
      {
        targetPath = Path.Combine(targetPath, criteria.Path);
      }

      var entities = new List<Entity>();
      foreach (var entityFileName in Directory.GetFiles(targetPath, "*.dse", SearchOption.AllDirectories))
      {
        using (var package = GetEntityPackage(entityFileName))
        {
          if (package == null)
            continue;

          var entity = FSDocumentStoreHelper.MapEntityFromPackage(package, criteria.IncludeData);
          entities.Add(entity);
        }
      }

      IEnumerable<Entity> result = entities;

      if (criteria.Top.HasValue)
        result = result.Take((int)criteria.Top.Value);

      if (criteria.Skip.HasValue)
        result = result.Skip((int)criteria.Skip.Value);

      return result.ToList();
    }

    /// <summary>
    /// Filters the specified collection of Entity List Items according to the specified criteria.
    /// </summary>
    /// <param name="entities"></param>
    /// <param name="criteria"></param>
    /// <returns></returns>
    protected IEnumerable<Entity> FilterListItemEntities(IEnumerable<Entity> entities, EntityFilterCriteria criteria)
    {
      if (criteria == null)
        return entities;

      if (String.IsNullOrEmpty(criteria.Namespace) == false)
      {
        switch (criteria.NamespaceMatchType)
        {
          case NamespaceMatchType.Equals:
            entities = entities.Where(e => e.Namespace == criteria.Namespace);
            break;
          case NamespaceMatchType.StartsWith:
            entities = entities.Where(e => e.Namespace.StartsWith(criteria.Namespace));
            break;
          case NamespaceMatchType.EndsWith:
            entities = entities.Where(e => e.Namespace.EndsWith(criteria.Namespace));
            break;
          case NamespaceMatchType.Contains:
            entities = entities.Where(e => e.Namespace.Contains(criteria.Namespace));
            break;
          case NamespaceMatchType.StartsWithMatchAllQueryPairs:
            {
              entities = entities.Where(e =>
                {
                  var ns = e.Namespace;
                  Uri namespaceUri;
                  if (Uri.TryCreate(ns, UriKind.Absolute, out namespaceUri) == false)
                    return false;

                  var qs = new QueryString(namespaceUri.Query);
                  return
                    criteria.QueryPairs.All(
                      qp =>
                      qs.AllKeys.Contains(qp.Key, StringComparer.CurrentCultureIgnoreCase) &&
                      String.Compare(qs[qp.Key], qp.Value, StringComparison.CurrentCultureIgnoreCase) == 0);
                });
            }
            break;
          case NamespaceMatchType.StartsWithMatchAllQueryPairsContainsValue:
            {
              entities = entities.Where(e =>
                {
                  var ns = e.Namespace;
                  Uri namespaceUri;
                  if (Uri.TryCreate(ns, UriKind.Absolute, out namespaceUri) == false)
                    return false;

                  var qs = new QueryString(namespaceUri.Query);
                  return
                    criteria.QueryPairs.All(
                      qp =>
                      qs.AllKeys.Contains(qp.Key, StringComparer.CurrentCultureIgnoreCase) &&
                      qs[qp.Key].ToLower().Contains(qp.Value.ToLower()));
                });
            }
            break;
          case NamespaceMatchType.StartsWithMatchAnyQueryPairs:
            {
              entities = entities.Where(e =>
                {
                  var ns = e.Namespace;
                  Uri namespaceUri;
                  if (Uri.TryCreate(ns, UriKind.Absolute, out namespaceUri) == false)
                    return false;

                  var qs = new QueryString(namespaceUri.Query);
                  return
                    criteria.QueryPairs.Any(
                      qp =>
                      qs.AllKeys.Contains(qp.Key, StringComparer.CurrentCultureIgnoreCase) &&
                      String.Compare(qs[qp.Key], qp.Value, StringComparison.CurrentCultureIgnoreCase) == 0);
                });
            }
            break;
          case NamespaceMatchType.StartsWithMatchAnyQueryPairsContainsValue:
            {
              entities = entities.Where(e =>
                {
                  var ns = e.Namespace;
                  Uri namespaceUri;
                  if (Uri.TryCreate(ns, UriKind.Absolute, out namespaceUri))
                    return false;

                  var qs = new QueryString(namespaceUri.Query);
                  return
                    criteria.QueryPairs.Any(
                      qp =>
                      qs.AllKeys.Contains(qp.Key, StringComparer.CurrentCultureIgnoreCase) &&
                      qs[qp.Key].ToLower().Contains(qp.Value.ToLower()));
                });
            }
            break;
        }
      }

      return entities;
    }

    public int CountEntities(string containerTitle, EntityFilterCriteria criteria)
    {
      return ListEntities(containerTitle, criteria).Count;
    }

    public Entity UpdateEntity(string containerTitle, Guid entityId, string title, string description, string @namespace)
    {
      var packagePath = GetEntityPackagePath(containerTitle, null, entityId);

      if (File.Exists(packagePath) == false)
        return null;

      using (var package =
        Package.Open(packagePath, FileMode.Open))
      {
        // Update the metadata part in the Package.
        package.PackageProperties.ContentType = "application/barista-entity";
        package.PackageProperties.Subject = @namespace;
        package.PackageProperties.Title = title;
        package.PackageProperties.Description = description;
        package.PackageProperties.Modified = DateTime.Now;
        package.PackageProperties.LastModifiedBy = User.GetCurrentUser().LoginName;

        return FSDocumentStoreHelper.MapEntityFromPackage(package, true);
      }
    }

    public Entity UpdateEntityData(string containerTitle, Guid entityId, string eTag, string data)
    {
      return UpdateEntityData(containerTitle, null, entityId, eTag, data);
    }

    public Entity UpdateEntityData(string containerTitle, string path, Guid entityId, string eTag, string data)
    {
      using (var package = GetEntityPackage(containerTitle, path, entityId))
      {
        //TODO: Check to see if the eTag matches.

        var defaultPartUri = new Uri(Barista.DocumentStore.Constants.EntityPartV1Namespace + "default.dsep", UriKind.Relative);

        if (package.PartExists(defaultPartUri) == false)
          throw new InvalidOperationException("The default entity part for this package could not be located.");

        // Get the default entity part from the Package.
        var defaultEntityPart =
          package.GetPart(defaultPartUri);


        //Copy the data to the default entity part 
        using (var fileStream = new StringStream(data))
        {
          fileStream.CopyTo(defaultEntityPart.GetStream());
        }

        return FSDocumentStoreHelper.MapEntityFromPackage(package, true);
      }
    }

    protected string GetEntityPackagePath(string containerTitle, string path, Guid entityId)
    {
      var packagePath = path;

      packagePath = String.IsNullOrWhiteSpace(packagePath)
        ? Path.Combine(GetContainerPath(containerTitle), entityId + ".dse")
        : Path.Combine(GetContainerPath(containerTitle), packagePath, entityId + ".dse");

      return packagePath;
    }

    protected Package GetEntityPackage(string containerTitle, string path, Guid entityId)
    {
      var packagePath = GetEntityPackagePath(containerTitle, path, entityId);
      if (File.Exists(packagePath))
      {
        return Package.Open(packagePath, FileMode.Open);
      }

      var packages = Directory.GetFiles(GetContainerPath(containerTitle), entityId + ".dse", SearchOption.AllDirectories);

      if (packages.Length <= 0)
        return null;

      if (packages.Length > 1)
        throw new InvalidOperationException(
          String.Format("Multiple Entities with the same Id were found in the container: {0} {1}", containerTitle,
                        entityId));

      return Package.Open(packages[0], FileMode.Open);
    }

    protected Package GetEntityPackage(string packagePath)
    {
      return File.Exists(packagePath)
        ? Package.Open(packagePath, FileMode.Open)
        : null;
    }
  }
}
