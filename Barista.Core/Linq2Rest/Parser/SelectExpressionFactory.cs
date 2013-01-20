namespace Barista.Linq2Rest.Parser
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Linq.Expressions;
  using System.Reflection;
  using System.Threading;
  using Barista.Extensions;

  /// <summary>
  /// Defines the SelectExpressionFactory.
  /// </summary>
  /// <typeparam name="T">The <see cref="Type"/> of object to project.</typeparam>
  public class SelectExpressionFactory<T> : ISelectExpressionFactory<T>
  {
    private const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
    private readonly IDictionary<string, Expression<Func<T, object>>> m_knownSelections;
    private readonly IMemberNameResolver m_nameResolver;
    private readonly IRuntimeTypeProvider m_runtimeTypeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="SelectExpressionFactory{T}"/> class.
    /// </summary>
    public SelectExpressionFactory(IMemberNameResolver nameResolver, IRuntimeTypeProvider runtimeTypeProvider)
    {
      if (nameResolver == null)
        throw new ArgumentNullException("nameResolver");

      if (runtimeTypeProvider == null)
        throw new ArgumentNullException("runtimeTypeProvider");

      m_nameResolver = nameResolver;
      m_runtimeTypeProvider = runtimeTypeProvider;
      m_knownSelections = new Dictionary<string, Expression<Func<T, object>>>
        {
          {string.Empty, null}
        };
    }

    /// <summary>
    /// Creates a select expression.
    /// </summary>
    /// <param name="selection">The properties to select.</param>
    /// <returns>An instance of a <see cref="Func{T1,TResult}"/>.</returns>
    public Expression<Func<T, object>> Create(string selection)
    {
      var fieldNames = (selection ?? string.Empty).Split(',')
                                                  .Where(x => !x.IsNullOrWhiteSpace())
                                                  .Select(x => x.Trim())
                                                  .OrderBy(x => x);

      var key = fieldNames.Join(",");

      if (m_knownSelections.ContainsKey(key))
      {
        var knownSelection = m_knownSelections[key];

        return knownSelection;
      }

      var elementType = typeof (T);
      var elementMembers = elementType.GetProperties(Flags)
                                      .Cast<MemberInfo>()
                                      .Concat(elementType.GetFields(Flags))
                                      .ToArray();
      var sourceMembers = fieldNames.ToDictionary(name => name,
                                                  s => elementMembers.First(m => m_nameResolver.ResolveName(m) == s));
      var dynamicType = m_runtimeTypeProvider.Get(elementType, sourceMembers.Values);

      var sourceItem = Expression.Parameter(elementType, "t");
      var bindings = dynamicType
        .GetProperties()
        .Select(p =>
          {
            var member = sourceMembers[p.Name];
            var expression = member.MemberType == MemberTypes.Property
                               ? Expression.Property(sourceItem, (PropertyInfo) member)
                               : Expression.Field(sourceItem, (FieldInfo) member);
            return Expression.Bind(p, expression);
          });

      var constructorInfo = dynamicType.GetConstructor(Type.EmptyTypes);

      if (constructorInfo == null)
        throw new InvalidOperationException(String.Format("Unable to retrieve a constructor for the specified type: {0}", typeof(T)));

      var selector = Expression.Lambda<Func<T, object>>(
        Expression.MemberInit(Expression.New(constructorInfo), bindings.Cast<MemberBinding>()),
        sourceItem);

      if (Monitor.TryEnter(m_knownSelections, 1000))
      {
        m_knownSelections.Add(key, selector);

        Monitor.Exit(m_knownSelections);
      }

      return selector;
    }
  }
}