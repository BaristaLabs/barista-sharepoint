namespace Barista.Justache
{
  using System;

  public class EndSection : Part
  {
    private readonly string m_name;

    public EndSection(string name)
    {
      if (name == null)
      {
        throw new ArgumentNullException("name");
      }

      m_name = name;
    }

    public string Name
    {
      get { return m_name; }
    }

    public override void Render(RenderContext context)
    {
    }

    public override string Source()
    {
      return "";
    }

    public override string ToString()
    {
      return string.Format("EndSection(\"{0}\")", m_name);
    }
  }
}