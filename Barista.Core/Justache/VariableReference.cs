namespace Barista.Justache
{
  using System;
  using System.Text.RegularExpressions;

  public class VariableReference : Part
  {
    private static readonly Regex NotEscapedRegex = new Regex(@"^\{\s*(.+?)\s*\}$");
    private readonly string m_path;
    private readonly bool m_escaped;

    public VariableReference(string path)
    {
      if (path == null)
      {
        throw new ArgumentNullException("path");
      }

      m_path = path;

      var match = NotEscapedRegex.Match(path);
      m_escaped = !match.Success;

      if (match.Success)
      {
        m_path = match.Groups[1].Value;
      }
    }

    public string Path { get { return m_path; } }

    public override void Render(RenderContext context)
    {
      object value = context.GetValue(m_path);

      if (value != null)
      {
        context.Write(m_escaped
            ? Encoders.HtmlEncode(value.ToString())
            : value.ToString());
      }
    }

    public override string Source()
    {
      return "{{" + m_path + "}}";
    }

    public override string ToString()
    {
      return string.Format("VariableReference(\"{0}\")", m_path);
    }
  }
}