namespace Barista.SharePoint.Library
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Net;
  using System.Reflection;
  using System.ServiceModel.Web;
  using System.Web;
  using Jurassic;
  using Jurassic.Library;
  using System.Xml.Linq;
  using Barista.Extensions;
  using Newtonsoft.Json;
  using Barista.SharePoint.Library;
  using Barista.Library;

  public static class Help
  {
    public static object GenerateHelpJsonForObject(ScriptEngine engine, object obj)
    {
      object result = "Sorry, Dave. I didn't understand what you wanted.";

      if (obj == null || obj == Null.Value || obj == Undefined.Value)
      {
        result = JSONObject.Parse(engine, 
@"{
""console"": ""Console object that allows various logging/trace functions."",
""ad"": ""Active Directory namespace. Contains functions to interact with AD."",
""doc"": ""Document namespace. Contains functions to generate/parse documents."",
""log"": ""Log namespace. Contains fuctions to retrieve information from the ULS log."",
""web"": ""Web namespace. Contains functions to gather information from the current web context."",
""util"": ""Utility namespace. Contains utility, helper, functions."",
""sp"": ""SharePoint namespace. Contains functions to interact with SharePoint."",
""k2"": ""K2 namespace. Contains functions to interact with K2 services.""
}", null);
      }
      else if (obj is string)
      {
        var name = obj as string;
        switch(name)
        {
          case "console":
            result = "See http://getfirebug.com/logging";
            break;
          case "ad":
            result = GetObjectInfo(engine, typeof(ActiveDirectoryInstance));
            break;
          case "doc":
            result = GetObjectInfo(engine, typeof(DocumentInstance));
            break;
          case "log":
            result = GetObjectInfo(engine, typeof(LogInstance));
            break;
          case "web":
            result = GetObjectInfo(engine, typeof(WebInstance));
            break;
          case "util":
            result = GetObjectInfo(engine, typeof(UtilInstance));
            break;
          case "sp":
            result = GetObjectInfo(engine, typeof(SPInstance));
            break;
          case "k2":
            result = GetObjectInfo(engine, typeof(K2Instance));
            break;
        }
      }
      else if (obj is ObjectInstance)
      {
        var oi = obj as ObjectInstance;
        result = GetObjectInfo(engine, oi.GetType());
      }

      return result;
    }

    private static ObjectInstance GetObjectInfo(ScriptEngine engine, Type type)
    {
      var result = engine.Object.Construct();

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
                           .Select(mi => new {
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

      var resultFunctions = engine.Object.Construct();

      var functions = type.GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public)
                     .Select(mi => new {
                       MemberInfo = mi,
                       FunctionInfo = mi.GetCustomAttributes(typeof(JSFunctionAttribute), true).OfType<JSFunctionAttribute>().FirstOrDefault()
                     })
                     .Where(pi => pi.FunctionInfo != null);

      foreach (var function in functions)
      {
        var doc = GetMemberDocumentationObject(engine, function.MemberInfo);

        resultFunctions.SetPropertyValue(function.FunctionInfo.Name, doc, false);
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
        foreach (var parameter in methodInfo.GetParameters().OrderByDescending(p => p.Position))
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
        var propertyInfo = member as PropertyInfo;
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
  }
}
