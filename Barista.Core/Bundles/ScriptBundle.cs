namespace Barista.Bundles
{
  using System.Collections.Generic;
  using Barista.Extensions;
  using Barista.Jurassic.Library;

  public class ScriptBundle : IBundle
  {
    private readonly List<string> m_dependencies = new List<string>();
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
      var resultArray = engine.Array.Construct();

      //Require all dependencies..
      foreach (var dependency in m_dependencies)
      {
        var result = engine.Evaluate("require('" + dependency + "');");
        ArrayInstance.Push(resultArray, result);
      }

      //If it's a function dependency, execute that.
      if (this.ScriptFunction != null)
      {
        return this.ScriptFunction.Call(engine.Global, new object[0]);
      }

      //Use the implemented "include" function to include the scripts.
      //this promotes loose coupling between the script bundle and the include.
      foreach (var scriptReference in m_scriptReferences)
      {
        var result = engine.Evaluate("include('" + scriptReference + "');");
        ArrayInstance.Push(resultArray, result);
      }

      return resultArray.Length == 1
        ? resultArray[0]
        : resultArray;
    }

    public void AddDependency(string dependency)
    {
      m_dependencies.Add(dependency);
    }

    public void AddScriptReference(string reference)
    {
      m_scriptReferences.Add(reference);
    }
  }
}
