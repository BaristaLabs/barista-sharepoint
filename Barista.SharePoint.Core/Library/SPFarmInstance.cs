namespace Barista.SharePoint.Library
{
  using System;
  using Jurassic;
  using Jurassic.Library;
  using Microsoft.SharePoint.Administration;

  [Serializable]
  public class SPFarmInstance : ObjectInstance
  {
    [NonSerialized]
    private readonly SPFarm m_farm;

    public SPFarmInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public SPFarmInstance(ObjectInstance prototype, SPFarm farm)
      : this(prototype)
    {
      this.m_farm = farm;
    }

    [JSFunction(Name = "getFarmKeyValue")]
    public object GetFarmKeyValueAsObject(string key)
    {
      if (m_farm == null|| m_farm.Properties.ContainsKey(key) == false)
        return Undefined.Value;

      string val = Convert.ToString(m_farm.Properties[key]);

      object result;

      //Attempt to convert the string into a JSON Object.
      try
      {
        result = JSONObject.Parse(this.Engine, val, null);
      }
      catch
      {
        result = val;
      }

      return result;
    }

    [JSFunction(Name = "setFarmKeyValue")]
    public void SetFarmKeyValue(string key, object value)
    {
      if (value == null || value == Undefined.Value || value == Null.Value)
        throw new ArgumentNullException("value");

      string stringValue;
      if (value is ObjectInstance)
      {
        stringValue = JSONObject.Stringify(this.Engine, value, null, null);
      }
      else
      {
        stringValue = value.ToString();
      }

      if (m_farm != null)
      {
        if (m_farm.Properties.ContainsKey(key))
          m_farm.Properties[key] = stringValue;
        else
          m_farm.Properties.Add(key, stringValue);

        m_farm.Update();
      }
    }
  }
}
