namespace Barista.DocumentStore
{
  using Jurassic;
  using Jurassic.Library;
  using Barista.Newtonsoft.Json;

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
      var jsonA = JsonConvert.SerializeObject(a, Formatting.None, settings);
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

      var parsedJson = JsonConvert.DeserializeObject(stringResult);
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
      var originalJson = JsonConvert.SerializeObject(original, settings);
      var sourceJson = JsonConvert.SerializeObject(source, settings);
      var targetJson = JsonConvert.SerializeObject(target, settings);

      var diffJson = JsonHelper.Diff(originalJson, sourceJson, settings);
      var mergeJson = JsonHelper.Merge(targetJson, diffJson, settings);

      return JsonConvert.DeserializeObject<T>(mergeJson, settings);
    }

    /// <summary>
    /// Given three objects, returns the target object with the changes applied.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="original"></param>
    /// <param name="changes"></param>
    /// <param name="target"></param>
    /// <param name="settings"></param>
    /// <returns></returns>
    public static T MergeChanges<T>(T original, T changes, T target, JsonSerializerSettings settings)
    {
      if (original.Equals(default(T)) && changes.Equals(default(T)))
        return target;

      if (original.Equals(default(T)))
        original = changes;
      else if (changes.Equals(default(T)))
        changes = original;

      if (target.Equals(default(T)))
        target = changes;

      var jsonA = JsonConvert.SerializeObject(original, settings);
      var jsonB = JsonConvert.SerializeObject(changes, settings);
      var jsonC = JsonConvert.SerializeObject(target, settings);

      var engine = JsonHelper.Engine;
      engine.Execute("var a = " + jsonA + ";");
      engine.Execute("var b = " + jsonB + ";");
      engine.Execute("var c = " + jsonC + ";");
      engine.Execute("var res = jsonDataHandler.mergeChanges(a, b, c);");
      var result = engine.Evaluate("res;");
      var stringResult = JSONObject.Stringify(engine, result, null, null);

      return JsonConvert.DeserializeObject<T>(stringResult, settings);
    }
  }
}
