namespace OFS.OrcaDB.Core
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using Jurassic;
  using Jurassic.Library;

  public static class JurassicHelper
  {
    public static double ToJSDate(DateTime dateTime)
    {
      return dateTime.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
    }

    public static T Coerce<T>(ScriptEngine engine, string typeName, object instance)
      where T : ObjectInstance
    {
      if (instance == null)
        return null;

      if (instance == Null.Value || instance == Undefined.Value)
        return null;

      if (instance is T)
        return instance as T;

      var objectInstance = instance as ObjectInstance;
      if (objectInstance == null)
        throw new InvalidOperationException("Cannot coerce type: " + instance.GetType());

      var resultObject = engine.Evaluate("new " + typeName + "();");
      var result = resultObject as T;

      if (result == null)
        return null;

      var targetProperties = typeof(T).GetProperties()
                                      .Select(p =>
                                      {
                                        return new
                                        {
                                          Property = p,
                                          Attribute = p.GetCustomAttributes(typeof(JSPropertyAttribute), false).OfType<JSPropertyAttribute>().FirstOrDefault(),
                                        };
                                      })
                                      .Where(p => p.Attribute != null);

      foreach (var property in objectInstance.Properties)
      {
        var targetProperty = targetProperties.Where(p => p.Attribute.Name == property.Name && p.Property.CanWrite == true).FirstOrDefault();

        if (targetProperty != null)
        {
          //TODO: Make this recursive.

          if (property.Value != Null.Value && property.Value != Undefined.Value)
            targetProperty.Property.SetValue(resultObject, property.Value, null);
        }
      }

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
