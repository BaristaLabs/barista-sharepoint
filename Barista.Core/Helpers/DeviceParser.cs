namespace Barista.Helpers
{
  using System.Collections.Generic;

  internal class DeviceParser
  {
    private readonly List<DevicePattern> m_patterns;
    private readonly UserAgentParser m_userAgentParser;

    public DeviceParser(List<DevicePattern> patterns, UserAgentParser uaParser)
    {
      m_patterns = patterns;
      m_userAgentParser = uaParser;
    }

    public Device ParseUserAgentString(string agentString)
    {
      return ParseUserAgentString(agentString, m_userAgentParser.ParseAgentString(agentString).Family);
    }

    public Device ParseUserAgentString(string agentString, string userAgentFamily)
    {
      string device = null;
      foreach (DevicePattern p in m_patterns)
      {
        if ((device = p.GetMatch(agentString)) != null)
        {
          break;
        }
      }

      string osFamily = device == null ? "Other" : device;
      userAgentFamily = userAgentFamily == null ? "Other" : userAgentFamily;

      return new Device(device, (device != null && device.Equals("Spider")));
    }
  }
}