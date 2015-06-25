namespace Barista.Imports.Linq2Rest.Parser
{
  using System;

  internal class TokenSet
  {
    private string m_left;
    private string m_operation;
    private string m_right;

    public TokenSet()
    {
      m_left = string.Empty;
      m_right = string.Empty;
      m_operation = string.Empty;
    }

    public string Left
    {
      get { return m_left; }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        m_left = value;
      }
    }

    public string Operation
    {
      get { return m_operation; }

      set
      {
        if (value == null)
          throw new ArgumentNullException("value");
        m_operation = value;
      }
    }

    public string Right
    {
      get { return m_right; }

      set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        m_right = value;
      }
    }

    public override string ToString()
    {
      return string.Format("{0} {1} {2}", Left, Operation, Right);
    }
  }
}