namespace Barista.Imports.Linq2Rest.Parser.Readers
{
  using System;
  using System.Linq.Expressions;
  using System.Text.RegularExpressions;

  internal class ByteArrayExpressionFactory : IValueExpressionFactory
  {
    private static readonly Regex ByteArrayRegex = new Regex(@"(X|binary)['\""]([A-Za-z0-9=\+\/]+)['\""]",
                                                             RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public virtual Type Handles
    {
      get { return typeof (byte[]); }
    }

    public virtual ConstantExpression Convert(string token)
    {
      var match = ByteArrayRegex.Match(token);
      if (match.Success)
      {
        try
        {
          var buffer = System.Convert.FromBase64String(match.Groups[2].Value);
          return Expression.Constant(buffer);
        }
        catch
        {
          return Expression.Constant(null);
        }
      }

      throw new FormatException("Could not read " + token + " as byte array.");
    }
  }
}