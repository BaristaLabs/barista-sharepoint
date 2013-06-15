namespace Barista.Helpers
{
  using System.Collections.Generic;

  internal class UserAgentParser
  {
    private readonly List<UserAgentPattern> m_patterns;
    internal UserAgentParser(List<UserAgentPattern> patterns)
    {
      m_patterns = patterns;
    }

    public UserAgent ParseAgentString(string agentstring)
    {
      foreach (var p in m_patterns)
      {
        UserAgent agent;
        if ((agent = p.GetMatch(agentstring)) != null)
        {
          return agent;
        }
      }
      return new UserAgent("Other", null, null, null);
    }
  }
}
