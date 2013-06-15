namespace Barista.Helpers
{
  using System.Collections.Generic;

  internal class OSParser
  {
    private readonly List<OSPattern> m_patterns;
    internal OSParser(List<OSPattern> patterns)
    {
      m_patterns = patterns;
    }
    public OS ParseUserAgentString(string agentString)
    {
      foreach (var p in m_patterns)
      {
        OS os;
        if ((os = p.GetMatch(agentString)) != null)
        {
          return os;
        }
      }
      return new OS("Other", null, null, null, null);
    }
  }
}