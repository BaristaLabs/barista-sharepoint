namespace Barista
{
  using System;
  using System.Collections.Generic;
  using System.Reflection;
  using System.IO;
  using System.Linq;
  using System.Runtime.Serialization.Formatters.Binary;

  /// <summary>
  /// Assembly Util Class
  /// </summary>
  public static class AssemblyUtil
  {
    /// <summary>
    /// Creates the instance from type name.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="type">The type.</param>
    /// <returns></returns>
    public static T CreateInstance<T>(string type)
    {
      return CreateInstance<T>(type, new object[0]);
    }

    /// <summary>
    /// Creates the instance from type name and parameters.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="type">The type.</param>
    /// <param name="parameters">The parameters.</param>
    /// <returns></returns>
    public static T CreateInstance<T>(string type, object[] parameters)
    {
      Type instanceType = Type.GetType(type, true);

      if (instanceType == null)
        throw new Exception(string.Format("The type '{0}' was not found!", type));

      object instance = Activator.CreateInstance(instanceType, parameters);
      T result = (T)instance;
      return result;
    }

    /// <summary>
    /// Gets the implement types from assembly.
    /// </summary>
    /// <typeparam name="TBaseType">The type of the base type.</typeparam>
    /// <param name="assembly">The assembly.</param>
    /// <returns></returns>
    public static IEnumerable<Type> GetImplementTypes<TBaseType>(this Assembly assembly)
    {
      return assembly.GetExportedTypes().Where(t =>
          t.IsSubclassOf(typeof(TBaseType)) && t.IsClass && !t.IsAbstract);
    }

    /// <summary>
    /// Gets the implemented objects by interface.
    /// </summary>
    /// <typeparam name="TBaseInterface">The type of the base interface.</typeparam>
    /// <param name="assembly">The assembly.</param>
    /// <returns></returns>
    public static IEnumerable<TBaseInterface> GetImplementedObjectsByInterface<TBaseInterface>(this Assembly assembly)
        where TBaseInterface : class
    {
      return GetImplementedObjectsByInterface<TBaseInterface>(assembly, typeof(TBaseInterface));
    }

    /// <summary>
    /// Gets the implemented objects by interface.
    /// </summary>
    /// <typeparam name="TBaseInterface">The type of the base interface.</typeparam>
    /// <param name="assembly">The assembly.</param>
    /// <param name="targetType">Type of the target.</param>
    /// <returns></returns>
    public static IEnumerable<TBaseInterface> GetImplementedObjectsByInterface<TBaseInterface>(this Assembly assembly, Type targetType)
        where TBaseInterface : class
    {
      Type[] arrType = assembly.GetExportedTypes();

      return (from currentImplementType in arrType 
              where !currentImplementType.IsAbstract 
              where targetType.IsAssignableFrom(currentImplementType)
              select (TBaseInterface) Activator.CreateInstance(currentImplementType))
              .ToList();
    }

    /// <summary>
    /// Clone object in binary format.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="target">The target.</param>
    /// <returns></returns>
    public static T BinaryClone<T>(this T target)
    {
      BinaryFormatter formatter = new BinaryFormatter();
      using (MemoryStream ms = new MemoryStream())
      {
        formatter.Serialize(ms, target);
        ms.Position = 0;
        return (T)formatter.Deserialize(ms);
      }
    }

    private static readonly object[] EmptyObjectArray = new object[] { };

    /// <summary>
    /// Copies the properties of one object to another object.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="target">The target.</param>
    public static T CopyPropertiesTo<T>(this T source, T target)
    {
      PropertyInfo[] properties = source.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);
      Dictionary<string, PropertyInfo> sourcePropertiesDict = properties.ToDictionary(p => p.Name);

      PropertyInfo[] targetProperties = target.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);
      foreach (var p in targetProperties)
      {
        PropertyInfo sourceProperty;

        if (sourcePropertiesDict.TryGetValue(p.Name, out sourceProperty))
        {
          if (sourceProperty.PropertyType != p.PropertyType)
            continue;

          if (!sourceProperty.PropertyType.IsSerializable)
            continue;

          p.SetValue(target, sourceProperty.GetValue(source, EmptyObjectArray), EmptyObjectArray);
        }
      }

      return target;
    }

    /// <summary>
    /// Gets the assemblies from string.
    /// </summary>
    /// <param name="assemblyDef">The assembly def.</param>
    /// <returns></returns>
    public static IEnumerable<Assembly> GetAssembliesFromString(string assemblyDef)
    {
      return GetAssembliesFromStrings(assemblyDef.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries));
    }

    /// <summary>
    /// Gets the assemblies from strings.
    /// </summary>
    /// <param name="assemblies">The assemblies.</param>
    /// <returns></returns>
    public static IEnumerable<Assembly> GetAssembliesFromStrings(string[] assemblies)
    {
      List<Assembly> result = new List<Assembly>(assemblies.Length);
      result.AddRange(assemblies.Select(a => Assembly.Load(a)));

      return result;
    }
  }
}
