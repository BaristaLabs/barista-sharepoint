namespace Barista.OrcaDB
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using Jurassic;
  using Jurassic.Library;
  using Newtonsoft.Json;

  public static class JsonHelper
  {
    private static ScriptEngine Engine
    {
      get
      {
        ScriptEngine engine = new ScriptEngine();
        engine.Execute(Properties.Resources.jsonDataHandler);
        return engine;
      }
    }

    /// <summary>
    /// Given two json strings perform a diff on the objects and return the result as a formatted json string.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static string Diff(string a, string b, JsonSerializerSettings settings)
    {
      if (a.ToLowerInvariant() == "null" && b.ToLowerInvariant() == "null")
        return "null";
      else if (a.ToLowerInvariant() == "null")
        return b;
      else if (b.ToLowerInvariant() == "null")
        return a;

      var engine = JsonHelper.Engine;
      engine.Execute("var a = " + a + ";");
      engine.Execute("var b = " + b + ";");
      var result = engine.Evaluate("jsonDataHandler.diff(a, b);");
      var stringResult = JSONObject.Stringify(engine, result, null, null);

      object parsedJson = JsonConvert.DeserializeObject(stringResult);
      return JsonConvert.SerializeObject(parsedJson, Formatting.Indented, settings);
    }

    /// <summary>
    /// Given two json strings, merge a with b and return the result as a formatted json string.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static string Merge(string a, string b, JsonSerializerSettings settings)
    {
      var engine = JsonHelper.Engine;
      engine.Execute("var a = " + a + ";");
      engine.Execute("var b = " + b + ";");
      engine.Execute("jsonDataHandler.merge(a, b);");
      var result = engine.Evaluate("a;");
      var stringResult = JSONObject.Stringify(engine, result, null, null);

      object parsedJson = JsonConvert.DeserializeObject(stringResult);
      return JsonConvert.SerializeObject(parsedJson, Formatting.Indented, settings);
    }

    /// <summary>
    /// Given three objects, returns the delta between the source and the target given the original.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="original"></param>
    /// <param name="source"></param>
    /// <param name="target"></param>
    /// <param name="settings"></param>
    /// <returns></returns>
    public static T DiffAndMerge<T>(T original, T source, T target, JsonSerializerSettings settings)
    {
      string originalJson = JsonConvert.SerializeObject(original, settings);
      string sourceJson = JsonConvert.SerializeObject(source, settings);
      string targetJson = JsonConvert.SerializeObject(target, settings);

      var diffJson = JsonHelper.Diff(originalJson, sourceJson, settings);
      var mergeJson = JsonHelper.Merge(targetJson, diffJson, settings);

      return JsonConvert.DeserializeObject<T>(mergeJson, settings);
    }
  }
}
