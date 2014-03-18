namespace Barista.Library
{
  using Barista.Jurassic;
  using Barista.Jurassic.Library;
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Text;
  using System.Xml;

  [Serializable]
  public class BundlerInstance : ObjectInstance
  {
    private readonly Dictionary<string, Tuple<DateTime, string>> m_fileModifiedDates = new Dictionary<string, Tuple<DateTime, string>>(); 

    public BundlerInstance(ScriptEngine engine)
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

    [JSFunction(Name = "bundle")]
    [JSDoc("Returns a bundle as a Base64EncodedByteArray based on the specified bundle definition.")]
    public Base64EncodedByteArrayInstance Bundle(string bundleDefinitionXml, string fileName)
    {
      if (String.IsNullOrEmpty(fileName))
        fileName = "bundle.txt";

      var doc = new XmlDocument();
      doc.LoadXml(bundleDefinitionXml);
      var result = GenerateBundleFromBundleDefinition(fileName, doc);

      var bytes = new Base64EncodedByteArrayInstance(this.Engine.Object.InstancePrototype, Encoding.UTF8.GetBytes(result))
      {
        FileName = fileName,
        MimeType = StringHelper.GetMimeTypeFromFileName(fileName)
      };
      return bytes;
    }

    private string GenerateBundleFromBundleDefinition(string filePath, XmlDocument doc)
    {
      var bundleNode = doc.SelectSingleNode("//bundle");

      if (bundleNode == null || bundleNode.Attributes == null)
        return String.Empty;

      XmlNode outputAttr = bundleNode.Attributes["output"];

      if (outputAttr != null && (outputAttr.InnerText.Contains("/") || outputAttr.InnerText.Contains("\\")))
        throw new JavaScriptException(this.Engine, "Error", "The 'output' attribute is for file names only - not paths");

      var files = new Dictionary<string, string>();
      var extension = Path.GetExtension(filePath);
      if (String.IsNullOrEmpty(extension))
        extension = "txt";

      var nodes = doc.SelectNodes("//file");

      if (nodes == null)
        return "";

      foreach (XmlNode node in nodes)
      {
        if (!files.ContainsKey(node.InnerText))
          files.Add(node.InnerText, node.InnerText);
      }

      var sb = new StringBuilder();

      foreach (var file in files.Keys)
      {
        if (extension.Equals(".js", StringComparison.OrdinalIgnoreCase))
        {
          sb.AppendLine("///#source 1 1 " + files[file]);
        }

        Tuple<DateTime, string> contents;
        if (m_fileModifiedDates.ContainsKey(file))
        {
          var fileContents = m_fileModifiedDates[file];
          var lastModified = this.GetLastModifiedDate(file);
          if (lastModified > fileContents.Item1)
          {
            var text = this.ReadAllText(file);
            contents = new Tuple<DateTime, string>(lastModified, text);
            m_fileModifiedDates[file] = contents;
          }
          else
          {
            contents = fileContents;
          }
        }
        else
        {
          var lastModified = this.GetLastModifiedDate(file);
          var text = this.ReadAllText(file);
          contents = new Tuple<DateTime, string>(lastModified, text);
          m_fileModifiedDates.Add(file, contents);
        }

        sb.AppendLine(contents.Item2);
      }

      return sb.ToString();
    }
  }
}
