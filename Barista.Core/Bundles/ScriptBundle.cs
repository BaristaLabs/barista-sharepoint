namespace Barista.Bundles
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using Barista.Extensions;
  using Barista.Jurassic;
  using Barista.Jurassic.Compiler;
  using Barista.Jurassic.Library;
  using Barista.Library;

  public class ScriptBundle : IBundle
  {
    private readonly Dictionary<string, string> m_dependencies = new Dictionary<string, string>();
    private readonly List<string> m_scriptReferences = new List<string>();
    
    private string m_bundleDescription;

    public FunctionInstance ScriptFunction
    {
      get;
      set;
    }

    public bool IsSystemBundle
    {
      get { return false; }
    }

    public string BundleName
    {
      get;
      set;
    }

    public string BundleDescription
    {
      get
      {
        if (m_bundleDescription.IsNullOrWhiteSpace() && this.ScriptFunction != null)
          return "User-Defined Function.";

        return m_bundleDescription.IsNullOrWhiteSpace()
          ? m_scriptReferences.Join(", ")
          : m_bundleDescription;
      }
      set { m_bundleDescription = value; }
    }

    public object InstallBundle(Jurassic.ScriptEngine engine)
    {
      var baristaInstance = engine.GetGlobalValue("barista") as BaristaGlobal;
      if (baristaInstance == null)
        throw new InvalidOperationException("Barista Global bundle could not be obtained.");

      var dependencyResultObj = engine.Object.Construct();

      //Require all dependencies..
      foreach (var dependency in m_dependencies)
      {
        var result = baristaInstance.Common.Require(dependency.Key, null);
        dependencyResultObj.SetPropertyValue(dependency.Value, result, false);
      }

      //If it's a function dependency, execute that.
      if (this.ScriptFunction != null)
      {
        var args = new List<object>();
        if (this.ScriptFunction is UserDefinedFunction)
        {
          var udf = this.ScriptFunction as UserDefinedFunction;

          args.AddRange(udf.ArgumentNames.Select(argumentName => dependencyResultObj.HasProperty(argumentName) ? dependencyResultObj.GetPropertyValue(argumentName) : Null.Value));
        }

        return this.ScriptFunction.Call(engine.Global, args.ToArray());
      }

      //Use the implemented "include" function to include the scripts.
      //this promotes loose coupling between the script bundle and the include.
      var variableNames = dependencyResultObj.Properties.Select(pn => pn.Name).ToArray();
      var resultArray = engine.Array.Construct();

      foreach (var scriptReference in m_scriptReferences)
      {
        //Create a new scope, and eval the script within the new scope.
        var scope = DeclarativeScope.CreateRuntimeScope(engine.CreateGlobalScope(), variableNames);
        foreach (var property in dependencyResultObj.Properties)
        {
          if (property.Value == Undefined.Value || property.Value == Null.Value || property.Value == null)
            continue;

          scope.SetValue(property.Name, property.Value);
        }

        var result = baristaInstance.Include(scriptReference, scope, engine.Global, engine.ForceStrictMode);

        ArrayInstance.Push(resultArray, result);
      }

      return resultArray.Length == 1
        ? resultArray[0]
        : resultArray;
    }

    public void AddDependency(string dependencyName, string diName)
    {
      m_dependencies.Add(dependencyName, diName);
    }

    public void AddScriptReference(string reference)
    {
      m_scriptReferences.Add(reference);
    }
  }
}
