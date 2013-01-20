namespace Barista.Linq2Rest.Parser
{
  using System;
  using System.Collections.Specialized;

  /// <summary>
  /// Defines the public interface for a parameter parser.
  /// </summary>
  /// <typeparam name="T">The <see cref="Type"/> of object to parse parameters for.</typeparam>
  public interface IParameterParser<T>
  {
    /// <summary>
    /// Parses the passes parameters into a <see cref="ModelFilter{T}"/>.
    /// </summary>
    /// <param name="queryParameters">The parameters to parse.</param>
    /// <returns>A <see cref="ModelFilter{T}"/> representing the restrictions in the parameters.</returns>
    IModelFilter<T> Parse(NameValueCollection queryParameters);
  }

  internal abstract class ParameterParserContracts<T> : IParameterParser<T>
  {
    public IModelFilter<T> Parse(NameValueCollection queryParameters)
    {
      if (queryParameters == null)
        throw new ArgumentNullException("queryParameters");

      throw new System.NotImplementedException();
    }
  }
}