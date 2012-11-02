namespace Barista.Library
{
  using System.IO;
  using System.Web;
  using Jurassic;
  using Jurassic.Library;

  public class FileSystemInstance : ObjectInstance
  {
    public FileSystemInstance(ScriptEngine engine)
      : base(engine)
    {
      this.PopulateFunctions();
    }

    #region Functions

    [JSFunction(Name = "exists")]
    public bool Exists(string path)
    {
      return File.Exists(HttpContext.Current.Server.MapPath(path));
    }

    [JSFunction(Name = "load")]
    public string Load(string path)
    {
      return File.ReadAllText(HttpContext.Current.Server.MapPath(path));
    }

    #endregion
  }
}
