namespace Barista.Linq2Rest.Parser
{
  using System;
  using System.Linq;
  using System.Text.RegularExpressions;
  using Barista.Extensions;

  internal static class TokenOperatorExtensions
  {
    private static readonly string[] Operations = new[] {"eq", "ne", "gt", "ge", "lt", "le", "and", "or", "not"};
    private static readonly string[] Combiners = new[] {"and", "or", "not"};
    private static readonly string[] Arithmetic = new[] {"add", "sub", "mul", "div", "mod"};

    private static readonly string[] BooleanFunctions = new[] {"substringof", "endswith", "startswith"};

    private static readonly Regex CollectionFunctionRx = new Regex(@"^[0-9a-zA-Z_]+/(all|any)\((.+)\)$",
                                                                   RegexOptions.Compiled);

    private static readonly Regex CleanRx = new Regex(@"^\((.+)\)$", RegexOptions.Compiled);
    private static readonly Regex StringStartRx = new Regex("^[(]*'", RegexOptions.Compiled);
    private static readonly Regex StringEndRx = new Regex("'[)]*$", RegexOptions.Compiled);

    public static bool IsCombinationOperation(this string operation)
    {
      if (operation == null)
        throw new ArgumentNullException("operation");

      return Combiners.Any(x => string.Equals(x, operation, StringComparison.OrdinalIgnoreCase));
    }

    public static bool IsOperation(this string operation)
    {
      if (operation == null)
        throw new ArgumentNullException("operation");

      return Operations.Any(x => string.Equals(x, operation, StringComparison.OrdinalIgnoreCase));
    }

    public static bool IsArithmetic(this string operation)
    {
      if (operation == null)
        throw new ArgumentNullException("operation");

      return Arithmetic.Any(x => string.Equals(x, operation, StringComparison.OrdinalIgnoreCase));
    }

    public static bool IsImpliedBoolean(this string expression)
    {
      if (expression == null)
        throw new ArgumentNullException("expression");

      if (!expression.IsNullOrWhiteSpace() && !expression.IsEnclosed() && expression.IsFunction())
      {
        var split = expression.Split(' ');
        return !split.Intersect(Operations).Any()
               && !split.Intersect(Combiners).Any()
               && (BooleanFunctions.Any(x => split[0].StartsWith(x, StringComparison.OrdinalIgnoreCase)) ||
                   CollectionFunctionRx.IsMatch(expression));
      }

      return false;
    }

    public static Match EnclosedMatch(this string expression)
    {
      if (expression == null)
        throw new ArgumentNullException("expression");

      return CleanRx.Match(expression);
    }

    public static bool IsEnclosed(this string expression)
    {
      if (expression == null)
        throw new ArgumentNullException("expression");

      var match = expression.EnclosedMatch();
      return match != null && match.Success;
    }

    public static bool IsStringStart(this string expression)
    {
      return !expression.IsNullOrWhiteSpace() && StringStartRx.IsMatch(expression);
    }

    public static bool IsStringEnd(this string expression)
    {
      return !expression.IsNullOrWhiteSpace() && StringEndRx.IsMatch(expression);
    }

    private static bool IsFunction(this string expression)
    {
      if (expression == null)
        throw new ArgumentNullException("expression");

      var open = expression.IndexOf('(');
      var close = expression.IndexOf(')');

      return open > -1 && close > -1;
    }
  }
}