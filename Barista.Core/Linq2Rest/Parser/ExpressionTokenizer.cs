namespace Barista.Linq2Rest.Parser
{
  using Barista;
  using Barista.Extensions;
  using System;
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  using System.Linq;
  using System.Text.RegularExpressions;

  internal static class ExpressionTokenizer
  {
    private static readonly Regex FunctionRx = new Regex(@"^([^\(\)]+)\((.+)\)$", RegexOptions.Compiled);

    private static readonly Regex FunctionContentRx =
      new Regex(@"^(.*\((?>[^()]+|\((?<Depth>.*)|\)(?<-Depth>.*))*(?(Depth)(?!))\)|.*?)\s*,\s*(.+)$",
                RegexOptions.Compiled);

    private static readonly Regex AnyAllFunctionRx = new Regex(@"^(([0-9a-zA-Z_/]+/)+)(any|all)\((.*)\)$",
                                                               RegexOptions.Compiled);

    public static ICollection<TokenSet> GetTokens(this string expression)
    {
      var tokens = new Collection<TokenSet>();
      if (expression.IsNullOrWhiteSpace())
      {
        return tokens;
      }

      var cleanMatch = expression.EnclosedMatch();

      if (cleanMatch.Success)
      {
        var match = cleanMatch.Groups[1].Value;
        if (!HasOrphanedOpenParenthesis(match))
        {
          expression = match;
        }
      }

      if (expression.IsImpliedBoolean())
      {
        return tokens;
      }

      var blocks = expression.Split(new[] {' '});

      var openGroups = 0;
      var startExpression = 0;
      var currentTokens = new TokenSet();

      var processingString = false;

      for (int i = 0; i < blocks.Length; i++)
      {
        if (blocks[i].IsStringStart())
        {
          processingString = true;
        }

        var netEnclosed = blocks[i].Count(c => c == '(') - blocks[i].Count(c => c == ')');
        openGroups += netEnclosed;

        if (openGroups == 0)
        {
          if (!processingString && blocks[i].IsOperation())
          {
            var expression1 = startExpression;

            if (currentTokens.Left.IsNullOrWhiteSpace())
            {
              var i1 = i;
              Func<string, int, bool> leftPredicate = (x, j) => j >= expression1 && j < i1;

              currentTokens.Left = blocks.Where(leftPredicate).Join(" ");
              currentTokens.Operation = blocks[i];
              startExpression = i + 1;

              if (blocks[i].IsCombinationOperation())
              {
                currentTokens.Right = blocks.Where((x, j) => j > i).Join(" ");

                tokens.Add(currentTokens);
                return tokens;
              }
            }
            else
            {
              var i2 = i;
              Func<string, int, bool> rightPredicate = (x, j) => j >= expression1 && j < i2;
              currentTokens.Right = blocks.Where(rightPredicate).Join(" ");

              tokens.Add(currentTokens);

              startExpression = i + 1;
              currentTokens = new TokenSet();

              if (blocks[i].IsCombinationOperation())
              {
                tokens.Add(new TokenSet {Operation = blocks[i].ToLowerInvariant()});
              }
            }
          }
        }

        if (blocks[i].IsStringEnd())
        {
          processingString = false;
        }
      }

      var remainingToken = blocks.Where((x, j) => j >= startExpression).Join(" ");

      if (!currentTokens.Left.IsNullOrWhiteSpace())
      {
        currentTokens.Right = remainingToken;
        tokens.Add(currentTokens);
      }
      else if (remainingToken.IsEnclosed())
      {
        currentTokens.Left = remainingToken;
        tokens.Add(currentTokens);
      }
      else if (tokens.Count > 0)
      {
        currentTokens.Left = remainingToken;
        tokens.Add(currentTokens);
      }

      return tokens;
    }

    public static TokenSet GetArithmeticToken(this string expression)
    {
      if (expression == null)
        throw new ArgumentNullException("expression");

      var cleanMatch = expression.EnclosedMatch();

      if (cleanMatch.Success)
      {
        var match = cleanMatch.Groups[1].Value;
        if (!HasOrphanedOpenParenthesis(match))
        {
          expression = match;
        }
      }

      var blocks = expression.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
      var hasOperation = blocks.Any(x => x.IsArithmetic());
      if (!hasOperation)
      {
        return null;
      }

      var operationIndex = GetArithmeticOperationIndex(blocks);

      var left = blocks.Where((x, i) => i < operationIndex).Join(" ");
      var right = blocks.Where((x, i) => i > operationIndex).Join(" ");
      var operation = blocks[operationIndex];

      return new TokenSet {Left = left, Operation = operation, Right = right};
    }

    public static TokenSet GetAnyAllFunctionTokens(this string filter)
    {
      if (filter == null)
        throw new ArgumentNullException("filter");

      var functionMatch = AnyAllFunctionRx.Match(filter);
      if (!functionMatch.Success)
      {
        return null;
      }

      var functionCollection = functionMatch.Groups[1].Value.Trim('/');
      var functionName = functionMatch.Groups[3].Value;
      var functionContent = functionMatch.Groups[4].Value;

      return new FunctionTokenSet
        {
          Operation = functionName,
          Left = functionCollection,
          Right = functionContent
        };
    }

    public static TokenSet GetFunctionTokens(this string filter)
    {
      if (filter == null)
        throw new ArgumentNullException("filter");

      var functionMatch = FunctionRx.Match(filter);
      if (!functionMatch.Success)
      {
        return null;
      }

      var functionName = functionMatch.Groups[1].Value;
      var functionContent = functionMatch.Groups[2].Value;
      var functionContentMatch = FunctionContentRx.Match(functionContent);
      if (!functionContentMatch.Success)
      {
        return new FunctionTokenSet
          {
            Operation = functionName,
            Left = functionContent
          };
      }

      return new FunctionTokenSet
        {
          Operation = functionName,
          Left = functionContentMatch.Groups[1].Value,
          Right = functionContentMatch.Groups[2].Value
        };
    }

    private static int GetArithmeticOperationIndex(IList<string> blocks)
    {
      if (blocks == null)
        throw new ArgumentNullException("blocks");

      var openGroups = 0;
      var operationIndex = -1;
      for (var i = 0; i < blocks.Count; i++)
      {
        var source = blocks[i];

        var netEnclosed = source.Count(c => c == '(') - source.Count(c => c == ')');
        openGroups += netEnclosed;

        if (openGroups == 0 && source.IsArithmetic())
        {
          operationIndex = i;
        }
      }

      return operationIndex;
    }

    private static bool HasOrphanedOpenParenthesis(string expression)
    {
      if (expression == null)
        throw new ArgumentNullException("expression");

      var opens = new List<int>();
      var closes = new List<int>();
      var index = expression.IndexOf('(');
      while (index > -1)
      {
        opens.Add(index);
        index = expression.IndexOf('(', index + 1);
      }

      index = expression.IndexOf(')');
      while (index > -1)
      {
        closes.Add(index);
        index = expression.IndexOf(')', index + 1);
      }

      var pairs = opens.Zip(closes, (o, c) => new Tuple<int, int>(o, c));
      var hasOrphan = opens.Count == closes.Count && pairs.Any(x => x.Item2 < x.Item1);

      return hasOrphan;
    }
  }
}