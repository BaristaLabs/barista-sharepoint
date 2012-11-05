namespace OFS.OrcaDB.Core
{
  using System;
  using System.Configuration;
  using System.IO;
  using System.Linq;
  using System.ServiceModel.Web;
  using Newtonsoft.Json;
  using OFS.OrcaDB.Core.Framework;

  /// <summary>
  /// Represents the base class of Document Store Services.
  /// </summary>
  public abstract class DocumentStoreServiceBase
  {
    protected bool TryParseEntityGuid(string guid, out Guid id)
    {
      id = Guid.Empty;
      try
      {
        id = new Guid(guid);
      }
      catch
      {
        return false;
      }
      return true;
    }

    /// <summary>
    /// Dynamically loads the IDocumentStore defined in the web.config.
    /// </summary>
    /// <returns></returns>
    protected IDocumentStore GetDocumentStore()
    {
      if (ConfigurationManager.AppSettings.AllKeys.Any(k => k == "DocumentStoreServiceType"))
      {
        string fullTypeName = ConfigurationManager.AppSettings["DocumentStoreServiceType"];
        if (string.IsNullOrEmpty(fullTypeName))
          throw new InvalidOperationException("A DocumentStoreServiceType key was specified within web.config, but it did not contain a value.");

        Type documentStoreType = Type.GetType(fullTypeName, true, true);
        if (documentStoreType.GetInterfaces().Any(i => i == typeof(IDocumentStore)) == false)
          throw new InvalidOperationException("The DocumentStoreServiceType was specified within web.config, but it does not implement IDocumentStore");

        return Activator.CreateInstance(documentStoreType) as IDocumentStore;
      }

      throw new InvalidOperationException("The DocumentStore instance that the service will use must be defined in a 'DocumentStoreServiceType' value contained in the web.config");
    }

    protected T GetDocumentStore<T>()
    {
      IDocumentStore documentStore = GetDocumentStore();
      if ((documentStore is T) == false)
      {
        throw new NotImplementedException("The current Document Store does not implement the specified operation.");
      }
      return (T)documentStore;
    }

    protected JsonSerializerSettings JsonSerializerSettings
    {
      get
      {
        return new JsonSerializerSettings()
        {
          PreserveReferencesHandling = PreserveReferencesHandling.All,
          TypeNameHandling = TypeNameHandling.Auto,
        };
      }
    }

    protected Stream GetJsonStream(object result)
    {
      WebOperationContext.Current.OutgoingResponse.ContentType = "application/json; charset=utf-8";
      var jsonResult = JsonConvert.SerializeObject(result, Formatting.None, this.JsonSerializerSettings);
      return new StringStream(jsonResult);
    }

    public T GetObjectFromJsonStream<T>(Stream stream)
    {
      string json = String.Empty;
      using (var sr = new StreamReader(stream))
      {
        json = sr.ReadToEnd();
      }

      return JsonConvert.DeserializeObject<T>(json, this.JsonSerializerSettings);
    }
  }
}
