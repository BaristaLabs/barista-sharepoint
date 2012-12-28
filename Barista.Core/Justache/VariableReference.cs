namespace Barista.Justache
{
  using Jurassic.Library;
  using System;
  using System.Reflection;
  using System.Text.RegularExpressions;

  public class VariableReference : Part
  {
    private static readonly Regex NotEscapedRegex = new Regex(@"^\{\s*(.+?)\s*\}$");
    private static readonly Regex FormattingRegex = new Regex(@"^(.+?):\((?<Format>.*)\)$");
    private readonly string m_path;
    private readonly bool m_escaped;
    private readonly string m_formatString;

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

      if (FormattingRegex.IsMatch(path))
      {
        match = FormattingRegex.Match(path);
        m_formatString = match.Groups["Format"].Value;
        m_path = match.Groups[1].Value;
      }
    }

    public string Path { get { return m_path; } }

    public override void Render(RenderContext context)
    {
      object value = context.GetValue(m_path);

      if (value != null)
      {
        if (value is DateInstance)
        {
          var dt = value as DateInstance;
          value = DateTime.Parse(dt.ToISOString());
        }

        if (String.IsNullOrEmpty(m_formatString) == false)
        {
          var toStringMethod = value.GetType()
               .GetMethod("ToString", BindingFlags.Public | BindingFlags.Instance, null, new[] {typeof (string)},
                          null);

          if (toStringMethod != null)
          {
            value = toStringMethod.Invoke(value, new object[] {m_formatString});
          }
        }

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