namespace Barista.Linq2Rest.Parser.Readers
{
  using System;
  using System.IO;
  using System.Linq.Expressions;

  internal class StreamExpressionFactory : ByteArrayExpressionFactory
  {
    public override Type Handles
    {
      get { return typeof (Stream); }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability",
      "CA2000:Dispose objects before losing scope", Justification = "Cannot dispose here.")]
    public override ConstantExpression Convert(string token)
    {
      var baseResult = base.Convert(token);
      if (baseResult.Value != null)
      {
        var stream = new MemoryStream((byte[]) baseResult.Value);

        return Expression.Constant(stream);
      }

      return baseResult;
    }
  }
}