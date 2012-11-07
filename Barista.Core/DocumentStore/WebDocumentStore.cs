namespace Barista.DocumentStore
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Text;
  using RestSharp;
  using Barista.Framework;

  /// <summary>
  /// Represents a REST-Based Document Store service client.
  /// </summary>
  public class WebDocumentStore : IFullyCapableDocumentStore
  {
    public WebDocumentStore(string baseUrl)
    {
      this.BaseUrl = baseUrl;
    }

    public string BaseUrl
    {
      get;
      private set;
    }

    #region Execute Methods
    private IRestResponse Execute(RestRequest request)
    {
      request.JsonSerializer = new JsonNetSerializer();

      var client = new RestClient();
      client.BaseUrl = this.BaseUrl;
      client.Authenticator = new NtlmAuthenticator();
      client.DefaultParameters.Add(new Parameter() { Name = "Accept", Type = ParameterType.HttpHeader, Value = "application/json" });
      client.RemoveHandler("application/json");
      client.AddHandler("application/json", new JsonNetDeserializer());

      var response = client.Execute(request);
      if (response.StatusCode != System.Net.HttpStatusCode.OK)
        throw new InvalidOperationException(response.StatusDescription);

      return response;
    }

    private bool TryExecute(RestRequest request, out IRestResponse response)
    {
      request.JsonSerializer = new JsonNetSerializer();

      var client = new RestClient();
      client.BaseUrl = this.BaseUrl;
      client.Authenticator = new NtlmAuthenticator();
      client.DefaultParameters.Add(new Parameter() { Name = "Accept", Type = ParameterType.HttpHeader, Value = "application/json" });
      client.RemoveHandler("application/json");
      client.AddHandler("application/json", new JsonNetDeserializer());

      response = client.Execute(request);

      if (response.StatusCode != System.Net.HttpStatusCode.OK)
        return false;

      return true;
    }

    private T Execute<T>(RestRequest request) where T : new()
    {
      request.JsonSerializer = new JsonNetSerializer();

      var client = new RestClient();
      client.BaseUrl = this.BaseUrl;
      client.Authenticator = new NtlmAuthenticator();
      client.DefaultParameters.Add(new Parameter() { Name = "Accept", Type = ParameterType.HttpHeader, Value = "application/json" });
      client.RemoveHandler("application/json");
      client.AddHandler("application/json", new JsonNetDeserializer());

      var response = client.Execute<T>(request);

      if (response.StatusCode != System.Net.HttpStatusCode.OK)
        throw new InvalidOperationException(response.StatusDescription);
      
      return response.Data;
    }

    private bool TryExecute<T>(RestRequest request, out IRestResponse<T> response) where T : new()
    {
      request.JsonSerializer = new JsonNetSerializer();

      var client = new RestClient();
      client.BaseUrl = this.BaseUrl;
      client.Authenticator = new NtlmAuthenticator();
      client.DefaultParameters.Add(new Parameter() { Name = "Accept", Type = ParameterType.HttpHeader, Value = "application/json" });
      client.RemoveHandler("application/json");
      client.AddHandler("application/json", new JsonNetDeserializer());

      response = client.Execute<T>(request);

      if (response.StatusCode != System.Net.HttpStatusCode.OK)
        return false;
      return true;
    }
    #endregion

    #region Containers

    private bool TryCreateContainerInternal(string containerTitle, string description, out IRestResponse<Container> response)
    {
      var request = new RestRequest();
      request.Resource = "/v1/Containers.svc/Containers/?title={containerTitle}&description={description}";
      request.Method = Method.POST;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("description", description, ParameterType.UrlSegment);

      return TryExecute<Container>(request, out response);
    }

    public bool TryCreateContainer(string containerTitle, string description, out Container container)
    {
      container = null;
      IRestResponse<Container> response;
      if (TryCreateContainerInternal(containerTitle, description, out response))
      {
        container = response.Data;
        return true;
      }
      return false;
    }

    public Container CreateContainer(string containerTitle, string description)
    {
      IRestResponse<Container> response;
      if (TryCreateContainerInternal(containerTitle, description, out response))
      {
        return response.Data;
      }

      throw new InvalidOperationException(response.StatusDescription);
    }

    private bool TryGetContainerInternal(string containerTitle, out IRestResponse<Container> response)
    {
      var request = new RestRequest();
      request.Resource = "/v1/Containers.svc/Container({containerTitle})";
      request.Method = Method.GET;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      
      return TryExecute<Container>(request, out response);
    }

    public bool TryGetContainer(string containerTitle, out Container container)
    {
      container = null;
      IRestResponse<Container> response;
      if (TryGetContainerInternal(containerTitle, out response))
      {
        container = response.Data;
        return true;
      }
      return false;
    }

    public Container GetContainer(string containerTitle)
    {
      IRestResponse<Container> response;
      if (TryGetContainerInternal(containerTitle, out response))
      {
        return response.Data;
      }

      throw new InvalidOperationException(response.StatusDescription);
    }

    private bool TryUpdateContainerInternal(Container container, out IRestResponse response)
    {
      var request = new RestRequest();
      request.Resource = "/v1/Containers.svc/Containers/";
      request.Method = Method.PUT;
      request.RequestFormat = DataFormat.Json;
      request.XmlSerializer = null;
      
      request.AddBody(container);

      return TryExecute(request, out response);
    }

    public bool TryUpdateContainer(Container container)
    {
      IRestResponse response;
      if (TryUpdateContainerInternal(container, out response))
        return true;
      return false;
    }

    public bool UpdateContainer(Container container)
    {
      IRestResponse response;
      if (TryUpdateContainerInternal(container, out response))
        return true;

      throw new InvalidOperationException(response.StatusDescription);
    }

    private bool TryDeleteContainerInternal(string containerTitle, out IRestResponse response)
    {
      var request = new RestRequest();
      request.Resource = "/v1/Containers.svc/Container({containerTitle})";
      request.Method = Method.DELETE;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);

      return TryExecute(request, out response);
    }

    public bool TryDeleteContainer(Container container)
    {
      IRestResponse response;
      if (TryDeleteContainerInternal(container.Title, out response))
        return true;
      return false;
    }

    public bool TryDeleteContainer(string containerTitle)
    {
      IRestResponse response;
      if (TryDeleteContainerInternal(containerTitle, out response))
        return true;
      return false;
    }

    public void DeleteContainer(Container container)
    {
      IRestResponse response;
      if (TryDeleteContainerInternal(container.Title, out response))
        return;

      throw new InvalidOperationException(response.StatusDescription);
    }

    public void DeleteContainer(string containerTitle)
    {
      IRestResponse response;
      if (TryDeleteContainerInternal(containerTitle, out response))
        return;

      throw new InvalidOperationException(response.StatusDescription);
    }

    private bool TryListContainersInternal(out IRestResponse<List<Container>> response)
    {
      var request = new RestRequest();
      request.Resource = "/v1/Containers.svc/Containers";

      request.Method = Method.GET;
      request.RequestFormat = DataFormat.Json;
      request.XmlSerializer = null;

      return TryExecute<List<Container>>(request, out response);
    }

    public bool TryListContainers(IList<Container> containers)
    {
      containers = null;
      IRestResponse<List<Container>> response;
      if (TryListContainersInternal(out response))
      {
        containers = response.Data;
        return true;
      }
      return false;
    }

    public IList<Container> ListContainers()
    {
      IRestResponse<List<Container>> response;
      if (TryListContainersInternal(out response))
        return response.Data;

      throw new InvalidOperationException(response.StatusDescription);
    }
    #endregion

    #region Folders

    private bool TryCreateFolderInternal(string containerTitle, string path, out IRestResponse<Folder> response)
    {
      var request = new RestRequest();
      request.Resource = "/v1/Folders.svc/Container({containerTitle})/Folders/{path}";
      request.Method = Method.POST;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("path", path, ParameterType.UrlSegment);

      return TryExecute<Folder>(request, out response);
    }

    public bool TryCreateFolder(string containerTitle, string path, out Folder folder)
    {
      folder = null;
      IRestResponse<Folder> response;
      if (TryCreateFolderInternal(containerTitle, path, out response))
      {
        folder = response.Data;
        return true;
      }
      return false;
    }

    public Folder CreateFolder(string containerTitle, string path)
    {
      IRestResponse<Folder> response;
      if (TryCreateFolderInternal(containerTitle, path, out response))
      {
        return response.Data;
      }

      throw new InvalidOperationException(response.StatusDescription);
    }

    public bool TryCreateFolder(Container container, string path, out Folder folder)
    {
      return TryCreateFolder(container.Title, path, out folder);
    }

    public Folder CreateFolder(Container container, string path)
    {
      return CreateFolder(container.Title, path);
    }

    public bool TryGetFolderInternal(string containerTitle, string path, out IRestResponse<Folder> response)
    {
      var request = new RestRequest();
      request.Resource = "/v1/Folders.svc/Container({containerTitle})/Folder/{path}";
      request.Method = Method.GET;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("path", path, ParameterType.UrlSegment);

      return TryExecute<Folder>(request, out response);
    }

    public bool TryGetFolder(string containerTitle, string path, out Folder folder)
    {
      folder = null;
      IRestResponse<Folder> response;
      if (TryGetFolderInternal(containerTitle, path, out response))
      {
        folder = response.Data;
        return true;
      }
      return false;
    }

    public Folder GetFolder(string containerTitle, string path)
    {
      IRestResponse<Folder> response;
      if (TryGetFolderInternal(containerTitle, path, out response))
      {
        return response.Data;
      }

      throw new InvalidOperationException(response.StatusDescription);
    }

    public string GetFolderETag(string containerTitle, string path, EntityFilterCriteria criteria)
    {
      var request = new RestRequest();

      if (criteria != null)
      {
        UriBuilder builder = new UriBuilder("/v1/Folders.svc/Container({containerTitle})/FolderETag/{path}");
        QueryString qs = new QueryString();

        if (String.IsNullOrEmpty(criteria.Namespace) == false)
          qs.Add("$namespace", criteria.Namespace);

        if (criteria.Skip.HasValue)
          qs.Add("$skip", criteria.Skip.Value.ToString());

        if (criteria.Top.HasValue)
          qs.Add("$top", criteria.Top.Value.ToString());

        request.Resource = "/v1/Folders.svc/Container({containerTitle})/FolderETag/{path}" + qs.ToString();
      }
      else
        request.Resource = "/v1/Folders.svc/Container({containerTitle})/FolderETag/{path}";

      request.Method = Method.GET;
      request.RequestFormat = DataFormat.Json;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("path", path, ParameterType.UrlSegment);

      var result = Execute(request);
      return DocumentStoreHelper.DeserializeObjectFromJson<string>(result.Content);
    }

    private bool TryRenameFolderInternal(string containerTitle, string path, string newFolderName, out IRestResponse<Folder> response)
    {
      var request = new RestRequest();
      request.Resource = "/v1/Folders.svc/Container({containerTitle})/Folders/{path}?newFolderName={newFolderName}";
      request.Method = Method.PUT;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("newFolderName", newFolderName, ParameterType.UrlSegment);
      request.AddParameter("path", path, ParameterType.UrlSegment);

      return TryExecute<Folder>(request, out response);
    }

    public bool TryRenameFolder(string containerTitle, string path, string newFolderName, out Folder folder)
    {
      folder = null;
      IRestResponse<Folder> response;
      if (TryRenameFolderInternal(containerTitle, path, newFolderName, out response))
      {
        folder = response.Data;
        return true;
      }
      return false;
    }

    public Folder RenameFolder(string containerTitle, string path, string newFolderName)
    {
      IRestResponse<Folder> response;
      if (TryRenameFolderInternal(containerTitle, path, newFolderName, out response))
      {
        return response.Data;
      }

      throw new InvalidOperationException(response.StatusDescription);
    }

    public bool TryRenameFolder(Container container, string path, string newFolderName, out Folder folder)
    {
      return TryRenameFolder(container.Title, path, newFolderName, out folder);
    }

    public Folder RenameFolder(Container container, string path, string newFolderName)
    {
      return RenameFolder(container.Title, path, newFolderName);
    }

    private bool TryDeleteFolderInternal(string containerTitle, string path, out IRestResponse response)
    {
      var request = new RestRequest();
      request.Resource = "/v1/Folders.svc/Container({containerTitle})/Folders/{path}";
      request.Method = Method.DELETE;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("path", path, ParameterType.UrlSegment);

      return TryExecute(request, out response);
    }

    public bool TryDeleteFolder(string containerTitle, string path)
    {
      IRestResponse response;
      if (TryDeleteFolderInternal(containerTitle, path, out response))
        return true;
      return false;
    }

    public void DeleteFolder(string containerTitle, string path)
    {
      IRestResponse response;
      if (TryDeleteFolderInternal(containerTitle, path, out response))
        return;

      throw new InvalidOperationException(response.StatusDescription);
    }

    public bool TryDeleteFolder(Container container, string path)
    {
      return TryDeleteFolder(container.Title, path);
    }

    public void DeleteFolder(Container container, string path)
    {
      DeleteFolder(container.Title, path);
    }

    public bool TryListFoldersInternal(string containerTitle, string path, out IRestResponse<List<Folder>> response)
    {
      var request = new RestRequest();
      if (String.IsNullOrEmpty(path) || String.IsNullOrEmpty(path.Trim()))
        request.Resource = "/v1/Folders.svc/Container({containerTitle})/Folders";
      else
        request.Resource = "/v1/Folders.svc/Container({containerTitle})/Folders/{path}";

      request.Method = Method.GET;
      request.RequestFormat = DataFormat.Json;
      request.XmlSerializer = null;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("path", path, ParameterType.UrlSegment);

      return TryExecute<List<Folder>>(request, out response);
    }

    public bool TryListFolders(string containerTitle, string path, out IList<Folder> folders)
    {
      folders = null;
      IRestResponse<List<Folder>> response;
      if (TryListFoldersInternal(containerTitle, path, out response))
      {
        folders = response.Data;
        return true;
      }
      return false;
    }

    public IList<Folder> ListFolders(string containerTitle, string path)
    {
      IRestResponse<List<Folder>> response;
      if (TryListFoldersInternal(containerTitle, path, out response))
      {
        return response.Data;
      }

      throw new InvalidOperationException(response.StatusDescription);
    }

    public bool TryListFolders(Container container, string path, out IList<Folder> folders)
    {
      return TryListFolders(container.Title, path, out folders);
    }

    public IList<Folder> ListFolders(Container container, string path)
    {
      return ListFolders(container.Title, path);
    }

    public bool TryListAllFoldersInternal(string containerTitle, string path, out IRestResponse<List<Folder>> response)
    {
      var request = new RestRequest();
      if (String.IsNullOrEmpty(path) || String.IsNullOrEmpty(path.Trim()))
        request.Resource = "/v1/Folders.svc/Container({containerTitle})/Folders?AllFolders";
      else
        request.Resource = "/v1/Folders.svc/Container({containerTitle})/Folders/{path}?AllFolders";

      request.Method = Method.GET;
      request.RequestFormat = DataFormat.Json;
      request.XmlSerializer = null;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("path", path, ParameterType.UrlSegment);

      return TryExecute<List<Folder>>(request, out response);
    }

    public bool TryListAllFolders(string containerTitle, string path, out IList<Folder> folders)
    {
      folders = null;
      IRestResponse<List<Folder>> response;
      if (TryListAllFoldersInternal(containerTitle, path, out response))
      {
        folders = response.Data;
        return true;
      }
      return false;
    }

    public IList<Folder> ListAllFolders(string containerTitle, string path)
    {
      IRestResponse<List<Folder>> response;
      if (TryListAllFoldersInternal(containerTitle, path, out response))
      {
        return response.Data;
      }

      throw new InvalidOperationException(response.StatusDescription);
    }

    public bool TryListAllFolders(Container container, string path, out IList<Folder> folders)
    {
      return TryListAllFolders(container.Title, path, out folders);
    }

    public IList<Folder> ListAllFolders(Container container, string path)
    {
      return ListAllFolders(container.Title, path);
    }
    #endregion

    #region Entities
    private bool TryCreateEntityInternal(string containerTitle, string path, string @namespace, string data, out IRestResponse<Entity> response)
    {
      if (String.IsNullOrEmpty(@namespace))
        throw new ArgumentNullException("namespace");

      var request = new RestRequest();
      if (String.IsNullOrEmpty(path))
        request.Resource = "/v1/Entities.svc/Container({containerTitle})/Entities/?namespace={namespace}";
      else
        request.Resource = "/v1/Entities.svc/Container({containerTitle})/Entities/{path}?namespace={namespace}";
      request.Method = Method.POST;
      request.RequestFormat = DataFormat.Json;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      if (String.IsNullOrEmpty(path))
        request.AddParameter("path", String.Empty, ParameterType.UrlSegment);
      else
        request.AddParameter("path", path + "/", ParameterType.UrlSegment);

      request.AddParameter("namespace", @namespace, ParameterType.UrlSegment);
      request.AddBody(data);

      return TryExecute<Entity>(request, out response);
    }

    private bool TryCreateEntityInternal<T>(string containerTitle, string path, string @namespace, string data, out IRestResponse<Entity<T>> response)
    {
      if (String.IsNullOrEmpty(@namespace))
        throw new ArgumentNullException("namespace");

      var request = new RestRequest();
      request.Resource = "/v1/Entities.svc/Container({containerTitle})/Entities/{path}?namespace={namespace}";
      request.Method = Method.POST;
      request.RequestFormat = DataFormat.Json;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      if (String.IsNullOrEmpty(path))
        request.AddParameter("path", "/", ParameterType.UrlSegment);
      else
        request.AddParameter("path", path + "/", ParameterType.UrlSegment);

      request.AddParameter("namespace", @namespace, ParameterType.UrlSegment);
      request.AddBody(data);

      var result = TryExecute<Entity<T>>(request, out response);
      response.Data.Value = DocumentStoreHelper.DeserializeObjectFromJson<T>(response.Data.Data);
      return result;
    }

    public Entity CreateEntity(string containerTitle, string @namespace, string data)
    {
      return CreateEntity(containerTitle, String.Empty, @namespace, data);
    }

    public Entity<T> CreateEntity<T>(string containerTitle, string @namespace, string data)
    {
      return CreateEntity<T>(containerTitle, string.Empty, @namespace, data);
    }

    public Entity<T> CreateEntity<T>(string containerTitle, string @namespace, T value)
    {
      return CreateEntity<T>(containerTitle, string.Empty, @namespace, value);
    }

    public Entity CreateEntity(string containerTitle, string path, string @namespace, string data)
    {
      IRestResponse<Entity> response;
      if (TryCreateEntityInternal(containerTitle, path, @namespace, data, out response))
      {
        return response.Data;
      }

      throw new InvalidOperationException(response.StatusDescription);
    }

    public Entity<T> CreateEntity<T>(string containerTitle, string path, string @namespace, string data)
    {
      IRestResponse<Entity<T>> response;
      if (TryCreateEntityInternal<T>(containerTitle, path, @namespace, data, out response))
      {
        return response.Data;
      }

      throw new InvalidOperationException(response.StatusDescription);
    }

    public Entity<T> CreateEntity<T>(string containerTitle, string path, string @namespace, T value)
    {
      string data = DocumentStoreHelper.SerializeObjectToJson(value);

      IRestResponse<Entity<T>> response;
      if (TryCreateEntityInternal<T>(containerTitle, path, @namespace, data, out response))
      {
        return response.Data;
      }

      throw new InvalidOperationException(response.StatusDescription);
    }

    public Entity<T> GetEntity<T>(string containerTitle, Guid entityId)
    {
      var request = new RestRequest();
      request.Resource = "/v1/Entities.svc/Container({containerTitle})/Entity({guid})";
      request.Method = Method.GET;
      request.RequestFormat = DataFormat.Json;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("guid", entityId, ParameterType.UrlSegment);

      var result = Execute<Entity<T>>(request);
      result.Value = DocumentStoreHelper.DeserializeObjectFromJson<T>(result.Data);
      return result;
    }

    public Entity GetEntity(string containerTitle, Guid entityId)
    {
      var request = new RestRequest();
      request.Resource = "/v1/Entities.svc/Container({containerTitle})/Entity({guid})";
      request.Method = Method.GET;
      request.RequestFormat = DataFormat.Json;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("guid", entityId, ParameterType.UrlSegment);

      return Execute<Entity>(request);
    }

    public Entity<T> GetEntity<T>(string containerTitle, Guid entityId, string path)
    {
      var request = new RestRequest();
      request.Resource = "/v1/Entities.svc/Container({containerTitle})/Entity({guid})/{path}";
      request.Method = Method.GET;
      request.RequestFormat = DataFormat.Json;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("guid", entityId, ParameterType.UrlSegment);
      request.AddParameter("path", path, ParameterType.UrlSegment);

      var result = Execute<Entity<T>>(request);
      result.Value = DocumentStoreHelper.DeserializeObjectFromJson<T>(result.Data);
      return result;
    }

    public Entity GetEntity(string containerTitle, Guid entityId, string path)
    {
      var request = new RestRequest();
      request.Resource = "/v1/Entities.svc/Container({containerTitle})/Entity({guid})/{path}";
      request.Method = Method.GET;
      request.RequestFormat = DataFormat.Json;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("guid", entityId, ParameterType.UrlSegment);
      request.AddParameter("path", path, ParameterType.UrlSegment);

      return Execute<Entity>(request);
    }

    public string GetEntityContentsETag(string containerTitle, Guid entityId)
    {
      var request = new RestRequest();
      request.Resource = "/v1/Entities.svc/Container({containerTitle})/EntityContentsETag({guid})";
      request.Method = Method.GET;
      request.RequestFormat = DataFormat.Json;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("guid", entityId, ParameterType.UrlSegment);

      var result = Execute(request);
      return DocumentStoreHelper.DeserializeObjectFromJson<string>(result.Content);
    }

    public bool UpdateEntity<T>(string containerTitle, Guid entityId, T value)
    {
      var entity = GetEntity(containerTitle, entityId);
      entity.Data = DocumentStoreHelper.SerializeObjectToJson(value);
      return UpdateEntity(containerTitle, entity);
    }

    public bool UpdateEntity(string containerTitle, Entity entity)
    {
      var request = new RestRequest();
      request.Resource = "/v1/Entities.svc/Container({containerTitle})/Entities/";
      request.Method = Method.PUT;
      request.RequestFormat = DataFormat.Json;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddBody(entity);

      Execute(request);
      return true;
    }

    public bool DeleteEntity(string containerTitle, Guid entityId)
    {
      var request = new RestRequest();
      request.Resource = "/v1/Entities.svc/Container({containerTitle})/Entity({guid})";
      request.Method = Method.DELETE;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("guid", entityId, ParameterType.UrlSegment);

      Execute(request);
      return true;
    }

    public bool DeleteEntity(string containerTitle, Entity entity)
    {
      var request = new RestRequest();
      request.Resource = "/v1/Entities.svc/Container({containerTitle})/Entity({guid})";
      request.Method = Method.DELETE;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("guid", entity.Id, ParameterType.UrlSegment);

      Execute(request);
      return true;
    }

    public IList<Entity> ListEntities(string containerTitle)
    {
      return ListEntities(containerTitle, String.Empty, String.Empty);
    }

    public IList<Entity<T>> ListEntities<T>(string containerTitle)
    {
      return ListEntities<T>(containerTitle, String.Empty, String.Empty);
    }

    public IList<Entity> ListEntities(string containerTitle, string path)
    {
      return ListEntities(containerTitle, path, String.Empty);
    }

    public IList<Entity<T>> ListEntities<T>(string containerTitle, string path)
    {
      return ListEntities<T>(containerTitle, path, String.Empty);
    }

    public IList<Entity<T>> ListEntities<T>(string containerTitle, string path, string @namespace)
    {
      return ListEntities<T>(containerTitle, path, new EntityFilterCriteria() { Namespace = @namespace });
    }

    public IList<Entity> ListEntities(string containerTitle, string path, string @namespace)
    {
      return ListEntities(containerTitle, path, new EntityFilterCriteria() { Namespace = @namespace });
    }

    public IList<Entity<T>> ListEntities<T>(string containerTitle, EntityFilterCriteria criteria)
    {
      return ListEntities<T>(containerTitle, String.Empty, criteria);
    }

    public IList<Entity<T>> ListEntities<T>(string containerTitle, string path, EntityFilterCriteria criteria)
    {
      var request = new RestRequest();
      if (criteria != null)
      {
        UriBuilder builder = new UriBuilder("/v1/Entities.svc/Container({containerTitle})/Entities/{path}");
        QueryString qs = new QueryString();

        if (String.IsNullOrEmpty(criteria.Namespace) == false)
          qs.Add("$namespace", criteria.Namespace);

        if (criteria.Skip.HasValue)
          qs.Add("$skip", criteria.Skip.Value.ToString());

        if (criteria.Top.HasValue)
          qs.Add("$top", criteria.Top.Value.ToString());

        request.Resource = "/v1/Entities.svc/Container({containerTitle})/Entities/{path}" + qs.ToString();
      }
      else
        request.Resource = "/v1/Entities.svc/Container({containerTitle})/Entities/{path}";

      request.Method = Method.GET;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("path", path, ParameterType.UrlSegment);

      var result = Execute<List<Entity<T>>>(request);
      foreach (var r in result)
      {
        r.Value = DocumentStoreHelper.DeserializeObjectFromJson<T>(r.Data);
      }
      return result;
    }

    public IList<Entity> ListEntities(string containerTitle, EntityFilterCriteria criteria)
    {
      return ListEntities(containerTitle, String.Empty, criteria);
    }

    public IList<Entity> ListEntities(string containerTitle, string path, EntityFilterCriteria criteria)
    {
      var request = new RestRequest();
      if (criteria != null)
      {
        QueryString qs = new QueryString();

        if (String.IsNullOrEmpty(criteria.Namespace) == false)
          qs.Add("$namespace", criteria.Namespace);

        if (criteria.Skip.HasValue)
          qs.Add("$skip", criteria.Skip.Value.ToString());

        if (criteria.Top.HasValue)
          qs.Add("$top", criteria.Top.Value.ToString());

        request.Resource = "/v1/Entities.svc/Container({containerTitle})/Entities/{path}" + qs.RawString();
      }
      else
        request.Resource = "/v1/Entities.svc/Container({containerTitle})/Entities/{path}";

      request.Method = Method.GET;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("path", path, ParameterType.UrlSegment);

      return Execute<List<Entity>>(request);
    }

    public Entity ImportEntity(string containerTitle, Guid entityId, string @namespace, byte[] archiveData)
    {
      if (String.IsNullOrEmpty(@namespace))
        throw new ArgumentNullException("namespace");

      var request = new RestRequest();
      request.Resource = "/v1/Entities.svc/Container({containerTitle})/Entity({guid})/ImportEntity?namespace={namespace}";
      request.Method = Method.POST;
      request.RequestFormat = DataFormat.Json;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("guid", entityId, ParameterType.UrlSegment);
      request.AddParameter("namespace", @namespace, ParameterType.UrlSegment);
      request.AddFile("data", archiveData, "data.zip");

      return Execute<Entity>(request);
    }

    public Entity ImportEntity(string containerTitle, string path, Guid entityId, string @namespace, byte[] archiveData)
    {
      if (String.IsNullOrEmpty(@namespace))
        throw new ArgumentNullException("namespace");

      var request = new RestRequest();
      request.Resource = "/v1/Entities.svc/Container({containerTitle})/Entity({guid})/ImportEntity/{path}?namespace={namespace}";
      request.Method = Method.POST;
      request.RequestFormat = DataFormat.Json;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("guid", entityId, ParameterType.UrlSegment);
      request.AddParameter("path", path, ParameterType.UrlSegment);
      request.AddParameter("namespace", @namespace, ParameterType.UrlSegment);
      request.AddFile("data", archiveData, "data.zip");

      return Execute<Entity>(request);
    }

    public Stream ExportEntity(string containerTitle, Guid entityId)
    {
      var request = new RestRequest();
      request.Resource = "/v1/Entities.svc/Container({containerTitle})/Entity({guid})/ExportEntity";
      request.Method = Method.GET;
      request.RequestFormat = DataFormat.Json;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("guid", entityId, ParameterType.UrlSegment);

      var response = Execute(request);

      MemoryStream ms = new MemoryStream(response.RawBytes);
      return ms;
    }

    public bool MoveEntity(string containerTitle, Guid entityId, string destinationPath)
    {
      var request = new RestRequest();
      request.Resource = "/v1/Entities.svc/Container({containerTitle})/Entity({guid})/MoveEntity?destination={destinationPath}";
      request.Method = Method.POST;
      request.RequestFormat = DataFormat.Json;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("guid", entityId, ParameterType.UrlSegment);
      request.AddParameter("destinationPath", destinationPath, ParameterType.UrlSegment);

      var response = Execute(request);
      return true;
    }
    #endregion

    #region EntityParts
    public EntityPart<T> CreateEntityPart<T>(string containerTitle, Guid entityId, string partName, T value)
    {
      return CreateEntityPart(containerTitle, entityId, partName, String.Empty, value);
    }

    public EntityPart CreateEntityPart(string containerTitle, Guid entityId, string partName, string data)
    {
      return CreateEntityPart(containerTitle, entityId, partName, String.Empty, data);
    }

    public EntityPart<T> CreateEntityPart<T>(string containerTitle, Guid entityId, string partName, string category, T value)
    {
      string data = DocumentStoreHelper.SerializeObjectToJson(value);

      if (String.IsNullOrEmpty(partName))
        throw new ArgumentNullException("partName");

      var request = new RestRequest();
      request.Resource = "/v1/EntityParts.svc/Container({containerTitle})/Entity({entityId})/EntityParts?partName={partName}";
      request.Method = Method.POST;
      request.RequestFormat = DataFormat.Json;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("entityId", entityId, ParameterType.UrlSegment);
      request.AddParameter("partName", partName, ParameterType.UrlSegment);
      request.AddBody(data);

      var result = Execute<EntityPart<T>>(request);
      result.Value = DocumentStoreHelper.DeserializeObjectFromJson<T>(result.Data);
      return result;
    }

    public EntityPart CreateEntityPart(string containerTitle, Guid entityId, string partName, string category, string data)
    {
      if (String.IsNullOrEmpty(partName))
        throw new ArgumentNullException("partName");

      var request = new RestRequest();
      request.Resource = "/v1/EntityParts.svc/Container({containerTitle})/Entity({entityId})/EntityParts?partName={partName}";
      request.Method = Method.POST;
      request.RequestFormat = DataFormat.Json;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("entityId", entityId, ParameterType.UrlSegment);
      request.AddParameter("partName", partName, ParameterType.UrlSegment);
      request.AddBody(data);

      return Execute<EntityPart>(request);
    }

    public bool TryGetEntityPart<T>(string containerTitle, Guid entityId, string partName, out EntityPart<T> entityPart)
    {
      var request = new RestRequest();
      request.Resource = "/v1/EntityParts.svc/Container({containerTitle})/Entity({guid})/EntityPart({partName})";
      request.Method = Method.GET;
      request.RequestFormat = DataFormat.Json;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("guid", entityId, ParameterType.UrlSegment);
      request.AddParameter("partName", partName, ParameterType.UrlSegment);

      IRestResponse<EntityPart<T>> response;
      entityPart = null;
      bool success = TryExecute<EntityPart<T>>(request, out response);
      if (success)
      {
        entityPart = response.Data;
        entityPart.Value = DocumentStoreHelper.DeserializeObjectFromJson<T>(entityPart.Data);
      }
      return success;
    }

    public EntityPart<T> GetEntityPart<T>(string containerTitle, Guid entityId, string partName)
    {
      var request = new RestRequest();
      request.Resource = "/v1/EntityParts.svc/Container({containerTitle})/Entity({guid})/EntityPart({partName})";
      request.Method = Method.GET;
      request.RequestFormat = DataFormat.Json;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("guid", entityId, ParameterType.UrlSegment);
      request.AddParameter("partName", partName, ParameterType.UrlSegment);

      var result = Execute<EntityPart<T>>(request);
      result.Value = DocumentStoreHelper.DeserializeObjectFromJson<T>(result.Data);
      return result;
    }

    public EntityPart GetEntityPart(string containerTitle, Guid entityId, string partName)
    {
      var request = new RestRequest();
      request.Resource = "/v1/EntityParts.svc/Container({containerTitle})/Entity({guid})/EntityPart({partName})";
      request.Method = Method.GET;
      request.RequestFormat = DataFormat.Json;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("guid", entityId, ParameterType.UrlSegment);
      request.AddParameter("partName", partName, ParameterType.UrlSegment);

      return Execute<EntityPart>(request);
    }

    public string GetEntityPartETag(string containerTitle, Guid entityId, string partName)
    {
      var request = new RestRequest();
      request.Resource = "/v1/EntityParts.svc/Container({containerTitle})/Entity({guid})/EntityPartETag({partName})";
      request.Method = Method.GET;
      request.RequestFormat = DataFormat.Json;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("guid", entityId, ParameterType.UrlSegment);
      request.AddParameter("partName", partName, ParameterType.UrlSegment);

      var result = Execute(request);
      return DocumentStoreHelper.DeserializeObjectFromJson<string>(result.Content);
    }

    public bool RenameEntityPart(string containerTitle, Guid entityId, string partName, string newPartName)
    {
      var request = new RestRequest();
      request.Resource = "/v1/EntityParts.svc/Container({containerTitle})/Entity({guid})/EntityPart({partName})?newPartName={newPartName}";
      request.Method = Method.PUT;
      request.RequestFormat = DataFormat.Json;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("guid", entityId.ToString(), ParameterType.UrlSegment);
      request.AddParameter("partName", partName, ParameterType.UrlSegment);
      request.AddParameter("newPartName", newPartName, ParameterType.UrlSegment);

      Execute(request);
      return true;
    }

    public bool UpdateEntityPart<T>(string containerTitle, Guid entityId, string partName, T value)
    {
      var entityPart = GetEntityPart<T>(containerTitle, entityId, partName);
      entityPart.Data = DocumentStoreHelper.SerializeObjectToJson<T>(value);
      return UpdateEntityPart(containerTitle, entityId, entityPart);
    }

    public bool UpdateEntityPart(string containerTitle, Guid entityId, EntityPart entityPart)
    {
      var request = new RestRequest();
      request.Resource = "/v1/EntityParts.svc/Container({containerTitle})/Entity({guid})/EntityParts/";
      request.Method = Method.PUT;
      request.RequestFormat = DataFormat.Json;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("guid", entityId, ParameterType.UrlSegment);
      request.AddBody(entityPart);

      Execute(request);
      return true;
    }

    public bool DeleteEntityPart(string containerTitle, Guid entityId, string partName)
    {
      var request = new RestRequest();
      request.Resource = "/v1/EntityParts.svc/Container({containerTitle})/Entity({guid})/EntityPart({partName})";
      request.Method = Method.DELETE;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("guid", entityId, ParameterType.UrlSegment);
      request.AddParameter("partName", partName, ParameterType.UrlSegment);

      Execute(request);
      return true;
    }

    public IList<EntityPart> ListEntityParts(string containerTitle, Guid entityId)
    {
      var request = new RestRequest();
      request.Resource = "/v1/EntityParts.svc/Container({containerTitle})/Entity({entityId})/EntityParts";
      request.Method = Method.GET;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("entityId", entityId, ParameterType.UrlSegment);

      return Execute<List<EntityPart>>(request);
    }
    #endregion

    #region Attachments
    public Attachment UploadAttachment(string containerTitle, Guid entityId, string fileName, byte[] attachment)
    {
      return UploadAttachment(containerTitle, entityId, fileName, attachment, String.Empty, String.Empty);
    }

    public Attachment UploadAttachment(string containerTitle, Guid entityId, string fileName, byte[] attachment, string category, string path)
    {
      var request = new RestRequest();
      if (String.IsNullOrEmpty(category) && string.IsNullOrEmpty(path))
        request.Resource = "/v1/Attachments.svc/Container({containerTitle})/Entity({guid})/Attachments";
      else if (string.IsNullOrEmpty(category) == false && string.IsNullOrEmpty(path))
        request.Resource = "/v1/Attachments.svc/Container({containerTitle})/Entity({guid})/Attachments?category={category}";
      else if (string.IsNullOrEmpty(category) && string.IsNullOrEmpty(path) == false)
        request.Resource = "/v1/Attachments.svc/Container({containerTitle})/Entity({guid})/Attachments?path={path}";
      else
        request.Resource = "/v1/Attachments.svc/Container({containerTitle})/Entity({guid})/Attachments?category={category}&path={path}";
      request.Method = Method.POST;
      request.RequestFormat = DataFormat.Json;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("guid", entityId, ParameterType.UrlSegment);

      if (string.IsNullOrEmpty(category) == false)
        request.AddParameter("category", category, ParameterType.UrlSegment);

      if (string.IsNullOrEmpty(path) == false)
        request.AddParameter("path", path, ParameterType.UrlSegment);

      request.AddFile("data", attachment, fileName);

      return Execute<Attachment>(request);
    }

    public Attachment UploadAttachmentFromSourceUrl(string containerTitle, Guid entityId, string fileName, string sourceUrl, string category, string path)
    {
      var request = new RestRequest();
      StringBuilder url = new StringBuilder("/v1/Attachments.svc/Container({containerTitle})/Entity({guid})/Attachments?sourceUrl={sourceUrl}&fileName={fileName}");
      if (String.IsNullOrEmpty(category) == false)
        url.Append("&category={category}");
      if (String.IsNullOrEmpty(path) == false)
        url.Append("&path={path}");

      request.Resource = url.ToString();
      request.Method = Method.POST;
      request.RequestFormat = DataFormat.Json;

      request.AddParameter("fileName", fileName, ParameterType.UrlSegment);
      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("guid", entityId, ParameterType.UrlSegment);
      request.AddParameter("sourceUrl", sourceUrl, ParameterType.UrlSegment);
      request.AddParameter("path", path, ParameterType.UrlSegment);
      request.AddParameter("category", category, ParameterType.UrlSegment);

      return Execute<Attachment>(request);
    }

    public Attachment GetAttachment(string containerTitle, Guid entityId, string fileName)
    {
      var request = new RestRequest();
      request.Resource = "/v1/Attachments.svc/Container({containerTitle})/Entity({guid})/Attachment({fileName})";
      request.Method = Method.GET;
      request.RequestFormat = DataFormat.Json;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("guid", entityId, ParameterType.UrlSegment);
      request.AddParameter("fileName", fileName, ParameterType.UrlSegment);
      
      return Execute<Attachment>(request);
    }

    public bool RenameAttachment(string containerTitle, Guid entityId, string fileName, string newFileName)
    {
      var request = new RestRequest();
      request.Resource = "/v1/Attachments.svc/Container({containerTitle})/Entity({guid})/Attachment({fileName})?newFileName={newFileName}";
      request.Method = Method.PUT;
      request.RequestFormat = DataFormat.Json;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("guid", entityId, ParameterType.UrlSegment);
      request.AddParameter("fileName", fileName, ParameterType.UrlSegment);
      request.AddParameter("newFileName", newFileName, ParameterType.UrlSegment);

      Execute(request);
      return true;
    }

    public bool DeleteAttachment(string containerTitle, Guid entityId, string fileName)
    {
      var request = new RestRequest();
      request.Resource = "/v1/Attachments.svc/Container({containerTitle})/Entity({guid})/Attachment({fileName})";
      request.Method = Method.DELETE;
      request.RequestFormat = DataFormat.Json;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("guid", entityId, ParameterType.UrlSegment);
      request.AddParameter("fileName", fileName, ParameterType.UrlSegment);

      Execute(request);
      return true;
    }

    public IList<Attachment> ListAttachments(string containerTitle, Guid entityId)
    {
      var request = new RestRequest();
      request.Resource = "/v1/Attachments.svc/Container({containerTitle})/Entity({entityId})/Attachments";
      request.Method = Method.GET;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("entityId", entityId, ParameterType.UrlSegment);

      return Execute<List<Attachment>>(request);
    }

    public Stream DownloadAttachment(string containerTitle, Guid entityId, string fileName)
    {
      var request = new RestRequest();
      request.Resource = "/v1/Attachments.svc/Container({containerTitle})/Entity({entityId})/Attachments/{fileName}";
      request.Method = Method.GET;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("entityId", entityId, ParameterType.UrlSegment);
      request.AddParameter("fileName", fileName, ParameterType.UrlSegment);

      var response = Execute(request);
      MemoryStream ms = new MemoryStream(response.RawBytes);
      return ms;
    }
    #endregion

    #region Metadata
    
    public string GetContainerMetadata(string containerTitle, string key)
    {
      var request = new RestRequest();
      request.Resource = "/v1/Metadata.svc/Container({containerTitle})/Metadata({key})";
      request.Method = Method.GET;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("key", key, ParameterType.UrlSegment);

      var result = Execute(request);
      return Newtonsoft.Json.JsonConvert.DeserializeObject<string>(result.Content);
    }

    public bool SetContainerMetadata(string containerTitle, string key, string value)
    {
      var request = new RestRequest();
      request.Resource = "/v1/Metadata.svc/Container({containerTitle})/Metadata({key})";
      request.Method = Method.POST;
      request.RequestFormat = DataFormat.Json;
      request.XmlSerializer = null;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("key", key, ParameterType.UrlSegment);
      request.AddBody(value);

      Execute(request);
      return true;
    }

    public IDictionary<string, string> ListContainerMetadata(string containerTitle)
    {
      var request = new RestRequest();
      request.Resource = "/v1/Metadata.svc/Container({containerTitle})/Metadata";
      request.Method = Method.GET;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);

      var result = Execute<Dictionary<string, string>>(request);
      return result;
    }

    public string GetEntityMetadata(string containerTitle, Guid entityId, string key)
    {
      var request = new RestRequest();
      request.Resource = "/v1/Metadata.svc/Container({containerTitle})/Entity({entityId})/Metadata({key})";
      request.Method = Method.GET;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("entityId", entityId, ParameterType.UrlSegment);
      request.AddParameter("key", key, ParameterType.UrlSegment);

      var result = Execute(request);
      return Newtonsoft.Json.JsonConvert.DeserializeObject<string>(result.Content);
    }

    public bool SetEntityMetadata(string containerTitle, Guid entityId, string key, string value)
    {
      var request = new RestRequest();
      request.Resource = "/v1/Metadata.svc/Container({containerTitle})/Entity({entityId})/Metadata({key})";
      request.Method = Method.POST;
      request.RequestFormat = DataFormat.Json;
      request.XmlSerializer = null;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("entityId", entityId, ParameterType.UrlSegment);
      request.AddParameter("key", key, ParameterType.UrlSegment);
      request.AddBody(value);

      Execute(request);
      return true;
    }

    public IDictionary<string, string> ListEntityMetadata(string containerTitle, Guid entityId)
    {
      var request = new RestRequest();
      request.Resource = "/v1/Metadata.svc/Container({containerTitle})/Entity({entityId})/Metadata";
      request.Method = Method.GET;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("entityId", entityId, ParameterType.UrlSegment);

      var result = Execute<Dictionary<string, string>>(request);
      return result;
    }

    public string GetEntityPartMetadata(string containerTitle, Guid entityId, string partName, string key)
    {
      var request = new RestRequest();
      request.Resource = "/v1/Metadata.svc/Container({containerTitle})/Entity({entityId})/EntityPart({partName})/Metadata({key})";
      request.Method = Method.GET;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("entityId", entityId, ParameterType.UrlSegment);
      request.AddParameter("partName", partName, ParameterType.UrlSegment);
      request.AddParameter("key", key, ParameterType.UrlSegment);

      var result = Execute(request);
      return Newtonsoft.Json.JsonConvert.DeserializeObject<string>(result.Content);
    }

    public bool SetEntityPartMetadata(string containerTitle, Guid entityId, string partName, string key, string value)
    {
      var request = new RestRequest();
      request.Resource = "/v1/Metadata.svc/Container({containerTitle})/Entity({entityId})/EntityPart({partName})/Metadata({key})";
      request.Method = Method.POST;
      request.RequestFormat = DataFormat.Json;
      request.XmlSerializer = null;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("entityId", entityId, ParameterType.UrlSegment);
      request.AddParameter("partName", partName, ParameterType.UrlSegment);
      request.AddParameter("key", key, ParameterType.UrlSegment);
      request.AddBody(value);

      Execute(request);
      return true;
    }

    public IDictionary<string, string> ListEntityPartMetadata(string containerTitle, Guid entityId, string partName)
    {
      var request = new RestRequest();
      request.Resource = "/v1/Metadata.svc/Container({containerTitle})/Entity({entityId})/EntityPart({partName})/Metadata";
      request.Method = Method.GET;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("entityId", entityId, ParameterType.UrlSegment);
      request.AddParameter("partName", partName, ParameterType.UrlSegment);

      var result = Execute<Dictionary<string, string>>(request);
      return result;
    }

    public string GetAttachmentMetadata(string containerTitle, Guid entityId, string fileName, string key)
    {
      var request = new RestRequest();
      request.Resource = "/v1/Metadata.svc/Container({containerTitle})/Entity({entityId})/Attachment({fileName})/Metadata({key})";
      request.Method = Method.GET;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("entityId", entityId, ParameterType.UrlSegment);
      request.AddParameter("fileName", fileName, ParameterType.UrlSegment);
      request.AddParameter("key", key, ParameterType.UrlSegment);

      var result = Execute(request);
      return Newtonsoft.Json.JsonConvert.DeserializeObject<string>(result.Content);
    }

    public bool SetAttachmentMetadata(string containerTitle, Guid entityId, string fileName, string key, string value)
    {
      var request = new RestRequest();
      request.Resource = "/v1/Metadata.svc/Container({containerTitle})/Entity({entityId})/Attachment({fileName})/Metadata({key})";
      request.Method = Method.POST;
      request.RequestFormat = DataFormat.Json;
      request.XmlSerializer = null;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("entityId", entityId, ParameterType.UrlSegment);
      request.AddParameter("fileName", fileName, ParameterType.UrlSegment);
      request.AddParameter("key", key, ParameterType.UrlSegment);
      request.AddBody(value);

      Execute(request);
      return true;
    }

    public IDictionary<string, string> ListAttachmentMetadata(string containerTitle, Guid entityId, string fileName)
    {
      var request = new RestRequest();
      request.Resource = "/v1/Metadata.svc/Container({containerTitle})/Entity({entityId})/Attachment({fileName})/Metadata";
      request.Method = Method.GET;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("entityId", entityId, ParameterType.UrlSegment);
      request.AddParameter("fileName", fileName, ParameterType.UrlSegment);

      var result = Execute<Dictionary<string, string>>(request);
      return result;
    }
    #endregion

    #region Comments
    public Comment AddEntityComment(string containerTitle, Guid entityId, string comment)
    {
      var request = new RestRequest();
      request.Resource = "/v1/Comments.svc/Container({containerTitle})/Entity({entityId})/Comments";
      request.Method = Method.POST;
      request.RequestFormat = DataFormat.Json;
      request.XmlSerializer = null;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("entityId", entityId, ParameterType.UrlSegment);
      request.AddBody(comment);

      return Execute<Comment>(request);
    }

    public IList<Comment> ListEntityComments(string containerTitle, Guid entityId)
    {
      var request = new RestRequest();
      request.Resource = "/v1/Comments.svc/Container({containerTitle})/Entity({entityId})/Comments";
      request.Method = Method.GET;
      request.RequestFormat = DataFormat.Json;
      request.XmlSerializer = null;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("entityId", entityId, ParameterType.UrlSegment);

      return Execute<List<Comment>>(request);
    }

    public Comment AddEntityPartComment(string containerTitle, Guid entityId, string partName, string comment)
    {
      var request = new RestRequest();
      request.Resource = "/v1/Comments.svc/Container({containerTitle})/Entity({entityId})/EntityPart({partName})/Comments";
      request.Method = Method.POST;
      request.RequestFormat = DataFormat.Json;
      request.XmlSerializer = null;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("entityId", entityId, ParameterType.UrlSegment);
      request.AddParameter("partName", partName, ParameterType.UrlSegment);
      request.AddBody(comment);

      return Execute<Comment>(request);
    }

    public IList<Comment> ListEntityPartComments(string containerTitle, Guid entityId, string partName)
    {
      var request = new RestRequest();
      request.Resource = "/v1/Comments.svc/Container({containerTitle})/Entity({entityId})/EntityPart({partName})/Comments";
      request.Method = Method.GET;
      request.RequestFormat = DataFormat.Json;
      request.XmlSerializer = null;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("entityId", entityId, ParameterType.UrlSegment);
      request.AddParameter("partName", partName, ParameterType.UrlSegment);

      return Execute<List<Comment>>(request);
    }

    public Comment AddAttachmentComment(string containerTitle, Guid entityId, string fileName, string comment)
    {
      var request = new RestRequest();
      request.Resource = "/v1/Comments.svc/Container({containerTitle})/Entity({entityId})/Attachment({fileName})/Comments";
      request.Method = Method.POST;
      request.RequestFormat = DataFormat.Json;
      request.XmlSerializer = null;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("entityId", entityId, ParameterType.UrlSegment);
      request.AddParameter("fileName", fileName, ParameterType.UrlSegment);
      request.AddBody(comment);

      return Execute<Comment>(request);
    }

    public IList<Comment> ListAttachmentComments(string containerTitle, Guid entityId, string fileName)
    {
      var request = new RestRequest();
      request.Resource = "/v1/Comments.svc/Container({containerTitle})/Entity({entityId})/Attachment({fileName})/Comments";
      request.Method = Method.GET;
      request.RequestFormat = DataFormat.Json;
      request.XmlSerializer = null;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("entityId", entityId, ParameterType.UrlSegment);
      request.AddParameter("fileName", fileName, ParameterType.UrlSegment);

      return Execute<List<Comment>>(request);
    }
    #endregion

    #region Permissions
    public PrincipalRoleInfo AddPrincipalRoleToContainer(string containerTitle, string principalName, string principalType, string roleName)
    {
      var request = new RestRequest();
      request.Resource = "/v1/Permissions.svc/Container({containerTitle})/Permissions/{principalType}/{roleName}/{principalName}";
      request.Method = Method.POST;
      request.RequestFormat = DataFormat.Json;
      request.XmlSerializer = null;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("principalName", principalName, ParameterType.UrlSegment);
      request.AddParameter("principalType", principalType, ParameterType.UrlSegment);
      request.AddParameter("roleName", roleName, ParameterType.UrlSegment);

      var result = Execute(request);

      if (String.IsNullOrEmpty(result.Content))
        return null;

      return DocumentStoreHelper.DeserializeObjectFromJson<PrincipalRoleInfo>(result.Content);
    }
    public bool RemovePrincipalRoleFromContainer(string containerTitle, string principalName, string principalType, string roleName)
    {
      var request = new RestRequest();
      request.Resource = "/v1/Permissions.svc/Container({containerTitle})/Permissions/{principalType}/{roleName}/{principalName}";
      request.Method = Method.DELETE;
      request.RequestFormat = DataFormat.Json;
      request.XmlSerializer = null;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("principalName", principalName, ParameterType.UrlSegment);
      request.AddParameter("principalType", principalType, ParameterType.UrlSegment);
      request.AddParameter("roleName", roleName, ParameterType.UrlSegment);

      var response = Execute(request);
      return DocumentStoreHelper.DeserializeObjectFromJson<bool>(response.Content);
    }

    public PermissionsInfo GetContainerPermissions(string containerTitle)
    {
      var request = new RestRequest();
      request.Resource = "/v1/Permissions.svc/Container({containerTitle})/Permissions";
      request.Method = Method.GET;
      request.RequestFormat = DataFormat.Json;
      request.XmlSerializer = null;
      
      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);

      var result = Execute(request);

      if (String.IsNullOrEmpty(result.Content))
        return null;

      return DocumentStoreHelper.DeserializeObjectFromJson<PermissionsInfo>(result.Content);
    }

    public PrincipalRoleInfo GetContainerPermissionsForPrincipal(string containerTitle, string principalName, string principalType)
    {
      var request = new RestRequest();
      request.Resource = "/v1/Permissions.svc/Container({containerTitle})/Permissions/{principalType}/?name={principalName}";
      request.Method = Method.GET;
      request.RequestFormat = DataFormat.Json;
      request.XmlSerializer = null;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("principalName", principalName, ParameterType.UrlSegment);
      request.AddParameter("principalType", principalType, ParameterType.UrlSegment);

      var result = Execute(request);

      if (String.IsNullOrEmpty(result.Content))
        return null;

      return DocumentStoreHelper.DeserializeObjectFromJson<PrincipalRoleInfo>(result.Content);
    }

    public PermissionsInfo ResetContainerPermissions(string containerTitle)
    {
      var request = new RestRequest();
      request.Resource = "/v1/Permissions.svc/Container({containerTitle})/Permissions";
      request.Method = Method.DELETE;
      request.RequestFormat = DataFormat.Json;
      request.XmlSerializer = null;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);

      var result = Execute(request);

      if (String.IsNullOrEmpty(result.Content))
        return null;

      return DocumentStoreHelper.DeserializeObjectFromJson<PermissionsInfo>(result.Content);
    }

    public PrincipalRoleInfo AddPrincipalRoleToEntity(string containerTitle, Guid guid, string principalName, string principalType, string roleName)
    {
      var request = new RestRequest();
      request.Resource = "/v1/Permissions.svc/Container({containerTitle})/Entity({guid})/Permissions/{principalType}/{roleName}/{principalName}";
      request.Method = Method.POST;
      request.RequestFormat = DataFormat.Json;
      request.XmlSerializer = null;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("guid", guid, ParameterType.UrlSegment);
      request.AddParameter("principalName", principalName, ParameterType.UrlSegment);
      request.AddParameter("principalType", principalType, ParameterType.UrlSegment);
      request.AddParameter("roleName", roleName, ParameterType.UrlSegment);

      var result = Execute(request);

      if (String.IsNullOrEmpty(result.Content))
        return null;

      return DocumentStoreHelper.DeserializeObjectFromJson<PrincipalRoleInfo>(result.Content);
    }

    public bool RemovePrincipalRoleFromEntity(string containerTitle, Guid guid, string principalName, string principalType, string roleName)
    {
      var request = new RestRequest();
      request.Resource = "/v1/Permissions.svc/Container({containerTitle})/Entity({guid})/Permissions/{principalType}/{roleName}/{principalName}";
      request.Method = Method.DELETE;
      request.RequestFormat = DataFormat.Json;
      request.XmlSerializer = null;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("guid", guid, ParameterType.UrlSegment);
      request.AddParameter("principalName", principalName, ParameterType.UrlSegment);
      request.AddParameter("principalType", principalType, ParameterType.UrlSegment);
      request.AddParameter("roleName", roleName, ParameterType.UrlSegment);

      var response = Execute(request);
      return DocumentStoreHelper.DeserializeObjectFromJson<bool>(response.Content);
    }

    public PermissionsInfo GetEntityPermissions(string containerTitle, Guid guid)
    {
      var request = new RestRequest();
      request.Resource = "/v1/Permissions.svc/Container({containerTitle}/Entity({guid})/Permissions";
      request.Method = Method.GET;
      request.RequestFormat = DataFormat.Json;
      request.XmlSerializer = null;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("guid", guid, ParameterType.UrlSegment);

      var result = Execute(request);

      if (String.IsNullOrEmpty(result.Content))
        return null;

      return DocumentStoreHelper.DeserializeObjectFromJson<PermissionsInfo>(result.Content);
    }

    public PrincipalRoleInfo GetEntityPermissionsForPrincipal(string containerTitle, Guid guid, string principalName, string principalType)
    {
      var request = new RestRequest();
      request.Resource = "/v1/Permissions.svc/Container({containerTitle})/Entity({guid})/Permissions/{principalType}/?name={principalName}";
      request.Method = Method.GET;
      request.RequestFormat = DataFormat.Json;
      request.XmlSerializer = null;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("guid", guid, ParameterType.UrlSegment);
      request.AddParameter("principalName", principalName, ParameterType.UrlSegment);
      request.AddParameter("principalType", principalType, ParameterType.UrlSegment);

      var result = Execute(request);

      if (String.IsNullOrEmpty(result.Content))
        return null;

      return DocumentStoreHelper.DeserializeObjectFromJson<PrincipalRoleInfo>(result.Content);
    }

    public PermissionsInfo ResetEntityPermissions(string containerTitle, Guid guid)
    {
      var request = new RestRequest();
      request.Resource = "/v1/Permissions.svc/Container({containerTitle})/Entity({guid})/Permissions";
      request.Method = Method.DELETE;
      request.RequestFormat = DataFormat.Json;
      request.XmlSerializer = null;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("guid", guid, ParameterType.UrlSegment);

      var result = Execute(request);

      if (String.IsNullOrEmpty(result.Content))
        return null;

      return DocumentStoreHelper.DeserializeObjectFromJson<PermissionsInfo>(result.Content);
    }

    public PrincipalRoleInfo AddPrincipalRoleToEntityPart(string containerTitle, Guid guid, string partName, string principalName, string principalType, string roleName)
    {
      var request = new RestRequest();
      request.Resource = "/v1/Permissions.svc/Container({containerTitle})/Entity({guid})/EntityPart({partName})/Permissions/{principalType}/{roleName}/{principalName}";
      request.Method = Method.POST;
      request.RequestFormat = DataFormat.Json;
      request.XmlSerializer = null;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("guid", guid, ParameterType.UrlSegment);
      request.AddParameter("partName", partName, ParameterType.UrlSegment);
      request.AddParameter("principalName", principalName, ParameterType.UrlSegment);
      request.AddParameter("principalType", principalType, ParameterType.UrlSegment);
      request.AddParameter("roleName", roleName, ParameterType.UrlSegment);

      var result = Execute(request);

      if (String.IsNullOrEmpty(result.Content))
        return null;

      return DocumentStoreHelper.DeserializeObjectFromJson<PrincipalRoleInfo>(result.Content);
    }

    public bool RemovePrincipalRoleFromEntityPart(string containerTitle, Guid guid, string partName, string principalName, string principalType, string roleName)
    {
      var request = new RestRequest();
      request.Resource = "/v1/Permissions.svc/Container({containerTitle})/Entity({guid})/EntityPart({partName})/Permissions/{principalType}/{roleName}/{principalName}";
      request.Method = Method.DELETE;
      request.RequestFormat = DataFormat.Json;
      request.XmlSerializer = null;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("guid", guid, ParameterType.UrlSegment);
      request.AddParameter("partName", partName, ParameterType.UrlSegment);
      request.AddParameter("principalName", principalName, ParameterType.UrlSegment);
      request.AddParameter("principalType", principalType, ParameterType.UrlSegment);
      request.AddParameter("roleName", roleName, ParameterType.UrlSegment);

      var response = Execute(request);
      return DocumentStoreHelper.DeserializeObjectFromJson<bool>(response.Content);
    }

    public PermissionsInfo GetEntityPartPermissions(string containerTitle, Guid guid, string partName)
    {
      var request = new RestRequest();
      request.Resource = "/v1/Permissions.svc/Container({containerTitle}/Entity({guid})/EntityPart({partName})/Permissions";
      request.Method = Method.GET;
      request.RequestFormat = DataFormat.Json;
      request.XmlSerializer = null;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("guid", guid, ParameterType.UrlSegment);
      request.AddParameter("partName", partName, ParameterType.UrlSegment);

      var result = Execute(request);

      if (String.IsNullOrEmpty(result.Content))
        return null;

      return DocumentStoreHelper.DeserializeObjectFromJson<PermissionsInfo>(result.Content);
    }

    public PrincipalRoleInfo GetEntityPartPermissionsForPrincipal(string containerTitle, Guid guid, string partName, string principalName, string principalType)
    {
      var request = new RestRequest();
      request.Resource = "/v1/Permissions.svc/Container({containerTitle})/Entity({guid})/EntityPart({partName})/Permissions/{principalType}/?name={principalName}";
      request.Method = Method.GET;
      request.RequestFormat = DataFormat.Json;
      request.XmlSerializer = null;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("guid", guid, ParameterType.UrlSegment);
      request.AddParameter("partName", partName, ParameterType.UrlSegment);
      request.AddParameter("principalName", principalName, ParameterType.UrlSegment);
      request.AddParameter("principalType", principalType, ParameterType.UrlSegment);

      var result = Execute(request);

      if (String.IsNullOrEmpty(result.Content))
        return null;

      return DocumentStoreHelper.DeserializeObjectFromJson<PrincipalRoleInfo>(result.Content);
    }

    public PermissionsInfo ResetEntityPartPermissions(string containerTitle, Guid guid, string partName)
    {
      var request = new RestRequest();
      request.Resource = "/v1/Permissions.svc/Container({containerTitle})/Entity({guid})/EntityPart({partName})/Permissions";
      request.Method = Method.DELETE;
      request.RequestFormat = DataFormat.Json;
      request.XmlSerializer = null;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("guid", guid, ParameterType.UrlSegment);
      request.AddParameter("partName", partName, ParameterType.UrlSegment);

      var result = Execute(request);

      if (String.IsNullOrEmpty(result.Content))
        return null;

      return DocumentStoreHelper.DeserializeObjectFromJson<PermissionsInfo>(result.Content);
    }

    public PrincipalRoleInfo AddPrincipalRoleToAttachment(string containerTitle, Guid guid, string fileName, string principalName, string principalType, string roleName)
    {
      var request = new RestRequest();
      request.Resource = "/v1/Permissions.svc/Container({containerTitle})/Entity({guid})/Attachment({fileName})/Permissions/{principalType}/{roleName}/{principalName}";
      request.Method = Method.POST;
      request.RequestFormat = DataFormat.Json;
      request.XmlSerializer = null;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("guid", guid, ParameterType.UrlSegment);
      request.AddParameter("fileName", fileName, ParameterType.UrlSegment);
      request.AddParameter("principalName", principalName, ParameterType.UrlSegment);
      request.AddParameter("principalType", principalType, ParameterType.UrlSegment);
      request.AddParameter("roleName", roleName, ParameterType.UrlSegment);

      var result = Execute(request);

      if (String.IsNullOrEmpty(result.Content))
        return null;

      return DocumentStoreHelper.DeserializeObjectFromJson<PrincipalRoleInfo>(result.Content);
    }

    public bool RemovePrincipalRoleFromAttachment(string containerTitle, Guid guid, string fileName, string principalName, string principalType, string roleName)
    {
      var request = new RestRequest();
      request.Resource = "/v1/Permissions.svc/Container({containerTitle})/Entity({guid})/Attachment({fileName})/Permissions/{principalType}/{roleName}/{principalName}";
      request.Method = Method.DELETE;
      request.RequestFormat = DataFormat.Json;
      request.XmlSerializer = null;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("guid", guid, ParameterType.UrlSegment);
      request.AddParameter("fileName", fileName, ParameterType.UrlSegment);
      request.AddParameter("principalName", principalName, ParameterType.UrlSegment);
      request.AddParameter("principalType", principalType, ParameterType.UrlSegment);
      request.AddParameter("roleName", roleName, ParameterType.UrlSegment);

      var response = Execute(request);
      return DocumentStoreHelper.DeserializeObjectFromJson<bool>(response.Content);
    }

    public PermissionsInfo GetAttachmentPermissions(string containerTitle, Guid guid, string fileName)
    {
      var request = new RestRequest();
      request.Resource = "/v1/Permissions.svc/Container({containerTitle}/Entity({guid})/Attachment({fileName})/Permissions";
      request.Method = Method.GET;
      request.RequestFormat = DataFormat.Json;
      request.XmlSerializer = null;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("guid", guid, ParameterType.UrlSegment);
      request.AddParameter("fileName", fileName, ParameterType.UrlSegment);

      var result = Execute(request);

      if (String.IsNullOrEmpty(result.Content))
        return null;

      return DocumentStoreHelper.DeserializeObjectFromJson<PermissionsInfo>(result.Content);
    }

    public PrincipalRoleInfo GetAttachmentPermissionsForPrincipal(string containerTitle, Guid guid, string fileName, string principalName, string principalType)
    {
      var request = new RestRequest();
      request.Resource = "/v1/Permissions.svc/Container({containerTitle})/Entity({guid})/Attachment({fileName})/Permissions/{principalType}/?name={principalName}";
      request.Method = Method.GET;
      request.RequestFormat = DataFormat.Json;
      request.XmlSerializer = null;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("guid", guid, ParameterType.UrlSegment);
      request.AddParameter("fileName", fileName, ParameterType.UrlSegment);
      request.AddParameter("principalName", principalName, ParameterType.UrlSegment);
      request.AddParameter("principalType", principalType, ParameterType.UrlSegment);

      var result = Execute(request);

      if (String.IsNullOrEmpty(result.Content))
        return null;

      return DocumentStoreHelper.DeserializeObjectFromJson<PrincipalRoleInfo>(result.Content);
    }

    public PermissionsInfo ResetAttachmentPermissions(string containerTitle, Guid guid, string fileName)
    {
      var request = new RestRequest();
      request.Resource = "/v1/Permissions.svc/Container({containerTitle})/Entity({guid})/Attachment({fileName})/Permissions";
      request.Method = Method.DELETE;
      request.RequestFormat = DataFormat.Json;
      request.XmlSerializer = null;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("guid", guid, ParameterType.UrlSegment);
      request.AddParameter("fileName", fileName, ParameterType.UrlSegment);

      var result = Execute(request);

      if (String.IsNullOrEmpty(result.Content))
        return null;

      return DocumentStoreHelper.DeserializeObjectFromJson<PermissionsInfo>(result.Content);
    }
    #endregion

    #region Version History
    public IList<EntityVersion> ListEntityVersions(string containerTitle, Guid entityId)
    {
      var request = new RestRequest();
      request.Resource = "/v1/VersionHistory.svc/Container({containerTitle})/Entity({entityId})/Versions";
      request.Method = Method.GET;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("entityId", entityId, ParameterType.UrlSegment);

      var result = Execute(request);
      return Newtonsoft.Json.JsonConvert.DeserializeObject<IList<EntityVersion>>(result.Content);
    }

    public EntityVersion GetEntityVersion(string containerTitle, Guid entityId, int versionId)
    {
      var request = new RestRequest();
      request.Resource = "/v1/VersionHistory.svc/Container({containerTitle})/Entity({entityId})/Versions/{versionId}";
      request.Method = Method.GET;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("entityId", entityId, ParameterType.UrlSegment);
      request.AddParameter("versionId", versionId, ParameterType.UrlSegment);

      var result = Execute(request);
      return Newtonsoft.Json.JsonConvert.DeserializeObject<EntityVersion>(result.Content);
    }

    public EntityVersion RevertEntityToVersion(string containerTitle, Guid entityId, int versionId)
    {
      var request = new RestRequest();
      request.Resource = "/v1/VersionHistory.svc/Container({containerTitle})/Entity({entityId})/Versions/RevertTo?version={versionId}";
      request.Method = Method.POST;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      request.AddParameter("entityId", entityId, ParameterType.UrlSegment);
      request.AddParameter("versionId", versionId, ParameterType.UrlSegment);

      var result = Execute(request);
      return Newtonsoft.Json.JsonConvert.DeserializeObject<EntityVersion>(result.Content);
    }
    #endregion

    #region Locking
    public LockStatus GetEntityLockStatus(string containerTitle, string path, Guid entityId)
    {
      var request = new RestRequest();
      request.Resource = "/v1/Lock.svc/Container({containerTitle})/Entity({guid})/EntityLockStatus/{path}";
      request.Method = Method.GET;
      request.RequestFormat = DataFormat.Json;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      if (String.IsNullOrEmpty(path))
        request.AddParameter("path", String.Empty, ParameterType.UrlSegment);
      else
        request.AddParameter("path", path + "/", ParameterType.UrlSegment);
      request.AddParameter("guid", entityId, ParameterType.UrlSegment);

      return Execute<LockStatus>(request);
    }

    public void LockEntity(string containerTitle, string path, Guid entityId, int? timeoutMs)
    {
      if (timeoutMs.HasValue == false || timeoutMs < 0)
        timeoutMs = 60000;

      var request = new RestRequest();
      request.Resource = "/v1/Lock.svc/Container({containerTitle})/Entity({guid})/LockEntity({timeoutMs})/{path}";
      request.Method = Method.POST;
      request.RequestFormat = DataFormat.Json;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      if (timeoutMs.HasValue)
        request.AddParameter("timeoutMs", timeoutMs);

      if (String.IsNullOrEmpty(path))
        request.AddParameter("path", String.Empty, ParameterType.UrlSegment);
      else
        request.AddParameter("path", path + "/", ParameterType.UrlSegment);

      request.AddParameter("timeoutMs", timeoutMs, ParameterType.UrlSegment);
      request.AddParameter("guid", entityId, ParameterType.UrlSegment);

      Execute(request);
    }

    public void UnlockEntity(string containerTitle, string path, Guid entityId)
    {
      var request = new RestRequest();
      request.Resource = "/v1/Lock.svc/Container({containerTitle})/Entity({guid})/UnlockEntity/{path}";
      request.Method = Method.POST;
      request.RequestFormat = DataFormat.Json;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);

      if (String.IsNullOrEmpty(path))
        request.AddParameter("path", String.Empty, ParameterType.UrlSegment);
      else
        request.AddParameter("path", path + "/", ParameterType.UrlSegment);
      request.AddParameter("guid", entityId, ParameterType.UrlSegment);

      Execute(request);
    }

    public void WaitForEntityLockRelease(string containerTitle, string path, Guid entityId, int? timeoutMs)
    {
      if (timeoutMs == null || timeoutMs < 0)
        timeoutMs = 60000;

      var request = new RestRequest();
      request.Resource = "/v1/Lock.svc/Container({containerTitle})/Entity({guid})/WaitForEntityLockRelease({timeoutMs})/{path}";
      request.Method = Method.GET;
      request.RequestFormat = DataFormat.Json;

      request.AddParameter("containerTitle", containerTitle, ParameterType.UrlSegment);
      if (String.IsNullOrEmpty(path))
        request.AddParameter("path", String.Empty, ParameterType.UrlSegment);
      else
        request.AddParameter("path", path + "/", ParameterType.UrlSegment);

      request.AddParameter("timeoutMs", timeoutMs, ParameterType.UrlSegment);
      request.AddParameter("guid", entityId, ParameterType.UrlSegment);

      Execute(request);
    }
    #endregion
  }
}
