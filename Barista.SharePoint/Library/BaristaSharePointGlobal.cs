namespace Barista.SharePoint.Library
{
  using Jurassic;
  using Jurassic.Library;
  using System;
  using System.Web;

  [Serializable]
  public class BaristaSharePointGlobal : ObjectInstance
  {
    public BaristaSharePointGlobal(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    [JSFunction(Name = "include")]
    public void Include(string scriptUrl)
    {
      var source = new SPFileScriptSource(this.Engine, scriptUrl);

      this.Engine.Execute(source);
    }

    [JSFunction(Name = "isCurrentInstanceSaved")]
    public static bool IsCurrentInstanceStored()
    {
      bool result = false;

      if (BaristaContext.Current.Request.InstanceMode == BaristaInstanceMode.PerCall)
        return false;

      var cached = HttpRuntime.Cache.Get("Barista_ScriptEngineInstance_" + BaristaContext.Current.Request.InstanceKey) as ScriptEngine;
      if (cached != null)
        result = true;

      return result;
    }

    [JSFunction(Name = "clearCurrentInstance")]
    public static bool ClearCurrentInstance()
    {
      if (BaristaContext.Current.Request.InstanceMode == BaristaInstanceMode.PerCall)
        return false;

      bool result = false;
      var cached = HttpRuntime.Cache.Get("Barista_ScriptEngineInstance_" + BaristaContext.Current.Request.InstanceKey) as ScriptEngine;
      if (cached != null)
      {
        HttpRuntime.Cache.Remove("Barista_ScriptEngineInstance_" + BaristaContext.Current.Request.InstanceKey);
        result = true;
      }

      return result;
    }
  }
}
