namespace Barista.SharePoint
{
  using Microsoft.SharePoint.Administration;
  using System;

  /// <summary>
  /// Includes operations related to the Farm scoped property bag
  /// </summary>
  public class FarmPropertyBag
  {
    /// <summary>
    /// Returns the property value
    /// </summary>
    /// <param name="name">property key</param>
    /// <returns>property value</returns>
    public static String Load(String name)
    {
      var response = String.Empty;

      var farm = SPFarm.Local;

      if (farm.Properties.ContainsKey(name))
      {
        response = farm.Properties[name].ToString();
      }

      return response;
    }

    public static void Save(String name, String value)
    {
      SPFarm farm = SPFarm.Local;
      if (farm.Properties.ContainsKey(name))
      {
        farm.Properties[name] = value;
      }
      else
      {
        farm.Properties.Add(name, value);
      }
      farm.Update();
    }
  }
}
