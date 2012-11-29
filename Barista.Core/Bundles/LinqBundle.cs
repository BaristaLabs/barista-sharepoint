﻿namespace Barista.Bundles
{
  using Jurassic;
  using System;

  [Serializable]
  public class LinqBundle : IBundle
  {
    private static readonly StringScriptSource LinqScriptSource = new StringScriptSource(Barista.Properties.Resources.linq);

    public string BundleName
    {
      get { return "Linq"; }
    }

    public string BundleDescription
    {
      get { return "Linq Bundle. Adds objects to allow javascript arrays to be queried via linq-like syntax."; } 
    }

    public object InstallBundle(Jurassic.ScriptEngine engine)
    {
      engine.Execute(LinqScriptSource);
      return Null.Value;
    }
  }
}
