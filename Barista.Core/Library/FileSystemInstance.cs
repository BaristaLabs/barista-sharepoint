namespace Barista.Library
{
  using System.IO;
  using System.Web;
  using Jurassic;
  using Jurassic.Library;
  using Barista.Extensions;
  using System.Linq;

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

    [JSFunction(Name = "loadAsByteArray")]
    public Base64EncodedByteArrayInstance LoadAsByteArray(string path)
    {
      var fileBytes = File.ReadAllBytes(HttpContext.Current.Server.MapPath(path));
      return new Base64EncodedByteArrayInstance(this.Engine.Object.InstancePrototype, fileBytes);
    }

    [JSFunction(Name = "loadAsObject")]
    public object LoadAsObject(string path)
    {
      var text = File.ReadAllText(HttpContext.Current.Server.MapPath(path));
      return JSONObject.Parse(this.Engine, text, null);
    }

    [JSFunction(Name = "getDirectories")]
    public ArrayInstance GetDirectories(string path, object searchPattern, object searchOption)
    {
      if (path.IsValidPath() == false)
        throw new JavaScriptException(this.Engine, "Error", "The specified path contained invalid characters.");

      DirectoryInfo[] directories;
      var di = new DirectoryInfo(HttpContext.Current.Server.MapPath(path));
      if (searchPattern != null && searchPattern != Null.Value && searchPattern != Undefined.Value)
      {
        directories = di.GetDirectories();
      }
      else
      {
        var eSearchOption = SearchOption.TopDirectoryOnly;
        if (searchOption != null && searchOption != Null.Value && searchPattern != Undefined.Value)
        {
          SearchOption specifiedSearchOption;
          if (TypeConverter.ToString(searchPattern)
                           .TryParseEnum(true, SearchOption.TopDirectoryOnly, out specifiedSearchOption))
            eSearchOption = specifiedSearchOption;
        }

        var strSearchPattern = TypeConverter.ToString(searchPattern);
        directories = di.GetDirectories(strSearchPattern, eSearchOption);
      }

// ReSharper disable CoVariantArrayConversion
      return Engine.Array.Construct(directories.Select(d => d.Name).ToArray());
// ReSharper restore CoVariantArrayConversion
    }

    [JSFunction(Name = "getFiles")]
    public ArrayInstance GetFiles(string path, object searchPattern, object searchOption)
    {
      if (path.IsValidPath() == false)
        throw new JavaScriptException(this.Engine, "Error", "The specified path contained invalid characters.");

      FileInfo[] files;
      var di = new DirectoryInfo(HttpContext.Current.Server.MapPath(path));
      if (searchPattern != null && searchPattern != Null.Value && searchPattern != Undefined.Value)
      {
        files = di.GetFiles();
      }
      else
      {
        var eSearchOption = SearchOption.TopDirectoryOnly;
        if (searchOption != null && searchOption != Null.Value && searchPattern != Undefined.Value)
        {
          SearchOption specifiedSearchOption;
          if (TypeConverter.ToString(searchPattern)
                           .TryParseEnum(true, SearchOption.TopDirectoryOnly, out specifiedSearchOption))
            eSearchOption = specifiedSearchOption;
        }

        var strSearchPattern = TypeConverter.ToString(searchPattern);
        files = di.GetFiles(strSearchPattern, eSearchOption);
      }

      // ReSharper disable CoVariantArrayConversion
      return Engine.Array.Construct(files.Select(d => d.Name).ToArray());
      // ReSharper restore CoVariantArrayConversion
    }

    #endregion
  }
}
