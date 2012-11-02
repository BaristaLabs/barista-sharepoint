namespace Barista
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using Jurassic;
  using Jurassic.Library;
  using Newtonsoft.Json;
  using System.Collections;

  public static class JurassicHelper
  {
    public static double ToJSDate(DateTime dateTime)
    {
      return dateTime.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
    }

    public static DateInstance ToDateInstance(ScriptEngine engine, DateTime dateTime)
    {
      var dateNumeric = ToJSDate(dateTime);
      return engine.Date.Construct(dateNumeric);
    }

    public static T Coerce<T>(ScriptEngine engine, object instance)
      where T : ObjectInstance
    {
      if (instance == null)
        return null;

      if (instance == Null.Value || instance == Undefined.Value)
        return null;

      if (instance is T)
        return instance as T;

      string serializedObject = String.Empty;

      var objectInstance = instance as ObjectInstance;
      if (objectInstance != null)
        serializedObject = JSONObject.Stringify(engine, objectInstance);
      else
        serializedObject = JsonConvert.SerializeObject(instance);

      T result = (T)Activator.CreateInstance(typeof(T), engine.Object.InstancePrototype);
      JsonConvert.PopulateObject(serializedObject, result);

      return result;
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
