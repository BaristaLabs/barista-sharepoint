

namespace Barista.SharePoint
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using Microsoft.SharePoint.Administration;

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
      String response = String.Empty;

      SPFarm farm = SPFarm.Local;

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
