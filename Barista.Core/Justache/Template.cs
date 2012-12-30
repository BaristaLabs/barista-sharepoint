﻿namespace Barista.Justache
{
  using Jurassic.Library;
  using System.IO;

  public class Template : Section
  {
    public Template()
      : this("#template") // I'm not happy about this fake name.
    {
    }

    public Template(string name)
      : base(name)
    {
    }

    /// <summary>
    /// Loads the template.
    /// </summary>
    /// <param name="reader">The object to read the template from.</param>
    /// <remarks>
    /// The <paramref name="reader" /> is read until it ends, but is not
    /// closed or disposed.
    /// </remarks>
    /// <exception cref="JustacheException">
    /// Thrown when the template contains a syntax error.
    /// </exception>
    public void Load(TextReader reader)
    {
      string template = reader.ReadToEnd();

      var scanner = new TemplateParser();
      var parser = new Parser();

      parser.Parse(this, scanner.Parse(template));
    }

    /// <summary>
    /// Renders the template.
    /// </summary>
    /// <param name="data">The data to use to render the template.</param>
    /// <param name="writer">The object to write the output to.</param>
    /// <param name="templateLocator">The delegate to use to locate templates for inclusion.</param>
    /// <remarks>
    /// The <paramref name="writer" /> is flushed, but not closed or disposed.
    /// </remarks>
    public void Render(object data, TextWriter writer, TemplateLocator templateLocator)
    {
      var context = new RenderContext(this, data, writer, templateLocator);

      Render(context);

      writer.Flush();
    }
  }
}