namespace Barista.Linq2Rest.Parser.Readers
{
  using System;
  using System.Linq.Expressions;
  using System.Text.RegularExpressions;
  using Barista.Extensions;

  internal class GuidExpressionFactory : IValueExpressionFactory
  {
    private static readonly Regex GuidRegex = new Regex(@"guid['\""]([a-f0-9\-]+)['\""]",
                                                        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public Type Handles
    {
      get { return typeof (Guid); }
    }

    public ConstantExpression Convert(string token)
    {
      var match = GuidRegex.Match(token);
      if (match.Success)
      {
        Guid guid;
        if (match.Groups[1].Value.TryParseGuid(out guid))
        {
          return Expression.Constant(guid);
        }
      }

      throw new FormatException("Could not read " + token + " as Guid.");
    }
  }
}