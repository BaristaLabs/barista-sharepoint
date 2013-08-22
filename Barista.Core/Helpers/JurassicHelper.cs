namespace Barista
{
  using System;
  using System.Text;
  using Jurassic;
  using Jurassic.Library;
  using Barista.Newtonsoft.Json;

  public static class JurassicHelper
  {

    public static bool IsRegexp(object obj)
    {
      return (obj is RegExpInstance);
    }

    public static bool IsObjectType(object obj)
    {
      if (obj == null || obj == Null.Value || obj == Undefined.Value)
        return false;

      return (TypeUtilities.TypeOf(obj) == "object" || IsRegexp(obj));
    }

    public static double ToJsDate(DateTime dateTime)
    {
      return dateTime.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
    }

    public static DateInstance ToDateInstance(ScriptEngine engine, DateTime dateTime)
    {
      var dateNumeric = ToJsDate(dateTime);
      return engine.Date.Construct(dateNumeric);
    }

    /// <summary>
    /// Helper method to be used with optional arguments.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="engine"></param>
    /// <param name="argumentValue"></param>
    /// <param name="defaultArgumentValue"></param>
    /// <returns></returns>
    public static T GetTypedArgumentValue<T>(ScriptEngine engine, object argumentValue, T defaultArgumentValue)
    {
      if (argumentValue == Undefined.Value)
        return defaultArgumentValue;

      return Object.Equals(argumentValue, default(T))
        ? default(T)
        : TypeConverter.ConvertTo<T>(engine, argumentValue);
    }

    /// <summary>
    /// Helper method that supports duck typing of Jurassic Object Instances with their .Net counterparts.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="engine"></param>
    /// <param name="instance"></param>
    /// <returns></returns>
    public static T Coerce<T>(ScriptEngine engine, object instance)
      where T : ObjectInstance
    {
      if (instance == null || instance == Null.Value || instance == Undefined.Value)
        return null;

      if (instance is T)
        return instance as T;

      var objectInstance = instance as ObjectInstance;
      var serializedObject = objectInstance != null
                               ? JSONObject.Stringify(engine, objectInstance, null, null)
                               : JsonConvert.SerializeObject(instance);

      var type = typeof (T);

      //If the target type contains a constructor that accepts an ObjectInstance as the parameter, create a new instance of the target type and 
      //supply the instance prototype. Otherwise, create an instance using the default constructor.
      var objectInstanceConstructor = type.GetConstructor(new [] {typeof (ObjectInstance) });
      if (objectInstanceConstructor != null)
      {
        var result = (T) Activator.CreateInstance(typeof (T), engine.Object.InstancePrototype);
        JsonConvert.PopulateObject(serializedObject, result);

        return result;
      }
      else
      {
        var result = (T)Activator.CreateInstance(typeof(T));
        JsonConvert.PopulateObject(serializedObject, result);

        return result;
      }
    }

    public static string JavaScriptStringEncode(string value)
    {
      return JavaScriptStringEncode(value, false);
    }

    public static string JavaScriptStringEncode(string value, bool addDoubleQuotes)
    {
      if (String.IsNullOrEmpty(value))
        return addDoubleQuotes ? "\"\"" : String.Empty;

      int len = value.Length;
      bool needEncode = false;
      char c;
      for (int i = 0; i < len; i++)
      {
        c = value[i];

        if (c >= 0 && c <= 31 || c == 34 || c == 39 || c == 60 || c == 62 || c == 92)
        {
          needEncode = true;
          break;
        }
      }

      if (!needEncode)
        return addDoubleQuotes ? "\"" + value + "\"" : value;

      var sb = new StringBuilder();
      if (addDoubleQuotes)
        sb.Append('"');

      for (int i = 0; i < len; i++)
      {
        c = value[i];
        if (c >= 0 && c <= 7 || c == 11 || c >= 14 && c <= 31 || c == 39 || c == 60 || c == 62)
          sb.AppendFormat("\\u{0:x4}", (int)c);
        else switch ((int)c)
          {
            case 8:
              sb.Append("\\b");
              break;

            case 9:
              sb.Append("\\t");
              break;

            case 10:
              sb.Append("\\n");
              break;

            case 12:
              sb.Append("\\f");
              break;

            case 13:
              sb.Append("\\r");
              break;

            case 34:
              sb.Append("\\\"");
              break;

            case 92:
              sb.Append("\\\\");
              break;

            default:
              sb.Append(c);
              break;
          }
      }

      if (addDoubleQuotes)
        sb.Append('"');

      return sb.ToString();
    }
  }
}
