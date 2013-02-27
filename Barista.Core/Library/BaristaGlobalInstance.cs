namespace Barista.Library
{
  using Jurassic;
  using Jurassic.Library;
  using System;
  using System.Linq;
  using System.Reflection;

  [Serializable]
  public class BaristaGlobal : ObjectInstance
  {
    public BaristaGlobal(ObjectInstance prototype)
      : base(prototype)
    {
      this.Common = new Common(prototype);

      this.PopulateFields();
      this.PopulateFunctions();
    }

    [JSProperty(Name = "common")]
    public Common Common
    {
      get;
      set;
    }

    [JSFunction(Name = "help")]
    public object Help(object obj)
    {
      object result = "Sorry, Dave. I didn't understand what you wanted.";

      if (obj == null || obj == Null.Value || obj == Undefined.Value)
      {
        if (this.Common != null)
        {
          return Common.ListBundles();
        }
      }
      else if (obj is ObjectInstance)
      {
        var oi = obj as ObjectInstance;
        result = GetObjectInfo(this.Engine, oi);
      }

      return result;
    }

    #region Members

    private static ObjectInstance GetObjectInfo(ScriptEngine engine, ObjectInstance obj)
    {
      var result = engine.Object.Construct();
      var type = obj.GetType();

      var jsDocAttributes = type.GetCustomAttributes(typeof(JSDocAttribute), false).OfType<JSDocAttribute>();
      foreach (var attribute in jsDocAttributes)
      {
        string tag = "Summary";
        if (String.IsNullOrEmpty(attribute.Tag) == false)
          tag = attribute.Tag;

        result.SetPropertyValue(tag, attribute.Text, false);
      }

      var resultProperties = engine.Object.Construct();

      var properties = type.GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public)
                           .Select(mi => new
                           {
                             MemberInfo = mi,
                             PropertyInfo = mi.GetCustomAttributes(typeof(JSPropertyAttribute), true).OfType<JSPropertyAttribute>().FirstOrDefault()
                           })
                           .Where(pi => pi.PropertyInfo != null)
                           .OrderBy(pi => pi.PropertyInfo.Name);

      foreach (var property in properties)
      {
        var doc = GetMemberDocumentationObject(engine, property.MemberInfo);

        resultProperties.SetPropertyValue(property.PropertyInfo.Name, doc, false);
      }

      foreach (var property in obj.Properties)
      {
        if ((property.Value is FunctionInstance) == false && resultProperties.HasProperty(property.Name) == false)
          resultProperties.SetPropertyValue(property.Name, "", false);
      }

      var resultFunctions = engine.Object.Construct();

      var functions = type.GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public)
                     .Select(mi => new
                     {
                       MemberInfo = mi,
                       FunctionInfo = mi.GetCustomAttributes(typeof(JSFunctionAttribute), true).OfType<JSFunctionAttribute>().FirstOrDefault()
                     })
                     .Where(pi => pi.FunctionInfo != null);

      foreach (var function in functions)
      {
        var doc = GetMemberDocumentationObject(engine, function.MemberInfo);

        resultFunctions.SetPropertyValue(function.FunctionInfo.Name, doc, false);
      }

      foreach (var property in obj.Properties)
      {
        if (property.Value is FunctionInstance && resultFunctions.HasProperty(property.Name) == false)
          resultFunctions.SetPropertyValue(property.Name, "", false);
      }

      result.SetPropertyValue("properties", resultProperties, false);
      result.SetPropertyValue("functions", resultFunctions, false);

      return result;
    }

    private static ObjectInstance GetMemberDocumentationObject(ScriptEngine engine, MemberInfo member)
    {
      var jsDocAttributes = member.GetCustomAttributes(typeof(JSDocAttribute), false).OfType<JSDocAttribute>();

      var doc = engine.Object.Construct();
      foreach (var attribute in jsDocAttributes)
      {
        string tag = "Summary";
        if (String.IsNullOrEmpty(attribute.Tag) == false)
          tag = attribute.Tag;

        doc.SetPropertyValue(tag, attribute.Text, false);
      }

      if (member is MethodInfo)
      {
        var methodInfo = member as MethodInfo;
        var methodParams = engine.Array.Construct();
        foreach (var parameter in methodInfo.GetParameters().OrderBy(p => p.Position))
        {
          var parameterDoc = engine.Object.Construct();
          parameterDoc.SetPropertyValue("name", parameter.Name, false);
          parameterDoc.SetPropertyValue("type", parameter.ParameterType.ToString().Replace("System.", ""), false);

          ArrayInstance.Push(methodParams, parameterDoc);
        }

        doc.SetPropertyValue("params", methodParams, false);
      }
      else if (member is PropertyInfo)
      {
        var propertyInfo = member as PropertyInfo;
        doc.SetPropertyValue("type", GetTypeString(propertyInfo.PropertyType), false);
        doc.SetPropertyValue("hasGetter", propertyInfo.CanRead, false);
        doc.SetPropertyValue("hasSetter", propertyInfo.CanWrite, false);
      }

      else if (member is FieldInfo)
      {
        var propertyInfo = member as FieldInfo;
        //TODO: Implement this.
      }

      return doc;
    }

    public static string GetTypeString(Type type)
    {
      string result = type.ToString();
      switch (result)
      {
        case "Jurassic.Library.DateInstance":
          result = "Date";
          break;
        case "Jurassic.Library.ArrayInstance":
          result = "Array";
          break;
        case "Jurassic.Library.ObjectInstance":
          result = "Object";
          break;
        default:
          result = result.Replace("System.", "");
          result = result.Replace("Barista.Library.", "");
          break;
      }

      return result;
    }
    #endregion
  }
}
