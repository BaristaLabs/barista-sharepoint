namespace Barista.Linq2Rest.Parser
{
  internal class FunctionTokenSet : TokenSet
  {
    public override string ToString()
    {
      return string.Format("{0} {1} {2}", Operation, Left, Right);
    }
  }
}