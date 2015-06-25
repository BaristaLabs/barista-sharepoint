using System.IO;
using Barista.Justache;

namespace Barista.Library
{
  using Jurassic;
  using Jurassic.Library;
  using System;

  [Serializable]
  public class MustacheInstance : ObjectInstance
  {
    public MustacheInstance(ObjectInstance prototype)
      : base(prototype)
    {
      this.PopulateFields();
      this.PopulateFunctions();
    }

    [JSFunction(Name = "render")]
    public string Render(string template, object data)
    {
      try
      {
        return Render(template, data, null);
      }
      catch (JustacheException ex)
      {
        //Wrap any JustacheException in a JavaScript Exception.
        throw new JavaScriptException(this.Engine, "Error", ex.Message);
      }
      
    }

    public static string Render(string template, object data, TemplateLocator templateLocator)
    {
      var reader = new StringReader(template);
      var writer = new StringWriter();
      Template(reader, data, writer, templateLocator);
      return writer.GetStringBuilder().ToString();
    }

    public static void Template(TextReader reader, object data, TextWriter writer)
    {
      Template(reader, data, writer, null);
    }

    public static void Template(TextReader reader, object data, TextWriter writer, TemplateLocator templateLocator)
    {
      var template = new Template();
      template.Load(reader);
      template.Render(data, writer, templateLocator);
    }
  }
}
