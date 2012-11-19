namespace Barista.SharePoint.Library
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using Jurassic;
  using Jurassic.Library;
  using Newtonsoft.Json;
  using Microsoft.SharePoint;

  [Serializable]
  public class SPListCreationInformation : ObjectInstance
  {
    public SPListCreationInformation(ObjectInstance prototype)
      : base(prototype)
    {
      this.DocumentTemplateType = "100";
      this.QuickLaunchOption = SPListTemplate.QuickLaunchOptions.Default.ToString();

      this.PopulateFields();
      //this.PopulateFunctions();
    }

    [JSProperty(Name = "customSchemaXml")]
    [JsonProperty("customSchemaXml")]
    public string CustomSchemaXml
    {
      get;
      set;
    }

    [JSProperty(Name = "dataSourceProperties")]
    [JsonProperty("dataSourceProperties")]
    public object DataSourceProperties
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

    [JSProperty(Name = "documentTemplateType")]
    [JsonProperty("documentTemplateType")]
    public string DocumentTemplateType
    {
      get;
      set;
    }

    [JSProperty(Name = "listInstanceFeatureDefinition")]
    [JsonProperty("listInstanceFeatureDefinition")]
    public object ListInstanceFeatureDefinition
    {
      get;
      set;
    }

    [JSProperty(Name = "quickLaunchOption")]
    [JsonProperty("quickLaunchOption")]
    public string QuickLaunchOption
    {
      get;
      set;
    }

    [JSProperty(Name = "templateFeatureId")]
    [JsonProperty("templateFeatureId")]
    public string TemplateFeatureId
    {
      get;
      set;
    }

    [JSProperty(Name = "templateType")]
    [JsonProperty("templateType")]
    public int TemplateType
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
  }
}
