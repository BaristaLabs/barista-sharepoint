namespace Barista.Library
{
  using Jurassic.Library;
  using Barista.Newtonsoft.Json;

  public class CsvOptions : ObjectInstance
  {
    public CsvOptions(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.HasHeader = false;
      this.PreserveLeadingWhiteSpace = false;
      this.PreserveTrailingWhiteSpace = false;
      this.ValueDelimiter = "\"";
      this.ValueSeparator = ",";
    }

    [JSProperty(Name = "hasHeader")]
    [JsonProperty("hasHeader")]
    public bool HasHeader
    {
      get;
      set;
    }

    [JSProperty(Name = "preserveLeadingWhiteSpace")]
    [JsonProperty("preserveLeadingWhiteSpace")]
    public bool PreserveLeadingWhiteSpace
    {
      get;
      set;
    }

    [JSProperty(Name = "preserveTrailingWhiteSpace")]
    [JsonProperty("preserveTrailingWhiteSpace")]
    public bool PreserveTrailingWhiteSpace
    {
      get;
      set;
    }

    [JSProperty(Name = "valueDelimiter")]
    [JsonProperty("valueDelimiter")]
    public string ValueDelimiter
    {
      get;
      set;
    }

    [JSProperty(Name = "valueSeparator")]
    [JsonProperty("valueSeparator")]
    public string ValueSeparator
    {
      get;
      set;
    }
  }
}
