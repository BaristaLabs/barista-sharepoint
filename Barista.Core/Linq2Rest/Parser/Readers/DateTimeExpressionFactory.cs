namespace Barista.Linq2Rest.Parser.Readers
{
  using System;
  using System.Linq.Expressions;
  using System.Text.RegularExpressions;
  using System.Xml;

  internal class DateTimeExpressionFactory : IValueExpressionFactory
  {
    private static readonly Regex DateTimeRegex =
      new Regex(@"datetime['\""](\d{4}\-\d{2}\-\d{2}(T\d{2}\:\d{2}\:\d{2})?(?<z>Z)?)['\""]",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public Type Handles
    {
      get { return typeof (DateTime); }
    }

    public ConstantExpression Convert(string token)
    {
      var match = DateTimeRegex.Match(token);
      if (match.Success)
      {
        var dateTime = match.Groups["z"].Success
                         ? XmlConvert.ToDateTime(match.Groups[1].Value, XmlDateTimeSerializationMode.Utc)
                         : DateTime.SpecifyKind(DateTime.Parse(match.Groups[1].Value), DateTimeKind.Unspecified);
        return Expression.Constant(dateTime);
      }

      throw new FormatException("Could not read " + token + " as DateTime.");
    }
  }
}