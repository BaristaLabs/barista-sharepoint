﻿namespace Barista.Jurassic
{
  using System;
  using System.Globalization;
  using Jurassic.Library;

  public enum PrimitiveTypeHint
  {
    None,
    Number,
    String,
  }

  /// <summary>
  /// Implements the JavaScript type conversion rules.
  /// </summary>
  public static class TypeConverter
  {
    /// <summary>
    /// Converts the given value to the given type.
    /// </summary>
    /// <param name="engine"> The script engine used to create new objects. </param>
    /// <param name="value"> The value to convert. </param>
    /// <typeparam name="T"> The type to convert the value to. </typeparam>
    /// <returns> The converted value. </returns>
    public static T ConvertTo<T>(ScriptEngine engine, object value)
    {
      return (T)ConvertTo(engine, value, typeof(T));
    }

    /// <summary>
    /// Converts the given value to the given type.
    /// </summary>
    /// <param name="engine"> The script engine used to create new objects. </param>
    /// <param name="value"> The value to convert. </param>
    /// <param name="type"> The type to convert the value to. </param>
    /// <returns> The converted value. </returns>
    public static object ConvertTo(ScriptEngine engine, object value, Type type)
    {
      if (type == typeof(bool))
        return ToBoolean(value);
      if (type == typeof(int))
        return ToInteger(value);
      if (type == typeof (uint))
        return ToUint32(value);
      if (type == typeof(double))
        return ToNumber(value);
      if (type == typeof(string))
        return ToString(value);
      if (typeof(ObjectInstance).IsAssignableFrom(type))
        return ToObject(engine, value);
      if (type == typeof(object))
        return value;
      throw new ArgumentException(string.Format("Cannot convert to '{0}'.  The type is unsupported.", type), "value");
    }

    /// <summary>
    /// Converts any JavaScript value to a primitive boolean value.
    /// </summary>
    /// <param name="value"> The value to convert. </param>
    /// <returns> A primitive boolean value. </returns>
    public static bool ToBoolean(object value)
    {
      if (value == null || value == Null.Value)
        return false;
      if (value == Undefined.Value)
        return false;
      if (value is bool)
        return (bool)value;
      if (value is int)
        return ((int)value) != 0;
      if (value is uint)
        return ((uint)value) != 0;
      if (value is double)
        return Math.Abs(((double)value) - 0) > Double.Epsilon && double.IsNaN((double)value) == false;
      if (value is string)
        return ((string)value).Length > 0;
      if (value is ConcatenatedString)
        return ((ConcatenatedString)value).Length > 0;
      if (value is ObjectInstance)
        return true;
      throw new ArgumentException(string.Format("Cannot convert object of type '{0}' to a boolean.", value.GetType()), "value");
    }

    /// <summary>
    /// Converts any JavaScript value to a primitive number value.
    /// </summary>
    /// <param name="value"> The value to convert. </param>
    /// <returns> A primitive number value. </returns>
    public static double ToNumber(object value)
    {
      if (value is double)
        return (double)value;
      if (value is SByte)
        return (SByte) value;
      if (value is Int16)
        return (Int16) value;
      if (value is Int32)
        return (Int32)value;
      if (value is Int64)
        return (Int64) value;
      if (value is Byte)
        return (Byte) value;
      if (value is UInt16)
        return (UInt16) value;
      if (value is UInt32)
        return (UInt32)value;
      if (value is UInt64)
        return (UInt64)value;
      if (value == null || value == Undefined.Value)
        return double.NaN;
      if (value == Null.Value)
        return +0;
      if (value is bool)
        return (bool)value ? 1 : 0;
      if (value is string)
        return NumberParser.CoerceToNumber((string)value);
      if (value is ConcatenatedString)
        return NumberParser.CoerceToNumber(value.ToString());
      if (value is ObjectInstance)
        return ToNumber(ToPrimitive(value, PrimitiveTypeHint.Number));
      throw new ArgumentException(string.Format("Cannot convert object of type '{0}' to a number.", value.GetType()), "value");
    }

    // Single-item cache.
    private class NumberToStringCache
    {
      public double Value;
      public string Result;
    }
    private static NumberToStringCache s_numberToStringCache = new NumberToStringCache { Value = 0.0, Result = "0" };

    /// <summary>
    /// Converts any JavaScript value to a primitive string value.
    /// </summary>
    /// <param name="value"> The value to convert. </param>
    /// <returns> A primitive string value. </returns>
    public static string ToString(object value)
    {
      if (value == null || value == Undefined.Value)
        return "undefined";
      if (value == Null.Value)
        return "null";
      if (value is bool)
        return (bool)value ? "true" : "false";
      if (value is int)
        return ((int)value).ToString(CultureInfo.InvariantCulture);
      if (value is uint)
        return ((uint)value).ToString(CultureInfo.InvariantCulture);
      if (value is double)
      {
        // Check if the value is in the cache.
        double doubleValue = (double)value;
        var cache = s_numberToStringCache;
        if (Math.Abs(doubleValue - cache.Value) < Double.Epsilon)
          return cache.Result;

        // Convert the number to a string.
        var result = NumberFormatter.ToString((double)value, 10, NumberFormatter.Style.Regular, 0);

        // Cache the result.
        // This is thread-safe on Intel but not architectures with weak write ordering.
        s_numberToStringCache = new NumberToStringCache { Value = doubleValue, Result = result };

        return result;
      }
      if (value is string)
        return (string)value;
      if (value is ConcatenatedString)
        return value.ToString();
      if (value is ObjectInstance)
        return ToString(ToPrimitive(value, PrimitiveTypeHint.String));
      throw new ArgumentException(string.Format("Cannot convert object of type '{0}' to a string.", value.GetType()), "value");
    }

    /// <summary>
    /// Converts any JavaScript value to a concatenated string value.
    /// </summary>
    /// <param name="value"> The value to convert. </param>
    /// <returns> A concatenated string value. </returns>
    public static ConcatenatedString ToConcatenatedString(object value)
    {
      if (value is ConcatenatedString)
        return (ConcatenatedString)value;
      return new ConcatenatedString(ToString(value));
    }

    /// <summary>
    /// Converts any JavaScript value to an object.
    /// </summary>
    /// <param name="engine"> The script engine used to create new objects. </param>
    /// <param name="value"> The value to convert. </param>
    /// <returns> An object. </returns>
    public static ObjectInstance ToObject(ScriptEngine engine, object value)
    {
      return ToObject(engine, value, 0, null, null);
    }

    /// <summary>
    /// Converts any JavaScript value to an object.
    /// </summary>
    /// <param name="engine"> The script engine used to create new objects. </param>
    /// <param name="value"> The value to convert. </param>
    /// <param name="lineNumber"> The line number in the source file the error occurred on. </param>
    /// <param name="sourcePath"> The path or URL of the source file.  Can be <c>null</c>. </param>
    /// <param name="functionName"> The name of the function.  Can be <c>null</c>. </param>
    /// <returns> An object. </returns>
    public static ObjectInstance ToObject(ScriptEngine engine, object value, int lineNumber, string sourcePath, string functionName)
    {
      if (engine == null)
        throw new ArgumentNullException("engine");
      if (value is ObjectInstance)
        return (ObjectInstance)value;
      if (value == null || value == Undefined.Value)
        throw new JavaScriptException(engine, "TypeError", "undefined cannot be converted to an object", lineNumber, sourcePath, functionName);
      if (value == Null.Value)
        throw new JavaScriptException(engine, "TypeError", "undefined cannot be converted to an object", lineNumber, sourcePath, functionName);
      if (value is bool)
        return engine.Boolean.Construct((bool)value);
      if (value is int)
        return engine.Number.Construct((int)value);
      if (value is uint)
        return engine.Number.Construct((uint)value);
      if (value is double)
        return engine.Number.Construct((double)value);
      if (value is string)
        return engine.String.Construct((string)value);
      if (value is ConcatenatedString)
        return engine.String.Construct(value.ToString());
      throw new ArgumentException(string.Format("Cannot convert object of type '{0}' to an object.", value.GetType()), "value");
    }

    /// <summary>
    /// Converts any JavaScript value to a primitive value.
    /// </summary>
    /// <param name="value"> The value to convert. </param>
    /// <param name="preferredType"> Specifies whether toString() or valueOf() should be
    /// preferred when converting to a primitive. </param>
    /// <returns> A primitive (non-object) value. </returns>
    public static object ToPrimitive(object value, PrimitiveTypeHint preferredType)
    {
      if (value is ObjectInstance)
        return ((ObjectInstance)value).GetPrimitiveValue(preferredType);
      return value;
    }

    /// <summary>
    /// Converts any JavaScript value to an integer.
    /// </summary>
    /// <param name="value"> The value to convert. </param>
    /// <returns> An integer value. </returns>
    public static int ToInteger(object value)
    {
      if (value == null || value is Undefined)
        return 0;
      var num = ToNumber(value);

      if (num > int.MaxValue)
        return int.MaxValue;

      if (num < int.MinValue)
        return int.MinValue;

      if (Double.IsNaN(num))
        return 0;

      return (int)num;
    }

    /// <summary>
    /// Converts any JavaScript value to a 32-bit integer.
    /// </summary>
    /// <param name="value"> The value to convert. </param>
    /// <returns> A 32-bit integer value. </returns>
    public static int ToInt32(object value)
    {
      if (value is SByte || value is Int16 || value is Int32 || value is Int64 || value is byte || value is UInt16 ||
          value is UInt32 || value is UInt64)
        return System.Convert.ToInt32(value);
      return (int)(uint)ToNumber(value);
    }

    /// <summary>
    /// Converts any JavaScript value to an unsigned 32-bit integer.
    /// </summary>
    /// <param name="value"> The value to convert. </param>
    /// <returns> An unsigned 32-bit integer value. </returns>
    public static uint ToUint32(object value)
    {
      if (value is uint)
        return (uint)value;
      return (uint)ToNumber(value);
    }

    /// <summary>
    /// Converts any JavaScript value to an unsigned 16-bit integer.
    /// </summary>
    /// <param name="value"> The value to convert. </param>
    /// <returns> An unsigned 16-bit integer value. </returns>
    public static ushort ToUint16(object value)
    {
      return (ushort)(uint)ToNumber(value);
    }

  }

}
