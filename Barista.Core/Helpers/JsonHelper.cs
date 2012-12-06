namespace Barista.DocumentStore
{
  using Jurassic;
  using Jurassic.Library;
  using Newtonsoft.Json;

  public static class JsonHelper
  {
    private static readonly object SyncRoot = new object();
    private static volatile ScriptEngine s_engine;

    private static ScriptEngine Engine
    {
      get
      {
        if (s_engine == null)
        {
          lock (SyncRoot)
          {
            if (s_engine == null)
            {
              var engine = new ScriptEngine();
              engine.Execute(Properties.Resources.jsonDataHandler);
              s_engine = engine;
            }
          }
        }

        return s_engine;
      }
    }

    /// <summary>
    /// Given two json strings perform a diff on the objects and return the result as a formatted json string.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="settings"></param>
    /// <returns></returns>
    public static string Diff(string a, string b, JsonSerializerSettings settings)
    {
      if (a.ToLowerInvariant() == "null" && b.ToLowerInvariant() == "null")
        return "null";
      if (a.ToLowerInvariant() == "null")
        return b;
      if (b.ToLowerInvariant() == "null")
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
    /// Given two objects, perform a diff on the objects and return the result as a new object that contains the differences.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="settings"></param>
    /// <returns></returns>
    public static T Diff<T>(T a, T b, JsonSerializerSettings settings)
    {
      var jsonA = JsonConvert.SerializeObject(a, settings);
      var jsonB = JsonConvert.SerializeObject(b, settings);

      var engine = JsonHelper.Engine;
      engine.Execute("var a = " + jsonA + ";");
      engine.Execute("var b = " + jsonB + ";");
      var result = engine.Evaluate("jsonDataHandler.diff(a, b);");
      var stringResult = JSONObject.Stringify(engine, result, null, null);

      return JsonConvert.DeserializeObject<T>(stringResult, settings);
    }

    /// <summary>
    /// Given two json strings, merge a with b and return the result as a formatted json string.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="settings"></param>
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
