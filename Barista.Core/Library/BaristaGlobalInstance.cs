namespace Barista.Library
{
  using System.Web;
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
      this.Environment = new EnvironmentInstance(prototype);

      this.PopulateFields();
      this.PopulateFunctions();
    }

    [JSProperty(Name = "common")]
    public Common Common
    {
      get;
      set;
    }

    [JSProperty(Name = "environment")]
    public EnvironmentInstance Environment
    {
      get;
      set;
    }

    [JSFunction(Name = "equals")]
    public bool JSEquals(object o1, object o2)
    {
      return TypeComparer.Equals(o1, o2);
    }

    /// <summary>
    /// Returns a JSON object that contains the shape of the specified object with any JSDoc attributes applied.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
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

    [JSFunction(Name = "include")]
    public void Include(string scriptPath)
    {
      //TODO: Um.. It's wierd accessing httpcontext here.. but BaristaContext.current will be null if sharepoint...
      var path = HttpContext.Current.Request.MapPath(scriptPath);
      var source = new FileScriptSource(path, System.Text.Encoding.Unicode);

      this.Engine.Execute(source);
    }

    /// <summary>
    /// Override of include intended to be used from .net
    /// </summary>
    /// <param name="engine"></param>
    /// <param name="scriptPath"></param>
    public void Include(ScriptEngine engine, string scriptPath)
    {
      if (this.Common.RegisteredBundles.ContainsKey("Web"))
      {
        var bundle = this.Common.RegisteredBundles["Web"];
        WebInstanceBase webInstance;

        if (this.Common.InstalledBundles.ContainsKey(bundle))
          webInstance = this.Common.InstalledBundles[bundle] as WebInstanceBase;
        else
          webInstance = bundle.InstallBundle(engine) as WebInstanceBase;

        if (webInstance != null)
        {
          //Hmm.. this works too well, it gets the file path.
          var req = new HttpRequest(null, webInstance.Request.Request.Url.ToString(), null);
          scriptPath = req.MapPath(scriptPath);
        }
      }

      var source = new FileScriptSource(scriptPath, System.Text.Encoding.Unicode);

      this.Engine.Execute(source);
    }

    [JSFunction(Name = "isArray")]
    public bool IsArray(object value)
    {
      return TypeUtilities.IsArray(value);
    }

    [JSFunction(Name = "isDate")]
    public bool IsDate(object value)
    {
      return TypeUtilities.IsDate(value);
    }

    [JSFunction(Name = "isDefined")]
    public bool IsDefined(object value)
    {
      return !TypeUtilities.IsUndefined(value);
    }

    [JSFunction(Name = "isFunction")]
    public bool IsFunction(object value)
    {
      return TypeUtilities.IsFunction(value);
    }

    [JSFunction(Name = "isNumber")]
    public bool IsNumber(object value)
    {
      return TypeUtilities.IsNumeric(value);
    }

    [JSFunction(Name = "isObject")]
    public bool IsObject(object value)
    {
      return TypeUtilities.IsObject(value);
    }

    [JSFunction(Name = "isString")]
    public bool IsString(object value)
    {
      return TypeUtilities.IsString(value);
    }

    [JSFunction(Name = "isUndefined")]
    public bool IsUndefined(object value)
    {
      return TypeUtilities.IsUndefined(value);
    }

    [JSFunction(Name = "lowercase")]
    public string Lowercase(object value)
    {
      return TypeConverter.ToString(value).ToLowerInvariant();
    }

    [JSFunction(Name = "uppercase")]
    public string Uppercase(object value)
    {
      return TypeConverter.ToString(value).ToUpperInvariant();
    }

    [JSFunction(Name = "version")]
    public ObjectInstance Version()
    {
      var assembly = Assembly.GetExecutingAssembly();
      var version = assembly.GetName().Version;
      var codeName = "rascally-rabbit"; //TODO: Pull this from a custom assembly attribute.

      var result = this.Engine.Object.Construct();
      result.SetPropertyValue("full", version + " " + codeName, true);
      result.SetPropertyValue("major", version.Major, true);
      result.SetPropertyValue("minor", version.Minor, true);
      result.SetPropertyValue("dot", version.Revision, true);
      result.SetPropertyValue("codeName", codeName, true);

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
        string tag = "summary";
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
        var tag = "summary";
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

          var propertyJSDocAttributes =
            parameter.GetCustomAttributes(typeof (JSDocAttribute), false).OfType<JSDocAttribute>();

          foreach (var attribute in propertyJSDocAttributes)
          {
            var tag = "param";
            if (String.IsNullOrEmpty(attribute.Tag) == false)
              tag = attribute.Tag;

            parameterDoc.SetPropertyValue(tag, attribute.Text, false);
          }

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
        var fieldInfo = member as FieldInfo;
        doc.SetPropertyValue("type", GetTypeString(fieldInfo.FieldType), false);
      }

      return doc;
    }

    public static string GetTypeString(Type type)
    {
      var result = type.ToString();
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
