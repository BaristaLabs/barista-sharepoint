namespace Barista.Justache
{
  using Jurassic;
  using Jurassic.Library;
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;

  public delegate Template TemplateLocator(string name);

  public delegate Object Lambda(string text);

  public class RenderContext
  {
    private const int IncludeLimit = 1024;
    private readonly Stack<Section> m_sectionStack = new Stack<Section>();
    private readonly Stack<object> m_dataStack = new Stack<object>();
    private readonly TextWriter m_writer;
    private readonly TemplateLocator m_templateLocator;
    private int m_includeLevel;

    public RenderContext(Section section, object data, TextWriter writer, TemplateLocator templateLocator)
    {
      m_sectionStack.Push(section);
      m_dataStack.Push(data);
      m_writer = writer;
      m_templateLocator = templateLocator;
      m_includeLevel = 0;
    }

    public object GetValue(string path)
    {
      if (path == ".")
      {
        return m_dataStack.Peek();
      }

      return m_dataStack
        .Where(data => data != null)
        .Select(data => GetValueFromPath(data, path))
        .FirstOrDefault(value => !ReferenceEquals(value, Null.Value));
    }

    private static object GetValueFromPath(object data, string path)
    {
      var names = path.Split('.');

      foreach (var name in names)
      {
        var jsObject = data as ObjectInstance;

        if (jsObject == null)
          break;

        if (jsObject.HasProperty(name))
        {
          data = jsObject.GetPropertyValue(name);
        }
        else
        {
          data = Null.Value;
          break;
        }
      }

      return data;
    }

    public IEnumerable<object> GetValues(string path)
    {
      object value = GetValue(path);

      if (value is bool)
      {
        if ((bool)value)
        {
          yield return value;
        }
      }
      else
      {
        var s = value as string;
        if (s != null)
        {
          if (!string.IsNullOrEmpty(s))
          {
            yield return value;
          }
        }
        else
        {
          var dictionary = value as IDictionary;
          if (dictionary != null) // Dictionaries also implement IEnumerable
            // so this has to be checked before it.
          {
            if (dictionary.Count > 0)
            {
              yield return value;
            }
          }
          else
          {
            var items = value as IEnumerable;
            if (items != null)
            {
              foreach (var item in items)
              {
                yield return item;
              }
            }
            else if (value != null)
            {
              yield return value;
            }
          }
        }
      }
    }

    public void Write(string text)
    {
      m_writer.Write(text);
    }

    public void Include(string templateName)
    {
      if (m_includeLevel >= IncludeLimit)
      {
        throw new JustacheException(
            string.Format("You have reached the include limit of {0}. Are you trying to render infinitely recursive templates or data?", IncludeLimit));
      }

      m_includeLevel++;

      TemplateDefinition templateDefinition = GetTemplateDefinition(templateName);

      if (templateDefinition != null)
      {
        templateDefinition.Render(this);
      }
      else if (m_templateLocator != null)
      {
        var template = m_templateLocator(templateName);

        if (template != null)
        {
          template.Render(this);
        }
      }

      m_includeLevel--;
    }

    private TemplateDefinition GetTemplateDefinition(string name)
    {
      return m_sectionStack
        .Select(section => section.GetTemplateDefinition(name))
        .FirstOrDefault(templateDefinition => templateDefinition != null);
    }

    public void Push(Section section, object data)
    {
      m_sectionStack.Push(section);
      m_dataStack.Push(data);
    }

    public void Pop()
    {
      m_sectionStack.Pop();
      m_dataStack.Pop();
    }
  }
}