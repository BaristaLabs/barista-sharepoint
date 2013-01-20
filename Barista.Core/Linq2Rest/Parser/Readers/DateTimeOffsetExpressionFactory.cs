namespace Barista.Linq2Rest.Parser.Readers
{
  using System;
  using System.Linq.Expressions;
  using System.Text.RegularExpressions;
  using System.Xml;

  internal class DateTimeOffsetExpressionFactory : IValueExpressionFactory
  {
    private static readonly Regex DateTimeOffsetRegex =
      new Regex(@"datetimeoffset['\""](\d{4}\-\d{2}\-\d{2}(T\d{2}\:\d{2}\:\d{2})?[\-\+]\d{2}:\d{2})['\""]",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public Type Handles
    {
      get { return typeof (DateTimeOffset); }
    }

    public ConstantExpression Convert(string token)
    {
      var match = DateTimeOffsetRegex.Match(token);
      if (match.Success)
      {
        var dateTimeOffset = XmlConvert.ToDateTimeOffset(match.Groups[1].Value);
        return Expression.Constant(dateTimeOffset);
      }

      throw new FormatException("Could not read " + token + " as DateTimeOffset.");
    }
  }
}