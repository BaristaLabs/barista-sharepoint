namespace Barista.Helpers
{
  using System;
  using System.Collections.Generic;
  using System.Text.RegularExpressions;

  internal static class YamlParsing
  {
    private static Regex GetRegex(string regularExpression)
    {
      //some expressions in the regex.yaml file causes parsing errors in
      //.NET such as the \_ token so we need to alter these
      if (regularExpression.IndexOf(@"\_", System.StringComparison.Ordinal) != -1)
        regularExpression = regularExpression.Replace(@"\_", "_");

      // TODO: potentially allow parser to specify e.g. to use compiled regular expressions
      // which are faster but increase startup time
      return new Regex(regularExpression);
    }

    internal static OSPattern OSPatternFromMap(IDictionary<string, string> configMap)
    {
      string regex = configMap["regex"];
      if (regex == null)
      {
        throw new ArgumentException("OS is missing regex");
      }

      string os, v1, v2;

      configMap.TryGetValue("os_replacement", out os);
      configMap.TryGetValue("os_v1_replacement", out v1);
      configMap.TryGetValue("os_v2_replacement", out v2);

      return (new OSPattern(GetRegex(regex),
                           os, v1, v2));
    }
    internal static UserAgentPattern UserAgentPatternFromMap(IDictionary<string, string> configMap)
    {
      string regex = configMap["regex"];
      if (regex == null)
      {
        throw new ArgumentException("User agent is missing regex");
      }

      string family, v1, v2;

      configMap.TryGetValue("family_replacement", out family);
      configMap.TryGetValue("v1_replacement", out v1);
      configMap.TryGetValue("v2_replacement", out v2);

      return (new UserAgentPattern(GetRegex(regex),
                           family, v1, v2));
    }

    internal static DevicePattern DevicePatternFromMap(IDictionary<string, string> configMap)
    {
      Regex regex = GetRegex(configMap["regex"]);

      if (regex == null)
      {
        throw new ArgumentException("Device is missing regex");
      }

      string device;

      configMap.TryGetValue("device_replacement", out device);

      return new DevicePattern(regex, device);
    }
  }
}