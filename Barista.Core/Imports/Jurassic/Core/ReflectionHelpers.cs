namespace Barista.Jurassic
{
  using System;
  using System.Collections.Generic;
  using System.Reflection;
  using Jurassic.Compiler;
  using Jurassic.Library;

  /// <summary>
  /// Contains static methods to ease reflection operations.
  /// </summary>
  internal static class ReflectionHelpers
  {
// ReSharper disable InconsistentNaming
    internal static Lazy<MethodInfo> TypeConverter_ToString = new Lazy<MethodInfo>(() => GetStaticMethod(typeof(TypeConverter), "ToString", typeof(object)));
    internal static Lazy<MethodInfo> TypeConverter_ToConcatenatedString = new Lazy<MethodInfo>(() => GetStaticMethod(typeof(TypeConverter), "ToConcatenatedString", typeof(object)));
    internal static Lazy<MethodInfo> TypeConverter_ToNumber = new Lazy<MethodInfo>(() => GetStaticMethod(typeof(TypeConverter), "ToNumber", typeof(object)));
    internal static Lazy<MethodInfo> TypeConverter_ToBoolean = new Lazy<MethodInfo>(() => GetStaticMethod(typeof(TypeConverter), "ToBoolean", typeof(object)));
    internal static Lazy<MethodInfo> TypeConverter_ToObject = new Lazy<MethodInfo>(() => GetStaticMethod(typeof(TypeConverter), "ToObject", typeof(ScriptEngine), typeof(object)));
    internal static Lazy<MethodInfo> TypeConverter_ToInteger = new Lazy<MethodInfo>(() => GetStaticMethod(typeof(TypeConverter), "ToInteger", typeof(object)));
    internal static Lazy<MethodInfo> TypeConverter_ToInt32 = new Lazy<MethodInfo>(() =>  GetStaticMethod(typeof(TypeConverter), "ToInt32", typeof(object)));
    internal static Lazy<MethodInfo> TypeConverter_ToUint32 = new Lazy<MethodInfo>(() => GetStaticMethod(typeof(TypeConverter), "ToUint32", typeof(object)));
    internal static Lazy<MethodInfo> TypeConverter_ToPrimitive = new Lazy<MethodInfo>(() => GetStaticMethod(typeof(TypeConverter), "ToPrimitive", typeof(object), typeof(PrimitiveTypeHint)));

    internal static Lazy<MethodInfo> TypeComparer_Equals = new Lazy<MethodInfo>(() =>  GetStaticMethod(typeof(TypeComparer), "Equals", typeof(object), typeof(object)));
    internal static Lazy<MethodInfo> TypeComparer_StrictEquals = new Lazy<MethodInfo>(() => GetStaticMethod(typeof(TypeComparer), "StrictEquals", typeof(object), typeof(object)));
    internal static Lazy<MethodInfo> TypeComparer_LessThan = new Lazy<MethodInfo>(() => GetStaticMethod(typeof(TypeComparer), "LessThan", typeof(object), typeof(object)));
    internal static Lazy<MethodInfo> TypeComparer_LessThanOrEqual = new Lazy<MethodInfo>(() => GetStaticMethod(typeof(TypeComparer), "LessThanOrEqual", typeof(object), typeof(object)));
    internal static Lazy<MethodInfo> TypeComparer_GreaterThan = new Lazy<MethodInfo>(() => GetStaticMethod(typeof(TypeComparer), "GreaterThan", typeof(object), typeof(object)));
    internal static Lazy<MethodInfo> TypeComparer_GreaterThanOrEqual = new Lazy<MethodInfo>(() =>  GetStaticMethod(typeof(TypeComparer), "GreaterThanOrEqual", typeof(object), typeof(object)));

    internal static Lazy<MethodInfo> TypeUtilities_TypeOf = new Lazy<MethodInfo>(() => GetStaticMethod(typeof(TypeUtilities), "TypeOf", typeof(object)));
    internal static Lazy<MethodInfo> TypeUtilities_EnumeratePropertyNames = new Lazy<MethodInfo>(() =>  GetStaticMethod(typeof(TypeUtilities), "EnumeratePropertyNames", typeof(ScriptEngine), typeof(object)));
    internal static Lazy<MethodInfo> TypeUtilities_Add = new Lazy<MethodInfo>(() => GetStaticMethod(typeof(TypeUtilities), "Add", typeof(object), typeof(object)));
    internal static Lazy<MethodInfo> TypeUtilities_IsPrimitiveOrObject = new Lazy<MethodInfo>(() => GetStaticMethod(typeof(TypeUtilities), "IsPrimitiveOrObject", typeof(object)));
    internal static Lazy<MethodInfo> TypeUtilities_VerifyThisObject = new Lazy<MethodInfo>(() => GetStaticMethod(typeof(TypeUtilities), "VerifyThisObject", typeof(ScriptEngine), typeof(object), typeof(string)));

    internal static Lazy<MethodInfo> FunctionInstance_HasInstance = new Lazy<MethodInfo>(() => GetInstanceMethod(typeof(FunctionInstance), "HasInstance", typeof(object)));
    internal static Lazy<MethodInfo> FunctionInstance_ConstructLateBound = new Lazy<MethodInfo>(() => GetInstanceMethod(typeof(FunctionInstance), "ConstructLateBound", typeof(object[])));
    internal static Lazy<MethodInfo> FunctionInstance_CallWithStackTrace = new Lazy<MethodInfo>(() => GetInstanceMethod(typeof(FunctionInstance), "CallWithStackTrace", typeof(string), typeof(string), typeof(int), typeof(object), typeof(object[])));
    internal static Lazy<MethodInfo> FunctionInstance_InstancePrototype = new Lazy<MethodInfo>(() => GetInstanceMethod(typeof(FunctionInstance), "get_InstancePrototype"));

    internal static Lazy<MethodInfo> ScriptEngine_Global = new Lazy<MethodInfo>(() => GetInstanceMethod(typeof(ScriptEngine), "get_Global"));
    internal static Lazy<MethodInfo> ScriptEngine_Boolean = new Lazy<MethodInfo>(() => GetInstanceMethod(typeof(ScriptEngine), "get_Boolean"));
    internal static Lazy<MethodInfo> ScriptEngine_Function = new Lazy<MethodInfo>(() => GetInstanceMethod(typeof(ScriptEngine), "get_Function"));
    internal static Lazy<MethodInfo> ScriptEngine_RegExp = new Lazy<MethodInfo>(() => GetInstanceMethod(typeof(ScriptEngine), "get_RegExp"));
    internal static Lazy<MethodInfo> ScriptEngine_Array = new Lazy<MethodInfo>(() => GetInstanceMethod(typeof(ScriptEngine), "get_Array"));
    internal static Lazy<MethodInfo> ScriptEngine_Object = new Lazy<MethodInfo>(() => GetInstanceMethod(typeof(ScriptEngine), "get_Object"));
    internal static Lazy<MethodInfo> Global_Eval = new Lazy<MethodInfo>(() => GetStaticMethod(typeof(GlobalObject), "Eval", typeof(ScriptEngine), typeof(object), typeof(Scope), typeof(object), typeof(bool)));

    internal static Lazy<ConstructorInfo> String_Constructor_Char_Int = new Lazy<ConstructorInfo>(() => GetConstructor(typeof(string), typeof(char), typeof(int)));
    internal static Lazy<MethodInfo> String_Concat = new Lazy<MethodInfo>(() => GetStaticMethod(typeof(string), "Concat", typeof(string), typeof(string)));
    internal static Lazy<MethodInfo> String_Length = new Lazy<MethodInfo>(() => GetInstanceMethod(typeof(string), "get_Length"));
    internal static Lazy<MethodInfo> String_CompareOrdinal = new Lazy<MethodInfo>(() => GetStaticMethod(typeof(string), "CompareOrdinal", typeof(string), typeof(string)));
    internal static Lazy<MethodInfo> String_Format = new Lazy<MethodInfo>(() => GetStaticMethod(typeof(string), "Format", typeof(string), typeof(object[])));
    internal static Lazy<MethodInfo> String_GetChars = new Lazy<MethodInfo>(() => GetInstanceMethod(typeof(string), "get_Chars", typeof(int)));

    internal static Lazy<ConstructorInfo> ConcatenatedString_Constructor_String = new Lazy<ConstructorInfo>(() => GetConstructor(typeof(ConcatenatedString), typeof(string)));
    internal static Lazy<ConstructorInfo> ConcatenatedString_Constructor_String_String = new Lazy<ConstructorInfo>(() => GetConstructor(typeof(ConcatenatedString), typeof(string), typeof(string)));
    internal static Lazy<MethodInfo> ConcatenatedString_Length = new Lazy<MethodInfo>(() => GetInstanceMethod(typeof(ConcatenatedString), "get_Length"));
    internal static Lazy<MethodInfo> ConcatenatedString_Concatenate_Object = new Lazy<MethodInfo>(() => GetInstanceMethod(typeof(ConcatenatedString), "Concatenate", typeof(object)));
    internal static Lazy<MethodInfo> ConcatenatedString_Concatenate_String = new Lazy<MethodInfo>(() => GetInstanceMethod(typeof(ConcatenatedString), "Concatenate", typeof(string)));
    internal static Lazy<MethodInfo> ConcatenatedString_Concatenate_ConcatenatedString = new Lazy<MethodInfo>(() => GetInstanceMethod(typeof(ConcatenatedString), "Concatenate", typeof(ConcatenatedString)));
    internal static Lazy<MethodInfo> ConcatenatedString_Append_Object = new Lazy<MethodInfo>(() => GetInstanceMethod(typeof(ConcatenatedString), "Append", typeof(object)));
    internal static Lazy<MethodInfo> ConcatenatedString_Append_String = new Lazy<MethodInfo>(() => GetInstanceMethod(typeof(ConcatenatedString), "Append", typeof(string)));
    internal static Lazy<MethodInfo> ConcatenatedString_Append_ConcatenatedString = new Lazy<MethodInfo>(() => GetInstanceMethod(typeof(ConcatenatedString), "Append", typeof(ConcatenatedString)));
    internal static Lazy<MethodInfo> ConcatenatedString_ToString = new Lazy<MethodInfo>(() => GetInstanceMethod(typeof(ConcatenatedString), "ToString"));

    internal static Lazy<MethodInfo> IEnumerable_GetEnumerator = new Lazy<MethodInfo>(() => GetInstanceMethod(typeof(IEnumerable<string>), "GetEnumerator"));
    internal static Lazy<MethodInfo> IEnumerator_MoveNext = new Lazy<MethodInfo>(() => GetInstanceMethod(typeof(System.Collections.IEnumerator), "MoveNext"));
    internal static Lazy<MethodInfo> IEnumerator_Current = new Lazy<MethodInfo>(() => GetInstanceMethod(typeof(IEnumerator<string>), "get_Current"));

    internal static Lazy<MethodInfo> Debugger_Break = new Lazy<MethodInfo>(() => GetStaticMethod(typeof(System.Diagnostics.Debugger), "Break"));
    internal static Lazy<MethodInfo> JavaScriptException_ErrorObject = new Lazy<MethodInfo>(() => GetInstanceMethod(typeof(JavaScriptException), "get_ErrorObject"));
    internal static Lazy<MethodInfo> Boolean_Construct = new Lazy<MethodInfo>(() => GetInstanceMethod(typeof(BooleanConstructor), "Construct", typeof(bool)));
    internal static Lazy<MethodInfo> Object_Construct = new Lazy<MethodInfo>(() => GetInstanceMethod(typeof(ObjectConstructor), "Construct"));

    internal static Lazy<MethodInfo> RegExp_Construct = new Lazy<MethodInfo>(() => GetInstanceMethod(typeof(Jurassic.Library.RegExpConstructor), "Construct", typeof(object), typeof(object)));
    internal static Lazy<MethodInfo> Array_New = new Lazy<MethodInfo>(() => GetInstanceMethod(typeof(ArrayConstructor), "New", typeof(object[])));
    internal static Lazy<MethodInfo> Delegate_CreateDelegate = new Lazy<MethodInfo>(() => GetStaticMethod(typeof(Delegate), "CreateDelegate", typeof(Type), typeof(MethodInfo)));
    internal static Lazy<MethodInfo> Type_GetTypeFromHandle = new Lazy<MethodInfo>(() => GetStaticMethod(typeof(Type), "GetTypeFromHandle", typeof(RuntimeTypeHandle)));
    internal static Lazy<MethodInfo> MethodBase_GetMethodFromHandle = new Lazy<MethodInfo>(() => GetStaticMethod(typeof(MethodBase), "GetMethodFromHandle", typeof(RuntimeMethodHandle)));

    internal static Lazy<MethodInfo> GeneratedMethod_Load = new Lazy<MethodInfo>(() => GetStaticMethod(typeof(GeneratedMethod), "Load", typeof(long)));
    internal static Lazy<MethodInfo> ClrInstanceWrapper_GetWrappedInstance = new Lazy<MethodInfo>(() => GetInstanceMethod(typeof(ClrInstanceWrapper), "get_WrappedInstance"));
    internal static Lazy<MethodInfo> Decimal_ToDouble = new Lazy<MethodInfo>(() =>  GetStaticMethod(typeof(decimal), "ToDouble", typeof(decimal)));
    internal static Lazy<MethodInfo> BinderUtilities_ResolveOverloads = new Lazy<MethodInfo>(() =>  GetStaticMethod(typeof(BinderUtilities), "ResolveOverloads", typeof(RuntimeMethodHandle[]), typeof(ScriptEngine), typeof(object), typeof(object[])));
    internal static Lazy<MethodInfo> Convert_ToInt32_Double = new Lazy<MethodInfo>(() => GetStaticMethod(typeof(Convert), "ToInt32", typeof(double)));

    internal static Lazy<MethodInfo> ObjectInstance_Delete = new Lazy<MethodInfo>(() => GetInstanceMethod(typeof(ObjectInstance), "Delete", typeof(string), typeof(bool)));
    internal static Lazy<MethodInfo> ObjectInstance_DefineProperty = new Lazy<MethodInfo>(() => GetInstanceMethod(typeof(ObjectInstance), "DefineProperty", typeof(string), typeof(PropertyDescriptor), typeof(bool)));
    internal static Lazy<MethodInfo> ObjectInstance_HasProperty = new Lazy<MethodInfo>(() => GetInstanceMethod(typeof(ObjectInstance), "HasProperty", typeof(string)));
    internal static Lazy<MethodInfo> ObjectInstance_GetPropertyValue_String = new Lazy<MethodInfo>(() => GetInstanceMethod(typeof(ObjectInstance), "GetPropertyValue", typeof(string)));
    internal static Lazy<MethodInfo> ObjectInstance_GetPropertyValue_Int = new Lazy<MethodInfo>(() => GetInstanceMethod(typeof(ObjectInstance), "GetPropertyValue", typeof(uint)));
    internal static Lazy<MethodInfo> ObjectInstance_SetPropertyValue_String = new Lazy<MethodInfo>(() => GetInstanceMethod(typeof(ObjectInstance), "SetPropertyValue", typeof(string), typeof(object), typeof(bool)));
    internal static Lazy<MethodInfo> ObjectInstance_SetPropertyValue_Int = new Lazy<MethodInfo>(() => GetInstanceMethod(typeof(ObjectInstance), "SetPropertyValue", typeof(uint), typeof(object), typeof(bool)));
    internal static Lazy<MethodInfo> ObjectInstance_SetPropertyValueIfExists = new Lazy<MethodInfo>(() => GetInstanceMethod(typeof(ObjectInstance), "SetPropertyValueIfExists", typeof(string), typeof(object), typeof(bool)));
    internal static Lazy<MethodInfo> ObjectInstance_InlinePropertyValues = new Lazy<MethodInfo>(() => GetInstanceMethod(typeof(ObjectInstance), "get_InlinePropertyValues"));
    internal static Lazy<MethodInfo> ObjectInstance_InlineCacheKey = new Lazy<MethodInfo>(() => GetInstanceMethod(typeof(ObjectInstance), "get_InlineCacheKey"));
    internal static Lazy<MethodInfo> ObjectInstance_InlineGetPropertyValue = new Lazy<MethodInfo>(() => GetInstanceMethod(typeof(ObjectInstance), "InlineGetPropertyValue",
          new[] { typeof(string), typeof(int).MakeByRefType(), typeof(object).MakeByRefType() }));
    internal static Lazy<MethodInfo> ObjectInstance_InlineSetPropertyValue = new Lazy<MethodInfo>(() => GetInstanceMethod(typeof(ObjectInstance), "InlineSetPropertyValue",
          new[] { typeof(string), typeof(object), typeof(bool), typeof(int).MakeByRefType(), typeof(object).MakeByRefType() }));
    internal static Lazy<MethodInfo> ObjectInstance_InlineSetPropertyValueIfExists = new Lazy<MethodInfo>(() => GetInstanceMethod(typeof(ObjectInstance), "InlineSetPropertyValueIfExists",
          new[] { typeof(string), typeof(object), typeof(bool), typeof(int).MakeByRefType(), typeof(object).MakeByRefType() }));

    internal static Lazy<MethodInfo> Scope_ParentScope = new Lazy<MethodInfo>(() => GetInstanceMethod(typeof(Scope), "get_ParentScope"));
    internal static Lazy<MethodInfo> ObjectScope_CreateRuntimeScope = new Lazy<MethodInfo>(() => GetStaticMethod(typeof(ObjectScope), "CreateRuntimeScope", typeof(Scope), typeof(ObjectInstance), typeof(bool), typeof(bool)));
    internal static Lazy<MethodInfo> ObjectScope_ScopeObject = new Lazy<MethodInfo>(() => GetInstanceMethod(typeof(ObjectScope), "get_ScopeObject"));
    internal static Lazy<MethodInfo> DeclarativeScope_CreateRuntimeScope = new Lazy<MethodInfo>(() => GetStaticMethod(typeof(DeclarativeScope), "CreateRuntimeScope", typeof(Scope), typeof(string[])));
    internal static Lazy<MethodInfo> DeclarativeScope_Values = new Lazy<MethodInfo>(() => GetInstanceMethod(typeof(DeclarativeScope), "get_Values"));
    internal static Lazy<MethodInfo> Scope_HasValue = new Lazy<MethodInfo>(() => GetInstanceMethod(typeof(Scope), "HasValue", typeof(string)));
    internal static Lazy<MethodInfo> Scope_GetValue = new Lazy<MethodInfo>(() => GetInstanceMethod(typeof(Scope), "GetValue", typeof(string)));
    internal static Lazy<MethodInfo> Scope_SetValue = new Lazy<MethodInfo>(() => GetInstanceMethod(typeof(Scope), "SetValue", typeof(string), typeof(object)));
    internal static Lazy<MethodInfo> Scope_Delete = new Lazy<MethodInfo>(() => GetInstanceMethod(typeof(Scope), "Delete", typeof(string)));

    internal static Lazy<ConstructorInfo> JavaScriptException_Constructor_Error = new Lazy<ConstructorInfo>(() => GetConstructor(typeof(JavaScriptException), typeof(ScriptEngine), typeof(string), typeof(string), typeof(int), typeof(string), typeof(string)));
    internal static Lazy<ConstructorInfo> JavaScriptException_Constructor_Object = new Lazy<ConstructorInfo>(() => GetConstructor(typeof(JavaScriptException), typeof(object), typeof(int), typeof(string), typeof(string)));
    internal static Lazy<ConstructorInfo> UserDefinedFunction_Constructor = new Lazy<ConstructorInfo>(() => GetConstructor(typeof(UserDefinedFunction), typeof(ObjectInstance),
          typeof(string), typeof(IList<string>), typeof(Scope), typeof(string), typeof(GeneratedMethod), typeof(bool)));
    internal static Lazy<ConstructorInfo> FunctionDelegate_Constructor = new Lazy<ConstructorInfo>(() => GetConstructor(typeof(Library.FunctionDelegate), typeof(object), typeof(IntPtr)));
    internal static Lazy<ConstructorInfo> Arguments_Constructor = new Lazy<ConstructorInfo>(() => GetConstructor(typeof(ArgumentsInstance), typeof(ObjectInstance), typeof(UserDefinedFunction), typeof(DeclarativeScope), typeof(object[])));
    internal static Lazy<ConstructorInfo> PropertyDescriptor_Constructor2 = new Lazy<ConstructorInfo>(() => GetConstructor(typeof(PropertyDescriptor), typeof(object), typeof(Library.PropertyAttributes)));
    internal static Lazy<ConstructorInfo> PropertyDescriptor_Constructor3 = new Lazy<ConstructorInfo>(() => GetConstructor(typeof(PropertyDescriptor), typeof(FunctionInstance), typeof(FunctionInstance), typeof(Library.PropertyAttributes)));
    internal static Lazy<ConstructorInfo> ClrInstanceWrapper_Constructor = new Lazy<ConstructorInfo>(() => GetConstructor(typeof(ClrInstanceWrapper), typeof(ScriptEngine), typeof(object)));
    internal static Lazy<ConstructorInfo> Decimal_Constructor_Double = new Lazy<ConstructorInfo>(() => GetConstructor(typeof(decimal), typeof(double)));

    internal static Lazy<FieldInfo> Undefined_Value = new Lazy<FieldInfo>(() => GetField(typeof(Undefined), "Value"));
    internal static Lazy<FieldInfo> Null_Value = new Lazy<FieldInfo>(() => GetField(typeof(Null), "Value"));

    internal static Lazy<ConstructorInfo> LongJumpException_Constructor = new Lazy<ConstructorInfo>(() => GetConstructor(typeof(LongJumpException), typeof(int)));
    internal static Lazy<MethodInfo> LongJumpException_RouteID = new Lazy<MethodInfo>(() => GetInstanceMethod(typeof(LongJumpException), "get_RouteID"));
// ReSharper restore InconsistentNaming

    /// <summary>
    /// Initializes static members of this class.
    /// </summary>
    static ReflectionHelpers()
    {
#if DEBUG
      // When using Reflection Emit, all calls into Jurassic.dll are cross-assembly and thus
      // must be public.
      var text = new System.Text.StringBuilder();
      foreach (var reflectionField in GetMembers())
      {
        var methodBase = reflectionField.MemberInfo as MethodBase;
        if (methodBase != null && (methodBase.Attributes & MethodAttributes.Public) != MethodAttributes.Public)
        {
          if (methodBase.DeclaringType != null)
            text.Append(methodBase.DeclaringType);

          text.Append("/");
          text.AppendLine(methodBase.ToString());
        }
        var field = reflectionField.MemberInfo as FieldInfo;
        if (field != null && (field.Attributes & FieldAttributes.Public) != FieldAttributes.Public)
          text.AppendLine(field.ToString());
        if (reflectionField.MemberInfo.DeclaringType != null && (reflectionField.MemberInfo.DeclaringType.Attributes & TypeAttributes.Public) != TypeAttributes.Public)
          text.AppendLine(reflectionField.MemberInfo.DeclaringType.ToString());
      }
      if (text.Length > 0)
        throw new InvalidOperationException("The following members need to be public: " + Environment.NewLine + text);

      // For ease of debugging, all runtime calls should have the DebuggerHidden
      // attribute.
      //text.Clear();
      //foreach (var reflectionField in GetMembers())
      //{
      //    var methodBase = reflectionField.MemberInfo as MethodBase;
      //    if (methodBase != null && Attribute.GetCustomAttribute(methodBase, typeof(System.Diagnostics.DebuggerHiddenAttribute)) == null)
      //    {
      //        text.Append(methodBase.DeclaringType.ToString());
      //        text.Append("/");
      //        text.AppendLine(methodBase.ToString());
      //    }
      //}
      //if (text.Length > 0)
      //    throw new InvalidOperationException("The following methods do not have [DebuggerHidden]: " + Environment.NewLine + text.ToString());
#endif
    }

    internal struct ReflectionField
    {
      public string FieldName;
      public MemberInfo MemberInfo;
    }

#if DEBUG
    /// <summary>
    /// Quick and dirty way to verify that the Method/Field/ConstructorInfo objects can be retrieved.
    /// </summary>
    internal static void TestFunctionRetrieval()
    {
      object result;
      result = ReflectionHelpers.Arguments_Constructor.Value;
      result = ReflectionHelpers.Array_New;
      result = ReflectionHelpers.BinderUtilities_ResolveOverloads;
      result = ReflectionHelpers.Boolean_Construct;
      result = ReflectionHelpers.ClrInstanceWrapper_Constructor;
      result = ReflectionHelpers.ClrInstanceWrapper_GetWrappedInstance;
      result = ReflectionHelpers.ConcatenatedString_Append_ConcatenatedString;
      result = ReflectionHelpers.ConcatenatedString_Append_Object;
      result = ReflectionHelpers.ConcatenatedString_Append_String;
      result = ReflectionHelpers.ConcatenatedString_Concatenate_ConcatenatedString;
      result = ReflectionHelpers.ConcatenatedString_Concatenate_Object;
      result = ReflectionHelpers.ConcatenatedString_Concatenate_String;
      result = ReflectionHelpers.ConcatenatedString_Constructor_String;
      result = ReflectionHelpers.ConcatenatedString_Constructor_String_String;
      result = ReflectionHelpers.ConcatenatedString_Length;
      result = ReflectionHelpers.ConcatenatedString_ToString;
      result = ReflectionHelpers.Convert_ToInt32_Double;
      result = ReflectionHelpers.Debugger_Break;
      result = ReflectionHelpers.Decimal_Constructor_Double;
      result = ReflectionHelpers.Decimal_ToDouble;
      result = ReflectionHelpers.DeclarativeScope_CreateRuntimeScope;
      result = ReflectionHelpers.DeclarativeScope_Values;
      result = ReflectionHelpers.Delegate_CreateDelegate;
      result = ReflectionHelpers.FunctionDelegate_Constructor;
      result = ReflectionHelpers.FunctionInstance_CallWithStackTrace;
      result = ReflectionHelpers.FunctionInstance_ConstructLateBound;
      result = ReflectionHelpers.FunctionInstance_HasInstance;
      result = ReflectionHelpers.FunctionInstance_InstancePrototype;
      result = ReflectionHelpers.GeneratedMethod_Load;
      result = ReflectionHelpers.IEnumerable_GetEnumerator;
      result = ReflectionHelpers.IEnumerator_Current;
      result = ReflectionHelpers.IEnumerator_MoveNext;
      result = ReflectionHelpers.JavaScriptException_Constructor_Error;
      result = ReflectionHelpers.JavaScriptException_Constructor_Object;
      result = ReflectionHelpers.JavaScriptException_ErrorObject;
      result = ReflectionHelpers.LongJumpException_Constructor;
      result = ReflectionHelpers.LongJumpException_RouteID;
      result = ReflectionHelpers.MethodBase_GetMethodFromHandle;
      result = ReflectionHelpers.Null_Value;
      result = ReflectionHelpers.ObjectInstance_DefineProperty;
      result = ReflectionHelpers.ObjectInstance_Delete;
      result = ReflectionHelpers.ObjectInstance_GetPropertyValue_Int;
      result = ReflectionHelpers.ObjectInstance_GetPropertyValue_String;
      result = ReflectionHelpers.ObjectInstance_HasProperty;
      result = ReflectionHelpers.ObjectInstance_InlineCacheKey;
      result = ReflectionHelpers.ObjectInstance_InlineGetPropertyValue;
      result = ReflectionHelpers.ObjectInstance_InlinePropertyValues;
      result = ReflectionHelpers.ObjectInstance_InlineSetPropertyValue;
      result = ReflectionHelpers.ObjectInstance_InlineSetPropertyValueIfExists;
      result = ReflectionHelpers.ObjectInstance_SetPropertyValueIfExists;
      result = ReflectionHelpers.ObjectInstance_SetPropertyValue_Int;
      result = ReflectionHelpers.ObjectInstance_SetPropertyValue_String;
      result = ReflectionHelpers.ObjectScope_CreateRuntimeScope;
      result = ReflectionHelpers.ObjectScope_ScopeObject;
      result = ReflectionHelpers.Object_Construct;
      result = ReflectionHelpers.PropertyDescriptor_Constructor2;
      result = ReflectionHelpers.PropertyDescriptor_Constructor3;
      result = ReflectionHelpers.RegExp_Construct;
      result = ReflectionHelpers.Scope_Delete;
      result = ReflectionHelpers.Scope_GetValue;
      result = ReflectionHelpers.Scope_HasValue;
      result = ReflectionHelpers.Scope_ParentScope;
      result = ReflectionHelpers.Scope_SetValue;
      result = ReflectionHelpers.ScriptEngine_Array;
      result = ReflectionHelpers.ScriptEngine_Boolean;
      result = ReflectionHelpers.ScriptEngine_Function;
      result = ReflectionHelpers.ScriptEngine_Global;
      result = ReflectionHelpers.ScriptEngine_Object;
      result = ReflectionHelpers.ScriptEngine_RegExp;
      result = ReflectionHelpers.String_CompareOrdinal;
      result = ReflectionHelpers.String_Concat;
      result = ReflectionHelpers.String_Constructor_Char_Int;
      result = ReflectionHelpers.String_Format;
      result = ReflectionHelpers.String_GetChars;
      result = ReflectionHelpers.String_Length;
      result = ReflectionHelpers.TypeComparer_Equals;
      result = ReflectionHelpers.TypeComparer_GreaterThan;
      result = ReflectionHelpers.TypeComparer_GreaterThanOrEqual;
      result = ReflectionHelpers.TypeComparer_LessThan;
      result = ReflectionHelpers.TypeComparer_LessThanOrEqual;
      result = ReflectionHelpers.TypeComparer_StrictEquals;
      result = ReflectionHelpers.TypeConverter_ToBoolean;
      result = ReflectionHelpers.TypeConverter_ToConcatenatedString;
      result = ReflectionHelpers.TypeConverter_ToInt32;
      result = ReflectionHelpers.TypeConverter_ToInteger;
      result = ReflectionHelpers.TypeConverter_ToNumber;
      result = ReflectionHelpers.TypeConverter_ToObject;
      result = ReflectionHelpers.TypeConverter_ToPrimitive;
      result = ReflectionHelpers.TypeConverter_ToString;
      result = ReflectionHelpers.TypeConverter_ToUint32;
      result = ReflectionHelpers.TypeUtilities_Add;
      result = ReflectionHelpers.TypeUtilities_EnumeratePropertyNames;
      result = ReflectionHelpers.TypeUtilities_IsPrimitiveOrObject;
      result = ReflectionHelpers.TypeUtilities_TypeOf;
      result = ReflectionHelpers.TypeUtilities_VerifyThisObject;
      result = ReflectionHelpers.Type_GetTypeFromHandle;
      result = ReflectionHelpers.Undefined_Value;
      result = ReflectionHelpers.UserDefinedFunction_Constructor;
    }
#endif

    /// <summary>
    /// Gets an enumerable list of all the MemberInfos that are statically known to be used by this DLL.
    /// </summary>
    /// <returns> An enumerable list of all the MemberInfos that are used by this DLL. </returns>
    internal static IEnumerable<ReflectionField> GetMembers()
    {
      foreach (FieldInfo field in typeof(ReflectionHelpers).GetFields(BindingFlags.NonPublic | BindingFlags.Static))
      {
        if (field.FieldType != typeof(MethodInfo) && field.FieldType != typeof(ConstructorInfo) && field.FieldType != typeof(FieldInfo))
          continue;
        yield return new ReflectionField { FieldName = field.Name, MemberInfo = (MemberInfo)field.GetValue(null) };
      }
    }

    /// <summary>
    /// Gets the FieldInfo for a field.  Throws an exception if the search fails.
    /// </summary>
    /// <param name="type"> The type to search. </param>
    /// <param name="name"> The name of the field. </param>
    /// <returns> The FieldInfo for a field. </returns>
    public static FieldInfo GetField(Type type, string name)
    {
      var result = type.GetField(name);
      if (result == null)
        throw new InvalidOperationException(string.Format("The field '{1}' does not exist on type '{0}'.", type, name));
      return result;
    }

    /// <summary>
    /// Gets the ConstructorInfo for a constructor.  Throws an exception if the search fails.
    /// </summary>
    /// <param name="type"> The type to search. </param>
    /// <param name="parameterTypes"> The types of the parameters accepted by the constructor. </param>
    /// <returns> The ConstructorInfo for the constructor. </returns>
    public static ConstructorInfo GetConstructor(Type type, params Type[] parameterTypes)
    {
      const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
      var result = type.GetConstructor(flags, null, parameterTypes, null);
      if (result == null)
        throw new InvalidOperationException(string.Format("The constructor {0}({1}) does not exist.", type.FullName, StringHelpers.Join(", ", parameterTypes)));
      return result;
    }

    /// <summary>
    /// Gets the MethodInfo for an instance method.  Throws an exception if the search fails.
    /// </summary>
    /// <param name="type"> The type to search. </param>
    /// <param name="name"> The name of the method to search for. </param>
    /// <param name="parameterTypes"> The types of the parameters accepted by the method. </param>
    /// <returns> The MethodInfo for the method. </returns>
    public static MethodInfo GetInstanceMethod(Type type, string name, params Type[] parameterTypes)
    {
      const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.ExactBinding;
      var result = type.GetMethod(name, flags, null, parameterTypes, null);
      if (result == null)
        throw new InvalidOperationException(string.Format("The instance method {0}.{1}({2}) does not exist.", type.FullName, name, StringHelpers.Join(", ", parameterTypes)));
      return result;
    }

    /// <summary>
    /// Gets the MethodInfo for a static method.  Throws an exception if the search fails.
    /// </summary>
    /// <param name="type"> The type to search. </param>
    /// <param name="name"> The name of the method to search for. </param>
    /// <param name="parameterTypes"> The types of the parameters accepted by the method. </param>
    /// <returns> The MethodInfo for the method. </returns>
    public static MethodInfo GetStaticMethod(Type type, string name, params Type[] parameterTypes)
    {
      const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.ExactBinding;
      var result = type.GetMethod(name, flags, null, parameterTypes, null);
      if (result == null)
        throw new InvalidOperationException(string.Format("The static method {0}.{1}({2}) does not exist.", type.FullName, name, StringHelpers.Join(", ", parameterTypes)));
      return result;
    }

    /// <summary>
    /// Gets the MethodInfo for a generic instance method.  Throws an exception if the search fails.
    /// </summary>
    /// <param name="type"> The type to search. </param>
    /// <param name="name"> The name of the method to search for. </param>
    /// <returns> The MethodInfo for the method. </returns>
    private static MethodInfo GetGenericInstanceMethod(Type type, string name)
    {
      const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
      var result = type.GetMethod(name, flags);
      if (result == null)
        throw new InvalidOperationException(string.Format("The instance method {0}.{1}(...) does not exist.", type.FullName, name));
      return result;
    }

  }
}