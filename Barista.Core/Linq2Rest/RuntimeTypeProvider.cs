namespace Barista.Linq2Rest
{
  using System;
  using System.Collections.Concurrent;
  using System.Collections.Generic;
  using System.Linq;
  using System.Reflection;
  using System.Reflection.Emit;
  using System.Threading;
  using Linq2Rest.Parser;

  /// <summary>
  /// Defines the RuntimeTypeProvider.
  /// </summary>
  public class RuntimeTypeProvider : IRuntimeTypeProvider
  {
    private const MethodAttributes GetSetAttr = MethodAttributes.Final | MethodAttributes.Public;
    private static readonly AssemblyName AssemblyName = new AssemblyName {Name = "Linq2RestTypes"};
    private static readonly ModuleBuilder ModuleBuilder;
    private static readonly ConcurrentDictionary<string, Type> BuiltTypes = new ConcurrentDictionary<string, Type>();

    private static readonly ConcurrentDictionary<Type, CustomAttributeBuilder[]> TypeAttributeBuilders =
      new ConcurrentDictionary<Type, CustomAttributeBuilder[]>();

    private static readonly ConcurrentDictionary<MemberInfo, CustomAttributeBuilder[]> PropertyAttributeBuilders =
      new ConcurrentDictionary<MemberInfo, CustomAttributeBuilder[]>();

    private readonly IMemberNameResolver m_nameResolver;

    static RuntimeTypeProvider()
    {
      ModuleBuilder = Thread
        .GetDomain()
        .DefineDynamicAssembly(AssemblyName, AssemblyBuilderAccess.Run)
        .DefineDynamicModule(AssemblyName.Name);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RuntimeTypeProvider"/> class.
    /// </summary>
    /// <param name="nameResolver"></param>
    public RuntimeTypeProvider(IMemberNameResolver nameResolver)
    {
      if (nameResolver == null)
        throw new ArgumentNullException("nameResolver");

      m_nameResolver = nameResolver;
    }

    /// <summary>
    /// Gets the <see cref="Type"/> matching the provided members.
    /// </summary>
    /// <param name="sourceType">The <see cref="Type"/> to generate the runtime type from.</param>
    /// <param name="properties">The <see cref="MemberInfo"/> to use to generate properties.</param>
    /// <returns>A <see cref="Type"/> mathing the provided properties.</returns>
    public Type Get(Type sourceType, IEnumerable<MemberInfo> properties)
    {
      properties = properties.ToArray();
      if (!properties.Any())
      {
        throw new ArgumentOutOfRangeException("properties", @"properties must have at least 1 property definition");
      }

      var dictionary = properties.ToDictionary(f => m_nameResolver.ResolveName(f), memberInfo => memberInfo);

      var className = GetTypeKey(sourceType, dictionary);
      return BuiltTypes.GetOrAdd(
        className,
        s =>
          {
            var typeBuilder = ModuleBuilder.DefineType(
              className,
              TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Serializable);

            SetAttributes(typeBuilder, sourceType);

            foreach (var field in dictionary)
            {
              CreateProperty(typeBuilder, field);
            }

            return typeBuilder.CreateType();
          });
    }

    private static void CreateProperty(TypeBuilder typeBuilder, KeyValuePair<string, MemberInfo> field)
    {
      if (typeBuilder == null)
        throw new ArgumentNullException("typeBuilder");

      var propertyType = field.Value.MemberType == MemberTypes.Property
                           ? ((PropertyInfo) field.Value).PropertyType
                           : ((FieldInfo) field.Value).FieldType;
      var fieldBuilder = typeBuilder.DefineField("_" + field.Key, propertyType, FieldAttributes.Private);

      var propertyBuilder = typeBuilder.DefineProperty(field.Key, PropertyAttributes.None, propertyType, null);

      SetAttributes(propertyBuilder, field.Value);

      var getAccessor = typeBuilder.DefineMethod(
        "get_" + field.Key,
        GetSetAttr,
        propertyType,
        Type.EmptyTypes);

      var getIl = getAccessor.GetILGenerator();
      getIl.Emit(OpCodes.Ldarg_0);
      getIl.Emit(OpCodes.Ldfld, fieldBuilder);
      getIl.Emit(OpCodes.Ret);

      var setAccessor = typeBuilder.DefineMethod(
        "set_" + field.Key,
        GetSetAttr,
        null,
        new[] {propertyType});

      var setIl = setAccessor.GetILGenerator();
      setIl.Emit(OpCodes.Ldarg_0);
      setIl.Emit(OpCodes.Ldarg_1);
      setIl.Emit(OpCodes.Stfld, fieldBuilder);
      setIl.Emit(OpCodes.Ret);

      propertyBuilder.SetGetMethod(getAccessor);
      propertyBuilder.SetSetMethod(setAccessor);
    }

    private static void SetAttributes(TypeBuilder typeBuilder, Type type)
    {
      if (typeBuilder == null)
        throw new ArgumentNullException("typeBuilder");

      var attributeBuilders = TypeAttributeBuilders
        .GetOrAdd(
          type,
          t =>
            {
              var customAttributes = CustomAttributeData.GetCustomAttributes(t);
              return CreateCustomAttributeBuilders(customAttributes).ToArray();
            });

      foreach (var attributeBuilder in attributeBuilders)
      {
        typeBuilder.SetCustomAttribute(attributeBuilder);
      }
    }

    private static void SetAttributes(PropertyBuilder propertyBuilder, MemberInfo memberInfo)
    {
      if (propertyBuilder == null)
        throw new ArgumentNullException("propertyBuilder");

      if (memberInfo == null)
        throw new ArgumentNullException("memberInfo");

      var customAttributeBuilders = PropertyAttributeBuilders
        .GetOrAdd(
          memberInfo,
          p =>
            {
              var customAttributes = CustomAttributeData.GetCustomAttributes(p);
              return CreateCustomAttributeBuilders(customAttributes).ToArray();
            });
      foreach (var attribute in customAttributeBuilders)
      {
        propertyBuilder.SetCustomAttribute(attribute);
      }
    }

    private static IEnumerable<CustomAttributeBuilder> CreateCustomAttributeBuilders(
      IEnumerable<CustomAttributeData> customAttributes)
    {
      if (customAttributes == null)
        throw new ArgumentNullException("customAttributes");

      var attributeBuilders = customAttributes
        .Select(
          x =>

            {
              var namedArguments = x.NamedArguments;

              if (namedArguments == null)
                throw new InvalidOperationException(String.Format("{0} named arguments was null.", x));
               
              var properties = namedArguments.Select(a => a.MemberInfo).OfType<PropertyInfo>().ToArray();
              var values = namedArguments.Select(a => a.TypedValue.Value).ToArray();
              var constructorArgs = x.ConstructorArguments.Select(a => a.Value).ToArray();
              var constructor = x.Constructor;
              return new CustomAttributeBuilder(constructor, constructorArgs, properties, values);
            });
      return attributeBuilders;
    }

    private static string GetTypeKey(Type sourceType, Dictionary<string, MemberInfo> fields)
    {
      if (sourceType == null)
        throw new ArgumentNullException("sourceType");

      if (fields == null)
        throw new ArgumentNullException("fields");

      return fields.Aggregate("Linq2Rest<>" + sourceType.Name,
                              (current, field) => current + (field.Key + field.Value.MemberType));
    }
  }
}