namespace Barista.Justache
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;

  /// <summary>
  /// Represents a section in a Mustache template.
  /// </summary>
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

    /// <summary>
    /// Gets the name of the section
    /// </summary>
    /// <value>The name of the section</value>
    public string Name
    {
      get { return m_name; }
    }

    /// <summary>
    /// Gets the list of parts that are contained in the section.
    /// </summary>
    /// <value>The parts.</value>
    public IList<Part> Parts
    {
      get
      {
        return m_parts;
      }
    }

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

      Parts.Where(part => 
                  (part is EndSection) == false
                 )
           .ToList()
           .ForEach(part =>
                    sb.Append(part.Source())
                   );

      return sb.ToString();
    }

    public override string Source()
    {
      return "{{#" + m_name + "}}" + InnerSource() + "{{/" + m_name + "}}";
    }
  }
}