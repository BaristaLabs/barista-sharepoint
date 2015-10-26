namespace Barista.SharePoint.Library
{
  using System;
  using Jurassic.Library;
  using Newtonsoft.Json;

  [Serializable]
  public class SPWebCreationInformation : ObjectInstance
  {
    public SPWebCreationInformation(ObjectInstance prototype)
      : base(prototype)
    {
      //this.PopulateFields();
      this.PopulateFunctions();
    }

    [JSProperty(Name = "convertIfThere")]
    [JsonProperty("convertIfThere")]
    public bool ConvertIfThere
    {
      get;
      set;
    }

    [JSProperty(Name = "description")]
    [JsonProperty("description")]
    public string Description
    {
      get;
      set;
    }

    [JSProperty(Name = "language")]
    [JsonProperty("language")]
    public int Language
    {
      get;
      set;
    }

    [JSProperty(Name = "title")]
    [JsonProperty("title")]
    public string Title
    {
      get;
      set;
    }

    [JSProperty(Name = "url")]
    [JsonProperty("url")]
    public string Url
    {
      get;
      set;
    }

    [JSProperty(Name = "useSamePermissionsAsParentSite")]
    [JsonProperty("useSamePermissionsAsParentSite")]
    public bool UseSamePermissionsAsParentSite
    {
      get;
      set;
    }

    [JSProperty(Name = "webTemplate")]
    [JsonProperty("webTemplate")]
    public object WebTemplate
    {
      get;
      set;
    }
  }
}
