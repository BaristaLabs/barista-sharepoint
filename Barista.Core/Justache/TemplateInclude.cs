namespace Barista.Justache
{
  using System;

  public class TemplateInclude : Part
  {
    private readonly string m_name;

    public TemplateInclude(string name)
    {
      if (name == null)
      {
        throw new ArgumentNullException("name");
      }

      m_name = name;
    }

    public string Name { get { return m_name; } }

    public override void Render(RenderContext context)
    {
      context.Include(m_name);
    }

    public override string Source()
    {
      return "{{> " + m_name + "}}";
    }

    public override string ToString()
    {
      return string.Format("TemplateInclude(\"{0}\")", m_name);
    }
  }
}