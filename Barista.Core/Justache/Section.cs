namespace Barista.Justache
{
  using System;
  using System.Collections.Generic;
  using System.Text;

  public class Section : Part
  {
    private readonly string m_name;
    private readonly List<Part> m_parts = new List<Part>();
    private readonly Dictionary<string, TemplateDefinition> m_templateDefinitions =
        new Dictionary<string, TemplateDefinition>();

    public Section(string name)
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

    public IEnumerable<Part> Parts { get { return m_parts; } }

    public void Load(IEnumerable<Part> parts)
    {
      foreach (var part in parts)
      {
        Add(part);
      }
    }

    public void Add(Part part)
    {
      var definition = part as TemplateDefinition;

      if (definition != null)
      {
        var templateDefinition = definition;
        m_templateDefinitions.Add(templateDefinition.Name, templateDefinition);
      }
      else
      {
        m_parts.Add(part);
      }
    }

    public TemplateDefinition GetTemplateDefinition(string name)
    {
      TemplateDefinition templateDefinition;
      m_templateDefinitions.TryGetValue(name, out templateDefinition);
      return templateDefinition;
    }

    public override void Render(RenderContext context)
    {
      foreach (var part in m_parts)
      {
        part.Render(context);
      }
    }

    protected string InnerSource()
    {
      var sb = new StringBuilder();
      foreach (var part in Parts)
      {
        if (!(part is EndSection))
        {
          sb.Append(part.Source());
        }
      }
      return sb.ToString();
    }

    public override string Source()
    {
      return "{{#" + m_name + "}}" + InnerSource() + "{{/" + m_name + "}}";
    }
  }
}