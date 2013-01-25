namespace Barista.Justache
{
  using Jurassic.Library;
  using System;
  using System.Reflection;
  using System.Text.RegularExpressions;

  public class VariableReference : Part
  {
    private static readonly Regex NotEscapedRegex = new Regex(@"^(?<LCB>{)?((?<NotEscaped>&)\s*?)?(?<Path>.+?)(?<TCB>})?$", RegexOptions.Compiled);
    private static readonly Regex FormattingRegex = new Regex(@"^(?<LCB>{)?((?<NotEscaped>&)\s*?)?(?<Path>.+?):\((?<Format>.*)\)(?<TCB>})?$", RegexOptions.Compiled);
    private readonly string m_path;
    private readonly bool m_escaped = true;
    private readonly string m_formatString;

    public VariableReference(string path)
    {
      if (path == null)
      {
        throw new ArgumentNullException("path");
      }

      m_path = path;

      if (!FormattingRegex.IsMatch(path))
      {
        if (!NotEscapedRegex.IsMatch(path))
          return;

        var notEscapedMatch = NotEscapedRegex.Match(path);
        if (notEscapedMatch.Groups["NotEscaped"].Success)
          m_escaped = false;
        else if (notEscapedMatch.Groups["LCB"].Success && notEscapedMatch.Groups["TCB"].Success)
          m_escaped = false;
        
        m_path = notEscapedMatch.Groups["Path"].Value;
        return;
      }

      var match = FormattingRegex.Match(path);
      if (match.Groups["NotEscaped"].Success)
        m_escaped = false;
      else if (match.Groups["LCB"].Success && match.Groups["TCB"].Success)
        m_escaped = false;

      m_formatString = match.Groups["Format"].Value;
      m_path = match.Groups["Path"].Value;
    }

    public string Path { get { return m_path; } }

    public override void Render(RenderContext context)
    {
      var value = context.GetValue(m_path);

      if (value == null)
        return;

      if (value is DateInstance)
      {
        var dt = value as DateInstance;

        if (dt.ToString() == "Invalid Date")
          return;

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