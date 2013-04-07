namespace Barista.Imports.Linq2Rest.Parser
{
  using System;
  using System.Reflection;

  /// <summary>
  /// Defines the public interface for a resolver of <see cref="MemberInfo"/> name.
  /// </summary>
  public interface IMemberNameResolver
  {
    /// <summary>
    /// Returns the resolved name for the <see cref="MemberInfo"/>.
    /// </summary>
    /// <param name="member">The <see cref="MemberInfo"/> to resolve the name of.</param>
    /// <returns>The resolved name.</returns>
    string ResolveName(MemberInfo member);
  }

  internal abstract class MemberNameResolverContracts : IMemberNameResolver
  {
    public string ResolveName(MemberInfo member)
    {
      if (member == null)
        throw new ArgumentNullException("member");

      throw new NotImplementedException();
    }
  }
}