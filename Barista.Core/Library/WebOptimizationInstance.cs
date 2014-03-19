namespace Barista.Library
{
  using System.Collections.Concurrent;
  using System.Linq;
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Text;
  using System.Xml;
  using Barista.Yahoo.Yui.Compressor;

  [Serializable]
  public class WebOptimizationInstance : ObjectInstance
  {
    private static readonly ConcurrentDictionary<string, Tuple<DateTime, string>> FileModifiedDates = new ConcurrentDictionary<string, Tuple<DateTime, string>>(); 

    public WebOptimizationInstance(ScriptEngine engine)
      : base(engine)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    public Func<string, DateTime> GetLastModifiedDate
    {
      get;
      set;
    }

    public Func<string, string> ReadAllText
    {
      get;
      set;
    }

    [JSFunction(Name = "hasBundleChangedSince")]
    [JSDoc("Using a xml bundle definition, determines if the contents of the bundle have changed since the specfied date.")]
    public bool HasBundleChangedSince(string bundleDefinitionXml, object date)
    {
      if (date == Undefined.Value || date == Null.Value || date == null)
        throw new JavaScriptException(this.Engine, "Error", "A date must be supplied as the second argument.");

      DateTime dateToCompare;
      if (date is DateTime)
        dateToCompare = (DateTime) date;
      else if (date is DateInstance)
        dateToCompare = (date as DateInstance).Value;
      else
        dateToCompare = (new DateInstance(this.Engine.Object.InstancePrototype, TypeConverter.ToString(date))).Value;

      var doc = new XmlDocument();
      doc.LoadXml(bundleDefinitionXml);

      var files = ParseBundleDefinition(doc);

      if (files == null || files.Count == 0)
        return false;

      UpdateFileCache(files);

      foreach (var file in files.Keys)
      {
        if (!FileModifiedDates.ContainsKey(file))
          continue;

        var contents = FileModifiedDates[file];
        if (contents.Item1.ToLocalTime() > dateToCompare.ToLocalTime())
          return true;
      }

      return false;
    }

    [JSFunction(Name = "bundle")]
    [JSDoc("Using a xml bundle definition, combines the specified files and returns the bundle as an object.")]
    public object Bundle(string bundleDefinitionXml, string fileName, object update)
    {
      if (String.IsNullOrEmpty(fileName))
        fileName = "bundle.txt";

      var bUpdate = JurassicHelper.GetTypedArgumentValue(this.Engine, update, true);

      var doc = new XmlDocument();
      doc.LoadXml(bundleDefinitionXml);
      var bundleText = GenerateBundleFromBundleDefinition(fileName, doc, bUpdate);

      var bytes = new Base64EncodedByteArrayInstance(this.Engine.Object.InstancePrototype, Encoding.UTF8.GetBytes(bundleText))
      {
        FileName = fileName,
        MimeType = StringHelper.GetMimeTypeFromFileName(fileName),
      };

      var result = this.Engine.Object.Construct();
      result.SetPropertyValue("lastModified", JurassicHelper.ToDateInstance(this.Engine, FileModifiedDates.Values.Max(v => v.Item1)), false);
      result.SetPropertyValue("data", bytes, false);
      return result;
    }

    [JSFunction(Name = "minifyCss")]
    [JSDoc("Returns a minified representation of the css string passed as the first argument.")]
    public string MinifyCss(string css)
    {
      var cssCompressor = new CssCompressor();
      return cssCompressor.Compress(css);
    }

    [JSFunction(Name = "minifyJs")]
    [JSDoc("Returns a minified representation of the javascript string passed as the first argument.")]
    public string MinifyJs(string javascript)
    {
      var jsCompressor = new JavaScriptCompressor();
      return jsCompressor.Compress(javascript);
    }

    private IDictionary<string, string> ParseBundleDefinition(XmlDocument doc)
    {
      var result = new Dictionary<string, string>();

      var bundleNode = doc.SelectSingleNode("//bundle");

      if (bundleNode == null || bundleNode.Attributes == null)
        return result;

      XmlNode outputAttr = bundleNode.Attributes["output"];

      if (outputAttr != null && (outputAttr.InnerText.Contains("/") || outputAttr.InnerText.Contains("\\")))
        throw new JavaScriptException(this.Engine, "Error", "The 'output' attribute is for file names only - not paths");

      var nodes = doc.SelectNodes("//file");

      if (nodes == null)
        return result;

      foreach (XmlNode node in nodes)
      {
        if (!result.ContainsKey(node.InnerText))
          result.Add(node.InnerText, node.InnerText);
      }

      return result;
    }

    private void UpdateFileCache(IDictionary<string, string> files)
    {
      foreach (var file in files.Keys)
      {
        Tuple<DateTime, string> contents;
        if (FileModifiedDates.ContainsKey(file))
        {
          var fileContents = FileModifiedDates[file];
          var lastModified = this.GetLastModifiedDate(file);
          if (lastModified > fileContents.Item1)
          {
            var text = this.ReadAllText(file);
            contents = new Tuple<DateTime, string>(lastModified, text);
            FileModifiedDates[file] = contents;
          }
        }
        else
        {
          var lastModified = this.GetLastModifiedDate(file);
          var text = this.ReadAllText(file);
          contents = new Tuple<DateTime, string>(lastModified, text);
          FileModifiedDates.TryAdd(file, contents);
        }
      }
    }

    private string GenerateBundleFromBundleDefinition(string filePath, XmlDocument doc, bool update)
    {
      var files = ParseBundleDefinition(doc);

      if (files == null || files.Count == 0)
        return String.Empty;

      if (update)
        UpdateFileCache(files);

      var extension = Path.GetExtension(filePath);
      if (String.IsNullOrEmpty(extension))
        extension = "txt";

      var sb = new StringBuilder();

      foreach (var file in files.Keys)
      {
        if (FileModifiedDates.ContainsKey(file))
        {
          var contents = FileModifiedDates[file];

          if (extension.Equals(".js", StringComparison.OrdinalIgnoreCase))
            sb.AppendLine("///#source 1 1 " + files[file]);
          
          sb.AppendLine(contents.Item2);
        }
      }

      return sb.ToString();
    }
  }
}
