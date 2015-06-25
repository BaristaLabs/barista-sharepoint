namespace Barista.Justache
{
  using System.Collections.Generic;
  using System.Text.RegularExpressions;

  public class TemplateParser
  {
    private static readonly Regex TokenRegex = new Regex(@"(^\s*)?\{\{(?<Token>[\{]?[^}]+?\}?)\}\}", RegexOptions.Compiled | RegexOptions.Multiline);
    private static readonly Regex StripRegex = new Regex(@"\G[\r\t\v ]+\n");

    public IEnumerable<Part> Parse(string template)
    {
      var i = 0;
      Match m;

      while ((m = TokenRegex.Match(template, i)).Success)
      {
        var literal = template.Substring(i, m.Index - i);

        if (literal != "")
          yield return new LiteralText(literal);

        var token = m.Groups["Token"].Value;
        var stripOutNewLine = false;

        //Look at the first character of the token, if it matches a processing instruction character,
        //perform special behavior, otherwise add as literal text.
        switch (token[0])
        {
          case '#':
            yield return new Block(token.Substring(1));
            stripOutNewLine = true;
            break;
          case '^':
            yield return new InvertedBlock(token.Substring(1));
            stripOutNewLine = true;
            break;
          case '<':
            yield return new TemplateDefinition(token.Substring(1));
            stripOutNewLine = true;
            break;
          case '/':
            yield return new EndSection(token.Substring(1));
            stripOutNewLine = true;
            break;
          case '>':
            yield return new TemplateInclude(token.Substring(1));
            stripOutNewLine = true;
            break;
          default:
            if (token[0] != '!') //Comment
              yield return new VariableReference(token.Trim());
            break;
        }

        i = m.Index + m.Length;

        Match s;
        if (stripOutNewLine && (s = StripRegex.Match(template, i)).Success)
          i += s.Length;
      }

      if (i < template.Length)
      {
        yield return new LiteralText(template.Substring(i));
      }
    }
  }
}