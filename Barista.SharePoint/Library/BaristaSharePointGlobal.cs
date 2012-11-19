namespace Barista.SharePoint.Library
{
  using Jurassic;
  using Jurassic.Library;
  using System;

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
  }
}
