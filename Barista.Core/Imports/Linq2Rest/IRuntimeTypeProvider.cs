namespace Barista.Imports.Linq2Rest
{
  using System;
  using System.Collections.Generic;
  using System.Reflection;

  /// <summary>
  /// Provides a type matching the provided members.
  /// </summary>
  public interface IRuntimeTypeProvider
  {
    /// <summary>
    /// Gets the <see cref="Type"/> matching the provided members.
    /// </summary>
    /// <param name="sourceType">The <see cref="Type"/> to generate the runtime type from.</param>
    /// <param name="properties">The <see cref="MemberInfo"/> to use to generate properties.</param>
    /// <returns>A <see cref="Type"/> matching the provided properties.</returns>
    Type Get(Type sourceType, IEnumerable<MemberInfo> properties);
  }

  internal abstract class RuntimeTypeProviderContracts : IRuntimeTypeProvider
  {
    public Type Get(Type sourceType, IEnumerable<MemberInfo> properties)
    {
      if (sourceType == null)
        throw new ArgumentNullException("sourceType");

      if (properties == null)
        throw new ArgumentNullException("properties");

      throw new NotImplementedException();
    }
  }
}