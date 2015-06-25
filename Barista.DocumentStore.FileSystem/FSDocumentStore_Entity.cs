namespace Barista.DocumentStore.FileSystem
{
  using Barista.Extensions;
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;

  public partial class FSDocumentStore
  {
    public Entity CreateEntity(string containerTitle, string title, string @namespace, string data)
    {
      return CreateEntity(containerTitle, null, title, @namespace, data);
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

    /// <summary>
    /// Exports the entity. In this implementation, returns the entity package as a stream.
    /// </summary>
    /// <param name="containerTitle"></param>
    /// <param name="entityId"></param>
    /// <returns></returns>
    public Stream ExportEntity(string containerTitle, Guid entityId)
    {
      throw new NotImplementedException();
    }

    public Entity GetEntity(string containerTitle, Guid entityId)
    {
      return GetEntity(containerTitle, entityId, null);
    }

    public Entity GetEntityLight(string containerTitle, Guid entityId)
    {
      return GetEntityLight(containerTitle, entityId, null);
    }

    public Entity ImportEntity(string containerTitle, Guid entityId, string @namespace, byte[] archiveData)
    {
      throw new NotImplementedException();
    }

    public IList<Entity> ListEntities(string containerTitle,EntityFilterCriteria criteria)
    {
      var targetPath = GetContainerPath(containerTitle);
      if (criteria.Path.IsNullOrWhiteSpace() == false)
      {
        targetPath = Path.Combine(targetPath, criteria.Path);
      }

      var entities = new List<Entity>();
      foreach (var entityFileName in Directory.GetFiles(targetPath, "*.dse", SearchOption.AllDirectories))
      {
        using (var package = EntityPackage.Open(entityFileName))
        {
          if (package == null)
            continue;

          var entity = package.MapEntityFromPackage(criteria.IncludeData);
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

      using (var package = EntityPackage.Open(packagePath))
      {
        package.UpdateDefaultEntityPart(title, description, @namespace);
        
        return package.MapEntityFromPackage(true);
      }
    }

    public Entity UpdateEntityData(string containerTitle, Guid entityId, string eTag, string data)
    {
      return UpdateEntityData(containerTitle, null, entityId, eTag, data);
    }

    public Entity UpdateEntityData(string containerTitle, string path, Guid entityId, string eTag, string data)
    {
      var packagePath = GetEntityPackagePath(containerTitle, null, entityId);

      using (var package = EntityPackage.Open(packagePath))
      {
        return package.UpdateDefaultEntityPartData(eTag, data);
      }
    }

    protected string GetEntityPackagePath(string containerTitle, string path, Guid entityId)
    {
      var packagePath = path;

      packagePath = packagePath.IsNullOrWhiteSpace()
        ? Path.Combine(GetContainerPath(containerTitle), entityId + ".dse")
        : Path.Combine(Path.Combine(GetContainerPath(containerTitle), packagePath), entityId + ".dse");

      return packagePath;
    }
  }
}
